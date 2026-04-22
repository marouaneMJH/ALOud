using ALOud.Data;
using ALOud.DTOs;
using ALOud.Models;
using Microsoft.EntityFrameworkCore;

namespace ALOud.Services
{
    public class OrderService : IOrderService
    {
        private readonly ALOudDbContext _context;
        private readonly ICartService _cartService;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            ALOudDbContext context,
            ICartService cartService,
            ILogger<OrderService> logger)
        {
            _context = context;
            _cartService = cartService;
            _logger = logger;
        }

        #region Order Creation and Management

        /// <summary>
        /// Creates an order from a completed checkout
        /// </summary>
        public async Task<OrderDto> CreateOrderFromCheckoutAsync(Guid checkoutId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Get checkout with all required data
                var checkout = await _context.Checkouts
                    .Include(c => c.CheckoutAddresses)
                    .Include(c => c.StockReservations)
                    .ThenInclude(sr => sr.Perfume)
                    .ThenInclude(p => p.Brand)
                    .FirstOrDefaultAsync(c => c.Id == checkoutId);

                if (checkout == null)
                {
                    throw new InvalidOperationException($"Checkout {checkoutId} not found");
                }

                if (checkout.Status != "Completed")
                {
                    throw new InvalidOperationException($"Cannot create order from checkout with status {checkout.Status}");
                }

                // Check if order already exists for this checkout
                var existingOrder = await _context.Orders
                    .FirstOrDefaultAsync(o => o.CheckoutId == checkoutId);
                
                if (existingOrder != null)
                {
                    _logger.LogWarning("Order already exists for checkout {CheckoutId}. Returning existing order {OrderId}", 
                        checkoutId, existingOrder.Id);
                    return await MapToOrderDtoAsync(existingOrder);
                }

                // Get addresses
                var shippingAddress = checkout.CheckoutAddresses
                    .FirstOrDefault(a => a.AddressType == "Shipping");
                var billingAddress = checkout.CheckoutAddresses
                    .FirstOrDefault(a => a.AddressType == "Billing") ?? shippingAddress;

                if (shippingAddress == null || billingAddress == null)
                {
                    throw new InvalidOperationException("Shipping address not found in checkout");
                }

                // Create order
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    OrderNumber = GenerateOrderNumber(),
                    CheckoutId = checkoutId,
                    UserId = checkout.UserId,
                    CustomerEmail = checkout.Email,
                    Status = "Pending",
                    
                    // Financial information
                    SubtotalAmount = checkout.SubtotalAmount,
                    TaxAmount = checkout.TaxAmount,
                    ShippingAmount = checkout.ShippingAmount,
                    DiscountAmount = checkout.DiscountAmount,
                    TotalAmount = checkout.TotalAmount,
                    TaxRate = checkout.TaxRate,
                    
                    // Methods
                    ShippingMethod = checkout.ShippingMethod,
                    PaymentMethod = checkout.PaymentMethod,
                    PaymentIntentId = checkout.PaymentIntentId,
                    PaymentStatus = "Pending",
                    
                    // Shipping address
                    ShippingFirstName = shippingAddress.FirstName,
                    ShippingLastName = shippingAddress.LastName,
                    ShippingCompany = shippingAddress.Company ?? string.Empty,
                    ShippingAddressLine1 = shippingAddress.AddressLine1,
                    ShippingAddressLine2 = shippingAddress.AddressLine2,
                    ShippingCity = shippingAddress.City,
                    ShippingState = shippingAddress.State,
                    ShippingPostalCode = shippingAddress.PostalCode,
                    ShippingCountry = shippingAddress.Country,
                    ShippingPhoneNumber = shippingAddress.PhoneNumber,
                    
                    // Billing address
                    BillingFirstName = billingAddress.FirstName,
                    BillingLastName = billingAddress.LastName,
                    BillingCompany = billingAddress.Company ?? string.Empty,
                    BillingAddressLine1 = billingAddress.AddressLine1,
                    BillingAddressLine2 = billingAddress.AddressLine2,
                    BillingCity = billingAddress.City,
                    BillingState = billingAddress.State,
                    BillingPostalCode = billingAddress.PostalCode,
                    BillingCountry = billingAddress.Country,
                    BillingPhoneNumber = billingAddress.PhoneNumber,
                    
                    IsGuestOrder = checkout.IsGuestCheckout,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Orders.Add(order);

                // Create order items from stock reservations
                var orderItems = new List<OrderItem>();
                foreach (var reservation in checkout.StockReservations.Where(sr => sr.Status == "Reserved"))
                {
                    var orderItem = new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        OrderId = order.Id,
                        ProductId = reservation.PerfumeId,
                        ProductName = reservation.Perfume.Name,
                        BrandName = reservation.Perfume.Brand.Name,
                        ProductSku = null, // SKU not available in Perfume model
                        UnitPrice = reservation.UnitPrice,
                        Quantity = reservation.Quantity,
                        LineTotal = reservation.UnitPrice * reservation.Quantity,
                        Size = null, // Size not available in Perfume model
                        ProductDescription = reservation.Perfume.Description,
                        ProductImageUrl = reservation.Perfume.ImageUrl,
                        Status = "Ordered",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    orderItems.Add(orderItem);
                    _context.OrderItems.Add(orderItem);
                }

                // Create initial status history
                var statusHistory = new OrderStatusHistory
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    PreviousStatus = null,
                    NewStatus = "Pending",
                    ChangeReason = "Order created from checkout completion",
                    ChangeSource = "System",
                    Notes = $"Order created from checkout {checkoutId}",
                    CustomerNotified = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.OrderStatusHistory.Add(statusHistory);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Order {OrderId} created successfully from checkout {CheckoutId}", 
                    order.Id, checkoutId);

