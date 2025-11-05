namespace ShoppeClone.Api.Domain.Entities
{
    public class Payment : BaseEntity
    {
        public int OrderId { get; set; }
        public string PaymentMethod { get; set; } = "VNPay";
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Success, Failed
        public string? TransactionId { get; set; }
        public string? PaymentUrl { get; set; }
        public string? ResponseCode { get; set; }
        public string? SecureHash { get; set; }
        public DateTime? PaymentDate { get; set; }

        // Navigation property
        public Order Order { get; set; } = null!;
    }

    public class VNPayPaymentRequest
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string OrderDescription { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
    }

    public class VNPayPaymentResponse
    {
        public bool Success { get; set; }
        public string PaymentUrl { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}