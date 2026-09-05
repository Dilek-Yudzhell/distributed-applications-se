using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FoodOrderingSystem.Web.Pages
{
    public class OrderItemsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public OrderItemsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // =========================================================
        // LIST
        // =========================================================

        public List<OrderItemViewModel> OrderItems { get; set; } = new();

        // =========================================================
        // SEARCH
        // =========================================================

        [BindProperty(SupportsGet = true)]
        public int? SearchOrderId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SearchMenuItemId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "id";

        [BindProperty(SupportsGet = true)]
        public int Page { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 5;

        public bool HasNextPage { get; set; }

        // =========================================================
        // ADD / EDIT
        // =========================================================

        [BindProperty]
        public OrderItemInputModel Input { get; set; } = new();

        public int? EditId { get; set; }

        // =========================================================
        // DROPDOWNS
        // =========================================================

        public List<SelectListItem> OrderOptions { get; set; } = new();

        public List<SelectListItem> MenuItemOptions { get; set; } = new();

        // =========================================================
        // MESSAGES
        // =========================================================

        public string ErrorMessage { get; set; } = string.Empty;

        public string SuccessMessage { get; set; } = string.Empty;

        // =========================================================
        // GET
        // =========================================================

        public async Task<IActionResult> OnGetAsync(int? editId)
        {
            EditId = editId;

            await LoadDropdownsAsync();

            if (editId.HasValue)
            {
                var client = CreateClient();

                var response = await client.GetAsync(
                    $"api/OrderItems/{editId.Value}");

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Login");
                }

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage =
                        "Could not load the order item.";
                }
                else
                {
                    var json =
                        await response.Content.ReadAsStringAsync();

                    var item =
                        JsonSerializer.Deserialize<OrderItemViewModel>(
                            json,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

                    if (item != null)
                    {
                        Input = new OrderItemInputModel
                        {
                            OrderId = item.OrderId,
                            MenuItemId = item.MenuItemId,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice,
                            Notes = item.Notes
                        };
                    }
                }
            }

            await LoadListAsync();

            return Page();
        }

        // =========================================================
        // ADD
        // =========================================================

        public async Task<IActionResult> OnPostAsync()
        {
            EditId = null;

            ModelState.Clear();

            if (Input.OrderId <= 0)
            {
                ErrorMessage = "Please select an Order.";

                await LoadDropdownsAsync();
                await LoadListAsync();

                return Page();
            }

            if (Input.MenuItemId <= 0)
            {
                ErrorMessage = "Please select a Menu Item.";

                await LoadDropdownsAsync();
                await LoadListAsync();

                return Page();
            }

            if (Input.Quantity < 1 || Input.Quantity > 100)
            {
                ErrorMessage =
                    "Quantity must be between 1 and 100.";

                await LoadDropdownsAsync();
                await LoadListAsync();

                return Page();
            }

            if (Input.UnitPrice <= 0 ||
                Input.UnitPrice > 10000)
            {
                ErrorMessage =
                    "Unit price must be between 0.01 and 10000.";

                await LoadDropdownsAsync();
                await LoadListAsync();

                return Page();
            }

            try
            {
                var client = CreateClient();

                var data = new
                {
                    orderId = Input.OrderId,
                    menuItemId = Input.MenuItemId,
                    quantity = Input.Quantity,
                    unitPrice = Input.UnitPrice,
                    notes = Input.Notes
                };

                var json = JsonSerializer.Serialize(data);

                using var content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync(
                    "api/OrderItems",
                    content);

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Login");
                }

                if (!response.IsSuccessStatusCode)
                {
                    var error =
                        await response.Content.ReadAsStringAsync();

                    ErrorMessage =
                        $"API Error ({(int)response.StatusCode}): {error}";

                    await LoadDropdownsAsync();
                    await LoadListAsync();

                    return Page();
                }

                SuccessMessage =
                    "Order item added successfully.";

                Input = new OrderItemInputModel();

                await LoadDropdownsAsync();
                await LoadListAsync();

                return Page();
            }
            catch
            {
                ErrorMessage =
                    "Could not connect to the API.";

                await LoadDropdownsAsync();
                await LoadListAsync();

                return Page();
            }
        }

        // =========================================================
        // EDIT
        // =========================================================

        public async Task<IActionResult> OnPostEditAsync(int id)
        {
            EditId = id;

            ModelState.Clear();

            if (Input.OrderId <= 0)
            {
                ErrorMessage = "Please select an Order.";

                await LoadDropdownsAsync();
                await LoadListAsync();

                return Page();
            }

            if (Input.MenuItemId <= 0)
            {
                ErrorMessage = "Please select a Menu Item.";

                await LoadDropdownsAsync();
                await LoadListAsync();

                return Page();
            }

            if (Input.Quantity < 1 || Input.Quantity > 100)
            {
                ErrorMessage =
                    "Quantity must be between 1 and 100.";

                await LoadDropdownsAsync();
                await LoadListAsync();

                return Page();
            }

            if (Input.UnitPrice <= 0 ||
                Input.UnitPrice > 10000)
            {
                ErrorMessage =
                    "Unit price must be between 0.01 and 10000.";

                await LoadDropdownsAsync();
                await LoadListAsync();

                return Page();
            }

            try
            {
                var client = CreateClient();

                var data = new
                {
                    id = id,
                    orderId = Input.OrderId,
                    menuItemId = Input.MenuItemId,
                    quantity = Input.Quantity,
                    unitPrice = Input.UnitPrice,
                    notes = Input.Notes
                };

                var json = JsonSerializer.Serialize(data);

                using var content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PutAsync(
                    $"api/OrderItems/{id}",
                    content);

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Login");
                }

                if (!response.IsSuccessStatusCode)
                {
                    var error =
                        await response.Content.ReadAsStringAsync();

                    ErrorMessage =
                        $"API Error ({(int)response.StatusCode}): {error}";

                    await LoadDropdownsAsync();
                    await LoadListAsync();

                    return Page();
                }

                SuccessMessage =
                    "Order item updated successfully.";

                EditId = null;

                Input = new OrderItemInputModel();

                await LoadDropdownsAsync();
                await LoadListAsync();

                return Page();
            }
            catch
            {
                ErrorMessage =
                    "Could not connect to the API.";

                await LoadDropdownsAsync();
                await LoadListAsync();

                return Page();
            }
        }

        // =========================================================
        // DELETE
        // =========================================================

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var client = CreateClient();

                var response = await client.DeleteAsync(
                    $"api/OrderItems/{id}");

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Login");
                }

                if (!response.IsSuccessStatusCode)
                {
                    var error =
                        await response.Content.ReadAsStringAsync();

                    ErrorMessage =
                        $"Could not delete the order item. {error}";
                }
                else
                {
                    SuccessMessage =
                        "Order item deleted successfully.";
                }
            }
            catch
            {
                ErrorMessage =
                    "Could not connect to the API.";
            }

            await LoadDropdownsAsync();
            await LoadListAsync();

            return Page();
        }

        // =========================================================
        // LOAD LIST
        // =========================================================

        private async Task LoadListAsync()
        {
            try
            {
                if (Page < 1)
                {
                    Page = 1;
                }

                if (PageSize < 1)
                {
                    PageSize = 5;
                }

                if (PageSize > 100)
                {
                    PageSize = 100;
                }

                var client = CreateClient();

                var url =
                    $"api/OrderItems?page={Page}" +
                    $"&pageSize={PageSize}" +
                    $"&sortBy={Uri.EscapeDataString(SortBy ?? "id")}";

                if (SearchOrderId.HasValue)
                {
                    url +=
                        $"&orderId={SearchOrderId.Value}";
                }

                if (SearchMenuItemId.HasValue)
                {
                    url +=
                        $"&menuItemId={SearchMenuItemId.Value}";
                }

                var response = await client.GetAsync(url);

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized)
                {
                    return;
                }

                if (!response.IsSuccessStatusCode)
                {
                    return;
                }

                var json =
                    await response.Content.ReadAsStringAsync();

                var result =
                    JsonSerializer.Deserialize<List<OrderItemViewModel>>(
                        json,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                OrderItems = result ?? new List<OrderItemViewModel>();

                HasNextPage =
                    OrderItems.Count == PageSize;
            }
            catch
            {
                OrderItems = new List<OrderItemViewModel>();
            }
        }

        // =========================================================
        // LOAD DROPDOWNS
        // =========================================================

        private async Task LoadDropdownsAsync()
        {
            OrderOptions = new List<SelectListItem>();

            MenuItemOptions = new List<SelectListItem>();

            try
            {
                var client = CreateClient();

                // =====================================================
                // ORDERS
                // =====================================================

                var ordersResponse =
                    await client.GetAsync(
                        "api/Orders?page=1&pageSize=100&sortBy=date");

                if (ordersResponse.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized)
                {
                    return;
                }

                if (ordersResponse.IsSuccessStatusCode)
                {
                    var ordersJson =
                        await ordersResponse.Content.ReadAsStringAsync();

                    var orders =
                        JsonSerializer.Deserialize<List<OrderOption>>(
                            ordersJson,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

                    if (orders != null)
                    {
                        foreach (var order in orders)
                        {
                            OrderOptions.Add(
                                new SelectListItem
                                {
                                    Value =
                                        order.Id.ToString(),

                                    Text =
                                        $"Order #{order.Id} - {order.TotalPrice:F2}"
                                });
                        }
                    }
                }

                // =====================================================
                // MENU ITEMS
                // =====================================================

                var menuResponse =
                    await client.GetAsync(
                        "api/MenuItems?page=1&pageSize=100&sortBy=name");

                if (menuResponse.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized)
                {
                    return;
                }

                if (menuResponse.IsSuccessStatusCode)
                {
                    var menuJson =
                        await menuResponse.Content.ReadAsStringAsync();

                    var menuItems =
                        JsonSerializer.Deserialize<List<MenuItemOption>>(
                            menuJson,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

                    if (menuItems != null)
                    {
                        foreach (var menuItem in menuItems)
                        {
                            MenuItemOptions.Add(
                                new SelectListItem
                                {
                                    Value =
                                        menuItem.Id.ToString(),

                                    Text =
                                        $"#{menuItem.Id} - {menuItem.Name} - {menuItem.Price:F2}"
                                });
                        }
                    }
                }
            }
            catch
            {
                // Keep dropdowns empty if API cannot be reached.
            }
        }

        // =========================================================
        // HTTP CLIENT
        // =========================================================

        private HttpClient CreateClient()
        {
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

        // =========================================================
        // VIEW MODELS
        // =========================================================

        public class OrderItemViewModel
        {
            public int Id { get; set; }

            public int OrderId { get; set; }

            public int MenuItemId { get; set; }

            public int Quantity { get; set; }

            public decimal UnitPrice { get; set; }

            public string Notes { get; set; } = string.Empty;
        }

        public class OrderItemInputModel
        {
            public int OrderId { get; set; }

            public int MenuItemId { get; set; }

            public int Quantity { get; set; }

            public decimal UnitPrice { get; set; }

            public string Notes { get; set; } = string.Empty;
        }

        public class OrderOption
        {
            public int Id { get; set; }

            public decimal TotalPrice { get; set; }
        }

        public class MenuItemOption
        {
            public int Id { get; set; }

            public string Name { get; set; } = string.Empty;

            public decimal Price { get; set; }
        }
    }
}