using Microsoft.Extensions.Logging;
using Orders.Core.Domain.Common;
using Orders.Core.Domain.Orders.Entities;
using Orders.Core.Domain.Orders.Models;
using Orders.Core.Interfaces;

namespace Orders.Core.Domain.Orders.Services
{
    public interface IOrderService
    {
        Task<OrderResponse> CreateOrder(OrderCreateModel model);

        Task<PagedResult<OrderResponse>> ListOrders(OrderFilterModel filters);

        Task<OrderResponse> GetOrderDetails(string code);
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

        public async Task<OrderResponse> CreateOrder(OrderCreateModel model)
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

            return new OrderResponse
            {
                Id = order.Id,
                Code = order.Code,
                Customer = order.Customer,
                TotalValue = order.TotalValue,
                Items = items.ConvertAll(x => new OrderItemResponse
                {
                    Id = x.Id,
                    OrderCode = x.OrderCode,
                    Product = x.Product,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                    TotalValue = x.TotalValue
                })
            };
        }

        public async Task<PagedResult<OrderResponse>> ListOrders(OrderFilterModel filters)
        {
            var query = _orderRepository.AsQueryable();

            // Filtros do pedido
            if (!string.IsNullOrEmpty(filters.Customer))
                query = query.Where(x => x.Customer == filters.Customer);

            // Para otimização, apenas os pedidos são retornados na lista.
            // Os itens devem ser retornados ao consultar os Detalhes do Pedido.
            var orders = await _orderRepository.GetPagedAsync(filters.Page, filters.PageSize, query);

            // Mapeia o pedido para o modelo de resposta
            var result = new PagedResult<OrderResponse>
            {
                Page = orders.Page,
                PageSize = orders.PageSize,
                TotalCount = orders.TotalCount,
                Data = orders.Data.ConvertAll(x => new OrderResponse
                {
                    Id = x.Id,
                    Code = x.Code,
                    Customer = x.Customer,
                    TotalValue = x.TotalValue
                })
            };

            return result;
        }

        public async Task<OrderResponse> GetOrderDetails(string code)
        {
            var order = await _orderRepository.GetFirstOrDefaultAsync(x => x.Code == code);

            if (order is null)
                throw new Exception($"Order with code {code} not found.");

            var itemsQuery = _orderItemRepository.AsQueryable()
                .Where(x => x.OrderCode == code);

            var items = await _orderItemRepository.GetPagedAsync(1, int.MaxValue, itemsQuery);

            return new OrderResponse
            {
                Id = order.Id,
                Code = order.Code,
                Customer = order.Customer,
                TotalValue = order.TotalValue,
                Items = items.Data.ConvertAll(x => new OrderItemResponse
                {
                    Id = x.Id,
                    OrderCode = x.OrderCode,
                    Product = x.Product,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                    TotalValue = x.TotalValue
                })
            };
        }
    }
}