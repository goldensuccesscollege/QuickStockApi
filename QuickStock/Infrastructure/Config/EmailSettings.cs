namespace QuickStock.Infrastructure.Config
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = "";
        public int SmtpPort { get; set; } = 587;
        public string SmtpUser { get; set; } = "";
        public string SmtpPass { get; set; } = "";
    }
}
