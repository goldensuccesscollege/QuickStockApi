using MediatR;
using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Accounts.Command;
using QuickStock.Applications.Accounts.Dto_s;
using QuickStock.Domain;
using QuickStock.Infrastructure.Data;
using QuickStock.Infrastructure.Security;
using QuickStock.Infrastructure.Services;

namespace QuickStock.Applications.Accounts.Handler
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
    {
        private readonly AppDbContext _db;
        private readonly EmailService _email;
        private readonly IConfiguration _config;

        public RegisterCommandHandler(
            AppDbContext db,
            EmailService email,
            IConfiguration config)
        {
            _db = db;
            _email = email;
            _config = config;
        }

        public async Task<RegisterResponse> Handle(
            RegisterCommand request,
            CancellationToken cancellationToken)
        {
            // Validate password
            if (request.Password != request.ConfirmPassword)
                return Fail("Password and Confirm Password do not match.");

            // Check username
            if (await UsernameExists(request.Username, cancellationToken))
                return Fail("Username already taken.");

            // Check email
            if (await EmailExists(request.Email, cancellationToken))
                return Fail("Email already registered.");

            var token = Guid.NewGuid().ToString();

            var account = new Account
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = PasswordHelper.HashPassword(request.Password),
                VerificationTokens = token,
                Created = DateTime.UtcNow,
                Role = request.Role ?? "User",
                Status = "Pending",
                Profile = new QuickStock.Domain.Profile
                {
                    FirstName = request.FirstName ?? string.Empty,
                    LastName = request.LastName ?? string.Empty
                }
            };

            await _db.Accounts.AddAsync(account, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            await SendVerificationEmail(request.Username, request.Email, token);

            return new RegisterResponse
            {
                Message = "Registration successful. Please check your email for verification.",
                VerificationTokens = token
            };
        }

        private async Task<bool> UsernameExists(string username, CancellationToken ct)
        {
            return await _db.Accounts.AnyAsync(x => x.Username == username, ct);
        }

        private async Task<bool> EmailExists(string email, CancellationToken ct)
        {
            return await _db.Accounts.AnyAsync(x => x.Email == email, ct);
        }

        private async Task SendVerificationEmail(string username, string email, string token)
        {
            var baseUrl = _config["Frontend:BaseUrl"]?.TrimEnd('/');
            var verifyUrl = $"{baseUrl}/Account/verify?token={token}";

            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px; background-color: #f9f9f9;'>
                    <div style='text-align: center; padding-bottom: 20px; border-bottom: 1px solid #e0e0e0;'>
                        <h2 style='color: #2c3e50; margin: 0;'>QuickStock Verification</h2>
                    </div>
                    <div style='padding: 20px 0; color: #333;'>
                        <h3 style='color: #2c3e50; margin-top: 0;'>Welcome, {username}!</h3>
                        <p style='line-height: 1.6;'>Thank you for registering with QuickStock. To complete your registration and secure your account, please verify your email address.</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{verifyUrl}' style='background-color: #3498db; color: #ffffff; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block;'>Verify Account</a>
                        </div>
                        <p style='line-height: 1.6;'>Or, if you prefer, you can manually enter this verification token:</p>
                        <div style='background-color: #e8f4fd; padding: 15px; text-align: center; font-size: 18px; font-weight: bold; letter-spacing: 2px; color: #2980b9; border-radius: 5px; border: 1px dashed #3498db;'>
                            {token}
                        </div>
                    </div>
                    <div style='text-align: center; padding-top: 20px; border-top: 1px solid #e0e0e0; font-size: 12px; color: #7f8c8d;'>
                        <p>If you did not create an account, no further action is required.</p>
                        <p>&copy; {DateTime.UtcNow.Year} QuickStock. All rights reserved.</p>
                    </div>
                </div>";

            try
            {
                await _email.SendEmailAsync(email, "Verify your account", body, isHtml: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending failed: {ex.Message}");
            }
        }

        private RegisterResponse Fail(string message)
        {
            return new RegisterResponse
            {
                Message = message
            };
        }
    }
}