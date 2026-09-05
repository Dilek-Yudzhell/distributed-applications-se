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

                using var content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync(
                    "api/MenuItems",
                    content);

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage =
                        await GetApiErrorMessage(response);

                    await LoadMenuItems();
                    return Page();
                }

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                ErrorMessage =
                    "Could not connect to the API: " +
                    ex.Message;

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
                    ErrorMessage =
                        await GetApiErrorMessage(response);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage =
                    "Could not connect to the API: " +
                    ex.Message;
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
                    ErrorMessage =
                        await GetApiErrorMessage(response);

                    MenuItems = new List<MenuItemViewModel>();
                    return;
                }

                var json =
                    await response.Content.ReadAsStringAsync();

                var result =
                    JsonSerializer.Deserialize<
                        List<MenuItemViewModel>>(
                            json,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

                MenuItems =
                    result ??
                    new List<MenuItemViewModel>();
            }
            catch (Exception ex)
            {
                ErrorMessage =
                    "Could not connect to the API: " +
                    ex.Message;

                MenuItems =
                    new List<MenuItemViewModel>();
            }
        }

        private HttpClient CreateClient()
        {
            // ВАЖНО: трябва да бъде "API"
            var client =
                _httpClientFactory.CreateClient("API");

            var token =
                HttpContext.Session.GetString("JwtToken");

            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer",
                        token);
            }

            return client;
        }

        private async Task<string> GetApiErrorMessage(
            HttpResponseMessage response)
        {
            var body =
                await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(body))
            {
                return
                    $"API Error ({(int)response.StatusCode}): " +
                    response.ReasonPhrase;
            }

            try
            {
                using var document =
                    JsonDocument.Parse(body);

                var root =
                    document.RootElement;

                if (root.TryGetProperty(
                    "message",
                    out var message))
                {
                    return
                        $"API Error ({(int)response.StatusCode}): " +
                        message.GetString();
                }

                if (root.TryGetProperty(
                    "detail",
                    out var detail))
                {
                    return
                        $"API Error ({(int)response.StatusCode}): " +
                        detail.GetString();
                }

                if (root.TryGetProperty(
                    "title",
                    out var title))
                {
                    return
                        $"API Error ({(int)response.StatusCode}): " +
                        title.GetString();
                }
            }
            catch
            {
            }

            return
                $"API Error ({(int)response.StatusCode}): {body}";
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