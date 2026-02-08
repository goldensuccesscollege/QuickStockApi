namespace QuickStock.Applications.Accounts.Dto_s
{
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

    }

    public class RegisterRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty; 
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsFromApi { get; set; } = false;
    }

    public class RegisterResponse
    {
        public string Message { get; set; } = string.Empty;
        public string? VerificationTokens { get; set; }

    }

    public class ForgotPass
    {
        public string Email { get; set; } = string.Empty;
    }

    // DTO for Reset Password
    public class ResetPass
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    // DTO for "User" in memory
    public class UserDto
    {
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }
    }
    public class CheckResetTokenRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
