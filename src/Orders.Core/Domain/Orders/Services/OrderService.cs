using Microsoft.Extensions.Logging;
using Orders.Core.Domain.Orders.Entities;
using Orders.Core.Domain.Orders.Models;
using Orders.Core.Interfaces;

namespace Orders.Core.Domain.Orders.Services
{
    public interface IOrderService
    {
        Task<OrderCreateResponse> CreateOrder(OrderCreateModel model);
    }

    public class OrderService : IOrderService
    {
        private readonly ILogger<OrderService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;

        public OrderService(ILogger<OrderService> logger, IUnitOfWork unityOfWork, IRepository<Order> orderRepository, IRepository<OrderItem> orderItemRepository)
        {
            _logger = logger;
            _unitOfWork = unityOfWork;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
        }

        public async Task<OrderCreateResponse> CreateOrder(OrderCreateModel model)
        {
            var exists = await _orderRepository.GetFirstOrDefaultAsync(x => x.Code == model.Code) is not null;

            if (exists)
                throw new Exception($"Order with code {model.Code} already exists.");

            var order = new Order
            {
                Code = model.Code,
                Customer = model.Customer,
            };

            var items = new List<OrderItem>();

            foreach (var item in model.Items)
            {
                items.Add(new OrderItem
                {
                    OrderCode = model.Code,
                    Product = item.Product,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalValue = item.Quantity * item.UnitPrice
                });
            }

            order.TotalValue = items.Sum(x => x.TotalValue);

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                await _orderRepository.InsertAsync(order);

                foreach (var item in items)
                {
                    await _orderItemRepository.InsertAsync(item);
                }

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                await _unitOfWork.RollbackAsync();
                throw;
            }

            return new OrderCreateResponse
            {
                Id = order.Id,
                Code = order.Code,
                Customer = order.Customer,
                TotalValue = order.TotalValue,
                Items = items.Select(x => new OrderItemCreateResponse
                {
                    Id = x.Id,
                    OrderCode = x.OrderCode,
                    Product = x.Product,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                    TotalValue = x.TotalValue
                }).ToList()
            };
        }
    }
}