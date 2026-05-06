using Application.DTOs.Order;
using Application.DTOs.Settings;
using Application.Interfaces;
using Application.Repositories;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Application.UnitTests.Services;

[TestClass]
public class OrderServiceTests
{
    private Mock<IOrderRepository> _orderRepositoryMock = null!;
    private Mock<IItemRepository> _itemRepositoryMock = null!;
    private Mock<ICustomerRepository> _customerRepositoryMock = null!;
    private Mock<IMapper> _mapperMock = null!;
    private Mock<IStorageService> _storageServiceMock = null!;
    private Mock<IEmailService> _emailServiceMock = null!;
    private Mock<ISystemSettingService> _settingServiceMock = null!;
    private Mock<ILogger<OrderService>> _loggerMock = null!;
    private OrderService _orderService = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _itemRepositoryMock = new Mock<IItemRepository>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _mapperMock = new Mock<IMapper>();
        _storageServiceMock = new Mock<IStorageService>();
        _emailServiceMock = new Mock<IEmailService>();
        _settingServiceMock = new Mock<ISystemSettingService>();
        _loggerMock = new Mock<ILogger<OrderService>>();

        // Setup mapper to return values based on type - using object to match any source type
        _mapperMock.Setup(x => x.Map<OrderDetailResponse>(It.IsAny<object>(), It.IsAny<Action<IMappingOperationOptions<object, OrderDetailResponse>>>()))
            .Returns(new OrderDetailResponse { OrderCode = "TEST" });
        _mapperMock.Setup(x => x.Map<IEnumerable<OrderSummaryResponse>>(It.IsAny<object>(), It.IsAny<Action<IMappingOperationOptions<object, IEnumerable<OrderSummaryResponse>>>>()))
            .Returns<object, Action<IMappingOperationOptions<object, IEnumerable<OrderSummaryResponse>>>>((src, opts) => 
            {
                if (src is List<Order> orders)
                {
                    return orders.Select((o, i) => new OrderSummaryResponse(i + 1, $"CODE{i}", 10.0m, 1, DateTimeOffset.UtcNow)).ToList();
                }
                return new List<OrderSummaryResponse>();
            });

