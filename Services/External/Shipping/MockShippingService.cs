using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ALOud.Services.External.Shipping
{
    public class MockShippingService : IShippingService
    {
        private readonly ILogger<MockShippingService> _logger;
        private readonly MockShippingOptions _options;
        private readonly Random _random = new();

        private readonly Dictionary<string, List<string>> _carrierServices = new()
        {
            ["UPS"] = new() { "Ground", "Next Day Air", "2nd Day Air", "3 Day Select" },
            ["FedEx"] = new() { "Ground", "Express Overnight", "2Day", "Standard Overnight" },
            ["DHL"] = new() { "Express", "Ground", "Next Day", "Worldwide Express" },
            ["USPS"] = new() { "Ground Advantage", "Priority Mail", "Priority Express", "First-Class Mail" }
        };

        public MockShippingService(
            ILogger<MockShippingService> logger,
            IOptions<MockShippingOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        public async Task<ShipmentResult> CreateShipmentAsync(ShipmentRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating mock shipment for Order {OrderId} with {Carrier} {Method}",
                request.Order.Id, request.Carrier, request.ShippingMethod);

            await Task.Delay(_random.Next(_options.MinDelayMs, _options.MaxDelayMs), cancellationToken);

            var isSuccess = ShouldSucceed(_options.ShipmentSuccessRate);

            if (!isSuccess)
            {
                return new ShipmentResult
                {
                    IsSuccess = false,
                    Status = "Failed",
                    ErrorCode = "shipment_creation_failed",
                    ErrorMessage = "Mock shipment creation failed for testing",
                    CreatedAt = DateTime.UtcNow
                };
            }

            var trackingNumber = GenerateTrackingNumber(request.Carrier);
            var estimatedDelivery = CalculateEstimatedDelivery(request.ShippingMethod);
            var shippingCost = CalculateShippingCost(request.ShippingMethod, request.Order.TotalAmount);

            return new ShipmentResult
            {
                IsSuccess = true,
                ShipmentId = GenerateShipmentId(),
                TrackingNumber = trackingNumber,
                Status = "Prepared",
                ShippingCost = shippingCost,
                ShippingLabelUrl = GenerateLabelUrl(trackingNumber),
                EstimatedDeliveryDate = estimatedDelivery,
                CreatedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, string>
                {
                    ["provider"] = "mock",
                    ["carrier"] = request.Carrier,
                    ["method"] = request.ShippingMethod,
                    ["order_id"] = request.Order.Id.ToString(),
                    ["requires_signature"] = request.RequiresSignature.ToString(),
                    ["requires_adult_signature"] = request.RequiresAdultSignature.ToString()
                }
            };
        }

        public async Task<ShippingRatesResult> GetShippingRatesAsync(ShippingRatesRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting mock shipping rates from {FromCity} to {ToCity}",
                request.FromAddress.City, request.ToAddress.City);

            await Task.Delay(_random.Next(100, 500), cancellationToken);

            var isSuccess = ShouldSucceed(_options.RatesSuccessRate);

            if (!isSuccess)
            {
                return new ShippingRatesResult
                {
                    IsSuccess = false,
                    ErrorCode = "rates_unavailable",
                    ErrorMessage = "Mock shipping rates unavailable for testing"
                };
            }

            var rates = new List<ShippingRate>();

            foreach (var carrier in _carrierServices.Keys)
            {
                foreach (var service in _carrierServices[carrier])
                {
                    var rate = GenerateShippingRate(carrier, service, request.Packages.Sum(p => p.WeightPounds));
                    rates.Add(rate);
                }
            }

            return new ShippingRatesResult
            {
                IsSuccess = true,
                Rates = rates.OrderBy(r => r.Rate).ToList(),
                Metadata = new Dictionary<string, string>
                {
                    ["provider"] = "mock",
                    ["total_weight"] = request.Packages.Sum(p => p.WeightPounds).ToString("F2"),
                    ["destination"] = $"{request.ToAddress.City}, {request.ToAddress.State}"
                }
            };
        }

        public async Task<TrackingResult> TrackShipmentAsync(string trackingNumber, string carrier, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Tracking mock shipment {TrackingNumber} with {Carrier}", trackingNumber, carrier);

            await Task.Delay(_random.Next(200, 800), cancellationToken);

            var isSuccess = ShouldSucceed(_options.TrackingSuccessRate);

            if (!isSuccess)
            {
                return new TrackingResult
                {
                    IsSuccess = false,
                    Status = "Not_Found",
                    ErrorCode = "tracking_not_found",
                    ErrorMessage = "Mock tracking information not available"
                };
            }

            var status = GenerateCurrentStatus();
            var events = GenerateTrackingEvents(status);

            return new TrackingResult
            {
                IsSuccess = true,
                Status = status,
                TrackingNumber = trackingNumber,
                Carrier = carrier,
                EstimatedDeliveryDate = DateTime.UtcNow.AddDays(_random.Next(1, 7)),
                ActualDeliveryDate = status == "Delivered" ? DateTime.UtcNow.AddDays(-_random.Next(0, 3)) : null,
                DeliverySignature = status == "Delivered" ? GenerateSignature() : null,
                TrackingEvents = events,
                Metadata = new Dictionary<string, string>
                {
                    ["provider"] = "mock",
                    ["last_updated"] = DateTime.UtcNow.ToString("O")
                }
            };
        }

        public async Task<ShipmentResult> CancelShipmentAsync(string shipmentId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Cancelling mock shipment {ShipmentId}", shipmentId);

            await Task.Delay(_random.Next(_options.MinDelayMs, _options.MaxDelayMs), cancellationToken);

            return new ShipmentResult
            {
                IsSuccess = true,
                ShipmentId = shipmentId,
                Status = "Cancelled",
                CreatedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, string>
                {
                    ["provider"] = "mock",
                    ["action"] = "cancel"
                }
            };
        }

        public async Task<TrackingResult> UpdateShipmentStatusAsync(string shipmentId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating mock shipment status for {ShipmentId}", shipmentId);

            await Task.Delay(_random.Next(100, 400), cancellationToken);

            var status = GenerateCurrentStatus();
            var events = GenerateTrackingEvents(status);

            return new TrackingResult
            {
                IsSuccess = true,
                Status = status,
                TrackingEvents = events,
                Metadata = new Dictionary<string, string>
                {
                    ["provider"] = "mock",
                    ["shipment_id"] = shipmentId,
                    ["updated_at"] = DateTime.UtcNow.ToString("O")
                }
            };
        }

        public async Task<LabelResult> GetShippingLabelAsync(string shipmentId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting mock shipping label for {ShipmentId}", shipmentId);

            await Task.Delay(_random.Next(100, 300), cancellationToken);

            return new LabelResult
            {
                IsSuccess = true,
                LabelUrl = $"https://mock-shipping.example.com/labels/{shipmentId}.pdf",
                Format = "PDF",
                Metadata = new Dictionary<string, string>
                {
                    ["provider"] = "mock",
                    ["shipment_id"] = shipmentId,
                    ["generated_at"] = DateTime.UtcNow.ToString("O")
                }
            };
        }

        private bool ShouldSucceed(double successRate)
        {
            return _random.NextDouble() < successRate;
        }

        private string GenerateTrackingNumber(string carrier)
        {
            return carrier.ToUpper() switch
            {
                "UPS" => $"1Z{_random.Next(100000, 999999)}{_random.Next(10000000, 99999999)}",
                "FEDEX" => $"{_random.Next(1000, 9999)}{_random.Next(1000, 9999)}{_random.Next(1000, 9999)}",
                "DHL" => $"{_random.Next(1000000000, int.MaxValue)}",
                "USPS" => $"9400{_random.Next(1000000000, int.MaxValue)}",
                _ => $"TRK{DateTime.UtcNow:yyyyMMdd}{_random.Next(100000, 999999)}"
            };
        }

        private string GenerateShipmentId()
        {
            return $"shp_mock_{DateTime.UtcNow:yyyyMMdd}_{_random.Next(100000, 999999)}";
        }

        private DateTime CalculateEstimatedDelivery(string shippingMethod)
        {
            var baseDays = shippingMethod.ToLower() switch
            {
                var method when method.Contains("overnight") || method.Contains("next") => 1,
                var method when method.Contains("2day") || method.Contains("2nd") => 2,
                var method when method.Contains("3day") || method.Contains("3rd") => 3,
                var method when method.Contains("express") => _random.Next(2, 4),
                var method when method.Contains("priority") => _random.Next(2, 5),
                _ => _random.Next(3, 8) // Standard ground
            };

            return DateTime.UtcNow.AddDays(baseDays);
        }

        private decimal CalculateShippingCost(string shippingMethod, decimal orderTotal)
        {
            var baseCost = shippingMethod.ToLower() switch
            {
                var method when method.Contains("overnight") || method.Contains("next") => 25.99m,
                var method when method.Contains("2day") || method.Contains("2nd") => 15.99m,
                var method when method.Contains("3day") || method.Contains("3rd") => 12.99m,
                var method when method.Contains("express") => 18.99m,
                var method when method.Contains("priority") => 8.99m,
                _ => 5.99m // Standard ground
            };

            // Add weight-based cost simulation
            var weightSurcharge = (decimal)(_random.NextDouble() * 5.0);
            
            return baseCost + weightSurcharge;
        }

        private ShippingRate GenerateShippingRate(string carrier, string service, decimal totalWeight)
        {
            var baseCost = CalculateShippingCost(service, 0);
            var weightCost = totalWeight * (decimal)(_random.NextDouble() * 2.0 + 0.5); // $0.50-$2.50 per lb
            
            return new ShippingRate
            {
                Carrier = carrier,
                ServiceName = service,
                ServiceCode = $"{carrier.ToUpper()}_{service.ToUpper().Replace(" ", "_")}",
                Rate = baseCost + weightCost,
                EstimatedDeliveryDate = CalculateEstimatedDelivery(service),
                DeliveryDays = GetDeliveryDays(service),
                GuaranteedService = service.Contains("Express") || service.Contains("Overnight"),
                Metadata = new Dictionary<string, string>
                {
                    ["weight_lbs"] = totalWeight.ToString("F2"),
                    ["base_cost"] = baseCost.ToString("F2"),
                    ["weight_cost"] = weightCost.ToString("F2")
                }
            };
        }

        private int GetDeliveryDays(string service)
        {
            return service.ToLower() switch
            {
                var method when method.Contains("overnight") || method.Contains("next") => 1,
                var method when method.Contains("2day") || method.Contains("2nd") => 2,
                var method when method.Contains("3day") || method.Contains("3rd") => 3,
                var method when method.Contains("express") => _random.Next(2, 4),
                _ => _random.Next(3, 8)
            };
        }

        private string GenerateCurrentStatus()
        {
            var statuses = new[] { "Prepared", "Shipped", "In_Transit", "Out_For_Delivery", "Delivered" };
            var weights = new[] { 0.1, 0.2, 0.4, 0.2, 0.1 }; // Most likely to be in transit
            
            var random = _random.NextDouble();
            var cumulative = 0.0;
            
            for (int i = 0; i < weights.Length; i++)
            {
                cumulative += weights[i];
                if (random <= cumulative)
                {
                    return statuses[i];
                }
            }
            
            return statuses[2]; // Default to In_Transit
        }

        private List<TrackingEvent> GenerateTrackingEvents(string currentStatus)
        {
            var events = new List<TrackingEvent>();
            var timestamp = DateTime.UtcNow.AddDays(-3);

            // Always start with prepared
            events.Add(new TrackingEvent
            {
                Timestamp = timestamp,
                Status = "Prepared",
                Description = "Shipment information received",
                Location = "ALOud Fulfillment Center, Casablanca, MA"
            });

            if (currentStatus == "Prepared") return events;

            timestamp = timestamp.AddHours(_random.Next(2, 12));
            events.Add(new TrackingEvent
            {
                Timestamp = timestamp,
                Status = "Shipped",
                Description = "Package picked up by carrier",
                Location = "ALOud Fulfillment Center, Casablanca, MA"
            });

            if (currentStatus == "Shipped") return events;

            // Add 1-3 in transit events
            var transitCount = _random.Next(1, 4);
            for (int i = 0; i < transitCount && (currentStatus == "In_Transit" || events.Count < 5); i++)
            {
                timestamp = timestamp.AddHours(_random.Next(4, 24));
                events.Add(new TrackingEvent
                {
                    Timestamp = timestamp,
                    Status = "In_Transit",
                    Description = "Package in transit",
                    Location = GenerateTransitLocation()
                });
            }

            if (currentStatus == "In_Transit") return events;

            timestamp = timestamp.AddHours(_random.Next(2, 8));
            events.Add(new TrackingEvent
            {
                Timestamp = timestamp,
                Status = "Out_For_Delivery",
                Description = "Out for delivery",
                Location = GenerateDeliveryLocation()
            });

            if (currentStatus == "Out_For_Delivery") return events;

            timestamp = timestamp.AddHours(_random.Next(1, 8));
            events.Add(new TrackingEvent
            {
                Timestamp = timestamp,
                Status = "Delivered",
                Description = "Package delivered",
                Location = GenerateDeliveryLocation()
            });

            return events.OrderBy(e => e.Timestamp).ToList();
        }

        private string GenerateTransitLocation()
        {
            var locations = new[]
            {
                "Rabat Distribution Center, MA",
                "Tangier Transit Hub, MA",
                "Marrakech Sorting Facility, MA",
                "Fez Regional Center, MA",
                "Agadir Processing Center, MA"
            };
            
            return locations[_random.Next(locations.Length)];
        }

        private string GenerateDeliveryLocation()
        {
            var locations = new[]
            {
                "Local Delivery Facility",
                "Delivery Vehicle",
                "Customer Address",
                "Front Door",
                "Mailbox"
            };
            
            return locations[_random.Next(locations.Length)];
        }

        private string GenerateSignature()
        {
            var signatures = new[] { "J. Smith", "M. Johnson", "A. Garcia", "L. Martinez", "Customer" };
            return signatures[_random.Next(signatures.Length)];
        }

        private string GenerateLabelUrl(string trackingNumber)
        {
            return $"https://mock-shipping.example.com/labels/{trackingNumber}.pdf";
        }
    }

    public class MockShippingOptions
    {
        public const string SectionName = "MockServices:Shipping";

        public double ShipmentSuccessRate { get; set; } = 0.95; // 95% success rate
        public double RatesSuccessRate { get; set; } = 0.98; // 98% success rate
        public double TrackingSuccessRate { get; set; } = 0.99; // 99% success rate
        public int MinDelayMs { get; set; } = 200;
        public int MaxDelayMs { get; set; } = 1500;
    }
}