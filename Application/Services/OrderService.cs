using Application.DTOs.Email;
using Application.DTOs.Order;
using Application.DTOs.Common;
using Application.Exceptions;
using Application.Interfaces;
using Application.Repositories;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IItemRepository _itemRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IMapper _mapper;
        private readonly IStorageService _storageService;
        private readonly IEmailService _emailService;
        private readonly ISystemSettingService _settingService;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepository,
            IItemRepository itemRepository,
            ICustomerRepository customerRepository,
            IMapper mapper,
            IStorageService storageService,
            IEmailService emailService,
            ISystemSettingService settingService,
            ILogger<OrderService> logger)
        {
            _orderRepository    = orderRepository    ?? throw new ArgumentNullException(nameof(orderRepository));
            _itemRepository     = itemRepository     ?? throw new ArgumentNullException(nameof(itemRepository));
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _mapper             = mapper             ?? throw new ArgumentNullException(nameof(mapper));
            _storageService     = storageService     ?? throw new ArgumentNullException(nameof(storageService));
            _emailService       = emailService       ?? throw new ArgumentNullException(nameof(emailService));
            _settingService     = settingService     ?? throw new ArgumentNullException(nameof(settingService));
            _logger             = logger             ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OrderDetailResponse> PlaceOrderAsync(PlaceOrderRequest request, Guid userId)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Items == null || !request.Items.Any())
                throw new InvalidOperationException("Order must contain at least one item");

            // Create customer entity
            var customer = new Customer(
                request.FirstName,
                request.LastName,
                request.Email,
                request.PhoneNumber,
                request.DeliveryAddress
            );

            // Add customer to database
            var createdCustomer = await _customerRepository.CreateAsync(customer);

            // Generate unique order code (5 characters)
            var orderCode = GenerateOrderCode();

            // Create order entity with fulfillment scope and optional out-house type
            var order = new Order(
                orderCode, 
                createdCustomer.Id, 
                request.FulfillmentScope,
                request.OutHouseFulfillmentType,
                request.DeliveryAddress,
                request.Notes
            );

            // Validate items and add to order
            var orderItemsList = new List<OrderItems>();

            foreach (var requestItem in request.Items)
            {
                // Get item from database to verify it exists and get price
                var item = await _itemRepository.GetByIdAsync(requestItem.ItemId);

                if (item == null)
                {
                    throw new InvalidOperationException($"Item with ID {requestItem.ItemId} not found");
                }

                if (!item.IsActive)
                {
                    throw new InvalidOperationException($"Item '{item.Name}' is not available");
                }

                // Create order item with just the ItemId (not the full entity to avoid tracking issues)
                var orderItem = new OrderItems(item.Id, requestItem.Quantity, item.Price);
                order.AddItem(orderItem);
            }

            // Save to database
            var createdOrder = await _orderRepository.CreateAsync(order);

            // Reload with full details
            var orderWithDetails = await _orderRepository.GetByIdWithDetailsAsync(createdOrder.Id);

            // Map to response DTO with StorageService for dynamic SAS URLs
            var response = _mapper.Map<OrderDetailResponse>(orderWithDetails!, opts => opts.Items["StorageService"] = _storageService);

            // Send confirmation email to the customer (user-placed orders only)
            // Admin-created orders use CreateOrderManuallyAsync which does NOT call this
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var settings = await _settingService.GetAppSettingsAsync();
                if (settings.EmailConfirmationEnabled)
                {
                    try
                    {
                        var emailDto = new OrderConfirmationEmail
                        {
                            ToEmail             = request.Email,
                            CustomerName        = $"{request.FirstName} {request.LastName}".Trim(),
                            OrderCode           = response.OrderCode,
                            TotalPrice          = response.TotalPrice,
                            FulfillmentDescription = BuildFulfillmentDescription(request.FulfillmentScope, request.OutHouseFulfillmentType),
                            DeliveryAddress     = request.DeliveryAddress,
                            Notes               = request.Notes,
                            CreatedDate         = response.CreatedDate,
                            Items               = response.Items.Select(i => new OrderItemEmailLine(
                                                    i.ItemName, i.Quantity, i.UnitPrice, i.TotalPrice)).ToList()
                        };

                        await _emailService.SendOrderConfirmationAsync(emailDto);
                    }
                    catch (Exception ex)
                    {
                        // Email failure must never roll back the order
                        _logger.LogError(ex, "Failed to send order confirmation email for order {OrderCode}", response.OrderCode);
                    }
                }
            }

            return response;
        }

        public async Task<OrderDetailResponse?> GetOrderDetailsAsync(int orderId)
        {
            var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);
            return order != null ? _mapper.Map<OrderDetailResponse>(order, opts => opts.Items["StorageService"] = _storageService) : null;
        }

        public async Task<IEnumerable<OrderSummaryResponse>> GetUserOrdersAsync(Guid userId)
        {
            // Now gets orders by CustomerId
            var orders = await _orderRepository.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<OrderSummaryResponse>>(orders, opts => opts.Items["StorageService"] = _storageService);
        }

        public async Task<OrderDetailResponse?> GetOrderByCodeAsync(string orderCode)
        {
            var order = await _orderRepository.GetByOrderCodeAsync(orderCode);
            return order != null ? _mapper.Map<OrderDetailResponse>(order, opts => opts.Items["StorageService"] = _storageService) : null;
        }

        public async Task<OrderDetailResponse> OnAddItem(int orderId, int itemId, int version)
        {
            if (orderId <= 0)
                throw new ArgumentException("Order ID must be greater than 0", nameof(orderId));

            if (itemId <= 0)
                throw new ArgumentException("Item ID must be greater than 0", nameof(itemId));

            var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);
            if (order == null)
            {
                throw new InvalidOperationException($"Order with ID {orderId} not found");
            }

            var item = await _itemRepository.GetByIdAsync(itemId);
            if (item == null)
            {
                throw new InvalidOperationException($"Item with ID {itemId} not found");
            }

            if (!item.IsActive)
            {
                throw new InvalidOperationException($"Item '{item.Name}' is not available");
            }

            var orderItem = order.OrderItems.FirstOrDefault(oi => oi.ItemId == itemId);
            if (orderItem == null)
            {
                throw new InvalidOperationException($"Item '{item.Name}' is not part of order {order.OrderCode}");
            }

            var newQuantity = orderItem.Quantity + 1;

            order.UpdateItem(itemId, newQuantity);

            order.IncrementVersion();

            try
            {
                await _orderRepository.UpdateAsync(order, version);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConcurrencyConflictException("This order was updated by another admin. Your changes were not saved. Please reload and try again.");
            }

            var updatedOrder = await _orderRepository.GetByIdWithDetailsAsync(orderId);
            return _mapper.Map<OrderDetailResponse>(updatedOrder!, opts => opts.Items["StorageService"] = _storageService);
        }

        public async Task<OrderDetailResponse> OnRemoveItem(int orderId, int itemId, int version)
        {
            if (orderId <= 0)
                throw new ArgumentException("Order ID must be greater than 0", nameof(orderId));

            if (itemId <= 0)
                throw new ArgumentException("Item ID must be greater than 0", nameof(itemId));

            var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);
            if (order == null)
            {
                throw new InvalidOperationException($"Order with ID {orderId} not found");
            }

            var item = await _itemRepository.GetByIdAsync(itemId);
            if (item == null)
            {
                throw new InvalidOperationException($"Item with ID {itemId} not found");
            }

            var orderItem = order.OrderItems.FirstOrDefault(oi => oi.ItemId == itemId);
            if (orderItem == null)
            {
                throw new InvalidOperationException($"Item '{item.Name}' is not part of order {order.OrderCode}");
            }

            var newQuantity = orderItem.Quantity - 1;
            if (newQuantity < 0)
            {
                throw new InvalidOperationException($"Cannot remove more '{item.Name}' from order {order.OrderCode} than exists");
            }

            if (newQuantity == 0)
            {
                order.RemoveItem(itemId);
            }
            else
            {
                order.UpdateItem(itemId, newQuantity);
            }

            order.IncrementVersion();

            try
            {
                await _orderRepository.UpdateAsync(order, version);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConcurrencyConflictException("This order was updated by another admin. Your changes were not saved. Please reload and try again.");
            }

            var updatedOrder = await _orderRepository.GetByIdWithDetailsAsync(orderId);
            return _mapper.Map<OrderDetailResponse>(updatedOrder!, opts => opts.Items["StorageService"] = _storageService);
        }

        public async Task<OrderDetailResponse> OnUpdateItemQuantity(int orderId, int itemId, int quantity, int version)
        {
            if (orderId <= 0)
                throw new ArgumentException("Order ID must be greater than 0", nameof(orderId));

            if (itemId <= 0)
                throw new ArgumentException("Item ID must be greater than 0", nameof(itemId));

            if(quantity < 0)
                throw new ArgumentException("Quantity must be greater than or equal to 0", nameof(quantity));

            var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);
            if (order == null)
            {
                throw new InvalidOperationException($"Order with ID {orderId} not found");
            }

            var item = await _itemRepository.GetByIdAsync(itemId);
            if (item == null)
            {
                throw new InvalidOperationException($"Item with ID {itemId} not found");
            }

            var orderItem = order.OrderItems.FirstOrDefault(oi => oi.ItemId == itemId);
            if (orderItem == null)
            {
                throw new InvalidOperationException($"Item '{item.Name}' is not part of order {order.OrderCode}");
            }

            if (quantity == 0)
            {
                order.RemoveItem(itemId);
            }
            else
            {
                order.UpdateItem(itemId, quantity);
            }

            order.IncrementVersion();

            try
            {
                await _orderRepository.UpdateAsync(order, version);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConcurrencyConflictException("This order was updated by another admin. Your changes were not saved. Please reload and try again.");
            }

            var updatedOrder = await _orderRepository.GetByIdWithDetailsAsync(orderId);
            return _mapper.Map<OrderDetailResponse>(updatedOrder!, opts => opts.Items["StorageService"] = _storageService);
        }

        private string GenerateOrderCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 5)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static string BuildFulfillmentDescription(OrderFulfillmentScope scope, OutHouseFulfillmentType? outHouseType)
            => scope switch
            {
                OrderFulfillmentScope.InHouse => "In House",
                OrderFulfillmentScope.OutHouse => outHouseType switch
                {
                    OutHouseFulfillmentType.Pickup   => "Out House — Pickup",
                    OutHouseFulfillmentType.Delivery => "Out House — Delivery",
                    _                                => "Out House"
                },
                _ => scope.ToString()
            };

        public async Task<OrderDetailResponse> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, int version)
        {
            var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);
            if (order == null)
            {
                throw new InvalidOperationException($"Order with ID {orderId} not found");
            }

            order.UpdateStatus(newStatus);
            order.IncrementVersion();

            try
            {
                await _orderRepository.UpdateAsync(order, version);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConcurrencyConflictException("This order was updated by another admin. Your changes were not saved. Please reload and try again.");
            }

            var updatedOrder = await _orderRepository.GetByIdWithDetailsAsync(orderId);
            return _mapper.Map<OrderDetailResponse>(updatedOrder!, opts => opts.Items["StorageService"] = _storageService);
        }

        public async Task CompleteOrderAndClearDataAsync(int orderId, int version)
        {
            var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);
            if (order == null)
            {
                throw new InvalidOperationException($"Order with ID {orderId} not found");
            }

            // Update order status to completed
            order.UpdateStatus(Domain.Enums.OrderStatus.Completed);

            // Clear delivery address from order
            order.ClearDeliveryAddress();

            // Clear customer personal data (customer is already loaded with the order)
            if (order.Customer != null)
            {
                order.Customer.ClearPersonalData();
            }

            order.IncrementVersion();

            try
            {
                await _orderRepository.UpdateAsync(order, version);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConcurrencyConflictException("This order was updated by another admin. Your changes were not saved. Please reload and try again.");
            }
        }

        public async Task<PaginatedResult<OrderListResponse>> GetOrdersWithFilterAsync(
            string? customerCode = null,
            string? orderCode = null,
            DateTime? createdDate = null,
            OrderStatus? status = null,
            int pageNumber = 1,
            int pageSize = 10,
            string sortBy = "CreatedDate",
            bool sortDescending = true)
        {
            var (orders, totalCount) = await _orderRepository.GetOrdersWithFilterAsync(
                customerCode, orderCode, createdDate, status, pageNumber, pageSize, sortBy, sortDescending);

            var orderListResponses = orders.Select(o => new OrderListResponse
            {
                Id = o.Id,
                OrderCode = o.OrderCode,
                Version = o.Version,
                CustomerCode = o.Customer.CustomerCode,
                CustomerName = $"{o.Customer.FirstName} {o.Customer.LastName}",
                Status = o.Status,
                CreatedDate = o.CreatedDate,
                CompletedDate = o.CompletedDate,
                TotalPrice = o.TotalPrice,
                TotalItemsAmount = o.TotalItemsAmount,
                FulfillmentScope = o.FulfillmentScope,
                OutHouseFulfillmentType = o.OutHouseFulfillmentType
            }).ToList();

            return new PaginatedResult<OrderListResponse>(orderListResponses, totalCount, pageNumber, pageSize);
        }

        public async Task<OrderDetailResponse> CreateOrderManuallyAsync(PlaceOrderRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Items == null || !request.Items.Any())
                throw new InvalidOperationException("Order must contain at least one item");

            // Create customer entity
            var customer = new Customer(
                request.FirstName,
                request.LastName,
                request.Email,
                request.PhoneNumber,
                request.DeliveryAddress
            );

            // Add customer to database
            var createdCustomer = await _customerRepository.CreateAsync(customer);

            // Generate unique order code (5 characters)
            var orderCode = GenerateOrderCode();

            // Create order entity with fulfillment scope and optional out-house type
            var order = new Order(
                orderCode,
                createdCustomer.Id,
                request.FulfillmentScope,
                request.OutHouseFulfillmentType,
                request.DeliveryAddress,
                request.Notes
            );

            // Validate items and add to order
            foreach (var requestItem in request.Items)
            {
                // Get item from database to verify it exists and get price
                var item = await _itemRepository.GetByIdAsync(requestItem.ItemId);

                if (item == null)
                {
                    throw new InvalidOperationException($"Item with ID {requestItem.ItemId} not found");
                }

                if (!item.IsActive)
                {
                    throw new InvalidOperationException($"Item '{item.Name}' is not available");
                }

                // Create order item with just the ItemId
                var orderItem = new OrderItems(item.Id, requestItem.Quantity, item.Price);
                order.AddItem(orderItem);
            }

            // Save to database
            var createdOrder = await _orderRepository.CreateAsync(order);

            // Reload with full details
            var orderWithDetails = await _orderRepository.GetByIdWithDetailsAsync(createdOrder.Id);

            // Map to response DTO with StorageService for dynamic SAS URLs
            return _mapper.Map<OrderDetailResponse>(orderWithDetails!, opts => opts.Items["StorageService"] = _storageService);
        }
    }
}
