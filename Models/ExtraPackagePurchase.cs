using System.Text.Json.Serialization;

namespace ElcomCustomerTelegramBot.Models
{
    public class ExtraPackagePurchase
    {
        [JsonPropertyName("purchaseId")]
        public int PurchaseId { get; set; }

        [JsonPropertyName("subscriberId")]
        public int SubscriberId { get; set; }

        [JsonPropertyName("extraPackageId")]
        public int ExtraPackageId { get; set; }

        [JsonPropertyName("purchaseDate")]
        public DateTime PurchaseDate { get; set; }

        [JsonPropertyName("chargedAmount")]
        public decimal ChargedAmount { get; set; }

        [JsonPropertyName("activationStatus")]
        public string ActivationStatus { get; set; }

        [JsonPropertyName("purchaseType")]
        public string PurchaseType { get; set; }

        [JsonPropertyName("notes")]
        public string Notes { get; set; }
    }
}
