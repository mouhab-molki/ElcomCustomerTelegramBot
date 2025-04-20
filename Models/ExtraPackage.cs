using System.Text.Json.Serialization;

namespace ElcomCustomerTelegramBot.Models
{
    public class ExtraPackage
    {
        [JsonPropertyName("extraPackageId")]
        public int ExtraPackageId { get; set; }

        [JsonPropertyName("sizeGb")]
        public int SizeGb { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }
    }
}