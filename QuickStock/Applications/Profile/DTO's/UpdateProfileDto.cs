namespace QuickStock.Applications.Profile.DTO_s
{
    public class UpdateProfileDto
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public DateTime? Birthday { get; set; }

        public string? Address { get; set; }

        public string? PhoneNumber { get; set; }

        public IFormFile? ImageProfile { get; set; }
    }
}
