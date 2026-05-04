using Application.DTOs.Order;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebUI.Controllers
{
    [ApiController]
    [Route("api/order")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService orderService, ILogger<OrderController> logger)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Place a new order
        /// POST: api/order
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<OrderDetailResponse>> PlaceOrder([FromBody] PlaceOrderRequest request)
        {
            try
            {
                // Get user ID from token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                var order = await _orderService.PlaceOrderAsync(request, userId);
                _logger.LogInformation("Order {OrderCode} placed successfully by user {UserId}", order.OrderCode, userId);

                return CreatedAtAction(nameof(GetOrderDetails), new { id = order.OrderCode }, order);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Order placement failed: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error placing order");
                return StatusCode(500, new { message = "An error occurred while placing the order" });
            }
        }

        /// <summary>
        /// Get order details by order code
        /// GET: api/order/{orderCode}
        /// </summary>
        [HttpGet("{orderCode}")]
        [Authorize]
        public async Task<ActionResult<OrderDetailResponse>> GetOrderDetails(string orderCode)
        {
            try
            {
                var order = await _orderService.GetOrderByCodeAsync(orderCode);
                
                if (order == null)
                {
                    return NotFound(new { message = $"Order with code '{orderCode}' not found" });
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order details");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get all orders for current user
        /// GET: api/order/my-orders
        /// </summary>
        [HttpGet("my-orders")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<OrderSummaryResponse>>> GetMyOrders()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                var orders = await _orderService.GetUserOrdersAsync(userId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user orders");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }
    }
}
