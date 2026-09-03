using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FoodOrderingSystem.Web.Pages
{
    public class MenuItemsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public MenuItemsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<MenuItemViewModel> MenuItems { get; set; } = new();

        public string ErrorMessage { get; set; } = string.Empty;

        [BindProperty]
        public MenuItemViewModel NewItem { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadMenuItems();
        }

        public async Task<IActionResult> OnPostAddAsync()
        {
            try
            {
                var client = CreateClient();

                var json = JsonSerializer.Serialize(NewItem);

                var content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync(
                    "api/MenuItems",
                    content);

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = "Could not add menu item.";
                    await LoadMenuItems();
                    return Page();
                }

                return RedirectToPage();
            }
            catch
            {
                ErrorMessage = "Could not connect to the API.";
                await LoadMenuItems();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var client = CreateClient();

                var response = await client.DeleteAsync(
                    $"api/MenuItems/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = "Could not delete menu item.";
                }
            }
            catch
            {
                ErrorMessage = "Could not connect to the API.";
            }

            await LoadMenuItems();
            return Page();
        }

        private async Task LoadMenuItems()
        {
            try
            {
                var client = CreateClient();

                var response = await client.GetAsync(
                    "api/MenuItems?page=1&pageSize=100&sortBy=name");

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = "Could not load menu items.";
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();

                MenuItems = JsonSerializer.Deserialize<List<MenuItemViewModel>>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<MenuItemViewModel>();
            }
            catch
            {
                ErrorMessage = "Could not connect to the API.";
            }
        }

        private HttpClient CreateClient()
        {
            var client = _httpClientFactory.CreateClient(
                "FoodOrderingAPI");

            var token = HttpContext.Session.GetString(
                "JwtToken");

            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer",
                        token);
            }

            return client;
        }

        public class MenuItemViewModel
        {
            public int Id { get; set; }

            public int RestaurantId { get; set; }

            public string Name { get; set; } = string.Empty;

            public string Description { get; set; } = string.Empty;

            public decimal Price { get; set; }

            public string Category { get; set; } = string.Empty;

            public bool IsAvailable { get; set; }
        }
    }
}