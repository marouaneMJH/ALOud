using ALOud.Models;

namespace ALOud.Services.External.Payment
{
    public interface IPaymentService
    {
        /// <summary>
        /// Process a payment for an order
        /// </summary>
        Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Capture an authorized payment
        /// </summary>
        Task<PaymentResult> CapturePaymentAsync(string paymentIntentId, decimal amount, CancellationToken cancellationToken = default);

        /// <summary>
        /// Refund a payment
        /// </summary>
        Task<RefundResult> RefundPaymentAsync(RefundRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get payment status from external provider
        /// </summary>
        Task<PaymentStatusResult> GetPaymentStatusAsync(string paymentIntentId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cancel/void an authorized payment
        /// </summary>
        Task<PaymentResult> CancelPaymentAsync(string paymentIntentId, CancellationToken cancellationToken = default);
    }

    public class PaymentRequest
    {
        public required string PaymentIntentId { get; set; }
        public required string PaymentMethod { get; set; }
        public required decimal Amount { get; set; }
        public required string Currency { get; set; }
        public required string CustomerEmail { get; set; }
        public required string Description { get; set; }
        public required Order Order { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    public class RefundRequest
    {
        public required string PaymentIntentId { get; set; }
        public required decimal Amount { get; set; }
        public required string Reason { get; set; }
        public Guid? RefundedByUserId { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    public class PaymentResult
    {
        public bool IsSuccess { get; set; }
        public required string Status { get; set; } // "Pending", "Processing", "Succeeded", "Failed", "Cancelled"
        public string? TransactionId { get; set; }
        public string? AuthorizationCode { get; set; }
        public decimal ProcessingFee { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public string? RiskLevel { get; set; }
        public decimal? RiskScore { get; set; }
        public DateTime ProcessedAt { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new();

        // Card information (if applicable)
        public string? CardLast4 { get; set; }
        public string? CardBrand { get; set; }
        public string? CardExpMonth { get; set; }
        public string? CardExpYear { get; set; }
        public string? AvsResult { get; set; }
        public string? CvvResult { get; set; }
    }

    public class RefundResult
    {
        public bool IsSuccess { get; set; }
        public required string Status { get; set; }
        public string? RefundId { get; set; }
        public decimal RefundedAmount { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime ProcessedAt { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    public class PaymentStatusResult
    {
        public bool IsSuccess { get; set; }
        public required string Status { get; set; }
        public string? TransactionId { get; set; }
        public decimal? Amount { get; set; }
        public decimal? RefundedAmount { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}