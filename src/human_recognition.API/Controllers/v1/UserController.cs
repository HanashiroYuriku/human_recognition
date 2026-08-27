using Asp.Versioning;
using human_recognition.Application.Features.Users.Commands;
using human_recognition.Application.Features.Users.Queries;
using human_recognition.Domain.Enums;
using Cortex.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace human_recognition.Api.Controllers.v1;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class UserController : ControllerBase
{
    // Inject Mediator
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // This end point isn't self register for user
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.SendAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result }, new { id = result });
    }

    [Authorize(Roles = nameof(Role.Admin))]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var query = new GetUserByIdQuery(id);

        var result = await _mediator.QueryAsync(query, ct);

        return Ok(result);
    }

    [Authorize(Roles = nameof(Role.Admin))]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetAllUsersQuery query, CancellationToken ct)
    {
        var result = await _mediator.QueryAsync(query, ct);
        return Ok(result);
    }
}