using Microsoft.AspNetCore.Mvc;
using Orders.Core.Domain.Orders.Models;
using Orders.Core.Domain.Orders.Services;

namespace Orders.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly ILogger<OrdersController> _logger;
        private readonly IOrderService _orderService;

        public OrdersController(ILogger<OrdersController> logger, IOrderService orderService)
        {
            _logger = logger;
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(OrderCreateModel model)
        {
            var result = await _orderService.CreateOrder(model);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> Get(OrderFilterModel filters)
        {
            var result = await _orderService.ListOrders(filters);
            return Ok(result);
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> GetDetails(string code)
        {
            var result = await _orderService.GetOrderDetails(code);
            return Ok(result);
        }
    }
}