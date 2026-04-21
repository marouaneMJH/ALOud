using ALOud.Data;
using ALOud.DTOs;
using ALOud.Services;
using ALOud.Services.External.Payment;
using ALOud.Services.External.Shipping;
using ALOud.Services.Infrastructure.Cache;
using ALOud.Services.Infrastructure.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ALOud.Services.Business.Jobs.Processors
{
    public class OrderProcessingJobProcessor : JobProcessor<OrderProcessingJob>
    {
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IShippingService _shippingService;
        private readonly ICacheService _cacheService;
        private readonly ALOudDbContext _dbContext;

        public override string JobType => "OrderProcessing";

        public OrderProcessingJobProcessor(
            ILogger<OrderProcessingJobProcessor> logger,
            IOrderService orderService,
            IPaymentService paymentService,
            IShippingService shippingService,
            ICacheService cacheService,
            ALOudDbContext dbContext)
            : base(logger)
        {
            _orderService = orderService;
            _paymentService = paymentService;
            _shippingService = shippingService;
            _cacheService = cacheService;
            _dbContext = dbContext;
        }

        public override async Task<JobResult> ProcessAsync(OrderProcessingJob job, CancellationToken cancellationToken = default)
        {
            Logger.LogInformation("Processing order job {JobId} for Order {OrderId}, Action: {Action}",
                job.Id, job.OrderId, job.Action);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                var order = await _dbContext.Orders
                    .Include(o => o.OrderItems)
                    .Include(o => o.Payments)
                    .Include(o => o.Shipments)
                    .FirstOrDefaultAsync(o => o.Id == job.OrderId, cancellationToken);

                if (order == null)
                {
                    Logger.LogWarning("Order {OrderId} not found for job {JobId}", job.OrderId, job.Id);
                    return Failure($"Order {job.OrderId} not found", stopwatch.Elapsed);
                }

                var message = job.Action.ToLower() switch
                {
                    "processpayment" => await ProcessPaymentAsync(order, job.Parameters, cancellationToken),
                    "createshipment" => await CreateShipmentAsync(order, job.Parameters, cancellationToken),
                    "sendnotification" => await SendNotificationAsync(order, job.Parameters, cancellationToken),
                    "updatestatus" => await UpdateOrderStatusAsync(order, job.Parameters, cancellationToken),
                    "validateorder" => await ValidateOrderAsync(order, job.Parameters, cancellationToken),
                    _ => $"Unknown action: {job.Action}"
                };

                stopwatch.Stop();

                if (message.StartsWith("Error:") || message.StartsWith("Unknown action:"))
                {
                    return Failure(message, stopwatch.Elapsed);
                }

                return Success(stopwatch.Elapsed, new Dictionary<string, object> { ["message"] = message });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Logger.LogError(ex, "Error processing order job {JobId} for Order {OrderId}, Action: {Action}",
                    job.Id, job.OrderId, job.Action);
                return Failure($"Error processing job: {ex.Message}", stopwatch.Elapsed);
            }
        }

        private async Task<string> ProcessPaymentAsync(Models.Order order, Dictionary<string, object> parameters, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Processing payment for Order {OrderId}", order.Id);

            if (order.PaymentStatus == "Paid")
            {
                return "Order already paid";
            }

            if (string.IsNullOrEmpty(order.PaymentIntentId))
            {
                return "Error: No payment intent ID found";
            }

            try
            {
                // Mock payment processing - in a real implementation this would call external payment service
                await Task.Delay(500, cancellationToken);
                
                // Update order status to paid
                order.PaymentStatus = "Paid";
                order.Status = "Paid";
                order.PaidAt = DateTime.UtcNow;
                order.UpdatedAt = DateTime.UtcNow;

                await _orderService.UpdateOrderStatusAsync(new UpdateOrderStatusDto
                {
                    OrderId = order.Id,
                    NewStatus = order.Status,
                    ChangeReason = "Payment processed successfully"
                });
                await _dbContext.SaveChangesAsync(cancellationToken);

                await _cacheService.RemoveAsync($"order:{order.Id}");

                Logger.LogInformation("Payment processed successfully for Order {OrderId}", order.Id);
                return "Payment processed successfully";
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error processing payment for Order {OrderId}", order.Id);
                return $"Error: {ex.Message}";
            }
        }

        private async Task<string> CreateShipmentAsync(Models.Order order, Dictionary<string, object> parameters, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Creating shipment for Order {OrderId}", order.Id);

            if (order.Status != "Paid" && order.Status != "Processing")
            {
                return $"Error: Order status '{order.Status}' is not valid for shipping";
            }

            try
            {
                // Mock shipment creation
                await Task.Delay(300, cancellationToken);
                
                var trackingNumber = $"TRK{DateTime.UtcNow:yyyyMMdd}{new Random().Next(100000, 999999)}";
                
                order.Status = "Processing";
                order.ProcessedAt = DateTime.UtcNow;
                order.TrackingNumber = trackingNumber;
                order.ShippingCarrier = "UPS";
                order.TrackingUrl = $"https://www.ups.com/track?tracknum={trackingNumber}";
                order.UpdatedAt = DateTime.UtcNow;

                await _orderService.UpdateOrderStatusAsync(new UpdateOrderStatusDto
                {
                    OrderId = order.Id,
                    NewStatus = order.Status,
                    ChangeReason = $"Shipment created with tracking number: {trackingNumber}"
                });
                await _dbContext.SaveChangesAsync(cancellationToken);

                await _cacheService.RemoveAsync($"order:{order.Id}");

                Logger.LogInformation("Shipment created successfully for Order {OrderId}, Tracking: {TrackingNumber}", 
                    order.Id, trackingNumber);
                
                return $"Shipment created with tracking number: {trackingNumber}";
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating shipment for Order {OrderId}", order.Id);
                return $"Error: {ex.Message}";
            }
        }

        private async Task<string> SendNotificationAsync(Models.Order order, Dictionary<string, object> parameters, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Sending notification for Order {OrderId}", order.Id);

            var notificationType = parameters.TryGetValue("Type", out var type) ? type.ToString() : "OrderUpdate";
            
            // Mock notification
            await Task.Delay(100, cancellationToken);
            
            Logger.LogInformation("Mock notification sent for Order {OrderId}, Type: {Type}", order.Id, notificationType);
            
            return $"Notification sent: {notificationType}";
        }

        private async Task<string> UpdateOrderStatusAsync(Models.Order order, Dictionary<string, object> parameters, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Updating status for Order {OrderId}", order.Id);

            if (!parameters.TryGetValue("Status", out var statusObj) || statusObj?.ToString() is not string newStatus)
            {
                return "Error: Status parameter is required";
            }

            var reason = parameters.TryGetValue("Reason", out var reasonObj) ? reasonObj?.ToString() : null;

            try
            {
                await _orderService.UpdateOrderStatusAsync(new UpdateOrderStatusDto
                {
                    OrderId = order.Id,
                    NewStatus = newStatus,
                    ChangeReason = reason ?? $"Status updated to {newStatus}"
                });
                await _cacheService.RemoveAsync($"order:{order.Id}");

                Logger.LogInformation("Order {OrderId} status updated to {Status}", order.Id, newStatus);
                return $"Status updated to {newStatus}";
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating status for Order {OrderId}", order.Id);
                return $"Error: {ex.Message}";
            }
        }

        private async Task<string> ValidateOrderAsync(Models.Order order, Dictionary<string, object> parameters, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Validating Order {OrderId}", order.Id);

            await Task.Delay(50, cancellationToken);

            var validationErrors = new List<string>();

            // Validate order items
            if (!order.OrderItems.Any())
            {
                validationErrors.Add("Order has no items");
            }

            // Validate addresses
            if (string.IsNullOrEmpty(order.ShippingAddressLine1))
            {
                validationErrors.Add("Shipping address is incomplete");
            }

            // Validate payment information
            if (order.TotalAmount <= 0)
            {
                validationErrors.Add("Order total must be greater than zero");
            }

            if (validationErrors.Any())
            {
                var errorMessage = string.Join("; ", validationErrors);
                Logger.LogWarning("Order {OrderId} validation failed: {Errors}", order.Id, errorMessage);
                return $"Error: Validation failed: {errorMessage}";
            }

            Logger.LogInformation("Order {OrderId} validation passed", order.Id);
            return "Order validation passed";
        }
    }
}