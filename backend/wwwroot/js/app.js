const { createApp, ref, computed, onMounted, watch } = Vue;

const app = createApp({
  setup() {
    // Auth State
    const token = ref(localStorage.getItem('smms_token') || '');
    const currentUser = ref(JSON.parse(localStorage.getItem('smms_user') || 'null'));
    const loginForm = ref({ username: 'admin', password: 'Admin@123' });
    const loginError = ref('');
    const isLoading = ref(false);

    // Active Navigation Screen
    const currentView = ref('dashboard');

    // Toast & Alerts
    const alertMessage = ref(null); // { type: 'success'|'danger'|'warning', text: '' }
    function showAlert(text, type = 'success') {
      alertMessage.value = { text, type };
      setTimeout(() => { alertMessage.value = null; }, 5000);
    }

    // API Base URL (thông minh: tự nhận diện khi chạy trên port 3000, 5050 hoặc file://)
    const API_BASE = window.API_BASE_URL || (window.location.protocol === 'file:' || window.location.port === '3000' ? 'http://localhost:5050/api' : '/api');

    // HTTP Helper
    async function apiCall(endpoint, options = {}) {
      const headers = {
        'Content-Type': 'application/json',
        ...(options.headers || {})
      };
      if (token.value) {
        headers['Authorization'] = `Bearer ${token.value}`;
      }

      try {
        const url = endpoint.startsWith('http') ? endpoint : `${API_BASE}/${endpoint}`;
        const response = await fetch(url, {
          ...options,
          headers
        });

        if (response.status === 401) {
          logout();
          showAlert('Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.', 'warning');
          throw new Error('Unauthorized');
        }

        if (response.headers.get('content-type')?.includes('text/csv')) {
          return await response.blob();
        }

        const data = await response.json().catch(() => ({}));

        if (!response.ok) {
          throw new Error(data.message || `Lỗi yêu cầu: ${response.statusText}`);
        }

        return data;
      } catch (err) {
        throw err;
      }
    }

    // Authentication Functions
    async function handleLogin() {
      loginError.value = '';
      isLoading.value = true;
      try {
        const res = await apiCall('auth/login', {
          method: 'POST',
          body: JSON.stringify(loginForm.value)
        });

        token.value = res.token;
        currentUser.value = res;
        localStorage.setItem('smms_token', res.token);
        localStorage.setItem('smms_user', JSON.stringify(res));

        showAlert(`Xin chào, ${res.fullName}! Đăng nhập thành công.`);
        currentView.value = 'dashboard';
        loadDashboard();
      } catch (err) {
        loginError.value = err.message || 'Đăng nhập thất bại.';
      } finally {
        isLoading.value = false;
      }
    }

    function quickLogin(username, password) {
      loginForm.value.username = username;
      loginForm.value.password = password;
      handleLogin();
    }

    function logout() {
      token.value = '';
      currentUser.value = null;
      localStorage.removeItem('smms_token');
      localStorage.removeItem('smms_user');
      currentView.value = 'dashboard';
    }

    // 1. Dashboard
    const dashboardData = ref(null);
    let gpaChartInstance = null;

    async function loadDashboard() {
      if (!token.value) return;
      try {
        dashboardData.value = await apiCall('reports/dashboard');
        setTimeout(initDashboardChart, 100);
      } catch (err) {
        console.error(err);
      }
    }

    function initDashboardChart() {
      if (!dashboardData.value?.gpaDistribution) return;
      const ctx = document.getElementById('gpaChart');
      if (!ctx) return;

      if (gpaChartInstance) {
        gpaChartInstance.destroy();
      }

      const dist = dashboardData.value.gpaDistribution;
      gpaChartInstance = new Chart(ctx, {
        type: 'bar',
        data: {
          labels: dist.map(d => d.rank),
          datasets: [{
            label: 'Số lượng sinh viên',
            data: dist.map(d => d.count),
            backgroundColor: ['#48bb78', '#38a169', '#3182ce', '#dd6b20', '#e53e3e']
          }]
        },
        options: {
          responsive: true,
          plugins: { legend: { display: false } },
          scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }
        }
      });
    }

    // 2. Users Management
    const usersList = ref([]);
    const userRoleFilter = ref('');
    const userSearch = ref('');
    const newUserModal = ref({
      username: '', password: '', fullName: '', email: '', phoneNumber: '',
      role: 'Student', code: '', majorId: 1, administrativeClassId: 1
    });

    async function loadUsers() {
      try {
        let url = `auth/users?role=${userRoleFilter.value}&search=${encodeURIComponent(userSearch.value)}`;
        usersList.value = await apiCall(url);
      } catch (err) {
        showAlert(err.message, 'danger');
      }
    }

    async function toggleUserStatus(user) {
      const confirmAction = confirm(`Bạn có chắc muốn ${user.isActive ? 'vô hiệu hóa' : 'kích hoạt lại'} tài khoản ${user.username}?`);
      if (!confirmAction) return;

      try {
        await apiCall(`auth/users/${user.id}/status`, {
          method: 'PATCH',
          body: JSON.stringify({ isActive: !user.isActive })
        });
        showAlert(`Đã cập nhật trạng thái tài khoản ${user.username}.`);
        loadUsers();
      } catch (err) {
        showAlert(err.message, 'danger');
      }
    }

    async function createUser() {
      try {
        await apiCall('auth/users', {
          method: 'POST',
          body: JSON.stringify(newUserModal.value)
        });
        showAlert('Tạo tài khoản mới thành công.');
        loadUsers();
        // Reset form
        newUserModal.value = { username: '', password: '', fullName: '', email: '', phoneNumber: '', role: 'Student', code: '', majorId: 1, administrativeClassId: 1 };
        bootstrap.Modal.getInstance(document.getElementById('modalCreateUser'))?.hide();
      } catch (err) {
        showAlert(err.message, 'danger');
      }
    }

    // 3. Majors & Specializations
    const majorsList = ref([]);
    const newMajor = ref({ code: '', name: '', description: '', isActive: true });
    const newSpec = ref({ majorId: 1, code: '', name: '', description: '', isActive: true });

    async function loadMajors() {
      try {
        majorsList.value = await apiCall('majors');
      } catch (err) {
        showAlert(err.message, 'danger');
      }
    }

    async function createMajor() {
      try {
        await apiCall('majors', { method: 'POST', body: JSON.stringify(newMajor.value) });
        showAlert('Thêm ngành mới thành công.');
        newMajor.value = { code: '', name: '', description: '', isActive: true };
        loadMajors();
      } catch (err) {
        showAlert(err.message, 'danger');
      }
    }

    async function createSpecialization() {
      try {
        await apiCall(`majors/${newSpec.value.majorId}/specializations`, {
          method: 'POST',
          body: JSON.stringify(newSpec.value)
        });
        showAlert('Thêm ngành hẹp thành công.');
        newSpec.value = { majorId: 1, code: '', name: '', description: '', isActive: true };
        loadMajors();
      } catch (err) {
        showAlert(err.message, 'danger');
      }
    }

    // 4. Subjects & Grade Components & Prerequisites
    const subjectsList = ref([]);
    const newSubject = ref({ code: '', name: '', credits: 3, description: '', isActive: true });
    const selectedSubjectForComponents = ref(null);
    const editingComponents = ref([]);
    const selectedSubjectForPrereq = ref(null);
    const newPrereq = ref({ prerequisiteSubjectId: 0, minScore: 5.0 });

    async function loadSubjects() {
      try {
        subjectsList.value = await apiCall('subjects');
      } catch (err) {
        showAlert(err.message, 'danger');
      }
    }

    async function createSubject() {
      try {
        await apiCall('subjects', { method: 'POST', body: JSON.stringify(newSubject.value) });
        showAlert('Tạo môn học thành công.');
        newSubject.value = { code: '', name: '', credits: 3, description: '', isActive: true };
        loadSubjects();
      } catch (err) {
        showAlert(err.message, 'danger');
      }
    }

    function openGradeComponentsModal(subject) {
      selectedSubjectForComponents.value = subject;
      editingComponents.value = (subject.gradeComponents || []).map(c => ({ ...c }));
      if (editingComponents.value.length === 0) {
        editingComponents.value = [
          { name: 'Lab / Thực hành', weight: 20, scale: 10, isAdminOnly: false },
          { name: 'Assignment / Bài tập lớn', weight: 20, scale: 10, isAdminOnly: false },
          { name: 'Progress Test / Kiểm tra', weight: 20, scale: 10, isAdminOnly: false },
          { name: 'Final Exam / Thi cuối kỳ', weight: 40, scale: 10, isAdminOnly: true }
        ];
      }
      const modal = new bootstrap.Modal(document.getElementById('modalGradeComponents'));
      modal.show();
    }

    function addComponentRow() {
      editingComponents.value.push({ name: '', weight: 0, scale: 10, isAdminOnly: false });
    }

    function removeComponentRow(index) {
      editingComponents.value.splice(index, 1);
    }

    const totalComponentWeight = computed(() => {
      return editingComponents.value.reduce((sum, c) => sum + (Number(c.weight) || 0), 0);
    });

    async function saveGradeComponents() {
      if (Math.abs(totalComponentWeight.value - 100) > 0.01) {
        showAlert(`Tổng tỷ trọng phải bằng 100%. Hiện tại là ${totalComponentWeight.value}%.`, 'danger');
        return;
      }

      try {
        await apiCall(`subjects/${selectedSubjectForComponents.value.id}/components`, {
          method: 'PUT',
          body: JSON.stringify({ components: editingComponents.value })
        });
        showAlert(`Đã lưu cấu hình đầu điểm cho môn ${selectedSubjectForComponents.value.code}.`);
        loadSubjects();
        bootstrap.Modal.getInstance(document.getElementById('modalGradeComponents'))?.hide();
      } catch (err) {
        showAlert(err.message, 'danger');
      }
    }

    function openPrereqModal(subject) {
      selectedSubjectForPrereq.value = subject;
      newPrereq.value = { prerequisiteSubjectId: subjectsList.value.find(s => s.id !== subject.id)?.id || 0, minScore: 5.0 };
      const modal = new bootstrap.Modal(document.getElementById('modalPrerequisites'));
      modal.show();
    }

    async function addPrerequisite() {
      try {
        await apiCall(`subjects/${selectedSubjectForPrereq.value.id}/prerequisites`, {
          method: 'POST',
          body: JSON.stringify({
            subjectId: selectedSubjectForPrereq.value.id,
            prerequisiteSubjectId: newPrereq.value.prerequisiteSubjectId,
            minScore: newPrereq.value.minScore
          })
        });
        showAlert('Đã thêm môn tiên quyết.');
        loadSubjects();
        bootstrap.Modal.getInstance(document.getElementById('modalPrerequisites'))?.hide();
      } catch (err) {
        showAlert(err.message, 'danger');
      }
    }

    async function removePrerequisite(subjectId, prereqId) {
      if (!confirm('Xác nhận xóa môn tiên quyết này?')) return;
      try {
        await apiCall(`subjects/${subjectId}/prerequisites/${prereqId}`, { method: 'DELETE' });
        showAlert('Đã xóa điều kiện tiên quyết.');
        loadSubjects();
      } catch (err) {
        showAlert(err.message, 'danger');
      }
    }

    // 5. Curriculum Configurations & Versioning
    const curriculumsList = ref([]);
    const newCurriculum = ref({
      version: '2024.1', majorId: 1, semesterNumber: 1, specializationId: null,
      name: '', description: '', subjectIds: []
    });

    async function loadCurriculums() {
      try {
        curriculumsList.value = await apiCall('curriculums');
      } catch (err) {
        showAlert(err.message, 'danger');
      }
    }

    async function createCurriculum() {
      try {
        await apiCall('curriculums', {
          method: 'POST',
          body: JSON.stringify(newCurriculum.value)
        });
        showAlert('Lưu cấu hình chương trình đào tạo thành công.');
        loadCurriculums();
        bootstrap.Modal.getInstance(document.getElementById('modalCreateCurriculum'))?.hide();
      } catch (err) {
        showAlert(err.message, 'danger');
      }
    }

    // 6. Tra cứu cấu hình & Mở lớp học phần tự động (Core Business Step 3)
    const lookupForm = ref({
      administrativeClassId: 1,
      semesterId: 1,
      version: '2024.1'
    });
    const lookupResult = ref(null);
    const lookupError = ref('');
    const isLookingUp = ref(false);

    async function runCurriculumLookup() {
      lookupError.value = '';
      lookupResult.value = null;
      isLookingUp.value = true;

      const adminClass = administrativeClassesList.value.find(c => c.id === lookupForm.value.administrativeClassId);
      if (!adminClass) {
        lookupError.value = 'Vui lòng chọn lớp hành chính.';
        isLookingUp.value = false;
        return;
      }

      try {
        const res = await apiCall('curriculums/lookup', {
          method: 'POST',
          body: JSON.stringify({
            majorId: adminClass.majorId,
            semesterNumber: adminClass.currentSemester,
            specializationId: adminClass.specializationId,
            version: lookupForm.value.version
          })
        });

        lookupResult.value = res;
      } catch (err) {
        lookupError.value = err.message || 'Không tìm thấy cấu hình chương trình đào tạo phù hợp.';
      } finally {
        isLookingUp.value = false;
      }
    }

    async function batchCreateSectionsFromCurriculum() {
      if (!confirm('Xác nhận tạo tự động các lớp học phần cho lớp hành chính này theo cấu hình tra cứu?')) return;
      try {
        const res = await apiCall('coursesections/batch-create-from-curriculum', {
          method: 'POST',
          body: JSON.stringify({
            administrativeClassId: lookupForm.value.administrativeClassId,
            semesterId: lookupForm.value.semesterId,
            version: lookupForm.value.version,
            defaultMaxCapacity: 30
          })
        });

        showAlert(res.message);
        loadCourseSections();
        currentView.value = 'course-sections';
      } catch (err) {
        showAlert(err.message, 'danger');
      }
    }

    // 7. Administrative Classes
    const administrativeClassesList = ref([]);
    const newClass = ref({ code: '', name: '', majorId: 1, cohort: 17, academicYear: '2023-2027', currentSemester: 1, specializationId: null, isActive: true });

    async function loadAdministrativeClasses() {
      try {
        administrativeClassesList.value = await apiCall('administrativeclasses');
      } catch (err) {
        showAlert(err.message, 'danger');
      }
    }

    async function createAdministrativeClass() {
      try {
        await apiCall('administrativeclasses', { method: 'POST', body: JSON.stringify(newClass.value) });
        showAlert('Tạo lớp hành chính thành công.');
        newClass.value = { code: '', name: '', majorId: 1, cohort: 17, academicYear: '2023-2027', currentSemester: 1, specializationId: null, isActive: true };
        loadAdministrativeClasses();
      } catch (err) {
        showAlert(err.message, 'danger');
      }
    }

    // 8. Course Sections & Lecturer Assignment
    const courseSectionsList = ref([]);
    const filterSectionSemester = ref('');
    const selectedSectionForLecturer = ref(null);
    const newLecturerId = ref(null);

    async function loadCourseSections() {
      try {
        let url = 'coursesections';
        if (filterSectionSemester.value) url += `?semesterId=${filterSectionSemester.value}`;
        courseSectionsList.value = await apiCall(url);
      } catch (err) {
        showAlert(err.message, 'danger');
      }
    }

    function openAssignLecturerModal(section) {
      selectedSectionForLecturer.value = section;
      newLecturerId.value = section.lecturerId;
      const modal = new bootstrap.Modal(document.getElementById('modalAssignLecturer'));
      modal.show();
    }

    async function saveAssignLecturer() {
      if (!selectedSectionForLecturer.value) return;
      try {
        const res = await apiCall(`coursesections/${selectedSectionForLecturer.value.id}/lecturer`, {
          method: 'PATCH',
          body: JSON.stringify({ lecturerId: newLecturerId.value ? Number(newLecturerId.value) : null })
        });
        showAlert(res.message);
        loadCourseSections();
        bootstrap.Modal.getInstance(document.getElementById('modalAssignLecturer'))?.hide();
      } catch (err) {
        showAlert(err.message, 'danger');
      }
    }

    // 9. Student Allocation Engine
    const allocationForm = ref({ semesterId: 1, majorId: null, administrativeClassId: null, curriculumVersion: '2024.1' });
    const allocationResult = ref(null);
    const isAllocating = ref(false);

    async function runAllocation() {
      isAllocating.value = true;
      allocationResult.value = null;
      try {
        const res = await apiCall('allocations/run', {
          method: 'POST',
          body: JSON.stringify(allocationForm.value)
        });
        allocationResult.value = res;
        showAlert(`Phân bổ hoàn tất: Xếp thành công ${res.totalEnrollmentsCreated} lượt lớp, ${res.unallocatedCount} ngoại lệ.`);
      } catch (err) {
        showAlert(err.message, 'danger');
      } finally {
        isAllocating.value = false;
      }
    }

    // 10. Grade Sheet & Entry
    const selectedSectionIdForGrades = ref(null);
    const gradeSheet = ref(null);
    const isSavingGrades = ref(false);

    async function loadGradeSheet(sectionId) {
      selectedSectionIdForGrades.value = sectionId;
      try {
        gradeSheet.value = await apiCall(`grades/section/${sectionId}`);
      } catch (err) {
        showAlert(err.message, 'danger');
      }
    }

    async function saveGrades() {
      if (!gradeSheet.value) return;
      isSavingGrades.value = true;

      const payload = {
        courseSectionId: gradeSheet.value.courseSectionId,
        grades: []
      };

      for (const row of gradeSheet.value.studentRows) {
        for (const comp of gradeSheet.value.components) {
          const scoreVal = row.componentScores[comp.id];
          payload.grades.push({
            studentId: row.studentId,
            subjectGradeComponentId: comp.id,
            score: (scoreVal === '' || scoreVal === null || scoreVal === undefined) ? null : Number(scoreVal)
          });
        }
      }

      try {
        await apiCall('grades/section', {
          method: 'POST',
          body: JSON.stringify(payload)
        });
        showAlert('Đã lưu điểm cho lớp học phần thành công.');
        loadGradeSheet(gradeSheet.value.courseSectionId);
      } catch (err) {
        showAlert(err.message, 'danger');
      } finally {
        isSavingGrades.value = false;
      }
    }

    async function toggleGradeLock(isLocked) {
      if (!gradeSheet.value) return;
      const actionName = isLocked ? 'khóa' : 'mở khóa';
      if (!confirm(`Bạn có chắc muốn ${actionName} điểm cho lớp ${gradeSheet.value.courseSectionCode}?`)) return;

      try {
        await apiCall(`grades/section/${gradeSheet.value.courseSectionId}/lock?isLocked=${isLocked}`, {
          method: 'PUT'
        });
        showAlert(`Đã ${actionName} bảng điểm thành công.`);
        loadGradeSheet(gradeSheet.value.courseSectionId);
      } catch (err) {
        showAlert(err.message, 'danger');
      }
    }

    // 11. Student Transcript & GPA View
    const studentTranscript = ref(null);

    async function loadTranscript(studentId = null) {
      try {
        let url = 'grades/transcript';
        if (studentId) url += `?studentId=${studentId}`;
        studentTranscript.value = await apiCall(url);
      } catch (err) {
        showAlert(err.message, 'danger');
      }
    }

    // 12. Class Transfers
    const transferRequestsList = ref([]);
    const transferEligibility = ref(null);
    const newTransferForm = ref({ targetAdministrativeClassId: 1, reason: '' });

    async function checkTransferEligibility() {
      try {
        transferEligibility.value = await apiCall(`classtransfers/eligibility?targetClassId=${newTransferForm.value.targetAdministrativeClassId}`);
      } catch (err) {
        showAlert(err.message, 'danger');
      }
    }

    async function submitTransferRequest() {
      try {
        await apiCall('classtransfers/request', {
          method: 'POST',
          body: JSON.stringify(newTransferForm.value)
        });
        showAlert('Đã gửi yêu cầu chuyển lớp hành chính.');
        newTransferForm.value.reason = '';
        transferEligibility.value = null;
      } catch (err) {
        showAlert(err.message, 'danger');
      }
    }

    async function loadTransferRequests() {
      try {
        transferRequestsList.value = await apiCall('classtransfers/requests');
      } catch (err) {
        showAlert(err.message, 'danger');
      }
    }

    async function processTransferRequest(requestId, approve) {
      const note = prompt(`Nhập ghi chú ${approve ? 'phê duyệt' : 'từ chối'}:`, approve ? 'Đồng ý chuyển lớp' : 'Không đáp ứng điều kiện');
      if (note === null) return;

      try {
        await apiCall('classtransfers/process', {
          method: 'POST',
          body: JSON.stringify({ requestId, approve, note })
        });
        showAlert(`Đã ${approve ? 'phê duyệt' : 'từ chối'} yêu cầu chuyển lớp.`);
        loadTransferRequests();
      } catch (err) {
        showAlert(err.message, 'danger');
      }
    }

    // 13. Semesters Management & Closing Check
    const semestersList = ref([]);
    const closingInspection = ref(null);
    const closeSemesterModal = ref({ semesterId: 1, forceWithExceptions: false, reason: '' });

    async function loadSemesters() {
      try {
        semestersList.value = await apiCall('semesters');
      } catch (err) {
        showAlert(err.message, 'danger');
      }
    }

    async function inspectSemesterClosing(semesterId) {
      closeSemesterModal.value.semesterId = semesterId;
      try {
        closingInspection.value = await apiCall(`semesterclosing/inspect/${semesterId}`);
        const modal = new bootstrap.Modal(document.getElementById('modalSemesterClosing'));
        modal.show();
      } catch (err) {
        showAlert(err.message, 'danger');
      }
    }

    async function executeCloseSemester() {
      try {
        await apiCall('semesterclosing/close', {
          method: 'POST',
          body: JSON.stringify(closeSemesterModal.value)
        });
        showAlert('Đã hoàn tất kết thúc học kỳ.');
        bootstrap.Modal.getInstance(document.getElementById('modalSemesterClosing'))?.hide();
        loadSemesters();
      } catch (err) {
        showAlert(err.message, 'danger');
      }
    }

    // 14. Reports & CSV Export
    const gpaReportList = ref([]);

    async function loadGpaReport() {
      try {
        gpaReportList.value = await apiCall('reports/gpa');
      } catch (err) {
        showAlert(err.message, 'danger');
      }
    }

    async function exportGpaCsv() {
      try {
        const blob = await apiCall('reports/export/gpa-csv');
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `SMMS_BaoCao_GPA_${new Date().toISOString().slice(0, 10)}.csv`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      } catch (err) {
        showAlert(err.message, 'danger');
      }
    }

    async function exportSectionGradesCsv(sectionId) {
      try {
        const blob = await apiCall(`reports/export/section-grades-csv/${sectionId}`);
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `SMMS_DiemLop_${sectionId}_${new Date().toISOString().slice(0, 10)}.csv`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      } catch (err) {
        showAlert(err.message, 'danger');
      }
    }

    // 15. Audit Logs
    const auditLogsList = ref([]);
    const auditActionFilter = ref('');

    async function loadAuditLogs() {
      try {
        let url = 'auditlogs?limit=200';
        if (auditActionFilter.value) url += `&action=${auditActionFilter.value}`;
        auditLogsList.value = await apiCall(url);
      } catch (err) {
        showAlert(err.message, 'danger');
      }
    }

    // 16. Backups & Restore
    const backupsList = ref([]);
    const restoreConfirmationText = ref('');
    const selectedBackupForRestore = ref(null);

    async function loadBackups() {
      try {
        backupsList.value = await apiCall('backups');
      } catch (err) {
        showAlert(err.message, 'danger');
      }
    }

    async function createBackup() {
      const notes = prompt('Nhập ghi chú cho bản sao lưu:', 'Sao lưu định kỳ');
      if (notes === null) return;
      try {
        await apiCall(`backups/create?notes=${encodeURIComponent(notes)}`, { method: 'POST' });
        showAlert('Tạo bản sao lưu dữ liệu thành công.');
        loadBackups();
      } catch (err) {
        showAlert(err.message, 'danger');
      }
    }

    function openRestoreModal(backup) {
      selectedBackupForRestore.value = backup;
      restoreConfirmationText.value = '';
      const modal = new bootstrap.Modal(document.getElementById('modalRestoreBackup'));
      modal.show();
    }

    async function executeRestore() {
      if (restoreConfirmationText.value !== 'RESTORE') {
        showAlert("Vui lòng gõ chính xác chữ 'RESTORE' để xác nhận.", 'warning');
        return;
      }

      try {
        await apiCall('backups/restore', {
          method: 'POST',
          body: JSON.stringify({
            backupId: selectedBackupForRestore.value.id,
            confirmationText: 'RESTORE'
          })
        });
        showAlert('Khôi phục cơ sở dữ liệu thành công!');
        bootstrap.Modal.getInstance(document.getElementById('modalRestoreBackup'))?.hide();
        loadBackups();
      } catch (err) {
        showAlert(err.message, 'danger');
      }
    }

    // View Navigation Handler
    watch(currentView, (newView) => {
      if (newView === 'dashboard') loadDashboard();
      else if (newView === 'users') loadUsers();
      else if (newView === 'majors') loadMajors();
      else if (newView === 'subjects') loadSubjects();
      else if (newView === 'curriculums') { loadCurriculums(); loadMajors(); loadSubjects(); }
      else if (newView === 'curriculum-lookup') { loadAdministrativeClasses(); loadSemesters(); }
      else if (newView === 'admin-classes') { loadAdministrativeClasses(); loadMajors(); }
      else if (newView === 'course-sections') { loadCourseSections(); loadSemesters(); }
      else if (newView === 'allocations') { loadSemesters(); loadMajors(); loadAdministrativeClasses(); }
      else if (newView === 'grades-entry') { loadCourseSections(); }
      else if (newView === 'student-transcript') { loadTranscript(); }
      else if (newView === 'class-transfers') { loadAdministrativeClasses(); loadTransferRequests(); }
      else if (newView === 'semesters') { loadSemesters(); }
      else if (newView === 'reports') { loadGpaReport(); }
      else if (newView === 'audit-logs') { loadAuditLogs(); }
      else if (newView === 'backups') { loadBackups(); }
    });

    onMounted(() => {
      if (token.value) {
        loadDashboard();
      }
    });

    return {
      // Auth
      token, currentUser, loginForm, loginError, isLoading, handleLogin, quickLogin, logout,
      currentView, alertMessage, showAlert,

      // Dashboard
      dashboardData,

      // Users
      usersList, userRoleFilter, userSearch, newUserModal, loadUsers, toggleUserStatus, createUser,

      // Majors
      majorsList, newMajor, newSpec, loadMajors, createMajor, createSpecialization,

      // Subjects & Components
      subjectsList, newSubject, selectedSubjectForComponents, editingComponents, totalComponentWeight,
      openGradeComponentsModal, addComponentRow, removeComponentRow, saveGradeComponents,
      selectedSubjectForPrereq, newPrereq, openPrereqModal, addPrerequisite, removePrerequisite, createSubject,

      // Curriculums
      curriculumsList, newCurriculum, loadCurriculums, createCurriculum,

      // Lookup & Batch Create
      lookupForm, lookupResult, lookupError, isLookingUp, runCurriculumLookup, batchCreateSectionsFromCurriculum,

      // Administrative Classes
      administrativeClassesList, newClass, loadAdministrativeClasses, createAdministrativeClass,

      // Course Sections & Lecturer
      courseSectionsList, filterSectionSemester, selectedSectionForLecturer, newLecturerId,
      loadCourseSections, openAssignLecturerModal, saveAssignLecturer,

      // Allocations
      allocationForm, allocationResult, isAllocating, runAllocation,

      // Grades
      selectedSectionIdForGrades, gradeSheet, isSavingGrades, loadGradeSheet, saveGrades, toggleGradeLock,

      // Student Transcript
      studentTranscript, loadTranscript,

      // Transfers
      transferRequestsList, transferEligibility, newTransferForm, checkTransferEligibility, submitTransferRequest, processTransferRequest,

      // Semesters & Closing
      semestersList, closingInspection, closeSemesterModal, inspectSemesterClosing, executeCloseSemester,

      // Reports
      gpaReportList, exportGpaCsv, exportSectionGradesCsv,

      // Logs & Backups
      auditLogsList, auditActionFilter, loadAuditLogs,
      backupsList, restoreConfirmationText, selectedBackupForRestore, loadBackups, createBackup, openRestoreModal, executeRestore
    };
  }
});

app.mount('#app');

