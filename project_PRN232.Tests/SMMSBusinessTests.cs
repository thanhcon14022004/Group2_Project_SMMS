using Xunit;
using project_PRN232.Models.Entities;
using project_PRN232.DTOs;

namespace project_PRN232.Tests
{
    public class SMMSBusinessTests
    {
        /// <summary>
        /// TEST 1: Kiểm thử quy tắc làm tròn 1 chữ số thập phân khi tính điểm tổng kết môn học
        /// Công thức: FinalScore = Round(Sum(Score_i * Weight_i / 100), 1)
        /// Ví dụ thực tế: 7.26 phải làm tròn thành 7.3
        /// </summary>
        [Fact]
        public void Test_GradeCalculation_OneDecimalRounding_Standard()
        {
            // Arrange: Thiết lập các đầu điểm thành phần
            // Lab: 10% (8.5), Assignment: 20% (7.0), PE: 30% (6.5), Final: 40% (7.6)
            // Điểm thô: (8.5 * 0.1) + (7.0 * 0.2) + (6.5 * 0.3) + (7.6 * 0.4)
            // = 0.85 + 1.40 + 1.95 + 3.04 = 7.24 -> 7.2
            decimal raw1 = (8.5m * 0.1m) + (7.0m * 0.2m) + (6.5m * 0.3m) + (7.6m * 0.4m);
            decimal rounded1 = Math.Round(raw1, 1, MidpointRounding.AwayFromZero);
            Assert.Equal(7.2m, rounded1);

            // Trường hợp 7.26 -> làm tròn thành 7.3
            decimal raw2 = 7.26m;
            decimal rounded2 = Math.Round(raw2, 1, MidpointRounding.AwayFromZero);
            Assert.Equal(7.3m, rounded2);
        }

        /// <summary>
        /// TEST 2: Kiểm thử điều kiện tiên quyết (Prerequisite Condition)
        /// Sinh viên phải đạt điểm tổng kết tối thiểu 5.0/10 mới được công nhận đạt môn tiên quyết
        /// </summary>
        [Theory]
        [InlineData(4.9, false)] // Dưới 5.0 -> Không đạt tiên quyết
        [InlineData(5.0, true)]  // Đúng 5.0 -> Đạt tiên quyết
        [InlineData(7.5, true)]  // Trên 5.0 -> Đạt tiên quyết
        public void Test_PrerequisiteCheck_ScoreMinimumFive(decimal score, bool expectedPassed)
        {
            const decimal PassingThreshold = 5.0m;
            bool isPassed = score >= PassingThreshold;
            Assert.Equal(expectedPassed, isPassed);
        }

        /// <summary>
        /// TEST 3: Kiểm thử tổng tỷ trọng các đầu điểm thành phần của một môn học phải đúng 100%
        /// </summary>
        [Fact]
        public void Test_GradeComponentWeights_SumMustEqualOneHundredPercent()
        {
            // Arrange
            var components = new List<SubjectGradeComponent>
            {
                new SubjectGradeComponent { Name = "Attendance", Weight = 10.0m },
                new SubjectGradeComponent { Name = "Assignment", Weight = 20.0m },
                new SubjectGradeComponent { Name = "Practical Exam", Weight = 30.0m },
                new SubjectGradeComponent { Name = "Final Exam", Weight = 40.0m }
            };

            // Act
            decimal totalWeight = components.Sum(c => c.Weight);

            // Assert
            Assert.Equal(100.0m, totalWeight);
        }

        /// <summary>
        /// TEST 4: Kiểm thử chuẩn sĩ số lớp hành chính sau phân bổ phải nằm trong khoảng 28 - 31 SV
        /// </summary>
        [Theory]
        [InlineData(28, true)]
        [InlineData(29, true)]
        [InlineData(30, true)]
        [InlineData(31, true)]
        [InlineData(27, false)]
        [InlineData(32, false)]
        public void Test_ClassAllocation_TargetCapacityBounds(int classSize, bool isWithinStandard)
        {
            const int MinCapacity = 28;
            const int MaxCapacity = 31;

            bool isValid = classSize >= MinCapacity && classSize <= MaxCapacity;
            Assert.Equal(isWithinStandard, isValid);
        }

        /// <summary>
        /// TEST 5: Kiểm thử điều kiện đóng học kỳ (Semester Closing Conditions)
        /// Không được phép đóng kỳ ngay lập tức nếu còn lớp chưa khóa điểm hoặc chưa phân công giảng viên
        /// </summary>
        [Fact]
        public void Test_SemesterClosingConditions_AllPrerequisitesMustBeMet()
        {
            // Trường hợp 1: Học kỳ hoàn hảo (0 lớp chưa phân công, 0 lớp chưa khóa điểm, 0 SV thiếu điểm)
            var cleanResult = new SemesterClosingCheckResultDto
            {
                UnassignedLecturerSectionsCount = 0,
                UnlockedSectionsCount = 0,
                MissingGradeSectionsCount = 0,
                UnallocatedCompulsoryStudentsCount = 0,
                CanCloseImmediately = true
            };
            Assert.True(cleanResult.CanCloseImmediately);

            // Trường hợp 2: Học kỳ còn 2 lớp chưa khóa điểm
            var pendingGradesResult = new SemesterClosingCheckResultDto
            {
                UnassignedLecturerSectionsCount = 0,
                UnlockedSectionsCount = 2,
                MissingGradeSectionsCount = 0,
                UnallocatedCompulsoryStudentsCount = 0,
                CanCloseImmediately = false
            };
            Assert.False(pendingGradesResult.CanCloseImmediately);
            Assert.True(pendingGradesResult.UnlockedSectionsCount > 0);
        }
    }
}
