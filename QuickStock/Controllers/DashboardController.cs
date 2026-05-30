using QuickStock.CQRS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickStock.Applications.Dashboard.Queries;

namespace QuickStock.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DashboardController(IMediator mediator) => _mediator = mediator;

        [HttpGet("{campusId}/stats")]
        public async Task<IActionResult> GetStats(int campusId)
        {
            var result = await _mediator.Send(new GetDashboardStatsQuery(campusId));
            return Ok(result);
        }
    }
}
