using System.Text.Json.Serialization;

namespace ElcomCustomerTelegramBot.Models
{
    public class InvoiceResponse
    {
        [JsonPropertyName("invoiceId")]
        public int InvoiceId { get; set; }

        [JsonPropertyName("subscriberId")]
        public int SubscriberId { get; set; }

        [JsonPropertyName("issueDate")]
        public DateTime IssueDate { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("isPaid")]
        public bool IsPaid { get; set; }

        [JsonPropertyName("paymentDate")]
        public DateTime? PaymentDate { get; set; }
    }
    
}
