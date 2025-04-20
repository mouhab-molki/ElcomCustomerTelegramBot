using System.Text.Json.Serialization;

namespace ElcomCustomerTelegramBot.Models
{
    public class PackageResponse
    {
        [JsonPropertyName("packageId")]
        public int PackageId { get; set; }

        [JsonPropertyName("packageName")]
        public string PackageName { get; set; }

        [JsonPropertyName("monthlyPrice")]
        public int MonthlyPrice { get; set; }

        [JsonPropertyName("monthlyDataVolumeGb")]
        public int MonthlyDataVolumeGb { get; set; }
    }
}
