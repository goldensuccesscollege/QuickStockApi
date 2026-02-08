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
            // 1️⃣ Password confirmation
            if (request.Password != request.ConfirmPassword)
                return new RegisterResponse
                {
                    Message = "Password and Confirm Password do not match."
                };

            // 2️⃣ Check username
            if (await _db.Accounts.AnyAsync(
                x => x.Username == request.Username,
                cancellationToken))
                return new RegisterResponse
                {
                    Message = "Username already taken."
                };

            // 3️⃣ Check email
            if (await _db.Accounts.AnyAsync(
                x => x.Email == request.Email,
                cancellationToken))
                return new RegisterResponse
                {
                    Message = "Email already registered."
                };

            // 4️⃣ Generate verification token
            var token = Guid.NewGuid().ToString();

            // 5️⃣ Create Account ONLY
            var user = new Account
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = PasswordHelper.HashPassword(request.Password),
                VerificationTokens = token,
                Created = DateTime.UtcNow,
                Role = "User",
                Status = "Pending"
            };

            await _db.Accounts.AddAsync(user, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            // 6️⃣ Generate verify link
            var baseUrl = _config["Frontend:BaseUrl"]?.TrimEnd('/');
            var verifyUrl = $"{baseUrl}/Account/verify?token={token}";

            // 7️⃣ Email
            var emailBody = $@"
                <h1>Welcome {request.Username}!</h1>
                <p>Please verify your account by clicking <a href='{verifyUrl}'>here</a>.</p>
                <p>Or use this token manually:</p>
                <h3>{token}</h3>
            ";

            await _email.SendEmailAsync(
                request.Email,
                "Verify your account",
                emailBody);

            // 8️⃣ Response
            return new RegisterResponse
            {
                Message = "Registration successful. Please check your email for verification.",
                VerificationTokens = token
            };
        }
    }
}