                return await MapToOrderDtoAsync(order);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating order from checkout {CheckoutId}", checkoutId);
                throw;
            }
        }

        /// <summary>
        /// Gets an order by ID
        /// </summary>
        public async Task<OrderDto?> GetOrderAsync(Guid orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            return order != null ? await MapToOrderDtoAsync(order) : null;
        }

        /// <summary>
        /// Gets an order by order number
        /// </summary>
        public async Task<OrderDto?> GetOrderByNumberAsync(string orderNumber)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

            return order != null ? await MapToOrderDtoAsync(order) : null;
        }

        /// <summary>
        /// Gets orders for a specific user
        /// </summary>
        public async Task<List<OrderSummaryDto>> GetUserOrdersAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            var skip = (page - 1) * pageSize;

            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .Select(o => new OrderSummaryDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,
                    TotalItems = o.OrderItems.Sum(oi => oi.Quantity),
                    CreatedAt = o.CreatedAt,
                    ShippedAt = o.ShippedAt,
                    DeliveredAt = o.DeliveredAt,
                    TrackingNumber = o.TrackingNumber,
                    IsCancellable = o.Status == "Pending" || o.Status == "Paid" || o.Status == "Processing",
                    IsRefundable = o.Status == "Paid" || o.Status == "Processing" || o.Status == "Shipped" || o.Status == "Delivered"
                })
                .ToListAsync();

            return orders;
        }

        /// <summary>
        /// Gets full orders for a specific user including items (used for UI previews)
        /// </summary>
        public async Task<List<OrderDto>> GetFullUserOrdersAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            var skip = (page - 1) * pageSize;

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            var dtos = new List<OrderDto>();
            foreach (var order in orders)
            {
                dtos.Add(await MapToOrderDtoAsync(order));
            }

            return dtos;
        }


        /// <summary>
        /// Searches orders based on criteria
        /// </summary>
        public async Task<List<OrderSummaryDto>> SearchOrdersAsync(OrderSearchDto searchDto)
        {
            var query = _context.Orders.AsQueryable();

            if (!string.IsNullOrEmpty(searchDto.OrderNumber))
            {
                query = query.Where(o => o.OrderNumber.Contains(searchDto.OrderNumber));
            }

            if (!string.IsNullOrEmpty(searchDto.CustomerEmail))
            {
                query = query.Where(o => o.CustomerEmail.Contains(searchDto.CustomerEmail));
            }

            if (!string.IsNullOrEmpty(searchDto.Status))
            {
                query = query.Where(o => o.Status == searchDto.Status);
            }

            if (searchDto.StartDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt >= searchDto.StartDate.Value);
            }

            if (searchDto.EndDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt <= searchDto.EndDate.Value);
            }

            if (searchDto.MinAmount.HasValue)
            {
                query = query.Where(o => o.TotalAmount >= searchDto.MinAmount.Value);
            }

            if (searchDto.MaxAmount.HasValue)
            {
                query = query.Where(o => o.TotalAmount <= searchDto.MaxAmount.Value);
            }

            // Apply sorting
            query = searchDto.SortBy.ToLower() switch
            {
                "ordernumber" => searchDto.SortDirection.ToUpper() == "ASC" 
                    ? query.OrderBy(o => o.OrderNumber) 
                    : query.OrderByDescending(o => o.OrderNumber),
                "customeremail" => searchDto.SortDirection.ToUpper() == "ASC" 
                    ? query.OrderBy(o => o.CustomerEmail) 
                    : query.OrderByDescending(o => o.CustomerEmail),
                "status" => searchDto.SortDirection.ToUpper() == "ASC" 
                    ? query.OrderBy(o => o.Status) 
                    : query.OrderByDescending(o => o.Status),
                "totalamount" => searchDto.SortDirection.ToUpper() == "ASC" 
                    ? query.OrderBy(o => o.TotalAmount) 
                    : query.OrderByDescending(o => o.TotalAmount),
                _ => searchDto.SortDirection.ToUpper() == "ASC" 
                    ? query.OrderBy(o => o.CreatedAt) 
                    : query.OrderByDescending(o => o.CreatedAt)
            };

            var skip = (searchDto.Page - 1) * searchDto.PageSize;

            var orders = await query
                .Skip(skip)
                .Take(searchDto.PageSize)
                .Select(o => new OrderSummaryDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,
                    TotalItems = o.OrderItems.Sum(oi => oi.Quantity),
                    CreatedAt = o.CreatedAt,
                    ShippedAt = o.ShippedAt,
                    DeliveredAt = o.DeliveredAt,
                    TrackingNumber = o.TrackingNumber,
                    IsCancellable = o.Status == "Pending" || o.Status == "Paid" || o.Status == "Processing",
                    IsRefundable = o.Status == "Paid" || o.Status == "Processing" || o.Status == "Shipped" || o.Status == "Delivered"
                })
                .ToListAsync();

            return orders;
        }

        #endregion

        #region Order Status Management

        /// <summary>
        /// Updates order status with history tracking
        /// </summary>
        public async Task<bool> UpdateOrderStatusAsync(UpdateOrderStatusDto dto, Guid? updatedByUserId = null)
        {
            try
            {
                var order = await _context.Orders.FindAsync(dto.OrderId);
                if (order == null) return false;

                var previousStatus = order.Status;
                
                if (!await IsValidOrderStatusTransitionAsync(previousStatus, dto.NewStatus))
                {
                    _logger.LogWarning("Invalid status transition from {PreviousStatus} to {NewStatus} for order {OrderId}", 
                        previousStatus, dto.NewStatus, dto.OrderId);
                    return false;
                }

                // Update order status and timestamp
                order.Status = dto.NewStatus;
                order.UpdatedAt = DateTime.UtcNow;

                // Set specific timestamp based on status
                switch (dto.NewStatus.ToLower())
                {
                    case "paid":
                        order.PaidAt = DateTime.UtcNow;
                        order.PaymentStatus = "Paid";
                        break;
                    case "processing":
                        order.ProcessedAt = DateTime.UtcNow;
                        break;
                    case "shipped":
                        order.ShippedAt = DateTime.UtcNow;
                        break;
                    case "delivered":
                        order.DeliveredAt = DateTime.UtcNow;
                        break;
                    case "cancelled":
                        order.CancelledAt = DateTime.UtcNow;
                        break;
                }

                // Create status history record
                var statusHistory = new OrderStatusHistory
                {
                    Id = Guid.NewGuid(),
                    OrderId = dto.OrderId,
                    PreviousStatus = previousStatus,
                    NewStatus = dto.NewStatus,
                    ChangeReason = dto.ChangeReason,
                    Notes = dto.Notes,
                    ExternalReference = dto.ExternalReference,
                    ChangedByUserId = updatedByUserId,
                    ChangeSource = updatedByUserId.HasValue ? "Admin" : "System",
                    CustomerNotified = dto.NotifyCustomer,
                    NotificationSentAt = dto.NotifyCustomer ? DateTime.UtcNow : null,
                    NotificationMethod = dto.NotifyCustomer ? "Email" : null,
                    CreatedAt = DateTime.UtcNow
                };

                _context.OrderStatusHistory.Add(statusHistory);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Order {OrderId} status updated from {PreviousStatus} to {NewStatus}", 
                    dto.OrderId, previousStatus, dto.NewStatus);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status for order {OrderId}", dto.OrderId);
                return false;
            }
        }

        /// <summary>
        /// Cancels an order
        /// </summary>
        public async Task<bool> CancelOrderAsync(CancelOrderDto dto, Guid? cancelledByUserId = null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == dto.OrderId);

                if (order == null) return false;

                if (!await CanCancelOrderAsync(dto.OrderId))
                {
                    _logger.LogWarning("Order {OrderId} cannot be cancelled in current status {Status}", 
                        dto.OrderId, order.Status);
                    return false;
                }


                var previousStatus = order.Status;

                // Update order status
                var updateStatusDto = new UpdateOrderStatusDto
                {
                    OrderId = dto.OrderId,
                    NewStatus = "Cancelled",
                    ChangeReason = dto.Reason,
                    Notes = dto.CustomerNote,
                    NotifyCustomer = dto.NotifyCustomer
                };

                var statusUpdated = await UpdateOrderStatusAsync(updateStatusDto, cancelledByUserId);
                if (!statusUpdated)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                // Restore stock for cancelled items
                // Only restore stock if the order progressed past Pending, meaning ProcessStockDeduction was likely executed
                if (order.Status != "Delivered" && previousStatus != "Pending") 
                {
                    await RestoreStockAsync(dto.OrderId, "Order cancelled");
                }

                // Mark all order items as cancelled
                foreach (var item in order.OrderItems)
                {
                    item.Status = "Cancelled";
                    item.CancelledQuantity = item.Quantity;
                    item.CancelledAt = DateTime.UtcNow;
                    item.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Order {OrderId} cancelled successfully", dto.OrderId);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error cancelling order {OrderId}", dto.OrderId);
                return false;
            }
        }

        /// <summary>
        /// Gets order tracking information
        /// </summary>
        public async Task<OrderTrackingDto?> GetOrderTrackingAsync(Guid orderId)
        {
            var order = await _context.Orders
                .Include(o => o.StatusHistory)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return null;

            var trackingDto = new OrderTrackingDto
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                Status = order.Status,
                TrackingNumber = order.TrackingNumber,
                ShippingCarrier = order.ShippingCarrier,
                TrackingUrl = order.TrackingUrl,
                ShippedAt = order.ShippedAt,
                DeliveredAt = order.DeliveredAt,
                StatusHistory = order.StatusHistory
                    .OrderBy(sh => sh.CreatedAt)
                    .Select(sh => new OrderStatusHistoryDto
                    {
                        Id = sh.Id,
                        PreviousStatus = sh.PreviousStatus,
                        NewStatus = sh.NewStatus,
                        ChangeReason = sh.ChangeReason,
                        ChangeSource = sh.ChangeSource,
                        Notes = sh.Notes,
                        CustomerNotified = sh.CustomerNotified,
                        CreatedAt = sh.CreatedAt
                    })
                    .ToList()
            };

            // Estimate delivery date if shipped but not delivered
            if (order.ShippedAt.HasValue && !order.DeliveredAt.HasValue)
            {
                trackingDto.EstimatedDeliveryDate = order.ShippedAt.Value.AddDays(
                    order.ShippingMethod?.ToLower() switch
                    {
                        "express" => 2,
                        "overnight" => 1,
                        _ => 5 // Standard shipping
                    });
            }

            return trackingDto;
        }

        /// <summary>
        /// Gets order status history
        /// </summary>
        public async Task<List<OrderStatusHistoryDto>> GetOrderStatusHistoryAsync(Guid orderId)
        {
            var history = await _context.OrderStatusHistory
                .Where(osh => osh.OrderId == orderId)
                .OrderBy(osh => osh.CreatedAt)
                .Select(osh => new OrderStatusHistoryDto
                {
                    Id = osh.Id,
                    PreviousStatus = osh.PreviousStatus,
                    NewStatus = osh.NewStatus,
                    ChangeReason = osh.ChangeReason,
                    ChangeSource = osh.ChangeSource,
                    Notes = osh.Notes,
                    CustomerNotified = osh.CustomerNotified,
                    CreatedAt = osh.CreatedAt
                })
                .ToListAsync();

            return history;
        }

        #endregion

        #region Order Fulfillment

        /// <summary>
        /// Marks order as paid
        /// </summary>
        public async Task<bool> MarkOrderAsPaidAsync(Guid orderId, string paymentIntentId, decimal paidAmount)
        {
            var updateDto = new UpdateOrderStatusDto
            {
                OrderId = orderId,
                NewStatus = "Paid",
                ChangeReason = "Payment confirmed",
                ExternalReference = paymentIntentId,
                Notes = $"Payment of {paidAmount:C} confirmed",
                NotifyCustomer = true
            };

            return await UpdateOrderStatusAsync(updateDto);
        }

        /// <summary>
        /// Marks order as processing
        /// </summary>
        public async Task<bool> MarkOrderAsProcessingAsync(Guid orderId, string? processingNotes = null)
        {
            var updateDto = new UpdateOrderStatusDto
            {
                OrderId = orderId,
                NewStatus = "Processing",
                ChangeReason = "Order is being prepared for shipment",
                Notes = processingNotes,
                NotifyCustomer = true
            };

            return await UpdateOrderStatusAsync(updateDto);
        }

        /// <summary>
        /// Marks order as shipped
        /// </summary>
        public async Task<bool> MarkOrderAsShippedAsync(Guid orderId, string trackingNumber, string carrier, string? shippingMethod = null)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null) return false;

                // Update tracking information
                order.TrackingNumber = trackingNumber;
                order.ShippingCarrier = carrier;
                if (!string.IsNullOrEmpty(shippingMethod))
                {
                    order.ShippingMethod = shippingMethod;
                }

                // Generate tracking URL based on carrier
                order.TrackingUrl = GenerateTrackingUrl(carrier, trackingNumber);

                var updateDto = new UpdateOrderStatusDto
                {
                    OrderId = orderId,
                    NewStatus = "Shipped",
                    ChangeReason = "Order shipped",
                    ExternalReference = trackingNumber,
                    Notes = $"Shipped via {carrier} - Tracking: {trackingNumber}",
                    NotifyCustomer = true
                };

                // Cascade shipped status to all ordered items
                var orderItems = await _context.OrderItems.Where(oi => oi.OrderId == orderId && oi.Status != "Cancelled").ToListAsync();
                foreach (var oi in orderItems)
                {
                    oi.Status = "Shipped";
                    oi.ShippedQuantity = oi.Quantity - oi.CancelledQuantity;
                    oi.UpdatedAt = DateTime.UtcNow;
                }
                
                return await UpdateOrderStatusAsync(updateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking order {OrderId} as shipped", orderId);
                return false;
            }
        }

        /// <summary>
        /// Marks order as delivered
        /// </summary>
        public async Task<bool> MarkOrderAsDeliveredAsync(Guid orderId, DateTime? deliveredAt = null, string? deliveryNotes = null)
        {
            try
            {
                var updateDto = new UpdateOrderStatusDto
                {
                    OrderId = orderId,
                    NewStatus = "Delivered",
                    ChangeReason = "Order delivered successfully",
                    Notes = deliveryNotes ?? "Package delivered",
                    NotifyCustomer = true
                };

                var success = await UpdateOrderStatusAsync(updateDto);

                if (success && deliveredAt.HasValue)
                {
                    var order = await _context.Orders.FindAsync(orderId);
                    if (order != null)
                    {
                        order.DeliveredAt = deliveredAt.Value;
                        
                        var orderItems = await _context.OrderItems.Where(oi => oi.OrderId == orderId && oi.Status != "Cancelled").ToListAsync();
                        foreach (var oi in orderItems)
                        {
                            oi.Status = "Delivered";
                            oi.DeliveredQuantity = oi.Quantity - oi.CancelledQuantity;
                            oi.UpdatedAt = DateTime.UtcNow;
                        }

                        await _context.SaveChangesAsync();
                    }
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking order {OrderId} as delivered", orderId);
                return false;
            }
        }

        #endregion

        #region Order Modifications

        /// <summary>
        /// Updates shipping address for an order
        /// </summary>
        public async Task<bool> UpdateShippingAddressAsync(Guid orderId, CheckoutAddressDto shippingAddress)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null) return false;

                // Only allow address changes for pending or paid orders
                if (order.Status != "Pending" && order.Status != "Paid")
                {
                    _logger.LogWarning("Cannot update shipping address for order {OrderId} with status {Status}", 
                        orderId, order.Status);
                    return false;
                }

                // Update shipping address
                order.ShippingFirstName = shippingAddress.FirstName;
                order.ShippingLastName = shippingAddress.LastName;
                order.ShippingCompany = shippingAddress.Company;
                order.ShippingAddressLine1 = shippingAddress.AddressLine1;
                order.ShippingAddressLine2 = shippingAddress.AddressLine2;
                order.ShippingCity = shippingAddress.City;
                order.ShippingState = shippingAddress.State;
                order.ShippingPostalCode = shippingAddress.PostalCode;
                order.ShippingCountry = shippingAddress.Country;
                order.ShippingPhoneNumber = shippingAddress.PhoneNumber;
                order.UpdatedAt = DateTime.UtcNow;

                // Add status history
                var statusHistory = new OrderStatusHistory
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    PreviousStatus = order.Status,
                    NewStatus = order.Status,
                    ChangeReason = "Shipping address updated",
                    Notes = $"Shipping address updated to: {order.FullShippingAddress}",
                    ChangeSource = "Admin",
                    CustomerNotified = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.OrderStatusHistory.Add(statusHistory);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Shipping address updated for order {OrderId}", orderId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating shipping address for order {OrderId}", orderId);
                return false;
            }
        }

        /// <summary>
        /// Adds a note to an order
        /// </summary>
        public async Task<bool> AddOrderNoteAsync(Guid orderId, string note, bool isInternal = false)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null) return false;

                // Update the appropriate notes field
                if (isInternal)
                {
                    order.InternalNotes = string.IsNullOrEmpty(order.InternalNotes) 
                        ? note 
                        : $"{order.InternalNotes}\n{DateTime.UtcNow:yyyy-MM-dd HH:mm}: {note}";
                }
                else
                {
                    order.CustomerNotes = string.IsNullOrEmpty(order.CustomerNotes) 
                        ? note 
                        : $"{order.CustomerNotes}\n{DateTime.UtcNow:yyyy-MM-dd HH:mm}: {note}";
                }

                order.UpdatedAt = DateTime.UtcNow;

                // Add status history
                var statusHistory = new OrderStatusHistory
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    PreviousStatus = order.Status,
                    NewStatus = order.Status,
                    ChangeReason = isInternal ? "Internal note added" : "Customer note added",
                    Notes = note,
                    ChangeSource = "Admin",
                    CustomerNotified = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.OrderStatusHistory.Add(statusHistory);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding note to order {OrderId}", orderId);
                return false;
            }
        }

        #endregion

        #region Business Analytics

        /// <summary>
        /// Gets order statistics
        /// </summary>
        public async Task<OrderStatisticsDto> GetOrderStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Orders.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(o => o.CreatedAt >= startDate.Value);
            
            if (endDate.HasValue)
                query = query.Where(o => o.CreatedAt <= endDate.Value);

            var orders = await query.ToListAsync();

            var stats = new OrderStatisticsDto
            {
                TotalOrders = orders.Count,
                TotalRevenue = orders.Where(o => o.Status != "Cancelled").Sum(o => o.TotalAmount),
                PendingOrders = orders.Count(o => o.Status == "Pending"),
                ProcessingOrders = orders.Count(o => o.Status == "Processing"),
                ShippedOrders = orders.Count(o => o.Status == "Shipped"),
                DeliveredOrders = orders.Count(o => o.Status == "Delivered"),
                CancelledOrders = orders.Count(o => o.Status == "Cancelled"),
                AverageOrderValue = orders.Where(o => o.Status != "Cancelled").Any() 
                    ? orders.Where(o => o.Status != "Cancelled").Average(o => o.TotalAmount) 
                    : 0
            };

            // Orders by status
            stats.OrdersByStatus = orders
                .GroupBy(o => o.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            // Revenue by month
            stats.RevenueByMonth = orders
                .Where(o => o.Status != "Cancelled")
                .GroupBy(o => o.CreatedAt.ToString("yyyy-MM"))
                .ToDictionary(g => g.Key, g => g.Sum(o => o.TotalAmount));

            return stats;
        }

        /// <summary>
        /// Gets count of pending orders
        /// </summary>
        public async Task<int> GetPendingOrdersCountAsync()
        {
            return await _context.Orders
                .CountAsync(o => o.Status == "Pending" || o.Status == "Paid");
        }

        /// <summary>
        /// Gets total revenue
        /// </summary>
        public async Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Orders
                .Where(o => o.Status != "Cancelled");

            if (startDate.HasValue)
                query = query.Where(o => o.CreatedAt >= startDate.Value);
            
            if (endDate.HasValue)
                query = query.Where(o => o.CreatedAt <= endDate.Value);

            return await query.SumAsync(o => o.TotalAmount);
        }

        #endregion

        #region Customer Service Helpers

        /// <summary>
        /// Gets orders by email address
        /// </summary>
        public async Task<List<OrderSummaryDto>> GetOrdersByEmailAsync(string email)
        {
            var orders = await _context.Orders
                .Where(o => o.CustomerEmail.ToLower() == email.ToLower())
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderSummaryDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,
                    TotalItems = o.OrderItems.Sum(oi => oi.Quantity),
                    CreatedAt = o.CreatedAt,
                    ShippedAt = o.ShippedAt,
                    DeliveredAt = o.DeliveredAt,
                    TrackingNumber = o.TrackingNumber,
                    IsCancellable = o.Status == "Pending" || o.Status == "Paid" || o.Status == "Processing",
                    IsRefundable = o.Status == "Paid" || o.Status == "Processing" || o.Status == "Shipped" || o.Status == "Delivered"
                })
                .ToListAsync();

            return orders;
        }

        /// <summary>
        /// Checks if an order can be cancelled
        /// </summary>
        public async Task<bool> CanCancelOrderAsync(Guid orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            return order.Status is "Pending" or "Paid" or "Processing";
        }

        /// <summary>
        /// Checks if an order can be refunded
        /// </summary>
        public async Task<bool> CanRefundOrderAsync(Guid orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            return order.Status is "Paid" or "Processing" or "Shipped" or "Delivered";
        }

        /// <summary>
        /// Gets refundable amount for an order
        /// </summary>
        public async Task<decimal> GetRefundableAmountAsync(Guid orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return 0;

            var totalPaid = order.Payments
                .Where(p => p.Status == "Succeeded")
                .Sum(p => p.Amount);

            var totalRefunded = order.Payments
                .Sum(p => p.RefundedAmount);

            return totalPaid - totalRefunded;
        }

        #endregion

        #region Integration Helpers

        /// <summary>
        /// Processes stock deduction for an order
        /// </summary>
        public async Task<bool> ProcessStockDeductionAsync(Guid orderId)
        {
            try
            {
                var orderItems = await _context.OrderItems
                    .Include(oi => oi.Product)
                    .Where(oi => oi.OrderId == orderId)
                    .ToListAsync();

                var deductedItems = new List<OrderItem>();

                foreach (var item in orderItems)
                {
                    var product = item.Product;
                    if (product.StockQuantity >= item.Quantity)
                    {
                        product.StockQuantity -= item.Quantity;
                        deductedItems.Add(item);
                        
                        _logger.LogInformation("Deducted {Quantity} units of product {ProductId} for order {OrderId}", 
                            item.Quantity, item.ProductId, orderId);
                    }
                    else
                    {
                        _logger.LogWarning("Insufficient stock for product {ProductId}. Required: {Required}, Available: {Available}", 
                            item.ProductId, item.Quantity, product.StockQuantity);

                        // Rollback in-memory state changes for previously processed items
                        foreach (var deducted in deductedItems)
                        {
                            deducted.Product.StockQuantity += deducted.Quantity;
                        }

                        return false;
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing stock deduction for order {OrderId}", orderId);
                return false;
            }
        }

        /// <summary>
        /// Restores stock for a cancelled or refunded order
        /// </summary>
        public async Task<bool> RestoreStockAsync(Guid orderId, string reason)
        {
            try
            {
                var orderItems = await _context.OrderItems
                    .Include(oi => oi.Product)
                    .Where(oi => oi.OrderId == orderId)
                    .ToListAsync();

                foreach (var item in orderItems)
                {
                    var product = item.Product;
                    product.StockQuantity += item.Quantity;
                    
                    _logger.LogInformation("Restored {Quantity} units of product {ProductId} for order {OrderId}. Reason: {Reason}", 
                        item.Quantity, item.ProductId, orderId, reason);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring stock for order {OrderId}", orderId);
                return false;
            }
        }

        /// <summary>
        /// Gets products with low stock based on recent orders
        /// </summary>
        public async Task<List<Guid>> GetLowStockProductsFromOrdersAsync()
        {
            var lowStockProducts = await _context.Perfumes
                .Where(p => p.StockQuantity <= 10) // Consider products with 10 or fewer units as low stock
                .Select(p => p.Id)
                .ToListAsync();

            return lowStockProducts;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Generates a unique order number
        /// </summary>
        public string GenerateOrderNumber()
        {
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var random = Guid.NewGuid().ToString("N")[..8].ToUpper();
            return $"ALO-{date}-{random}";
        }

        /// <summary>
        /// Validates order ownership
        /// </summary>
        public async Task<bool> ValidateOrderOwnershipAsync(Guid orderId, Guid userId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            return order?.UserId == userId;
        }

        /// <summary>
        /// Validates order status transition
        /// </summary>
        public async Task<bool> IsValidOrderStatusTransitionAsync(string currentStatus, string newStatus)
        {
            // Define valid status transitions
            var validTransitions = new Dictionary<string, List<string>>
            {
                ["Pending"] = new() { "Paid", "Cancelled" },
                ["Paid"] = new() { "Processing", "Cancelled" },
                ["Processing"] = new() { "Shipped", "Cancelled" },
                ["Shipped"] = new() { "Delivered", "Cancelled" }, // Can still cancel if not delivered
                ["Delivered"] = new() { "Refunded" }, // Only refund possible after delivery
                ["Cancelled"] = new() { }, // No transitions from cancelled
                ["Refunded"] = new() { } // No transitions from refunded
            };

            return await Task.FromResult(
                validTransitions.Keys.Any(k => string.Equals(k, currentStatus, StringComparison.OrdinalIgnoreCase)) && 
                validTransitions.First(kvp => string.Equals(kvp.Key, currentStatus, StringComparison.OrdinalIgnoreCase)).Value.Any(v => string.Equals(v, newStatus, StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// Maps Order entity to OrderDto
        /// </summary>
        private async Task<OrderDto> MapToOrderDtoAsync(Order order)
        {
            var orderItems = await _context.OrderItems
                .Where(oi => oi.OrderId == order.Id)
                .Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.ProductName,
                    BrandName = oi.BrandName,
                    ProductSku = oi.ProductSku,
                    UnitPrice = oi.UnitPrice,
                    Quantity = oi.Quantity,
                    LineTotal = oi.LineTotal,
                    Size = oi.Size,
                    ProductImageUrl = oi.ProductImageUrl,
                    Status = oi.Status,
                    ShippedQuantity = oi.ShippedQuantity,
                    DeliveredQuantity = oi.DeliveredQuantity,
                    CancelledQuantity = oi.CancelledQuantity
                })
                .ToListAsync();

            return new OrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                UserId = order.UserId,
                CustomerEmail = order.CustomerEmail,
                Status = order.Status,
                IsGuestOrder = order.IsGuestOrder,
                SubtotalAmount = order.SubtotalAmount,
                TaxAmount = order.TaxAmount,
                ShippingAmount = order.ShippingAmount,
                DiscountAmount = order.DiscountAmount,
                TotalAmount = order.TotalAmount,
                TaxRate = order.TaxRate,
                ShippingMethod = order.ShippingMethod,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                ShippingFirstName = order.ShippingFirstName,
                ShippingLastName = order.ShippingLastName,
                ShippingCompany = order.ShippingCompany,
                ShippingAddressLine1 = order.ShippingAddressLine1,
                ShippingAddressLine2 = order.ShippingAddressLine2,
                ShippingCity = order.ShippingCity,
                ShippingState = order.ShippingState,
                ShippingPostalCode = order.ShippingPostalCode,
                ShippingCountry = order.ShippingCountry,
                ShippingPhoneNumber = order.ShippingPhoneNumber,
                BillingFirstName = order.BillingFirstName,
                BillingLastName = order.BillingLastName,
                BillingAddressLine1 = order.BillingAddressLine1,
                BillingAddressLine2 = order.BillingAddressLine2,
                BillingCity = order.BillingCity,
                BillingState = order.BillingState,
                BillingPostalCode = order.BillingPostalCode,
                BillingCountry = order.BillingCountry,
                BillingPhoneNumber = order.BillingPhoneNumber,
                TrackingNumber = order.TrackingNumber,
                ShippingCarrier = order.ShippingCarrier,
                TrackingUrl = order.TrackingUrl,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                PaidAt = order.PaidAt,
                ProcessedAt = order.ProcessedAt,
                ShippedAt = order.ShippedAt,
                DeliveredAt = order.DeliveredAt,
                CancelledAt = order.CancelledAt,
                Items = orderItems
            };
        }

        /// <summary>
        /// Generates tracking URL based on carrier
        /// </summary>
        private static string GenerateTrackingUrl(string carrier, string trackingNumber)
        {
            return carrier.ToLower() switch
            {
                "ups" => $"https://www.ups.com/track?tracknum={trackingNumber}",
                "fedex" => $"https://www.fedex.com/apps/fedextrack/?tracknumbers={trackingNumber}",
                "dhl" => $"https://www.dhl.com/en/express/tracking.html?AWB={trackingNumber}",
                "usps" => $"https://tools.usps.com/go/TrackConfirmAction?qtc_tLabels1={trackingNumber}",
                _ => $"https://www.google.com/search?q=track+package+{trackingNumber}"
            };
        }

        #endregion
    }
}