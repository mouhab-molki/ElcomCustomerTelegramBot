using System.Text.Json.Serialization;

namespace ElcomCustomerTelegramBot.Models
{
    public class PaymentRecord
    {
        [JsonPropertyName("paymentAmount")]
        public int PaymentAmount { get; set; }

        [JsonPropertyName("issueDate")]
        public DateTime? issueDate { get; set; }

        [JsonPropertyName("paymentDate")]
        public DateTime? paymentDate { get; set; }
    }
}
