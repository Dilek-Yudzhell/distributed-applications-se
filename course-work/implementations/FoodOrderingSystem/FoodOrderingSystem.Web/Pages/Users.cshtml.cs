using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace FoodOrderingSystem.Web.Pages
{
    public class UsersModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public UsersModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<UserViewModel> Users { get; set; } = new();

        [BindProperty]
        public UserInputModel NewUser { get; set; } = new();

        [BindProperty]
        public UserEditModel EditUser { get; set; } = new();

        public string ErrorMessage { get; set; } = string.Empty;

        public string SuccessMessage { get; set; } = string.Empty;

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int TotalCount { get; set; }

        public bool HasNextPage { get; set; }

        public string? SearchFirstName { get; set; }

        public string? SearchLastName { get; set; }

        public string SortBy { get; set; } = "id";


        // =========================
        // GET USERS
        // =========================

        public async Task OnGetAsync(
            string? firstName = null,
            string? lastName = null,
            int page = 1,
            int pageSize = 10,
            string sortBy = "id")
        {
            SearchFirstName = firstName;
            SearchLastName = lastName;
            PageNumber = page;
            PageSize = pageSize;
            SortBy = sortBy;

            await LoadUsersAsync(
                firstName,
                lastName,
                page,
                pageSize,
                sortBy);
        }


        // =========================
        // ADD USER
        // =========================

        public async Task<IActionResult> OnPostAddAsync()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            // Remove validation belonging to Edit form
            foreach (var key in ModelState.Keys
                         .Where(k => k.StartsWith("EditUser."))
                         .ToList())
            {
                ModelState.Remove(key);
            }

            if (!ModelState.IsValid)
            {
                await LoadUsersAsync();
                return Page();
            }

            try
            {
                // Registration endpoint is AllowAnonymous
                var client = CreateClient();

                var response = await client.PostAsJsonAsync(
                    "api/Users",
                    new
                    {
                        firstName = NewUser.FirstName,
                        lastName = NewUser.LastName,
                        email = NewUser.Email,
                        password = NewUser.Password,
                        phone = NewUser.Phone
                    });

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage =
                        "User added successfully.";

                    NewUser = new UserInputModel();

                    await LoadUsersAsync();

                    return Page();
                }

                var error =
                    await response.Content.ReadAsStringAsync();

                ErrorMessage =
                    $"Could not add user. API response: {error}";

                await LoadUsersAsync();

                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage =
                    "Could not connect to the API: " +
                    ex.Message;

                await LoadUsersAsync();

                return Page();
            }
        }


        // =========================
        // EDIT USER
        // =========================

        public async Task<IActionResult> OnPostEditAsync()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            // Remove validation belonging to Add form
            foreach (var key in ModelState.Keys
                         .Where(k => k.StartsWith("NewUser."))
                         .ToList())
            {
                ModelState.Remove(key);
            }

            if (!ModelState.IsValid)
            {
                await LoadUsersAsync();
                return Page();
            }

            try
            {
                var token =
                    HttpContext.Session.GetString("JwtToken");

                if (string.IsNullOrWhiteSpace(token))
                {
                    ErrorMessage =
                        "You are not logged in.";

                    await LoadUsersAsync();

                    return Page();
                }

                var client = CreateClient(token);

                var response = await client.PutAsJsonAsync(
                    $"api/Users/{EditUser.Id}",
                    new
                    {
                        firstName = EditUser.FirstName,
                        lastName = EditUser.LastName,
                        email = EditUser.Email,
                        password = EditUser.Password,
                        phone = EditUser.Phone,
                        isActive = EditUser.IsActive
                    });

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage =
                        "User updated successfully.";

                    await LoadUsersAsync();

                    return Page();
                }

                var error =
                    await response.Content.ReadAsStringAsync();

                ErrorMessage =
                    $"Could not update user. API response: {error}";

                await LoadUsersAsync();

                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage =
                    "Could not connect to the API: " +
                    ex.Message;

                await LoadUsersAsync();

                return Page();
            }
        }


        // =========================
        // DELETE USER
        // =========================

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            try
            {
                var token =
                    HttpContext.Session.GetString("JwtToken");

                if (string.IsNullOrWhiteSpace(token))
                {
                    ErrorMessage =
                        "You are not logged in.";

                    await LoadUsersAsync();

                    return Page();
                }

                var client = CreateClient(token);

                var response =
                    await client.DeleteAsync(
                        $"api/Users/{id}");

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage =
                        "User deleted successfully.";

                    await LoadUsersAsync();

                    return Page();
                }

                var error =
                    await response.Content.ReadAsStringAsync();

                ErrorMessage =
                    $"Could not delete user. API response: {error}";

                await LoadUsersAsync();

                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage =
                    "Could not connect to the API: " +
                    ex.Message;

                await LoadUsersAsync();

                return Page();
            }
        }


        // =========================
        // LOAD USERS
        // =========================

        private async Task LoadUsersAsync(
            string? firstName = null,
            string? lastName = null,
            int page = 1,
            int pageSize = 10,
            string sortBy = "id")
        {
            try
            {
                var token =
                    HttpContext.Session.GetString("JwtToken");

                if (string.IsNullOrWhiteSpace(token))
                {
                    ErrorMessage =
                        "You are not logged in.";

                    return;
                }

                var client = CreateClient(token);

                var url =
                    $"api/Users?page={page}" +
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
                    var error =
                        await response.Content.ReadAsStringAsync();

                    ErrorMessage =
                        $"Could not load users. API response: {error}";

                    return;
                }

                var result =
                    await response.Content
                        .ReadFromJsonAsync<UserListResponse>();

                if (result == null)
                {
                    ErrorMessage =
                        "The API returned an empty response.";

                    return;
                }

                Users =
                    result.Data ?? new List<UserViewModel>();

                PageNumber = result.Page;
                PageSize = result.PageSize;
                TotalCount = result.TotalCount;
                HasNextPage = result.HasNextPage;

                SearchFirstName = firstName;
                SearchLastName = lastName;
                SortBy = sortBy;
            }
            catch (Exception ex)
            {
                ErrorMessage =
                    "Could not connect to the API: " +
                    ex.Message;
            }
        }


        // =========================
        // HTTP CLIENT
        // =========================

        private HttpClient CreateClient(string? token = null)
        {
            var client =
                _httpClientFactory.CreateClient("API");

            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer",
                        token);
            }

            return client;
        }


        // =========================
        // USER INPUT MODEL
        // =========================

        public class UserInputModel
        {
            [Required]
            [MaxLength(50)]
            public string FirstName { get; set; } = string.Empty;

            [Required]
            [MaxLength(50)]
            public string LastName { get; set; } = string.Empty;

            [Required]
            [EmailAddress]
            [MaxLength(100)]
            public string Email { get; set; } = string.Empty;

            [Required]
            [MinLength(6)]
            [MaxLength(255)]
            public string Password { get; set; } = string.Empty;

            [Required]
            [MaxLength(20)]
            public string Phone { get; set; } = string.Empty;
        }


        // =========================
        // EDIT MODEL
        // =========================

        public class UserEditModel
        {
            public int Id { get; set; }

            public string FirstName { get; set; } = string.Empty;

            public string LastName { get; set; } = string.Empty;

            public string Email { get; set; } = string.Empty;

            public string Password { get; set; } = string.Empty;

            public string Phone { get; set; } = string.Empty;

            public bool IsActive { get; set; }
        }


        // =========================
        // USER VIEW MODEL
        // =========================

        public class UserViewModel
        {
            public int Id { get; set; }

            public string FirstName { get; set; } = string.Empty;

            public string LastName { get; set; } = string.Empty;

            public string Email { get; set; } = string.Empty;

            public string Phone { get; set; } = string.Empty;

            public DateTime RegistrationDate { get; set; }

            public bool IsActive { get; set; }
        }


        // =========================
        // API RESPONSE
        // =========================

        public class UserListResponse
        {
            public int Page { get; set; }

            public int PageSize { get; set; }

            public int TotalCount { get; set; }

            public bool HasNextPage { get; set; }

            public List<UserViewModel>? Data { get; set; }
        }
    }
}