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
    public class StatusSyncJobProcessor : JobProcessor<StatusSyncJob>
    {
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IShippingService _shippingService;
        private readonly ICacheService _cacheService;
        private readonly ALOudDbContext _dbContext;

        public override string JobType => "StatusSync";

        public StatusSyncJobProcessor(
            ILogger<StatusSyncJobProcessor> logger,
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

        public override async Task<JobResult> ProcessAsync(StatusSyncJob job, CancellationToken cancellationToken = default)
        {
            Logger.LogInformation("Processing status sync job {JobId} for Order {OrderId}, Type: {SyncType}",
                job.Id, job.OrderId, job.SyncType);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                var message = job.SyncType.ToLower() switch
                {
                    "paymentstatus" => await SyncPaymentStatusAsync(job, cancellationToken),
                    "shipmenttracking" => await SyncShipmentTrackingAsync(job, cancellationToken),
                    "orderstatus" => await SyncOrderStatusAsync(job, cancellationToken),
                    _ => $"Unknown sync type: {job.SyncType}"
                };

                stopwatch.Stop();

                if (message.StartsWith("Error:") || message.StartsWith("Unknown sync type:"))
                {
                    return Failure(message, stopwatch.Elapsed);
                }

                return Success(stopwatch.Elapsed, new Dictionary<string, object> { ["message"] = message });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Logger.LogError(ex, "Error processing status sync job {JobId} for Order {OrderId}, Type: {SyncType}",
                    job.Id, job.OrderId, job.SyncType);
                return Failure($"Error processing sync job: {ex.Message}", stopwatch.Elapsed);
            }
        }

        private async Task<string> SyncPaymentStatusAsync(StatusSyncJob job, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Syncing payment status for Order {OrderId}, PaymentId: {EntityId}", 
                job.OrderId, job.EntityId);

            // Mock payment status sync
            await Task.Delay(200, cancellationToken);

            var payment = await _dbContext.OrderPayments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id.ToString() == job.EntityId, cancellationToken);

            if (payment == null)
            {
                return $"Error: Payment {job.EntityId} not found";
            }

            // Simulate status sync from external provider
            var mockStatuses = new[] { "Succeeded", "Processing", "Failed" };
            var newStatus = mockStatuses[new Random().Next(mockStatuses.Length)];
            
            if (payment.Status != newStatus)
            {
                payment.Status = newStatus;
                payment.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync(cancellationToken);
                
                await _cacheService.RemoveAsync($"order:{job.OrderId}");
                
                Logger.LogInformation("Payment status synced for Payment {PaymentId}: {Status}", 
                    payment.Id, newStatus);
            }

            return $"Payment status synced: {newStatus}";
        }

        private async Task<string> SyncShipmentTrackingAsync(StatusSyncJob job, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Syncing shipment tracking for Order {OrderId}, ShipmentId: {EntityId}", 
                job.OrderId, job.EntityId);

            // Mock shipment tracking sync
            await Task.Delay(300, cancellationToken);

            var shipment = await _dbContext.OrderShipments
                .Include(s => s.Order)
                .FirstOrDefaultAsync(s => s.Id.ToString() == job.EntityId, cancellationToken);

            if (shipment == null)
            {
                return $"Error: Shipment {job.EntityId} not found";
            }

            // Simulate tracking update from shipping provider
            var mockStatuses = new[] { "Prepared", "Shipped", "In_Transit", "Out_For_Delivery", "Delivered" };
            var newStatus = mockStatuses[new Random().Next(mockStatuses.Length)];
            
            if (shipment.Status != newStatus)
            {
                shipment.Status = newStatus;
                shipment.UpdatedAt = DateTime.UtcNow;
                
                if (newStatus == "Delivered")
                {
                    shipment.DeliveredAt = DateTime.UtcNow;
                }
                
                await _dbContext.SaveChangesAsync(cancellationToken);
                
                await _cacheService.RemoveAsync($"order:{job.OrderId}");
                
                Logger.LogInformation("Shipment tracking synced for Shipment {ShipmentId}: {Status}", 
                    shipment.Id, newStatus);
            }

            return $"Shipment tracking synced: {newStatus}";
        }

        private async Task<string> SyncOrderStatusAsync(StatusSyncJob job, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Syncing order status for Order {OrderId}", job.OrderId);

            var order = await _dbContext.Orders
                .Include(o => o.Payments)
                .Include(o => o.Shipments)
                .FirstOrDefaultAsync(o => o.Id == job.OrderId, cancellationToken);

            if (order == null)
            {
                return $"Error: Order {job.OrderId} not found";
            }

            // Mock order status determination
            await Task.Delay(100, cancellationToken);

            var originalStatus = order.Status;
            
            // Simple status logic based on payments and shipments
            string newStatus = originalStatus;
            
            if (order.Payments.Any(p => p.IsSuccessful))
            {
                if (order.Shipments.Any(s => s.IsDelivered))
                {
                    newStatus = "Delivered";
                }
                else if (order.Shipments.Any(s => s.IsInTransit))
                {
                    newStatus = "Shipped";
                }
                else if (order.Shipments.Any())
                {
                    newStatus = "Processing";
                }
                else
                {
                    newStatus = "Paid";
                }
            }

            if (order.Status != newStatus)
            {
                await _orderService.UpdateOrderStatusAsync(new UpdateOrderStatusDto
                {
                    OrderId = order.Id,
                    NewStatus = newStatus,
                    ChangeReason = $"Order status synced from {originalStatus}"
                });
                await _cacheService.RemoveAsync($"order:{job.OrderId}");

                Logger.LogInformation("Order status synced for Order {OrderId}: {OldStatus} -> {NewStatus}", 
                    job.OrderId, originalStatus, newStatus);

                return $"Order status synced: {originalStatus} -> {newStatus}";
            }

            return $"Order status confirmed: {order.Status}";
        }
    }
}