using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Accounts.Command;
using QuickStock.Applications.Accounts.Dto_s;
using QuickStock.Applications.Accounts.Queries;
using QuickStock.Infrastructure.Services;
using System.Security.Claims;


namespace QuickStock.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IAuthService _authService;
        public AuthController(IMediator mediator,IAuthService authService)
        {
            _mediator = mediator;
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterCommand command)
        {
            var response = await _mediator.Send(command);
            return Ok(response); ;
        }


        [HttpGet("verify")]
        public async Task<IActionResult> Verify([FromQuery] string token)
        {
            var result = await _mediator.Send(new VerifyAccountCommand(token));
            return Ok(result);
        }
        private int GetAccountId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (claim == null)
                throw new UnauthorizedAccessException("Invalid token");

            return int.Parse(claim); // ? no FormatException now
        }

        
        // Forgot Password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Reset Password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (result == "Password reset successful.")
                    return Ok(new { message = result });
                
                return BadRequest(new { message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("check-reset-token")]
        public async Task<IActionResult> CheckResetToken([FromQuery] string email, [FromQuery] string token)
        {
            var query = new CheckResetTokenQuery(email, token);
            var isValid = await _mediator.Send(query);
            return Ok(isValid);
        }


    }
}

