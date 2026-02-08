using QuickStock.Applications.Accounts.Dto_s;


namespace QuickStock.Infrastructure.Services
{
    public interface IAuthService
    {
        Task ResetPasswordAsync(ResetPass request);
        Task<string> GenerateForgotPasswordTokenAsync(string email);
    }
}