        _orderService = new OrderService(
            _orderRepositoryMock.Object,
            _itemRepositoryMock.Object,
            _customerRepositoryMock.Object,
            _mapperMock.Object,
            _storageServiceMock.Object,
            _emailServiceMock.Object,
            _settingServiceMock.Object,
            _loggerMock.Object
        );
    }

    [TestMethod]
    public void Constructor_NullOrderRepository_ThrowsArgumentNullException()
    {
        // Arrange
        ArgumentNullException? exception = null;

        // Act
        try
        {
            _ = new OrderService(
                null!,
                _itemRepositoryMock.Object,
                _customerRepositoryMock.Object,
                _mapperMock.Object,
                _storageServiceMock.Object,
                _emailServiceMock.Object,
                _settingServiceMock.Object,
                _loggerMock.Object
            );
        }
        catch (ArgumentNullException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
    }

    [TestMethod]
    public void Constructor_NullItemRepository_ThrowsArgumentNullException()
    {
        // Arrange
        ArgumentNullException? exception = null;

        // Act
        try
        {
            _ = new OrderService(
                _orderRepositoryMock.Object,
                null!,
                _customerRepositoryMock.Object,
                _mapperMock.Object,
                _storageServiceMock.Object,
                _emailServiceMock.Object,
                _settingServiceMock.Object,
                _loggerMock.Object
            );
        }
        catch (ArgumentNullException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
    }

    [TestMethod]
    public void Constructor_NullCustomerRepository_ThrowsArgumentNullException()
    {
        // Arrange
        ArgumentNullException? exception = null;

        // Act
        try
        {
            _ = new OrderService(
                _orderRepositoryMock.Object,
                _itemRepositoryMock.Object,
                null!,
                _mapperMock.Object,
                _storageServiceMock.Object,
                _emailServiceMock.Object,
                _settingServiceMock.Object,
                _loggerMock.Object
            );
        }
        catch (ArgumentNullException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
    }

    [TestMethod]
    public void Constructor_NullMapper_ThrowsArgumentNullException()
    {
        // Arrange
        ArgumentNullException? exception = null;

        // Act
        try
        {
            _ = new OrderService(
                _orderRepositoryMock.Object,
                _itemRepositoryMock.Object,
                _customerRepositoryMock.Object,
                null!,
                _storageServiceMock.Object,
                _emailServiceMock.Object,
                _settingServiceMock.Object,
                _loggerMock.Object
            );
        }
        catch (ArgumentNullException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
    }

    [TestMethod]
    public void Constructor_NullStorageService_ThrowsArgumentNullException()
    {
        // Arrange
        ArgumentNullException? exception = null;

        // Act
        try
        {
            _ = new OrderService(
                _orderRepositoryMock.Object,
                _itemRepositoryMock.Object,
                _customerRepositoryMock.Object,
                _mapperMock.Object,
                null!,
                _emailServiceMock.Object,
                _settingServiceMock.Object,
                _loggerMock.Object
            );
        }
        catch (ArgumentNullException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
    }

    [TestMethod]
    public void Constructor_NullEmailService_ThrowsArgumentNullException()
    {
        // Arrange
        ArgumentNullException? exception = null;

        // Act
        try
        {
            _ = new OrderService(
                _orderRepositoryMock.Object,
                _itemRepositoryMock.Object,
                _customerRepositoryMock.Object,
                _mapperMock.Object,
                _storageServiceMock.Object,
                null!,
                _settingServiceMock.Object,
                _loggerMock.Object
            );
        }
        catch (ArgumentNullException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
    }

    [TestMethod]
    public void Constructor_NullSettingService_ThrowsArgumentNullException()
    {
        // Arrange
        ArgumentNullException? exception = null;

        // Act
        try
        {
            _ = new OrderService(
                _orderRepositoryMock.Object,
                _itemRepositoryMock.Object,
                _customerRepositoryMock.Object,
                _mapperMock.Object,
                _storageServiceMock.Object,
                _emailServiceMock.Object,
                null!,
                _loggerMock.Object
            );
        }
        catch (ArgumentNullException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
    }

    [TestMethod]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        ArgumentNullException? exception = null;

        // Act
        try
        {
            _ = new OrderService(
                _orderRepositoryMock.Object,
                _itemRepositoryMock.Object,
                _customerRepositoryMock.Object,
                _mapperMock.Object,
                _storageServiceMock.Object,
                _emailServiceMock.Object,
                _settingServiceMock.Object,
                null!
            );
        }
        catch (ArgumentNullException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
    }

    [TestMethod]
    public void Constructor_ValidParameters_CreatesInstance()
    {
        // Act
        var service = new OrderService(
            _orderRepositoryMock.Object,
            _itemRepositoryMock.Object,
            _customerRepositoryMock.Object,
            _mapperMock.Object,
            _storageServiceMock.Object,
            _emailServiceMock.Object,
            _settingServiceMock.Object,
            _loggerMock.Object
        );

        // Assert
        Assert.IsNotNull(service);
    }

    [TestMethod]
    public async Task PlaceOrderAsync_NullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        ArgumentNullException? exception = null;

        // Act
        try
        {
            await _orderService.PlaceOrderAsync(null!, Guid.NewGuid());
        }
        catch (ArgumentNullException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
    }

    [TestMethod]
    public async Task PlaceOrderAsync_NullItems_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new PlaceOrderRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = "1234567890",
            Items = null!
        };
        InvalidOperationException? exception = null;

        // Act
        try
        {
            await _orderService.PlaceOrderAsync(request, Guid.NewGuid());
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual("Order must contain at least one item", exception!.Message);
    }

    [TestMethod]
    public async Task PlaceOrderAsync_EmptyItems_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new PlaceOrderRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = "1234567890",
            Items = new List<OrderItemRequest>()
        };
        InvalidOperationException? exception = null;

        // Act
        try
        {
            await _orderService.PlaceOrderAsync(request, Guid.NewGuid());
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual("Order must contain at least one item", exception!.Message);
    }

    [TestMethod]
    public async Task PlaceOrderAsync_ItemNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = CreateCustomer(customerId);
        var request = new PlaceOrderRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = "1234567890",
            Items = new List<OrderItemRequest>
            {
                new() { ItemId = 1, Quantity = 2 }
            }
        };

        _customerRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Customer>()))
            .ReturnsAsync(customer);
        _itemRepositoryMock.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync((Item?)null);

        InvalidOperationException? exception = null;

        // Act
        try
        {
            await _orderService.PlaceOrderAsync(request, Guid.NewGuid());
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual("Item with ID 1 not found", exception!.Message);
    }

    [TestMethod]
    public async Task PlaceOrderAsync_ItemNotActive_ThrowsInvalidOperationException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = CreateCustomer(customerId);
        var item = CreateItem(1, "Coffee", 5.99m, isActive: false);
        var request = new PlaceOrderRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = "1234567890",
            Items = new List<OrderItemRequest>
            {
                new() { ItemId = 1, Quantity = 2 }
            }
        };

        _customerRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Customer>()))
            .ReturnsAsync(customer);
        _itemRepositoryMock.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(item);

        InvalidOperationException? exception = null;

        // Act
        try
        {
            await _orderService.PlaceOrderAsync(request, Guid.NewGuid());
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual("Item 'Coffee' is not available", exception!.Message);
    }

    [TestMethod]
    public async Task PlaceOrderAsync_ValidRequest_CreatesOrderAndReturnsResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var customer = CreateCustomer(customerId);
        var item1 = CreateItem(1, "Coffee", 5.99m);
        var item2 = CreateItem(2, "Tea", 3.99m);
        var request = new PlaceOrderRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = "1234567890",
            DeliveryAddress = "123 Main St",
            Notes = "No sugar",
            FulfillmentScope = OrderFulfillmentScope.OutHouse,
            OutHouseFulfillmentType = OutHouseFulfillmentType.Delivery,
            Items = new List<OrderItemRequest>
            {
                new() { ItemId = 1, Quantity = 2 },
                new() { ItemId = 2, Quantity = 1 }
            }
        };

        var createdOrder = CreateOrder(1, "ABC12", customerId);
        var orderWithDetails = CreateOrderWithDetails(1, "ABC12", customerId);

        _customerRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Customer>()))
            .ReturnsAsync(customer);
        _itemRepositoryMock.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(item1);
        _itemRepositoryMock.Setup(x => x.GetByIdAsync(2))
            .ReturnsAsync(item2);
        _orderRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Order>()))
            .ReturnsAsync(createdOrder);
        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(orderWithDetails);
        _settingServiceMock.Setup(x => x.GetAppSettingsAsync())
            .ReturnsAsync(new AppSettings { EmailConfirmationEnabled = false });

        // Act
        var result = await _orderService.PlaceOrderAsync(request, userId);

        // Assert
        Assert.IsNotNull(result);
        _customerRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Customer>()), Times.Once);
        _orderRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Order>()), Times.Once);
        _orderRepositoryMock.Verify(x => x.GetByIdWithDetailsAsync(1), Times.Once);
    }

    [TestMethod]
    public async Task PlaceOrderAsync_EmailConfirmationEnabled_SendsEmail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var customer = CreateCustomer(customerId);
        var item = CreateItem(1, "Coffee", 5.99m);
        var request = new PlaceOrderRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = "1234567890",
            Items = new List<OrderItemRequest>
            {
                new() { ItemId = 1, Quantity = 2 }
            }
        };

        var createdOrder = CreateOrder(1, "ABC12", customerId);
        var orderWithDetails = CreateOrderWithDetails(1, "ABC12", customerId);
        var response = new OrderDetailResponse
        {
            OrderCode = "ABC12",
            TotalPrice = 11.98m,
            CreatedDate = DateTimeOffset.UtcNow,
            Items = new List<OrderItemResponse>
            {
                new() { ItemName = "Coffee", Quantity = 2, UnitPrice = 5.99m }
            }
        };

        _customerRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Customer>()))
            .ReturnsAsync(customer);
        _itemRepositoryMock.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(item);
        _orderRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Order>()))
            .ReturnsAsync(createdOrder);
        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(orderWithDetails);
        _mapperMock.Setup(x => x.Map<OrderDetailResponse>(It.IsAny<Order>(), It.IsAny<Action<IMappingOperationOptions>>()))
            .Returns(response);
        _settingServiceMock.Setup(x => x.GetAppSettingsAsync())
            .ReturnsAsync(new AppSettings { EmailConfirmationEnabled = true });

        // Act
        var result = await _orderService.PlaceOrderAsync(request, userId);

        // Assert
        Assert.IsNotNull(result);
        _emailServiceMock.Verify(x => x.SendOrderConfirmationAsync(It.IsAny<Application.DTOs.Email.OrderConfirmationEmail>()), Times.Once);
    }

    [TestMethod]
    public async Task PlaceOrderAsync_EmailConfirmationDisabled_DoesNotSendEmail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var customer = CreateCustomer(customerId);
        var item = CreateItem(1, "Coffee", 5.99m);
        var request = new PlaceOrderRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = "1234567890",
            Items = new List<OrderItemRequest>
            {
                new() { ItemId = 1, Quantity = 2 }
            }
        };

        var createdOrder = CreateOrder(1, "ABC12", customerId);
        var orderWithDetails = CreateOrderWithDetails(1, "ABC12", customerId);
        var response = new OrderDetailResponse { OrderCode = "ABC12" };

        _customerRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Customer>()))
            .ReturnsAsync(customer);
        _itemRepositoryMock.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(item);
        _orderRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Order>()))
            .ReturnsAsync(createdOrder);
        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(orderWithDetails);
        _mapperMock.Setup(x => x.Map<OrderDetailResponse>(It.IsAny<Order>(), It.IsAny<Action<IMappingOperationOptions>>()))
            .Returns(response);
        _settingServiceMock.Setup(x => x.GetAppSettingsAsync())
            .ReturnsAsync(new AppSettings { EmailConfirmationEnabled = false });

        // Act
        var result = await _orderService.PlaceOrderAsync(request, userId);

        // Assert
        Assert.IsNotNull(result);
        _emailServiceMock.Verify(x => x.SendOrderConfirmationAsync(It.IsAny<Application.DTOs.Email.OrderConfirmationEmail>()), Times.Never);
    }

    [TestMethod]
    public async Task PlaceOrderAsync_NoEmailProvided_DoesNotSendEmail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var customer = CreateCustomer(customerId);
        var item = CreateItem(1, "Coffee", 5.99m);
        var request = new PlaceOrderRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = null,
            PhoneNumber = "1234567890",
            Items = new List<OrderItemRequest>
            {
                new() { ItemId = 1, Quantity = 2 }
            }
        };

        var createdOrder = CreateOrder(1, "ABC12", customerId);
        var orderWithDetails = CreateOrderWithDetails(1, "ABC12", customerId);
        var response = new OrderDetailResponse { OrderCode = "ABC12" };

        _customerRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Customer>()))
            .ReturnsAsync(customer);
        _itemRepositoryMock.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(item);
        _orderRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Order>()))
            .ReturnsAsync(createdOrder);
        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(orderWithDetails);
        _mapperMock.Setup(x => x.Map<OrderDetailResponse>(It.IsAny<Order>(), It.IsAny<Action<IMappingOperationOptions>>()))
            .Returns(response);

        // Act
        var result = await _orderService.PlaceOrderAsync(request, userId);

        // Assert
        Assert.IsNotNull(result);
        _emailServiceMock.Verify(x => x.SendOrderConfirmationAsync(It.IsAny<Application.DTOs.Email.OrderConfirmationEmail>()), Times.Never);
        _settingServiceMock.Verify(x => x.GetAppSettingsAsync(), Times.Never);
    }

    [TestMethod]
    public async Task PlaceOrderAsync_EmailSendingFails_LogsErrorButDoesNotThrow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var customer = CreateCustomer(customerId);
        var item = CreateItem(1, "Coffee", 5.99m);
        var request = new PlaceOrderRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = "1234567890",
            Items = new List<OrderItemRequest>
            {
                new() { ItemId = 1, Quantity = 2 }
            }
        };

        var createdOrder = CreateOrder(1, "ABC12", customerId);
        var orderWithDetails = CreateOrderWithDetails(1, "ABC12", customerId);
        var response = new OrderDetailResponse
        {
            OrderCode = "ABC12",
            TotalPrice = 11.98m,
            CreatedDate = DateTimeOffset.UtcNow,
            Items = new List<OrderItemResponse>
            {
                new() { ItemName = "Coffee", Quantity = 2, UnitPrice = 5.99m }
            }
        };

        _customerRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Customer>()))
            .ReturnsAsync(customer);
        _itemRepositoryMock.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(item);
        _orderRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Order>()))
            .ReturnsAsync(createdOrder);
        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(orderWithDetails);
        _mapperMock.Setup(x => x.Map<OrderDetailResponse>(It.IsAny<Order>(), It.IsAny<Action<IMappingOperationOptions>>()))
            .Returns(response);
        _settingServiceMock.Setup(x => x.GetAppSettingsAsync())
            .ReturnsAsync(new AppSettings { EmailConfirmationEnabled = true });
        _emailServiceMock.Setup(x => x.SendOrderConfirmationAsync(It.IsAny<Application.DTOs.Email.OrderConfirmationEmail>()))
            .ThrowsAsync(new Exception("Email service failed"));

        // Act
        var result = await _orderService.PlaceOrderAsync(request, userId);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task GetOrderDetailsAsync_OrderExists_ReturnsOrderDetailResponse()
    {
        // Arrange
        var orderId = 1;
        var order = CreateOrderWithDetails(orderId, "ABC12", Guid.NewGuid());

        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(orderId))
            .ReturnsAsync(order);

        // Act
        var result = await _orderService.GetOrderDetailsAsync(orderId);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task GetOrderDetailsAsync_OrderDoesNotExist_ReturnsNull()
    {
        // Arrange
        var orderId = 1;

        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(orderId))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _orderService.GetOrderDetailsAsync(orderId);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetUserOrdersAsync_ReturnsOrderSummaryCollection()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orders = new List<Order>
        {
            CreateOrder(1, "ABC12", userId),
            CreateOrder(2, "XYZ34", userId)
        };
        var summaries = new List<OrderSummaryResponse>
        {
            new(1, "ABC12", 10.00m, 2, DateTimeOffset.UtcNow),
            new(2, "XYZ34", 20.00m, 3, DateTimeOffset.UtcNow)
        };

        _orderRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(orders);
        _mapperMock.Setup(x => x.Map<IEnumerable<OrderSummaryResponse>>(It.IsAny<List<Order>>(), It.IsAny<Action<IMappingOperationOptions>>()))
            .Returns(summaries);

        // Act
        var result = await _orderService.GetUserOrdersAsync(userId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count());
    }

    [TestMethod]
    public async Task GetOrderByCodeAsync_OrderExists_ReturnsOrderDetailResponse()
    {
        // Arrange
        var orderCode = "ABC12";
        var order = CreateOrderWithDetails(1, orderCode, Guid.NewGuid());

        _orderRepositoryMock.Setup(x => x.GetByOrderCodeAsync(orderCode))
            .ReturnsAsync(order);

        // Act
        var result = await _orderService.GetOrderByCodeAsync(orderCode);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task GetOrderByCodeAsync_OrderDoesNotExist_ReturnsNull()
    {
        // Arrange
        var orderCode = "ABC12";

        _orderRepositoryMock.Setup(x => x.GetByOrderCodeAsync(orderCode))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _orderService.GetOrderByCodeAsync(orderCode);

        // Assert
        Assert.IsNull(result);
    }

    private static Customer CreateCustomer(Guid id)
    {
        var customer = new Customer("John", "Doe", "john@example.com", "1234567890", "123 Main St");
        typeof(Customer).GetProperty("Id")!.SetValue(customer, id);
        return customer;
    }

    private static Item CreateItem(int id, string name, decimal price, bool isActive = true)
    {
        var item = new Item(name, price, 1, "Description");
        typeof(Item).GetProperty("Id")!.SetValue(item, id);
        typeof(Item).GetProperty("IsActive")!.SetValue(item, isActive);
        return item;
    }

    private static Order CreateOrder(int id, string orderCode, Guid customerId)
    {
        var order = new Order(orderCode, customerId, OrderFulfillmentScope.OutHouse, OutHouseFulfillmentType.Pickup, "123 Main St", "Notes");
        typeof(Order).GetProperty("Id")!.SetValue(order, id);
        return order;
    }

    private static Order CreateOrderWithDetails(int id, string orderCode, Guid customerId)
    {
        var order = CreateOrder(id, orderCode, customerId);
        return order;
    }

    private static Order CreateOrderWithOrderItems(int id, string orderCode, Guid customerId, params (int itemId, int quantity, decimal unitPrice)[] items)
    {
        var order = CreateOrder(id, orderCode, customerId);
        foreach (var (itemId, quantity, unitPrice) in items)
        {
            var orderItem = new Domain.Entities.OrderItems(itemId, quantity, unitPrice);
            typeof(Order).GetMethod("AddItem")!.Invoke(order, new object[] { orderItem });
        }
        return order;
    }

    [TestMethod]
    public async Task OnAddItem_OrderIdLessThanOrEqualToZero_ThrowsArgumentException()
    {
        // Arrange
        var orderId = 0;
        var itemId = 1;
        var version = 1;
        ArgumentException? exception = null;

        // Act
        try
        {
            await _orderService.OnAddItem(orderId, itemId, version);
        }
        catch (ArgumentException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual("orderId", exception!.ParamName);
    }

    [TestMethod]
    public async Task OnAddItem_ItemIdLessThanOrEqualToZero_ThrowsArgumentException()
    {
        // Arrange
        var orderId = 1;
        var itemId = 0;
        var version = 1;
        ArgumentException? exception = null;

        // Act
        try
        {
            await _orderService.OnAddItem(orderId, itemId, version);
        }
        catch (ArgumentException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual("itemId", exception!.ParamName);
    }

    [TestMethod]
    public async Task OnAddItem_OrderNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var orderId = 1;
        var itemId = 1;
        var version = 1;

        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(orderId))
            .ReturnsAsync((Order?)null);

        InvalidOperationException? exception = null;

        // Act
        try
        {
            await _orderService.OnAddItem(orderId, itemId, version);
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual($"Order with ID {orderId} not found", exception!.Message);
    }

    [TestMethod]
    public async Task OnAddItem_ItemNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var orderId = 1;
        var itemId = 1;
        var version = 1;
        var order = CreateOrderWithOrderItems(orderId, "ABC12", Guid.NewGuid(), (1, 2, 5.99m));

        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(orderId))
            .ReturnsAsync(order);
        _itemRepositoryMock.Setup(x => x.GetByIdAsync(itemId))
            .ReturnsAsync((Item?)null);

        InvalidOperationException? exception = null;

        // Act
        try
        {
            await _orderService.OnAddItem(orderId, itemId, version);
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual($"Item with ID {itemId} not found", exception!.Message);
    }

    [TestMethod]
    public async Task OnAddItem_ItemNotActive_ThrowsInvalidOperationException()
    {
        // Arrange
        var orderId = 1;
        var itemId = 1;
        var version = 1;
        var order = CreateOrderWithOrderItems(orderId, "ABC12", Guid.NewGuid(), (1, 2, 5.99m));
        var item = CreateItem(itemId, "Coffee", 5.99m, isActive: false);

        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(orderId))
            .ReturnsAsync(order);
        _itemRepositoryMock.Setup(x => x.GetByIdAsync(itemId))
            .ReturnsAsync(item);

        InvalidOperationException? exception = null;

        // Act
        try
        {
            await _orderService.OnAddItem(orderId, itemId, version);
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual($"Item '{item.Name}' is not available", exception!.Message);
    }

    [TestMethod]
    public async Task OnAddItem_ItemNotInOrder_ThrowsInvalidOperationException()
    {
        // Arrange
        var orderId = 1;
        var itemId = 2;
        var version = 1;
        var order = CreateOrderWithOrderItems(orderId, "ABC12", Guid.NewGuid(), (1, 2, 5.99m));
        var item = CreateItem(itemId, "Tea", 3.99m);

        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(orderId))
            .ReturnsAsync(order);
        _itemRepositoryMock.Setup(x => x.GetByIdAsync(itemId))
            .ReturnsAsync(item);

        InvalidOperationException? exception = null;

        // Act
        try
        {
            await _orderService.OnAddItem(orderId, itemId, version);
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual($"Item '{item.Name}' is not part of order {order.OrderCode}", exception!.Message);
    }

    [TestMethod]
    public async Task OnAddItem_ConcurrencyConflict_ThrowsConcurrencyConflictException()
    {
        // Arrange
        var orderId = 1;
        var itemId = 1;
        var version = 1;
        var order = CreateOrderWithOrderItems(orderId, "ABC12", Guid.NewGuid(), (1, 2, 5.99m));
        var item = CreateItem(itemId, "Coffee", 5.99m);

        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(orderId))
            .ReturnsAsync(order);
        _itemRepositoryMock.Setup(x => x.GetByIdAsync(itemId))
            .ReturnsAsync(item);
        _orderRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Order>(), version))
            .ThrowsAsync(new Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException());

        Application.Exceptions.ConcurrencyConflictException? exception = null;

        // Act
        try
        {
            await _orderService.OnAddItem(orderId, itemId, version);
        }
        catch (Application.Exceptions.ConcurrencyConflictException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual("This order was updated by another admin. Your changes were not saved. Please reload and try again.", exception!.Message);
    }

    [TestMethod]
    public async Task OnAddItem_ValidRequest_ReturnsOrderDetailResponse()
    {
        // Arrange
        var orderId = 1;
        var itemId = 1;
        var version = 1;
        var order = CreateOrderWithOrderItems(orderId, "ABC12", Guid.NewGuid(), (1, 2, 5.99m));
        var item = CreateItem(itemId, "Coffee", 5.99m);
        var updatedOrder = CreateOrderWithOrderItems(orderId, "ABC12", Guid.NewGuid(), (1, 3, 5.99m));

        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(orderId))
            .ReturnsAsync(order);
        _itemRepositoryMock.Setup(x => x.GetByIdAsync(itemId))
            .ReturnsAsync(item);
        _orderRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Order>(), version))
            .Returns(Task.CompletedTask);
        _orderRepositoryMock.SetupSequence(x => x.GetByIdWithDetailsAsync(orderId))
            .ReturnsAsync(order)
            .ReturnsAsync(updatedOrder);

        // Act
        var result = await _orderService.OnAddItem(orderId, itemId, version);

        // Assert
        Assert.IsNotNull(result);
        _orderRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Order>(), version), Times.Once);
    }

    [TestMethod]
    public async Task OnRemoveItem_OrderIdLessThanOrEqualToZero_ThrowsArgumentException()
    {
        // Arrange
        var orderId = 0;
        var itemId = 1;
        var version = 1;
        ArgumentException? exception = null;

        // Act
        try
        {
            await _orderService.OnRemoveItem(orderId, itemId, version);
        }
        catch (ArgumentException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual("orderId", exception!.ParamName);
    }

    [TestMethod]
    public async Task OnRemoveItem_ItemIdLessThanOrEqualToZero_ThrowsArgumentException()
    {
        // Arrange
        var orderId = 1;
        var itemId = 0;
        var version = 1;
        ArgumentException? exception = null;

        // Act
        try
        {
            await _orderService.OnRemoveItem(orderId, itemId, version);
        }
        catch (ArgumentException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual("itemId", exception!.ParamName);
    }

    [TestMethod]
    public async Task OnRemoveItem_OrderNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var orderId = 1;
        var itemId = 1;
        var version = 1;

        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(orderId))
            .ReturnsAsync((Order?)null);

        InvalidOperationException? exception = null;

        // Act
        try
        {
            await _orderService.OnRemoveItem(orderId, itemId, version);
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual($"Order with ID {orderId} not found", exception!.Message);
    }

    [TestMethod]
    public async Task OnRemoveItem_ItemNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var orderId = 1;
        var itemId = 1;
        var version = 1;
        var order = CreateOrderWithOrderItems(orderId, "ABC12", Guid.NewGuid(), (1, 2, 5.99m));

        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(orderId))
            .ReturnsAsync(order);
        _itemRepositoryMock.Setup(x => x.GetByIdAsync(itemId))
            .ReturnsAsync((Item?)null);

        InvalidOperationException? exception = null;

        // Act
        try
        {
            await _orderService.OnRemoveItem(orderId, itemId, version);
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual($"Item with ID {itemId} not found", exception!.Message);
    }

    [TestMethod]
    public async Task OnRemoveItem_ItemNotInOrder_ThrowsInvalidOperationException()
    {
        // Arrange
        var orderId = 1;
        var itemId = 2;
        var version = 1;
        var order = CreateOrderWithOrderItems(orderId, "ABC12", Guid.NewGuid(), (1, 2, 5.99m));
        var item = CreateItem(itemId, "Tea", 3.99m);

        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(orderId))
            .ReturnsAsync(order);
        _itemRepositoryMock.Setup(x => x.GetByIdAsync(itemId))
            .ReturnsAsync(item);

        InvalidOperationException? exception = null;

        // Act
        try
        {
            await _orderService.OnRemoveItem(orderId, itemId, version);
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual($"Item '{item.Name}' is not part of order {order.OrderCode}", exception!.Message);
    }

    [TestMethod]
    public async Task OnRemoveItem_QuantityBecomesNegative_ThrowsInvalidOperationException()
    {
        // Arrange
        var orderId = 1;
        var itemId = 1;
        var version = 1;
        var order = CreateOrderWithOrderItems(orderId, "ABC12", Guid.NewGuid(), (1, 1, 5.99m));
        var item = CreateItem(itemId, "Coffee", 5.99m);

        // Manually set quantity to 0 after creation to test the edge case
        var orderItem = order.OrderItems.First(oi => oi.ItemId == itemId);
        typeof(Domain.Entities.OrderItems).GetProperty("Quantity")!.SetValue(orderItem, 0);

        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(orderId))
            .ReturnsAsync(order);
        _itemRepositoryMock.Setup(x => x.GetByIdAsync(itemId))
            .ReturnsAsync(item);

        InvalidOperationException? exception = null;

        // Act
        try
        {
            await _orderService.OnRemoveItem(orderId, itemId, version);
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual($"Cannot remove more '{item.Name}' from order {order.OrderCode} than exists", exception!.Message);
    }

    [TestMethod]
    public async Task OnRemoveItem_QuantityBecomesZero_RemovesItemFromOrder()
    {
        // Arrange
        var orderId = 1;
        var itemId = 1;
        var version = 1;
        var order = CreateOrderWithOrderItems(orderId, "ABC12", Guid.NewGuid(), (1, 1, 5.99m));
        var item = CreateItem(itemId, "Coffee", 5.99m);
        var updatedOrder = CreateOrderWithDetails(orderId, "ABC12", Guid.NewGuid());

        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(orderId))
            .ReturnsAsync(order);
        _itemRepositoryMock.Setup(x => x.GetByIdAsync(itemId))
            .ReturnsAsync(item);
        _orderRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Order>(), version))
            .Returns(Task.CompletedTask);
        _orderRepositoryMock.SetupSequence(x => x.GetByIdWithDetailsAsync(orderId))
            .ReturnsAsync(order)
            .ReturnsAsync(updatedOrder);

        // Act
        var result = await _orderService.OnRemoveItem(orderId, itemId, version);

        // Assert
        Assert.IsNotNull(result);
        _orderRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Order>(), version), Times.Once);
    }

    [TestMethod]
    public async Task OnRemoveItem_ConcurrencyConflict_ThrowsConcurrencyConflictException()
    {
        // Arrange
        var orderId = 1;
        var itemId = 1;
        var version = 1;
        var order = CreateOrderWithOrderItems(orderId, "ABC12", Guid.NewGuid(), (1, 2, 5.99m));
        var item = CreateItem(itemId, "Coffee", 5.99m);

        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(orderId))
            .ReturnsAsync(order);
        _itemRepositoryMock.Setup(x => x.GetByIdAsync(itemId))
            .ReturnsAsync(item);
        _orderRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Order>(), version))
            .ThrowsAsync(new Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException());

        Application.Exceptions.ConcurrencyConflictException? exception = null;

        // Act
        try
        {
            await _orderService.OnRemoveItem(orderId, itemId, version);
        }
        catch (Application.Exceptions.ConcurrencyConflictException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual("This order was updated by another admin. Your changes were not saved. Please reload and try again.", exception!.Message);
    }

    [TestMethod]
    public async Task OnRemoveItem_ValidRequest_ReturnsOrderDetailResponse()
    {
        // Arrange
        var orderId = 1;
        var itemId = 1;
        var version = 1;
        var order = CreateOrderWithOrderItems(orderId, "ABC12", Guid.NewGuid(), (1, 3, 5.99m));
        var item = CreateItem(itemId, "Coffee", 5.99m);
        var updatedOrder = CreateOrderWithOrderItems(orderId, "ABC12", Guid.NewGuid(), (1, 2, 5.99m));

        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(orderId))
            .ReturnsAsync(order);
        _itemRepositoryMock.Setup(x => x.GetByIdAsync(itemId))
            .ReturnsAsync(item);
        _orderRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Order>(), version))
            .Returns(Task.CompletedTask);
        _orderRepositoryMock.SetupSequence(x => x.GetByIdWithDetailsAsync(orderId))
            .ReturnsAsync(order)
            .ReturnsAsync(updatedOrder);

        // Act
        var result = await _orderService.OnRemoveItem(orderId, itemId, version);

        // Assert
        Assert.IsNotNull(result);
        _orderRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Order>(), version), Times.Once);
    }

    [TestMethod]
    public async Task OnUpdateItemQuantity_OrderIdLessThanOrEqualToZero_ThrowsArgumentException()
    {
        // Arrange
        var orderId = 0;
        var itemId = 1;
        var quantity = 5;
        var version = 1;
        ArgumentException? exception = null;

        // Act
        try
        {
            await _orderService.OnUpdateItemQuantity(orderId, itemId, quantity, version);
        }
        catch (ArgumentException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual("orderId", exception!.ParamName);
    }

    [TestMethod]
    public async Task OnUpdateItemQuantity_ItemIdLessThanOrEqualToZero_ThrowsArgumentException()
    {
        // Arrange
        var orderId = 1;
        var itemId = 0;
        var quantity = 5;
        var version = 1;
        ArgumentException? exception = null;

        // Act
        try
        {
            await _orderService.OnUpdateItemQuantity(orderId, itemId, quantity, version);
        }
        catch (ArgumentException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual("itemId", exception!.ParamName);
    }

    [TestMethod]
    public async Task OnUpdateItemQuantity_QuantityLessThanZero_ThrowsArgumentException()
    {
        // Arrange
        var orderId = 1;
        var itemId = 1;
        var quantity = -1;
        var version = 1;
        ArgumentException? exception = null;

        // Act
        try
        {
            await _orderService.OnUpdateItemQuantity(orderId, itemId, quantity, version);
        }
        catch (ArgumentException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual("quantity", exception!.ParamName);
    }

    [TestMethod]
    public async Task OnUpdateItemQuantity_OrderNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var orderId = 1;
        var itemId = 1;
        var quantity = 5;
        var version = 1;

        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(orderId))
            .ReturnsAsync((Order?)null);

        InvalidOperationException? exception = null;

        // Act
        try
        {
            await _orderService.OnUpdateItemQuantity(orderId, itemId, quantity, version);
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual($"Order with ID {orderId} not found", exception!.Message);
    }

    [TestMethod]
    public async Task OnUpdateItemQuantity_ItemNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var orderId = 1;
        var itemId = 1;
        var quantity = 5;
        var version = 1;
        var order = CreateOrderWithOrderItems(orderId, "ABC12", Guid.NewGuid(), (1, 2, 5.99m));

        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(orderId))
            .ReturnsAsync(order);
        _itemRepositoryMock.Setup(x => x.GetByIdAsync(itemId))
            .ReturnsAsync((Item?)null);

        InvalidOperationException? exception = null;

        // Act
        try
        {
            await _orderService.OnUpdateItemQuantity(orderId, itemId, quantity, version);
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual($"Item with ID {itemId} not found", exception!.Message);
    }

    [TestMethod]
    public async Task OnUpdateItemQuantity_ItemNotInOrder_ThrowsInvalidOperationException()
    {
        // Arrange
        var orderId = 1;
        var itemId = 2;
        var quantity = 5;
        var version = 1;
        var order = CreateOrderWithOrderItems(orderId, "ABC12", Guid.NewGuid(), (1, 2, 5.99m));
        var item = CreateItem(itemId, "Tea", 3.99m);

        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(orderId))
            .ReturnsAsync(order);
        _itemRepositoryMock.Setup(x => x.GetByIdAsync(itemId))
            .ReturnsAsync(item);

        InvalidOperationException? exception = null;

        // Act
        try
        {
            await _orderService.OnUpdateItemQuantity(orderId, itemId, quantity, version);
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual($"Item '{item.Name}' is not part of order {order.OrderCode}", exception!.Message);
    }

    [TestMethod]
    public async Task OnUpdateItemQuantity_QuantityZero_RemovesItemFromOrder()
    {
        // Arrange
        var orderId = 1;
        var itemId = 1;
        var quantity = 0;
        var version = 1;
        var order = CreateOrderWithOrderItems(orderId, "ABC12", Guid.NewGuid(), (1, 2, 5.99m));
        var item = CreateItem(itemId, "Coffee", 5.99m);
        var updatedOrder = CreateOrderWithDetails(orderId, "ABC12", Guid.NewGuid());

        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(orderId))
            .ReturnsAsync(order);
        _itemRepositoryMock.Setup(x => x.GetByIdAsync(itemId))
            .ReturnsAsync(item);
        _orderRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Order>(), version))
            .Returns(Task.CompletedTask);
        _orderRepositoryMock.SetupSequence(x => x.GetByIdWithDetailsAsync(orderId))
            .ReturnsAsync(order)
            .ReturnsAsync(updatedOrder);

        // Act
        var result = await _orderService.OnUpdateItemQuantity(orderId, itemId, quantity, version);

        // Assert
        Assert.IsNotNull(result);
        _orderRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Order>(), version), Times.Once);
    }

    [TestMethod]
    public async Task OnUpdateItemQuantity_ConcurrencyConflict_ThrowsConcurrencyConflictException()
    {
        // Arrange
        var orderId = 1;
        var itemId = 1;
        var quantity = 5;
        var version = 1;
        var order = CreateOrderWithOrderItems(orderId, "ABC12", Guid.NewGuid(), (1, 2, 5.99m));
        var item = CreateItem(itemId, "Coffee", 5.99m);

        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(orderId))
            .ReturnsAsync(order);
        _itemRepositoryMock.Setup(x => x.GetByIdAsync(itemId))
            .ReturnsAsync(item);
        _orderRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Order>(), version))
            .ThrowsAsync(new Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException());

        Application.Exceptions.ConcurrencyConflictException? exception = null;

        // Act
        try
        {
            await _orderService.OnUpdateItemQuantity(orderId, itemId, quantity, version);
        }
        catch (Application.Exceptions.ConcurrencyConflictException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual("This order was updated by another admin. Your changes were not saved. Please reload and try again.", exception!.Message);
    }

    [TestMethod]
    public async Task OnUpdateItemQuantity_ValidRequest_ReturnsOrderDetailResponse()
    {
        // Arrange
        var orderId = 1;
        var itemId = 1;
        var quantity = 5;
        var version = 1;
        var order = CreateOrderWithOrderItems(orderId, "ABC12", Guid.NewGuid(), (1, 2, 5.99m));
        var item = CreateItem(itemId, "Coffee", 5.99m);
        var updatedOrder = CreateOrderWithOrderItems(orderId, "ABC12", Guid.NewGuid(), (1, 5, 5.99m));

        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(orderId))
            .ReturnsAsync(order);
        _itemRepositoryMock.Setup(x => x.GetByIdAsync(itemId))
            .ReturnsAsync(item);
        _orderRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Order>(), version))
            .Returns(Task.CompletedTask);
        _orderRepositoryMock.SetupSequence(x => x.GetByIdWithDetailsAsync(orderId))
            .ReturnsAsync(order)
            .ReturnsAsync(updatedOrder);

        // Act
        var result = await _orderService.OnUpdateItemQuantity(orderId, itemId, quantity, version);

        // Assert
        Assert.IsNotNull(result);
        _orderRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Order>(), version), Times.Once);
    }

    [TestMethod]
    public async Task UpdateOrderStatusAsync_OrderNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var orderId = 1;
        var newStatus = OrderStatus.InProgress;
        var version = 1;

        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(orderId))
            .ReturnsAsync((Order?)null);

        InvalidOperationException? exception = null;

        // Act
        try
        {
            await _orderService.UpdateOrderStatusAsync(orderId, newStatus, version);
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual($"Order with ID {orderId} not found", exception!.Message);
    }

    [TestMethod]
    public async Task UpdateOrderStatusAsync_ConcurrencyConflict_ThrowsConcurrencyConflictException()
    {
        // Arrange
        var orderId = 1;
        var newStatus = OrderStatus.InProgress;
        var version = 1;
        var order = CreateOrderWithDetails(orderId, "ABC12", Guid.NewGuid());

        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(orderId))
            .ReturnsAsync(order);
        _orderRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Order>(), version))
            .ThrowsAsync(new Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException());

        Application.Exceptions.ConcurrencyConflictException? exception = null;

        // Act
        try
        {
            await _orderService.UpdateOrderStatusAsync(orderId, newStatus, version);
        }
        catch (Application.Exceptions.ConcurrencyConflictException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual("This order was updated by another admin. Your changes were not saved. Please reload and try again.", exception!.Message);
    }

    [TestMethod]
    public async Task UpdateOrderStatusAsync_ValidRequest_ReturnsOrderDetailResponse()
    {
        // Arrange
        var orderId = 1;
        var newStatus = OrderStatus.InProgress;
        var version = 1;
        var order = CreateOrderWithDetails(orderId, "ABC12", Guid.NewGuid());
        var updatedOrder = CreateOrderWithDetails(orderId, "ABC12", Guid.NewGuid());

        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(orderId))
            .ReturnsAsync(order);
        _orderRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Order>(), version))
            .Returns(Task.CompletedTask);
        _orderRepositoryMock.SetupSequence(x => x.GetByIdWithDetailsAsync(orderId))
            .ReturnsAsync(order)
            .ReturnsAsync(updatedOrder);

        // Act
        var result = await _orderService.UpdateOrderStatusAsync(orderId, newStatus, version);

        // Assert
        Assert.IsNotNull(result);
        _orderRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Order>(), version), Times.Once);
    }

    [TestMethod]
    public async Task CompleteOrderAndClearDataAsync_OrderNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var orderId = 1;
        var version = 1;

        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(orderId))
            .ReturnsAsync((Order?)null);

        InvalidOperationException? exception = null;

        // Act
        try
        {
            await _orderService.CompleteOrderAndClearDataAsync(orderId, version);
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual($"Order with ID {orderId} not found", exception!.Message);
    }

    [TestMethod]
    public async Task CompleteOrderAndClearDataAsync_ConcurrencyConflict_ThrowsConcurrencyConflictException()
    {
        // Arrange
        var orderId = 1;
        var version = 1;
        var order = CreateOrderWithDetails(orderId, "ABC12", Guid.NewGuid());

        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(orderId))
            .ReturnsAsync(order);
        _orderRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Order>(), version))
            .ThrowsAsync(new Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException());

        Application.Exceptions.ConcurrencyConflictException? exception = null;

        // Act
        try
        {
            await _orderService.CompleteOrderAndClearDataAsync(orderId, version);
        }
        catch (Application.Exceptions.ConcurrencyConflictException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual("This order was updated by another admin. Your changes were not saved. Please reload and try again.", exception!.Message);
    }

    [TestMethod]
    public async Task CompleteOrderAndClearDataAsync_OrderWithCustomer_UpdatesOrderAndClearsCustomerData()
    {
        // Arrange
        var orderId = 1;
        var version = 1;
        var customerId = Guid.NewGuid();
        var customer = CreateCustomer(customerId);
        var order = CreateOrderWithDetails(orderId, "ABC12", customerId);
        typeof(Order).GetProperty("Customer")!.SetValue(order, customer);

        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(orderId))
            .ReturnsAsync(order);
        _orderRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Order>(), version))
            .Returns(Task.CompletedTask);

        // Act
        await _orderService.CompleteOrderAndClearDataAsync(orderId, version);

        // Assert
        _orderRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Order>(), version), Times.Once);
    }

    [TestMethod]
    public async Task CompleteOrderAndClearDataAsync_OrderWithoutCustomer_UpdatesOrderOnly()
    {
        // Arrange
        var orderId = 1;
        var version = 1;
        var order = CreateOrderWithDetails(orderId, "ABC12", Guid.NewGuid());

        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(orderId))
            .ReturnsAsync(order);
        _orderRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Order>(), version))
            .Returns(Task.CompletedTask);

        // Act
        await _orderService.CompleteOrderAndClearDataAsync(orderId, version);

        // Assert
        _orderRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Order>(), version), Times.Once);
    }

    [TestMethod]
    public async Task GetOrdersWithFilterAsync_NoFilters_ReturnsPaginatedOrders()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = CreateCustomer(customerId);
        var order1 = CreateOrder(1, "ORD01", customerId);
        var order2 = CreateOrder(2, "ORD02", customerId);
        typeof(Order).GetProperty("Customer")!.SetValue(order1, customer);
        typeof(Order).GetProperty("Customer")!.SetValue(order2, customer);
        
        var orders = new List<Order> { order1, order2 };
        var totalCount = 2;

        _orderRepositoryMock.Setup(x => x.GetOrdersWithFilterAsync(
            null, null, null, null, 1, 10, "CreatedDate", true))
            .ReturnsAsync((orders, totalCount));

        // Act
        var result = await _orderService.GetOrdersWithFilterAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Items.Count());
        Assert.AreEqual(2, result.TotalCount);
        Assert.AreEqual(1, result.PageNumber);
        Assert.AreEqual(10, result.PageSize);
    }

    [TestMethod]
    public async Task GetOrdersWithFilterAsync_WithAllFilters_ReturnsPaginatedOrders()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = CreateCustomer(customerId);
        var order = CreateOrder(1, "ORD01", customerId);
        typeof(Order).GetProperty("Customer")!.SetValue(order, customer);
        
        var orders = new List<Order> { order };
        var totalCount = 1;
        var createdDate = DateTime.UtcNow;

        _orderRepositoryMock.Setup(x => x.GetOrdersWithFilterAsync(
            "CUST01", "ORD01", createdDate, OrderStatus.Pending, 2, 20, "OrderCode", false))
            .ReturnsAsync((orders, totalCount));

        // Act
        var result = await _orderService.GetOrdersWithFilterAsync(
            "CUST01", "ORD01", createdDate, OrderStatus.Pending, 2, 20, "OrderCode", false);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Items.Count());
        Assert.AreEqual(1, result.TotalCount);
        Assert.AreEqual(2, result.PageNumber);
        Assert.AreEqual(20, result.PageSize);
    }

    [TestMethod]
    public async Task GetOrdersWithFilterAsync_EmptyResults_ReturnsEmptyPaginatedResult()
    {
        // Arrange
        var orders = new List<Order>();
        var totalCount = 0;

        _orderRepositoryMock.Setup(x => x.GetOrdersWithFilterAsync(
            null, null, null, null, 1, 10, "CreatedDate", true))
            .ReturnsAsync((orders, totalCount));

        // Act
        var result = await _orderService.GetOrdersWithFilterAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Items.Count());
        Assert.AreEqual(0, result.TotalCount);
        Assert.AreEqual(1, result.PageNumber);
        Assert.AreEqual(10, result.PageSize);
    }

    [TestMethod]
    public async Task GetOrdersWithFilterAsync_MapsOrderDetailsCorrectly()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = CreateCustomer(customerId);
        typeof(Customer).GetProperty("CustomerCode")!.SetValue(customer, "CUST123");
        typeof(Customer).GetProperty("FirstName")!.SetValue(customer, "John");
        typeof(Customer).GetProperty("LastName")!.SetValue(customer, "Doe");
        
        var order = CreateOrder(1, "ORD99", customerId);
        typeof(Order).GetProperty("Customer")!.SetValue(order, customer);
        typeof(Order).GetProperty("Status")!.SetValue(order, OrderStatus.Completed);
        typeof(Order).GetProperty("TotalPrice")!.SetValue(order, 123.45m);
        typeof(Order).GetProperty("TotalItemsAmount")!.SetValue(order, 5);
        
        var orders = new List<Order> { order };
        var totalCount = 1;

        _orderRepositoryMock.Setup(x => x.GetOrdersWithFilterAsync(
            null, null, null, null, 1, 10, "CreatedDate", true))
            .ReturnsAsync((orders, totalCount));

        // Act
        var result = await _orderService.GetOrdersWithFilterAsync();

        // Assert
        Assert.IsNotNull(result);
        var orderResponse = result.Items.First();
        Assert.AreEqual(1, orderResponse.Id);
        Assert.AreEqual("ORD99", orderResponse.OrderCode);
        Assert.AreEqual("CUST123", orderResponse.CustomerCode);
        Assert.AreEqual("John Doe", orderResponse.CustomerName);
        Assert.AreEqual(OrderStatus.Completed, orderResponse.Status);
        Assert.AreEqual(123.45m, orderResponse.TotalPrice);
        Assert.AreEqual(5, orderResponse.TotalItemsAmount);
    }

    [TestMethod]
    public async Task GetOrdersWithFilterAsync_WithCustomerCodeFilter_PassesCorrectParameters()
    {
        // Arrange
        var orders = new List<Order>();
        var totalCount = 0;

        _orderRepositoryMock.Setup(x => x.GetOrdersWithFilterAsync(
            "CUST01", null, null, null, 1, 10, "CreatedDate", true))
            .ReturnsAsync((orders, totalCount));

        // Act
        await _orderService.GetOrdersWithFilterAsync(customerCode: "CUST01");

        // Assert
        _orderRepositoryMock.Verify(x => x.GetOrdersWithFilterAsync(
            "CUST01", null, null, null, 1, 10, "CreatedDate", true), Times.Once);
    }

    [TestMethod]
    public async Task GetOrdersWithFilterAsync_WithOrderCodeFilter_PassesCorrectParameters()
    {
        // Arrange
        var orders = new List<Order>();
        var totalCount = 0;

        _orderRepositoryMock.Setup(x => x.GetOrdersWithFilterAsync(
            null, "ORD01", null, null, 1, 10, "CreatedDate", true))
            .ReturnsAsync((orders, totalCount));

        // Act
        await _orderService.GetOrdersWithFilterAsync(orderCode: "ORD01");

        // Assert
        _orderRepositoryMock.Verify(x => x.GetOrdersWithFilterAsync(
            null, "ORD01", null, null, 1, 10, "CreatedDate", true), Times.Once);
    }

    [TestMethod]
    public async Task GetOrdersWithFilterAsync_WithCreatedDateFilter_PassesCorrectParameters()
    {
        // Arrange
        var orders = new List<Order>();
        var totalCount = 0;
        var createdDate = new DateTime(2024, 1, 1);

        _orderRepositoryMock.Setup(x => x.GetOrdersWithFilterAsync(
            null, null, createdDate, null, 1, 10, "CreatedDate", true))
            .ReturnsAsync((orders, totalCount));

        // Act
        await _orderService.GetOrdersWithFilterAsync(createdDate: createdDate);

        // Assert
        _orderRepositoryMock.Verify(x => x.GetOrdersWithFilterAsync(
            null, null, createdDate, null, 1, 10, "CreatedDate", true), Times.Once);
    }

    [TestMethod]
    public async Task GetOrdersWithFilterAsync_WithStatusFilter_PassesCorrectParameters()
    {
        // Arrange
        var orders = new List<Order>();
        var totalCount = 0;

        _orderRepositoryMock.Setup(x => x.GetOrdersWithFilterAsync(
            null, null, null, OrderStatus.Completed, 1, 10, "CreatedDate", true))
            .ReturnsAsync((orders, totalCount));

        // Act
        await _orderService.GetOrdersWithFilterAsync(status: OrderStatus.Completed);

        // Assert
        _orderRepositoryMock.Verify(x => x.GetOrdersWithFilterAsync(
            null, null, null, OrderStatus.Completed, 1, 10, "CreatedDate", true), Times.Once);
    }

    [TestMethod]
    public async Task GetOrdersWithFilterAsync_WithCustomPagination_PassesCorrectParameters()
    {
        // Arrange
        var orders = new List<Order>();
        var totalCount = 0;

        _orderRepositoryMock.Setup(x => x.GetOrdersWithFilterAsync(
            null, null, null, null, 5, 50, "CreatedDate", true))
            .ReturnsAsync((orders, totalCount));

        // Act
        await _orderService.GetOrdersWithFilterAsync(pageNumber: 5, pageSize: 50);

        // Assert
        _orderRepositoryMock.Verify(x => x.GetOrdersWithFilterAsync(
            null, null, null, null, 5, 50, "CreatedDate", true), Times.Once);
    }

    [TestMethod]
    public async Task GetOrdersWithFilterAsync_WithCustomSorting_PassesCorrectParameters()
    {
        // Arrange
        var orders = new List<Order>();
        var totalCount = 0;

        _orderRepositoryMock.Setup(x => x.GetOrdersWithFilterAsync(
            null, null, null, null, 1, 10, "OrderCode", false))
            .ReturnsAsync((orders, totalCount));

        // Act
        await _orderService.GetOrdersWithFilterAsync(sortBy: "OrderCode", sortDescending: false);

        // Assert
        _orderRepositoryMock.Verify(x => x.GetOrdersWithFilterAsync(
            null, null, null, null, 1, 10, "OrderCode", false), Times.Once);
    }

    [TestMethod]
    public async Task CreateOrderManuallyAsync_NullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        ArgumentNullException? exception = null;

        // Act
        try
        {
            await _orderService.CreateOrderManuallyAsync(null!);
        }
        catch (ArgumentNullException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual("request", exception!.ParamName);
    }

    [TestMethod]
    public async Task CreateOrderManuallyAsync_NullItems_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new PlaceOrderRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = "1234567890",
            Items = null!
        };
        InvalidOperationException? exception = null;

        // Act
        try
        {
            await _orderService.CreateOrderManuallyAsync(request);
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual("Order must contain at least one item", exception!.Message);
    }

    [TestMethod]
    public async Task CreateOrderManuallyAsync_EmptyItems_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new PlaceOrderRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = "1234567890",
            Items = new List<OrderItemRequest>()
        };
        InvalidOperationException? exception = null;

        // Act
        try
        {
            await _orderService.CreateOrderManuallyAsync(request);
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual("Order must contain at least one item", exception!.Message);
    }

    [TestMethod]
    public async Task CreateOrderManuallyAsync_ItemNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new PlaceOrderRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = "1234567890",
            Items = new List<OrderItemRequest>
            {
                new() { ItemId = 999, Quantity = 2 }
            }
        };

        var customer = CreateCustomer(Guid.NewGuid());
        _customerRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Customer>()))
            .ReturnsAsync(customer);
        _itemRepositoryMock.Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((Item?)null);

        InvalidOperationException? exception = null;

        // Act
        try
        {
            await _orderService.CreateOrderManuallyAsync(request);
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual("Item with ID 999 not found", exception!.Message);
    }

    [TestMethod]
    public async Task CreateOrderManuallyAsync_InactiveItem_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new PlaceOrderRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = "1234567890",
            Items = new List<OrderItemRequest>
            {
                new() { ItemId = 1, Quantity = 2 }
            }
        };

        var customer = CreateCustomer(Guid.NewGuid());
        var inactiveItem = CreateItem(1, "Inactive Item", 10.0m, false);

        _customerRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Customer>()))
            .ReturnsAsync(customer);
        _itemRepositoryMock.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(inactiveItem);

        InvalidOperationException? exception = null;

        // Act
        try
        {
            await _orderService.CreateOrderManuallyAsync(request);
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual("Item 'Inactive Item' is not available", exception!.Message);
    }

    [TestMethod]
    public async Task CreateOrderManuallyAsync_ValidRequest_CreatesCustomer()
    {
        // Arrange
        var request = new PlaceOrderRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = "1234567890",
            DeliveryAddress = "123 Main St",
            Items = new List<OrderItemRequest>
            {
                new() { ItemId = 1, Quantity = 2 }
            }
        };

        var customerId = Guid.NewGuid();
        var customer = CreateCustomer(customerId);
        var item = CreateItem(1, "Coffee", 5.0m);
        var order = CreateOrder(1, "ABC12", customerId);

        _customerRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Customer>()))
            .ReturnsAsync(customer);
        _itemRepositoryMock.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(item);
        _orderRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Order>()))
            .ReturnsAsync(order);
        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(order);

        // Act
        await _orderService.CreateOrderManuallyAsync(request);

        // Assert
        _customerRepositoryMock.Verify(x => x.CreateAsync(It.Is<Customer>(c =>
            c.FirstName == "John" &&
            c.LastName == "Doe" &&
            c.Email == "john@example.com" &&
            c.PhoneNumber == "1234567890" &&
            c.Address == "123 Main St")), Times.Once);
    }

    [TestMethod]
    public async Task CreateOrderManuallyAsync_ValidRequest_CreatesOrderWithCorrectData()
    {
        // Arrange
        var request = new PlaceOrderRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = "1234567890",
            DeliveryAddress = "456 Oak Ave",
            Notes = "Test notes",
            FulfillmentScope = OrderFulfillmentScope.OutHouse,
            OutHouseFulfillmentType = OutHouseFulfillmentType.Pickup,
            Items = new List<OrderItemRequest>
            {
                new() { ItemId = 1, Quantity = 2 }
            }
        };

        var customerId = Guid.NewGuid();
        var customer = CreateCustomer(customerId);
        var item = CreateItem(1, "Coffee", 5.0m);
        var order = CreateOrder(1, "ABC12", customerId);

        _customerRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Customer>()))
            .ReturnsAsync(customer);
        _itemRepositoryMock.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(item);
        _orderRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Order>()))
            .ReturnsAsync(order);
        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(order);

        // Act
        await _orderService.CreateOrderManuallyAsync(request);

        // Assert
        _orderRepositoryMock.Verify(x => x.CreateAsync(It.Is<Order>(o =>
            o.CustomerId == customerId &&
            o.FulfillmentScope == OrderFulfillmentScope.OutHouse &&
            o.OutHouseFulfillmentType == OutHouseFulfillmentType.Pickup)), Times.Once);
    }

    [TestMethod]
    public async Task CreateOrderManuallyAsync_ValidRequest_AddsOrderItems()
    {
        // Arrange
        var request = new PlaceOrderRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = "1234567890",
            Items = new List<OrderItemRequest>
            {
                new() { ItemId = 1, Quantity = 2 },
                new() { ItemId = 2, Quantity = 3 }
            }
        };

        var customerId = Guid.NewGuid();
        var customer = CreateCustomer(customerId);
        var item1 = CreateItem(1, "Coffee", 5.0m);
        var item2 = CreateItem(2, "Tea", 4.0m);
        var order = CreateOrder(1, "ABC12", customerId);

        _customerRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Customer>()))
            .ReturnsAsync(customer);
        _itemRepositoryMock.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(item1);
        _itemRepositoryMock.Setup(x => x.GetByIdAsync(2))
            .ReturnsAsync(item2);
        _orderRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Order>()))
            .ReturnsAsync(order);
        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(order);

        // Act
        await _orderService.CreateOrderManuallyAsync(request);

        // Assert
        _itemRepositoryMock.Verify(x => x.GetByIdAsync(1), Times.Once);
        _itemRepositoryMock.Verify(x => x.GetByIdAsync(2), Times.Once);
    }

    [TestMethod]
    public async Task CreateOrderManuallyAsync_ValidRequest_ReturnsOrderDetailResponse()
    {
        // Arrange
        var request = new PlaceOrderRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = "1234567890",
            Items = new List<OrderItemRequest>
            {
                new() { ItemId = 1, Quantity = 2 }
            }
        };

        var customerId = Guid.NewGuid();
        var customer = CreateCustomer(customerId);
        var item = CreateItem(1, "Coffee", 5.0m);
        var order = CreateOrder(1, "ABC12", customerId);

        _customerRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Customer>()))
            .ReturnsAsync(customer);
        _itemRepositoryMock.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(item);
        _orderRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Order>()))
            .ReturnsAsync(order);
        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(order);

        // Act
        var result = await _orderService.CreateOrderManuallyAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("TEST", result.OrderCode);
    }

    [TestMethod]
    public async Task CreateOrderManuallyAsync_ValidRequest_ReloadsOrderWithDetails()
    {
        // Arrange
        var request = new PlaceOrderRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = "1234567890",
            Items = new List<OrderItemRequest>
            {
                new() { ItemId = 1, Quantity = 2 }
            }
        };

        var customerId = Guid.NewGuid();
        var customer = CreateCustomer(customerId);
        var item = CreateItem(1, "Coffee", 5.0m);
        var order = CreateOrder(1, "ABC12", customerId);

        _customerRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Customer>()))
            .ReturnsAsync(customer);
        _itemRepositoryMock.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(item);
        _orderRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Order>()))
            .ReturnsAsync(order);
        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(order);

        // Act
        await _orderService.CreateOrderManuallyAsync(request);

        // Assert
        _orderRepositoryMock.Verify(x => x.GetByIdWithDetailsAsync(1), Times.Once);
    }

    [TestMethod]
    public async Task CreateOrderManuallyAsync_ValidRequest_PassesStorageServiceToMapper()
    {
        // Arrange
        var request = new PlaceOrderRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = "1234567890",
            Items = new List<OrderItemRequest>
            {
                new() { ItemId = 1, Quantity = 2 }
            }
        };

        var customerId = Guid.NewGuid();
        var customer = CreateCustomer(customerId);
        var item = CreateItem(1, "Coffee", 5.0m);
        var order = CreateOrder(1, "ABC12", customerId);

        _customerRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Customer>()))
            .ReturnsAsync(customer);
        _itemRepositoryMock.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(item);
        _orderRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Order>()))
            .ReturnsAsync(order);
        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(order);

        // Act
        await _orderService.CreateOrderManuallyAsync(request);

        // Assert
        _mapperMock.Verify(x => x.Map<OrderDetailResponse>(
            It.IsAny<Order>(),
            It.Is<Action<IMappingOperationOptions<object, OrderDetailResponse>>>(opts => opts != null)), Times.Once);
    }

    [TestMethod]
    public async Task CreateOrderManuallyAsync_WithOutHouseFulfillment_CreatesOrderWithOutHouseType()
    {
        // Arrange
        var request = new PlaceOrderRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = "1234567890",
            DeliveryAddress = "123 Delivery St",
            FulfillmentScope = OrderFulfillmentScope.OutHouse,
            OutHouseFulfillmentType = OutHouseFulfillmentType.Delivery,
            Items = new List<OrderItemRequest>
            {
                new() { ItemId = 1, Quantity = 2 }
            }
        };

        var customerId = Guid.NewGuid();
        var customer = CreateCustomer(customerId);
        var item = CreateItem(1, "Coffee", 5.0m);
        var order = CreateOrder(1, "ABC12", customerId);

        _customerRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Customer>()))
            .ReturnsAsync(customer);
        _itemRepositoryMock.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(item);
        _orderRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Order>()))
            .ReturnsAsync(order);
        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(order);

        // Act
        await _orderService.CreateOrderManuallyAsync(request);

        // Assert
        _orderRepositoryMock.Verify(x => x.CreateAsync(It.Is<Order>(o =>
            o.FulfillmentScope == OrderFulfillmentScope.OutHouse &&
            o.OutHouseFulfillmentType == OutHouseFulfillmentType.Delivery)), Times.Once);
    }
}
