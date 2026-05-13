using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickStock.Applications.AuditLogs.Queries;

namespace QuickStock.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuditLogsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuditLogsController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> GetAuditLogs(int? campusId = null, string? entityType = null, int page = 1, int pageSize = 10)
        {
            var result = await _mediator.Send(new GetAuditLogsQuery(campusId, entityType, page, pageSize, User));
            return Ok(result);
        }
    }
}
