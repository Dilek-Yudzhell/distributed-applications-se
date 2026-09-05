using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace FoodOrderingSystem.Web.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public LoginModel(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage =
                    "Email and password are required.";

                return Page();
            }

            try
            {
                var client =
                    _httpClientFactory.CreateClient("API");

                var loginData = new
                {
                    email = Email.Trim(),
                    password = Password
                };

                var json =
                    JsonSerializer.Serialize(loginData);

                using var content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync(
                    "api/Login",
                    content);

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage =
                        "Invalid email or password.";

                    return Page();
                }

                var responseBody =
                    await response.Content.ReadAsStringAsync();

                using var document =
                    JsonDocument.Parse(responseBody);

                if (!document.RootElement.TryGetProperty(
                        "token",
                        out var tokenProperty))
                {
                    ErrorMessage =
                        "Login failed. Token was not received.";

                    return Page();
                }

                var token =
                    tokenProperty.GetString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    ErrorMessage =
                        "Login failed. Token was empty.";

                    return Page();
                }

                HttpContext.Session.SetString(
                    "JwtToken",
                    token);

                if (document.RootElement.TryGetProperty(
                        "firstName",
                        out var firstNameProperty))
                {
                    HttpContext.Session.SetString(
                        "FirstName",
                        firstNameProperty.GetString() ?? "");
                }

                if (document.RootElement.TryGetProperty(
                        "userId",
                        out var userIdProperty))
                {
                    HttpContext.Session.SetString(
                        "UserId",
                        userIdProperty.GetInt32().ToString());
                }

                if (document.RootElement.TryGetProperty(
                        "email",
                        out var emailProperty))
                {
                    HttpContext.Session.SetString(
                        "Email",
                        emailProperty.GetString() ?? "");
                }

                return RedirectToPage("/Index");
            }
            catch (Exception ex)
            {
                ErrorMessage =
                    "Could not connect to the API: " + ex.Message;

                return Page();
            }
        }
    }
}