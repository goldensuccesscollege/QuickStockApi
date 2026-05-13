namespace QuickStock.Applications.Users.Dtos
{
    public class CreateUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool CanAccessITAssets { get; set; } = true;
        public bool CanAccessApparel { get; set; } = true;
        public bool CanAccessMessages { get; set; } = true;
        public bool CanAccessLibrary { get; set; } = true;
        public bool CanAccessHomeEconomics { get; set; } = true;
        public bool CanAccessConsumables { get; set; } = true;
    }

    public class UpdateUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool CanAccessITAssets { get; set; }
        public bool CanAccessApparel { get; set; }
        public bool CanAccessMessages { get; set; }
        public bool CanAccessLibrary { get; set; }
        public bool CanAccessHomeEconomics { get; set; }
        public bool CanAccessConsumables { get; set; }
    }
}
