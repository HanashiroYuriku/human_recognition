using Cortex.Mediator;
using human_recognition.Application.Features.Cctv.Commands;
using Microsoft.AspNetCore.Mvc;

namespace human_recognition.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class CctvController : ControllerBase
{
    private readonly IMediator _mediator;

    public CctvController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("process-batch")]
    public async Task<IActionResult> ProcessBatch([FromBody] YoloxCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.SendAsync(command, cancellationToken);
        return Ok(result);
    }
}