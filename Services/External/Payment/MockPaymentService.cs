using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ALOud.Services.External.Payment
{
    public class MockPaymentService : IPaymentService
    {
        private readonly ILogger<MockPaymentService> _logger;
        private readonly MockPaymentOptions _options;
        private readonly Random _random = new();

        public MockPaymentService(
            ILogger<MockPaymentService> logger,
            IOptions<MockPaymentOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Processing mock payment for Order {OrderId}, Amount: {Amount} {Currency}",
                request.Order.Id, request.Amount, request.Currency);

            // Simulate API call delay
            await Task.Delay(_random.Next(_options.MinDelayMs, _options.MaxDelayMs), cancellationToken);

            // Simulate payment success/failure based on configuration
            var isSuccess = ShouldSucceed(_options.PaymentSuccessRate);
            var processingFee = CalculateProcessingFee(request.Amount, request.PaymentMethod);

            if (!isSuccess)
            {
                return CreateFailedPaymentResult(request);
            }

            var result = new PaymentResult
            {
                IsSuccess = true,
                Status = "Succeeded",
                TransactionId = GenerateTransactionId(),
                AuthorizationCode = GenerateAuthorizationCode(),
                ProcessingFee = processingFee,
                RiskLevel = GenerateRiskLevel(),
                RiskScore = GenerateRiskScore(),
                ProcessedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, string>
                {
                    ["provider"] = "mock",
                    ["environment"] = "development",
                    ["order_id"] = request.Order.Id.ToString()
                }
            };

            // Add card information for credit card payments
            if (request.PaymentMethod.Contains("Credit_Card", StringComparison.OrdinalIgnoreCase))
            {
                result.CardLast4 = "1234";
                result.CardBrand = "Visa";
                result.CardExpMonth = "12";
                result.CardExpYear = "2028";
                result.AvsResult = "Y";
                result.CvvResult = "M";
            }

            _logger.LogInformation("Mock payment processed successfully for Order {OrderId}, TransactionId: {TransactionId}",
                request.Order.Id, result.TransactionId);

            return result;
        }

        public async Task<PaymentResult> CapturePaymentAsync(string paymentIntentId, decimal amount, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Capturing mock payment {PaymentIntentId}, Amount: {Amount}", paymentIntentId, amount);

            await Task.Delay(_random.Next(_options.MinDelayMs, _options.MaxDelayMs), cancellationToken);

            var isSuccess = ShouldSucceed(_options.PaymentSuccessRate);

            if (!isSuccess)
            {
                return new PaymentResult
                {
                    IsSuccess = false,
                    Status = "Failed",
                    ErrorCode = "capture_failed",
                    ErrorMessage = "Mock payment capture failed for testing",
                    ProcessedAt = DateTime.UtcNow
                };
            }

            return new PaymentResult
            {
                IsSuccess = true,
                Status = "Succeeded",
                TransactionId = GenerateTransactionId(),
                ProcessingFee = CalculateProcessingFee(amount, "Credit_Card"),
                ProcessedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, string>
                {
                    ["provider"] = "mock",
                    ["action"] = "capture",
                    ["payment_intent_id"] = paymentIntentId
                }
            };
        }

        public async Task<RefundResult> RefundPaymentAsync(RefundRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Processing mock refund for PaymentIntent {PaymentIntentId}, Amount: {Amount}",
                request.PaymentIntentId, request.Amount);

            await Task.Delay(_random.Next(_options.MinDelayMs, _options.MaxDelayMs), cancellationToken);

            var isSuccess = ShouldSucceed(_options.RefundSuccessRate);

            if (!isSuccess)
            {
                return new RefundResult
                {
                    IsSuccess = false,
                    Status = "Failed",
                    RefundedAmount = 0,
                    ErrorCode = "refund_failed",
                    ErrorMessage = "Mock refund failed for testing",
                    ProcessedAt = DateTime.UtcNow
                };
            }

            return new RefundResult
            {
                IsSuccess = true,
                Status = "Succeeded",
                RefundId = GenerateRefundId(),
                RefundedAmount = request.Amount,
                ProcessedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, string>
                {
                    ["provider"] = "mock",
                    ["reason"] = request.Reason,
                    ["payment_intent_id"] = request.PaymentIntentId
                }
            };
        }

        public async Task<PaymentStatusResult> GetPaymentStatusAsync(string paymentIntentId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting mock payment status for {PaymentIntentId}", paymentIntentId);

            await Task.Delay(_random.Next(50, 200), cancellationToken);

            return new PaymentStatusResult
            {
                IsSuccess = true,
                Status = "Succeeded",
                TransactionId = GenerateTransactionId(),
                Amount = _random.Next(50, 500),
                RefundedAmount = 0,
                ProcessedAt = DateTime.UtcNow.AddMinutes(-_random.Next(1, 60)),
                Metadata = new Dictionary<string, string>
                {
                    ["provider"] = "mock",
                    ["payment_intent_id"] = paymentIntentId
                }
            };
        }

        public async Task<PaymentResult> CancelPaymentAsync(string paymentIntentId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Cancelling mock payment {PaymentIntentId}", paymentIntentId);

            await Task.Delay(_random.Next(_options.MinDelayMs, _options.MaxDelayMs), cancellationToken);

            return new PaymentResult
            {
                IsSuccess = true,
                Status = "Cancelled",
                ProcessedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, string>
                {
                    ["provider"] = "mock",
                    ["action"] = "cancel",
                    ["payment_intent_id"] = paymentIntentId
                }
            };
        }

        private bool ShouldSucceed(double successRate)
        {
            return _random.NextDouble() < successRate;
        }

        private decimal CalculateProcessingFee(decimal amount, string paymentMethod)
        {
            // Mock processing fees
            return paymentMethod.ToLower() switch
            {
                "credit_card" => amount * 0.029m + 0.30m, // 2.9% + $0.30
                "paypal" => amount * 0.034m + 0.30m,      // 3.4% + $0.30
                "bank_transfer" => 1.50m,                  // Flat $1.50
                _ => 0m
            };
        }

        private PaymentResult CreateFailedPaymentResult(PaymentRequest request)
        {
            var errorCodes = new[] { "card_declined", "insufficient_funds", "expired_card", "invalid_cvc", "processing_error" };
            var errorMessages = new[]
            {
                "Your card was declined.",
                "Insufficient funds available.",
                "Your card has expired.",
                "Your card's security code is incorrect.",
                "An error occurred while processing your payment."
            };

            var index = _random.Next(errorCodes.Length);

            return new PaymentResult
            {
                IsSuccess = false,
                Status = "Failed",
                ErrorCode = errorCodes[index],
                ErrorMessage = errorMessages[index],
                ProcessedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, string>
                {
                    ["provider"] = "mock",
                    ["order_id"] = request.Order.Id.ToString(),
                    ["failure_reason"] = "simulated_failure"
                }
            };
        }

        private string GenerateTransactionId()
        {
            return $"txn_mock_{DateTime.UtcNow:yyyyMMdd}_{_random.Next(100000, 999999)}";
        }

        private string GenerateAuthorizationCode()
        {
            return _random.Next(100000, 999999).ToString();
        }

        private string GenerateRefundId()
        {
            return $"rf_mock_{DateTime.UtcNow:yyyyMMdd}_{_random.Next(100000, 999999)}";
        }

        private string GenerateRiskLevel()
        {
            var levels = new[] { "Low", "Medium", "High" };
            var weights = new[] { 0.7, 0.25, 0.05 }; // 70% low, 25% medium, 5% high
            
            var random = _random.NextDouble();
            var cumulative = 0.0;
            
            for (int i = 0; i < weights.Length; i++)
            {
                cumulative += weights[i];
                if (random <= cumulative)
                {
                    return levels[i];
                }
            }
            
            return levels[0];
        }

        private decimal GenerateRiskScore()
        {
            var riskLevel = GenerateRiskLevel();
            return riskLevel switch
            {
                "Low" => (decimal)(_random.NextDouble() * 30), // 0-30
                "Medium" => (decimal)(_random.NextDouble() * 40 + 30), // 30-70
                "High" => (decimal)(_random.NextDouble() * 30 + 70), // 70-100
                _ => 0m
            };
        }
    }

    public class MockPaymentOptions
    {
        public const string SectionName = "MockServices:Payment";

        public double PaymentSuccessRate { get; set; } = 0.95; // 95% success rate
        public double RefundSuccessRate { get; set; } = 0.98; // 98% success rate
        public int MinDelayMs { get; set; } = 100;
        public int MaxDelayMs { get; set; } = 1000;
    }
}