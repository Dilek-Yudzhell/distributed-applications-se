using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
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
            // Check empty fields
            if (string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage =
                    "Email and password are required.";

                return Page();
            }

            try
            {
                // Create API client
                var client =
                    _httpClientFactory.CreateClient(
                        "FoodOrderingAPI");

                // Login data
                var loginData = new
                {
                    email = Email.Trim(),
                    password = Password
                };

                // Convert to JSON
                var json =
                    JsonSerializer.Serialize(loginData);

                var content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

                // Send login request
                var response = await client.PostAsync(
                    "api/Login",
                    content);

                // Login failed
                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage =
                        "Invalid email or password.";

                    return Page();
                }

                // Read API response
                var responseBody =
                    await response.Content
                        .ReadAsStringAsync();

                using var document =
                    JsonDocument.Parse(responseBody);

                // Check token
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

                // Save JWT token to session
                HttpContext.Session.SetString(
                    "JwtToken",
                    token);

                // Save first name
                if (document.RootElement.TryGetProperty(
                        "firstName",
                        out var firstNameProperty))
                {
                    HttpContext.Session.SetString(
                        "FirstName",
                        firstNameProperty.GetString() ?? "");
                }

                // Save user ID
                if (document.RootElement.TryGetProperty(
                        "userId",
                        out var userIdProperty))
                {
                    HttpContext.Session.SetString(
                        "UserId",
                        userIdProperty.GetInt32().ToString());
                }

                // Save email
                if (document.RootElement.TryGetProperty(
                        "email",
                        out var emailProperty))
                {
                    HttpContext.Session.SetString(
                        "Email",
                        emailProperty.GetString() ?? "");
                }

                // Redirect after successful login
                return RedirectToPage("/Index");
            }
            catch
            {
                ErrorMessage =
                    "Could not connect to the API.";

                return Page();
            }
        }
    }
}