using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FoodOrderingSystem.Web.Pages
{
    public class RestaurantsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public RestaurantsModel(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<RestaurantViewModel> Restaurants { get; set; } = new();

        public string ErrorMessage { get; set; } = string.Empty;

        public string SuccessMessage { get; set; } = string.Empty;

        // ================================
        // GET RESTAURANTS
        // ================================

        public async Task<IActionResult> OnGetAsync()
        {
            return await LoadRestaurantsAsync();
        }

        // ================================
        // ADD RESTAURANT
        // ================================

        public async Task<IActionResult> OnPostAddAsync(
            string name,
            string address,
            string phone,
            string cuisineType,
            decimal rating,
            bool isActive)
        {
            var token =
                HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToPage("/Login");
            }

            if (string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(address) ||
                string.IsNullOrWhiteSpace(phone) ||
                string.IsNullOrWhiteSpace(cuisineType))
            {
                ErrorMessage =
                    "Please fill in all required fields.";

                await LoadRestaurantsAsync();

                return Page();
            }

            if (rating < 0 || rating > 5)
            {
                ErrorMessage =
                    "Rating must be between 0 and 5.";

                await LoadRestaurantsAsync();

                return Page();
            }

            try
            {
                var client =
                    CreateAuthenticatedClient(token);

                var restaurant = new
                {
                    name = name.Trim(),
                    address = address.Trim(),
                    phone = phone.Trim(),
                    cuisineType = cuisineType.Trim(),
                    rating = rating,
                    createdDate = DateTime.UtcNow,
                    isActive = isActive
                };

                var json =
                    JsonSerializer.Serialize(restaurant);

                using var content =
                    new StringContent(
                        json,
                        Encoding.UTF8,
                        "application/json");

                var response =
                    await client.PostAsync(
                        "api/Restaurants",
                        content);

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Remove("JwtToken");

                    return RedirectToPage("/Login");
                }

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage =
                        await GetApiErrorMessage(response);

                    await LoadRestaurantsAsync();

                    return Page();
                }

                SuccessMessage =
                    "Restaurant added successfully.";

                await LoadRestaurantsAsync();

                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage =
                    "Could not connect to the API: " +
                    ex.Message;

                await LoadRestaurantsAsync();

                return Page();
            }
        }

        // ================================
        // EDIT RESTAURANT
        // ================================

        public async Task<IActionResult> OnPostEditAsync(
            int id,
            string name,
            string address,
            string phone,
            string cuisineType,
            decimal rating,
            bool isActive)
        {
            var token =
                HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToPage("/Login");
            }

            if (id <= 0 ||
                string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(address) ||
                string.IsNullOrWhiteSpace(phone) ||
                string.IsNullOrWhiteSpace(cuisineType))
            {
                ErrorMessage =
                    "Please enter valid restaurant information.";

                await LoadRestaurantsAsync();

                return Page();
            }

            if (rating < 0 || rating > 5)
            {
                ErrorMessage =
                    "Rating must be between 0 and 5.";

                await LoadRestaurantsAsync();

                return Page();
            }

            try
            {
                var client =
                    CreateAuthenticatedClient(token);

                var existingResponse =
                    await client.GetAsync(
                        $"api/Restaurants/{id}");

                if (existingResponse.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Remove("JwtToken");

                    return RedirectToPage("/Login");
                }

                if (!existingResponse.IsSuccessStatusCode)
                {
                    ErrorMessage =
                        "Restaurant was not found.";

                    await LoadRestaurantsAsync();

                    return Page();
                }

                var existingJson =
                    await existingResponse.Content
                        .ReadAsStringAsync();

                var existingRestaurant =
                    JsonSerializer.Deserialize<
                        RestaurantViewModel>(
                            existingJson,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

                var restaurant = new
                {
                    id = id,
                    name = name.Trim(),
                    address = address.Trim(),
                    phone = phone.Trim(),
                    cuisineType = cuisineType.Trim(),
                    rating = rating,
                    createdDate =
                        existingRestaurant?.CreatedDate
                        ?? DateTime.UtcNow,
                    isActive = isActive
                };

                var json =
                    JsonSerializer.Serialize(restaurant);

                using var content =
                    new StringContent(
                        json,
                        Encoding.UTF8,
                        "application/json");

                var response =
                    await client.PutAsync(
                        $"api/Restaurants/{id}",
                        content);

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Remove("JwtToken");

                    return RedirectToPage("/Login");
                }

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage =
                        await GetApiErrorMessage(response);

                    await LoadRestaurantsAsync();

                    return Page();
                }

                SuccessMessage =
                    "Restaurant updated successfully.";

                await LoadRestaurantsAsync();

                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage =
                    "Could not connect to the API: " +
                    ex.Message;

                await LoadRestaurantsAsync();

                return Page();
            }
        }

        // ================================
        // DELETE RESTAURANT
        // ================================

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
                    CreateAuthenticatedClient(token);

                var response =
                    await client.DeleteAsync(
                        $"api/Restaurants/{id}");

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Remove("JwtToken");

                    return RedirectToPage("/Login");
                }

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage =
                        await GetApiErrorMessage(response);

                    await LoadRestaurantsAsync();

                    return Page();
                }

                SuccessMessage =
                    "Restaurant deleted successfully.";

                await LoadRestaurantsAsync();

                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage =
                    "Could not connect to the API: " +
                    ex.Message;

                await LoadRestaurantsAsync();

                return Page();
            }
        }

        // ================================
        // LOAD RESTAURANTS
        // ================================

        private async Task<IActionResult> LoadRestaurantsAsync()
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
                    CreateAuthenticatedClient(token);

                var response =
                    await client.GetAsync(
                        "api/Restaurants");

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Remove("JwtToken");

                    return RedirectToPage("/Login");
                }

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage =
                        await GetApiErrorMessage(response);

                    return Page();
                }

                var json =
                    await response.Content
                        .ReadAsStringAsync();

                Restaurants =
                    JsonSerializer.Deserialize<
                        List<RestaurantViewModel>>(
                            json,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            })
                    ?? new List<RestaurantViewModel>();

                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage =
                    "Could not connect to the API: " +
                    ex.Message;

                return Page();
            }
        }

        // ================================
        // AUTHENTICATED HTTP CLIENT
        // ================================

        private HttpClient CreateAuthenticatedClient(
            string token)
        {
            // ВАЖНО:
            // Тук трябва да бъде "API",
            // а не "FoodOrderingAPI".

            var client =
                _httpClientFactory.CreateClient("API");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            return client;
        }

        // ================================
        // API ERROR MESSAGE
        // ================================

        private async Task<string> GetApiErrorMessage(
            HttpResponseMessage response)
        {
            var body =
                await response.Content
                    .ReadAsStringAsync();

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
                $"API Error ({(int)response.StatusCode}): " +
                body;
        }

        // ================================
        // VIEW MODEL
        // ================================

        public class RestaurantViewModel
        {
            public int Id { get; set; }

            public string Name { get; set; } =
                string.Empty;

            public string Address { get; set; } =
                string.Empty;

            public string Phone { get; set; } =
                string.Empty;

            public string CuisineType { get; set; } =
                string.Empty;

            public decimal Rating { get; set; }

            public DateTime CreatedDate { get; set; }

            public bool IsActive { get; set; }
        }
    }
}