using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FoodOrderingSystem.Web.Pages
{
    public class UsersModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public UsersModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // =====================================================
        // USERS
        // =====================================================

        public List<UserViewModel> Users { get; set; } = new();

        public string ErrorMessage { get; set; } = string.Empty;

        // =====================================================
        // SEARCH / PAGINATION / SORTING
        // =====================================================

        public int CurrentPage { get; set; } = 1;

        public int PageSize { get; set; } = 5;

        public string CurrentFirstName { get; set; } = string.Empty;

        public string CurrentLastName { get; set; } = string.Empty;

        public string CurrentSortBy { get; set; } = "firstName";

        // =====================================================
        // ADD USER
        // =====================================================

        [BindProperty]
        public string FirstName { get; set; } = string.Empty;

        [BindProperty]
        public string LastName { get; set; } = string.Empty;

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public string Phone { get; set; } = string.Empty;

        // =====================================================
        // EDIT USER
        // =====================================================

        [BindProperty]
        public int EditId { get; set; }

        [BindProperty]
        public string EditFirstName { get; set; } = string.Empty;

        [BindProperty]
        public string EditLastName { get; set; } = string.Empty;

        [BindProperty]
        public string EditEmail { get; set; } = string.Empty;

        [BindProperty]
        public string EditPassword { get; set; } = string.Empty;

        [BindProperty]
        public string EditPhone { get; set; } = string.Empty;

        // =====================================================
        // GET
        // =====================================================

        public async Task<IActionResult> OnGetAsync(
            string? firstName,
            string? lastName,
            int page = 1,
            int pageSize = 5,
            string sortBy = "firstName")
        {
            var token =
                HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToPage("/Login");
            }

            CurrentPage = page < 1 ? 1 : page;

            PageSize = pageSize < 1 ? 5 : pageSize;

            if (PageSize > 100)
            {
                PageSize = 100;
            }

            CurrentFirstName =
                firstName ?? string.Empty;

            CurrentLastName =
                lastName ?? string.Empty;

            CurrentSortBy =
                string.IsNullOrWhiteSpace(sortBy)
                    ? "firstName"
                    : sortBy;

            await LoadUsersAsync(
                token,
                CurrentPage,
                PageSize,
                CurrentFirstName,
                CurrentLastName,
                CurrentSortBy);

            return Page();
        }

        // =====================================================
        // ADD USER
        // =====================================================

        public async Task<IActionResult> OnPostAsync()
        {
            var token =
                HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToPage("/Login");
            }

            if (string.IsNullOrWhiteSpace(FirstName))
            {
                ErrorMessage =
                    "First Name is required.";

                await LoadUsersAsync(token);

                return Page();
            }

            if (FirstName.Length > 50)
            {
                ErrorMessage =
                    "First Name cannot be longer than 50 characters.";

                await LoadUsersAsync(token);

                return Page();
            }

            if (string.IsNullOrWhiteSpace(LastName))
            {
                ErrorMessage =
                    "Last Name is required.";

                await LoadUsersAsync(token);

                return Page();
            }

            if (LastName.Length > 50)
            {
                ErrorMessage =
                    "Last Name cannot be longer than 50 characters.";

                await LoadUsersAsync(token);

                return Page();
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage =
                    "Email is required.";

                await LoadUsersAsync(token);

                return Page();
            }

            if (Email.Length > 100)
            {
                ErrorMessage =
                    "Email cannot be longer than 100 characters.";

                await LoadUsersAsync(token);

                return Page();
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage =
                    "Password is required.";

                await LoadUsersAsync(token);

                return Page();
            }

            if (Password.Length < 6)
            {
                ErrorMessage =
                    "Password must contain at least 6 characters.";

                await LoadUsersAsync(token);

                return Page();
            }

            if (Password.Length > 100)
            {
                ErrorMessage =
                    "Password cannot be longer than 100 characters.";

                await LoadUsersAsync(token);

                return Page();
            }

            if (string.IsNullOrWhiteSpace(Phone))
            {
                ErrorMessage =
                    "Phone is required.";

                await LoadUsersAsync(token);

                return Page();
            }

            if (Phone.Length > 20)
            {
                ErrorMessage =
                    "Phone cannot be longer than 20 characters.";

                await LoadUsersAsync(token);

                return Page();
            }

            var user = new UserInputModel
            {
                FirstName = FirstName,
                LastName = LastName,
                Email = Email,
                Password = Password,
                Phone = Phone
            };

            try
            {
                var client =
                    CreateClient(token);

                var response =
                    await client.PostAsJsonAsync(
                        "api/Users",
                        user);

                var responseBody =
                    await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage();
                }

                if (response.StatusCode ==
                    HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Clear();

                    return RedirectToPage("/Login");
                }

                ErrorMessage =
                    $"API Error ({(int)response.StatusCode}): {responseBody}";

                await LoadUsersAsync(token);
            }
            catch (Exception ex)
            {
                ErrorMessage =
                    $"Connection Error: {ex.Message}";

                await LoadUsersAsync(token);
            }

            return Page();
        }

        // =====================================================
        // EDIT USER
        // =====================================================

        public async Task<IActionResult> OnPostEditAsync()
        {
            var token =
                HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToPage("/Login");
            }

            if (EditId < 1)
            {
                ErrorMessage =
                    "Invalid user ID.";

                await LoadUsersAsync(token);

                return Page();
            }

            if (string.IsNullOrWhiteSpace(EditFirstName))
            {
                ErrorMessage =
                    "First Name is required.";

                await LoadUsersAsync(token);

                return Page();
            }

            if (string.IsNullOrWhiteSpace(EditLastName))
            {
                ErrorMessage =
                    "Last Name is required.";

                await LoadUsersAsync(token);

                return Page();
            }

            if (string.IsNullOrWhiteSpace(EditEmail))
            {
                ErrorMessage =
                    "Email is required.";

                await LoadUsersAsync(token);

                return Page();
            }

            if (string.IsNullOrWhiteSpace(EditPassword))
            {
                ErrorMessage =
                    "Password is required for editing the user.";

                await LoadUsersAsync(token);

                return Page();
            }

            if (EditPassword.Length < 6)
            {
                ErrorMessage =
                    "Password must contain at least 6 characters.";

                await LoadUsersAsync(token);

                return Page();
            }

            if (string.IsNullOrWhiteSpace(EditPhone))
            {
                ErrorMessage =
                    "Phone is required.";

                await LoadUsersAsync(token);

                return Page();
            }

            var user = new UserInputModel
            {
                FirstName = EditFirstName,
                LastName = EditLastName,
                Email = EditEmail,
                Password = EditPassword,
                Phone = EditPhone
            };

            try
            {
                var client =
                    CreateClient(token);

                var response =
                    await client.PutAsJsonAsync(
                        $"api/Users/{EditId}",
                        user);

                var responseBody =
                    await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage();
                }

                if (response.StatusCode ==
                    HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Clear();

                    return RedirectToPage("/Login");
                }

                ErrorMessage =
                    $"API Error ({(int)response.StatusCode}): {responseBody}";

                await LoadUsersAsync(token);
            }
            catch (Exception ex)
            {
                ErrorMessage =
                    $"Connection Error: {ex.Message}";

                await LoadUsersAsync(token);
            }

            return Page();
        }

        // =====================================================
        // DELETE USER
        // =====================================================

        public async Task<IActionResult> OnPostDeleteAsync(
            int id)
        {
            var token =
                HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToPage("/Login");
            }

            try
            {
                var client =
                    CreateClient(token);

                var response =
                    await client.DeleteAsync(
                        $"api/Users/{id}");

                var responseBody =
                    await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage();
                }

                if (response.StatusCode ==
                    HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Clear();

                    return RedirectToPage("/Login");
                }

                ErrorMessage =
                    $"API Error ({(int)response.StatusCode}): {responseBody}";

                await LoadUsersAsync(token);
            }
            catch (Exception ex)
            {
                ErrorMessage =
                    $"Connection Error: {ex.Message}";

                await LoadUsersAsync(token);
            }

            return Page();
        }

        // =====================================================
        // HTTP CLIENT
        // =====================================================

        private HttpClient CreateClient(string token)
        {
            var client =
                _httpClientFactory.CreateClient(
                    "FoodOrderingAPI");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            return client;
        }

        // =====================================================
        // LOAD USERS
        // =====================================================

        private async Task LoadUsersAsync(
            string token,
            int page = 1,
            int pageSize = 5,
            string firstName = "",
            string lastName = "",
            string sortBy = "firstName")
        {
            try
            {
                var client =
                    CreateClient(token);

                var url =
                    $"api/Users" +
                    $"?page={page}" +
                    $"&pageSize={pageSize}" +
                    $"&sortBy={Uri.EscapeDataString(sortBy)}";

                if (!string.IsNullOrWhiteSpace(firstName))
                {
                    url +=
                        $"&firstName={Uri.EscapeDataString(firstName)}";
                }

                if (!string.IsNullOrWhiteSpace(lastName))
                {
                    url +=
                        $"&lastName={Uri.EscapeDataString(lastName)}";
                }

                var response =
                    await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Users =
                        new List<UserViewModel>();

                    return;
                }

                var json =
                    await response.Content.ReadAsStringAsync();

                Users =
                    JsonSerializer.Deserialize<
                        List<UserViewModel>>(
                        json,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        })
                    ?? new List<UserViewModel>();
            }
            catch
            {
                Users =
                    new List<UserViewModel>();
            }
        }

        // =====================================================
        // USER VIEW MODEL
        // =====================================================

        public class UserViewModel
        {
            public int Id { get; set; }

            public string FirstName { get; set; } =
                string.Empty;

            public string LastName { get; set; } =
                string.Empty;

            public string Email { get; set; } =
                string.Empty;

            public string Phone { get; set; } =
                string.Empty;

            public DateTime RegistrationDate { get; set; }

            public bool IsActive { get; set; }
        }

        // =====================================================
        // API INPUT MODEL
        // =====================================================

        public class UserInputModel
        {
            public string FirstName { get; set; } =
                string.Empty;

            public string LastName { get; set; } =
                string.Empty;

            public string Email { get; set; } =
                string.Empty;

            public string Password { get; set; } =
                string.Empty;

            public string Phone { get; set; } =
                string.Empty;
        }
    }
}