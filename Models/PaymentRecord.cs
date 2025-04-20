using System.Text.Json.Serialization;

namespace ElcomCustomerTelegramBot.Models
{
    public class PaymentRecord
    {
        [JsonPropertyName("paymentAmount")]
        public int PaymentAmount { get; set; }

        [JsonPropertyName("additionDate")]
        public DateTime? AdditionDate { get; set; }

        [JsonPropertyName("withdrawalDate")]
        public DateTime? WithdrawalDate { get; set; }
    }
}
