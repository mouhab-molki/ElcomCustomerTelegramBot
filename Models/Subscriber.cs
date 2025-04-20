namespace ElcomCustomerTelegramBot.Models
{
    public class Subscriber
    {
        public int SubscriberId { get; set; }
        public string FullName { get; set; }
        public string AdslUsername { get; set; }
        public string AdslPassword { get; set; }
        public string PhoneNumber { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string SubscriptionStatus { get; set; }
        public int Balance { get; set; }
        public string CustomerInterfacePasswordHash { get; set; }
        public string NationalId { get; set; }
        public string PackageName { get; set; }
        public int PackageId { get; set; }
        public decimal DownloadMonthlyBalance { get; set; }
        public int TotalDownloadMonthlyBalance { get; set; }
    }
}
