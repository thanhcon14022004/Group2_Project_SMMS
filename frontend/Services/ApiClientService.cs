using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using project_PRN232.Frontend.Models;

namespace project_PRN232.Frontend.Services
{
    public class ApiClientService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiClientService(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        private HttpRequestMessage CreateRequest(HttpMethod method, string endpoint)
        {
            var req = new HttpRequestMessage(method, endpoint);
            var token = _httpContextAccessor.HttpContext?.Session.GetString("SMMS_JWT_TOKEN");
            if (!string.IsNullOrWhiteSpace(token))
            {
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return req;
        }

        public CurrentUserViewModel? GetCurrentUser()
        {
            var json = _httpContextAccessor.HttpContext?.Session.GetString("SMMS_USER_INFO");
            if (string.IsNullOrWhiteSpace(json)) return null;
            return JsonSerializer.Deserialize<CurrentUserViewModel>(json, _jsonOptions);
        }

        public async Task<CurrentUserViewModel?> LoginAsync(string username, string password)
        {
            var content = new StringContent(JsonSerializer.Serialize(new { username, password }), Encoding.UTF8, "application/json");
            var response = await _http.PostAsync("auth/login", content);
            if (!response.IsSuccessStatusCode)
            {
                var errStr = await response.Content.ReadAsStringAsync();
                throw new Exception("Đăng nhập thất bại. Vui lòng kiểm tra tài khoản và mật khẩu.");
            }

            var respJson = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<CurrentUserViewModel>(respJson, _jsonOptions);
            if (user != null && !string.IsNullOrWhiteSpace(user.Token))
            {
                _httpContextAccessor.HttpContext?.Session.SetString("SMMS_JWT_TOKEN", user.Token);
                _httpContextAccessor.HttpContext?.Session.SetString("SMMS_USER_INFO", respJson);
                _httpContextAccessor.HttpContext?.Session.SetString("SMMS_USER_ROLE", user.Role);
                _httpContextAccessor.HttpContext?.Session.SetString("SMMS_USER_NAME", user.FullName);

                // Set token in Cookie so it is visible in DevTools -> Application -> Cookies
                _httpContextAccessor.HttpContext?.Response.Cookies.Append("SMMS_JWT_TOKEN", user.Token, new CookieOptions
                {
                    HttpOnly = false,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });
            }
            return user;
        }

        public void Logout()
        {
            _httpContextAccessor.HttpContext?.Response.Cookies.Delete("SMMS_JWT_TOKEN");
            _httpContextAccessor.HttpContext?.Session.Clear();
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            using var req = CreateRequest(HttpMethod.Get, endpoint);
            var response = await _http.SendAsync(req);
            if (!response.IsSuccessStatusCode) return default;

            var json = await response.Content.ReadAsStringAsync();
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    // Thử giải mã nếu là wrapper ApiResponse<T>
                    if (doc.RootElement.TryGetProperty("data", out var dataElem) &&
                        doc.RootElement.TryGetProperty("success", out _))
                    {
                        return JsonSerializer.Deserialize<T>(dataElem.GetRawText(), _jsonOptions);
                    }

                    // Hỗ trợ định dạng OData chuẩn v4 (Envelope có chứa thuộc tính "value")
                    if (doc.RootElement.TryGetProperty("value", out var valElem))
                    {
                        return JsonSerializer.Deserialize<T>(valElem.GetRawText(), _jsonOptions);
                    }
                }
            }
            catch (JsonException)
            {
                throw;
            }
            catch { }

            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }

        public async Task<(bool Success, string Message, T? Data)> PostAsync<T>(string endpoint, object body)
        {
            using var req = CreateRequest(HttpMethod.Post, endpoint);
            req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await _http.SendAsync(req);
            var json = await response.Content.ReadAsStringAsync();

            string message = response.IsSuccessStatusCode ? "Thành công" : "Lỗi xử lý yêu cầu.";
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("message", out var msgElem))
                {
                    message = msgElem.GetString() ?? message;
                }
                if (doc.RootElement.TryGetProperty("data", out var dataElem))
                {
                    var data = JsonSerializer.Deserialize<T>(dataElem.GetRawText(), _jsonOptions);
                    return (response.IsSuccessStatusCode, message, data);
                }
            }
            catch { }

            T? result = default;
            try
            {
                result = JsonSerializer.Deserialize<T>(json, _jsonOptions);
            }
            catch { }

            return (response.IsSuccessStatusCode, message, result);
        }

        public async Task<(bool Success, string Message)> PutAsync(string endpoint, object body)
        {
            using var req = CreateRequest(HttpMethod.Put, endpoint);
            req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await _http.SendAsync(req);
            var json = await response.Content.ReadAsStringAsync();

            string message = response.IsSuccessStatusCode ? "Cập nhật thành công." : "Lỗi cập nhật.";
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("message", out var msgElem))
                {
                    message = msgElem.GetString() ?? message;
                }
            }
            catch { }

            return (response.IsSuccessStatusCode, message);
        }

        public async Task<(bool Success, string Message)> PatchAsync(string endpoint, object body)
        {
            using var req = CreateRequest(HttpMethod.Patch, endpoint);
            req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await _http.SendAsync(req);
            var json = await response.Content.ReadAsStringAsync();

            string message = response.IsSuccessStatusCode ? "Cập nhật trạng thái thành công." : "Lỗi cập nhật trạng thái.";
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("message", out var msgElem))
                {
                    message = msgElem.GetString() ?? message;
                }
            }
            catch { }

            return (response.IsSuccessStatusCode, message);
        }

        public async Task<(bool Success, string Message)> DeleteAsync(string endpoint)
        {
            using var req = CreateRequest(HttpMethod.Delete, endpoint);
            var response = await _http.SendAsync(req);
            var json = await response.Content.ReadAsStringAsync();

            string message = response.IsSuccessStatusCode ? "Xóa thành công." : "Lỗi thao tác xóa.";
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("message", out var msgElem))
                {
                    message = msgElem.GetString() ?? message;
                }
            }
            catch { }

            return (response.IsSuccessStatusCode, message);
        }

        public async Task<byte[]?> GetByteArrayAsync(string endpoint)
        {
            using var req = CreateRequest(HttpMethod.Get, endpoint);
            var response = await _http.SendAsync(req);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}

