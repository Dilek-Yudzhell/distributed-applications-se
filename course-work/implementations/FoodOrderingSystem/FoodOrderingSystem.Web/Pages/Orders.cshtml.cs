using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FoodOrderingSystem.Web.Pages
{
    public class OrdersModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public OrdersModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // =====================================================
        // ORDERS
        // =====================================================

        public List<OrderViewModel> Orders { get; set; } = new();

        public string ErrorMessage { get; set; } = string.Empty;

        // =====================================================
        // SEARCH
        // =====================================================

        public int CurrentPage { get; set; } = 1;

        public int PageSize { get; set; } = 5;

        public string CurrentStatus { get; set; } = string.Empty;

        public int? CurrentUserId { get; set; }

        public string CurrentSortBy { get; set; } = "date";

        // =====================================================
        // ADD ORDER
        // =====================================================

        [BindProperty]
        public int UserId { get; set; }

        [BindProperty]
        public decimal TotalPrice { get; set; }

        [BindProperty]
        public string Status { get; set; } = "Pending";

        [BindProperty]
        public string DeliveryAddress { get; set; } = string.Empty;

        [BindProperty]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        // =====================================================
        // EDIT ORDER
        // =====================================================

        [BindProperty]
        public int EditId { get; set; }

        [BindProperty]
        public int EditUserId { get; set; }

        [BindProperty]
        public decimal EditTotalPrice { get; set; }

        [BindProperty]
        public string EditStatus { get; set; } = "Pending";

        [BindProperty]
        public string EditDeliveryAddress { get; set; } = string.Empty;

        [BindProperty]
        public DateTime EditOrderDate { get; set; }

        // =====================================================
        // GET
        // =====================================================

        public async Task<IActionResult> OnGetAsync(
            string? status,
            int? userId,
            int page = 1,
            int pageSize = 5,
            string sortBy = "date")
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

            CurrentStatus = status ?? string.Empty;

            CurrentUserId = userId;

            CurrentSortBy =
                string.IsNullOrWhiteSpace(sortBy)
                    ? "date"
                    : sortBy;

            await LoadOrdersAsync(
                token,
                CurrentPage,
                PageSize,
                CurrentStatus,
                CurrentUserId,
                CurrentSortBy);

            return Page();
        }

        // =====================================================
        // ADD ORDER
        // =====================================================

        public async Task<IActionResult> OnPostAsync()
        {
            var token =
                HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToPage("/Login");
            }

            // -------------------------------------------------
            // Ignore Edit fields during Add
            // -------------------------------------------------

            foreach (var key in ModelState.Keys
                .Where(x => x.StartsWith("Edit"))
                .ToList())
            {
                ModelState.Remove(key);
            }

            // -------------------------------------------------
            // BASIC VALIDATION
            // -------------------------------------------------

            if (UserId < 1)
            {
                ErrorMessage =
                    "User ID must be greater than 0.";

                await LoadOrdersAsync(token);

                return Page();
            }

            if (TotalPrice <= 0)
            {
                ErrorMessage =
                    "Total Price must be greater than 0.";

                await LoadOrdersAsync(token);

                return Page();
            }

            if (string.IsNullOrWhiteSpace(DeliveryAddress))
            {
                ErrorMessage =
                    "Delivery Address is required.";

                await LoadOrdersAsync(token);

                return Page();
            }

            if (DeliveryAddress.Length > 250)
            {
                ErrorMessage =
                    "Delivery Address cannot be longer than 250 characters.";

                await LoadOrdersAsync(token);

                return Page();
            }

            if (string.IsNullOrWhiteSpace(Status))
            {
                Status = "Pending";
            }

            if (OrderDate == default)
            {
                OrderDate = DateTime.Now;
            }

            // -------------------------------------------------
            // CREATE OBJECT FOR API
            // -------------------------------------------------

            var order = new OrderInputModel
            {
                UserId = UserId,
                TotalPrice = TotalPrice,
                Status = Status,
                DeliveryAddress = DeliveryAddress,
                OrderDate = OrderDate
            };

            // -------------------------------------------------
            // SEND TO API
            // -------------------------------------------------

            try
            {
                var client =
                    CreateClient(token);

                var response =
                    await client.PostAsJsonAsync(
                        "api/Orders",
                        order);

                var responseBody =
                    await response.Content.ReadAsStringAsync();

                // SUCCESS
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage();
                }

                // UNAUTHORIZED
                if (response.StatusCode ==
                    HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Clear();

                    return RedirectToPage("/Login");
                }

                ErrorMessage =
                    $"API Error ({(int)response.StatusCode}): {responseBody}";

                await LoadOrdersAsync(token);
            }
            catch (Exception ex)
            {
                ErrorMessage =
                    $"Connection Error: {ex.Message}";

                await LoadOrdersAsync(token);
            }

            return Page();
        }

        // =====================================================
        // EDIT ORDER
        // =====================================================

        public async Task<IActionResult> OnPostEditAsync()
        {
            var token =
                HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToPage("/Login");
            }

            if (EditUserId < 1)
            {
                ErrorMessage =
                    "User ID must be greater than 0.";

                await LoadOrdersAsync(token);

                return Page();
            }

            if (EditTotalPrice <= 0)
            {
                ErrorMessage =
                    "Total Price must be greater than 0.";

                await LoadOrdersAsync(token);

                return Page();
            }

            if (string.IsNullOrWhiteSpace(EditDeliveryAddress))
            {
                ErrorMessage =
                    "Delivery Address is required.";

                await LoadOrdersAsync(token);

                return Page();
            }

            if (EditDeliveryAddress.Length > 250)
            {
                ErrorMessage =
                    "Delivery Address cannot be longer than 250 characters.";

                await LoadOrdersAsync(token);

                return Page();
            }

            if (string.IsNullOrWhiteSpace(EditStatus))
            {
                EditStatus = "Pending";
            }

            if (EditOrderDate == default)
            {
                EditOrderDate = DateTime.Now;
            }

            var order = new OrderInputModel
            {
                UserId = EditUserId,
                TotalPrice = EditTotalPrice,
                Status = EditStatus,
                DeliveryAddress = EditDeliveryAddress,
                OrderDate = EditOrderDate
            };

            try
            {
                var client =
                    CreateClient(token);

                var response =
                    await client.PutAsJsonAsync(
                        $"api/Orders/{EditId}",
                        order);

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

                await LoadOrdersAsync(token);
            }
            catch (Exception ex)
            {
                ErrorMessage =
                    $"Connection Error: {ex.Message}";

                await LoadOrdersAsync(token);
            }

            return Page();
        }

        // =====================================================
        // DELETE ORDER
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
                        $"api/Orders/{id}");

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

                await LoadOrdersAsync(token);
            }
            catch (Exception ex)
            {
                ErrorMessage =
                    $"Connection Error: {ex.Message}";

                await LoadOrdersAsync(token);
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
        // LOAD ORDERS
        // =====================================================

        private async Task LoadOrdersAsync(
            string token,
            int page = 1,
            int pageSize = 5,
            string status = "",
            int? userId = null,
            string sortBy = "date")
        {
            try
            {
                var client =
                    CreateClient(token);

                var url =
                    $"api/Orders" +
                    $"?page={page}" +
                    $"&pageSize={pageSize}" +
                    $"&sortBy={Uri.EscapeDataString(sortBy)}";

                if (!string.IsNullOrWhiteSpace(status))
                {
                    url +=
                        $"&status={Uri.EscapeDataString(status)}";
                }

                if (userId.HasValue)
                {
                    url +=
                        $"&userId={userId.Value}";
                }

                var response =
                    await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Orders =
                        new List<OrderViewModel>();

                    return;
                }

                var json =
                    await response.Content.ReadAsStringAsync();

                Orders =
                    JsonSerializer.Deserialize<
                        List<OrderViewModel>>(
                        json,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        })
                    ?? new List<OrderViewModel>();
            }
            catch
            {
                Orders =
                    new List<OrderViewModel>();
            }
        }

        // =====================================================
        // ORDER VIEW MODEL
        // =====================================================

        public class OrderViewModel
        {
            public int Id { get; set; }

            public int UserId { get; set; }

            public decimal TotalPrice { get; set; }

            public string Status { get; set; } =
                string.Empty;

            public string DeliveryAddress { get; set; } =
                string.Empty;

            public DateTime OrderDate { get; set; }
        }

        // =====================================================
        // API INPUT MODEL
        // =====================================================

        public class OrderInputModel
        {
            public int UserId { get; set; }

            public decimal TotalPrice { get; set; }

            public string Status { get; set; } =
                "Pending";

            public string DeliveryAddress { get; set; } =
                string.Empty;

            public DateTime OrderDate { get; set; }
        }
    }
}