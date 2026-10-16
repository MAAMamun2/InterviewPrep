using InterviewPrep.Application.Members.Commands.CreateMember;
using InterviewPrep.Application.Members.Queries.GetMemberById;
using InterviewPrep.Application.Members.Queries.GetMembers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterviewPrep.Api.Controllers;

[ApiController]
[Route("api/members")]
[Authorize] // AUTHENTICATION gate: any endpoint here requires a valid JWT bearer token
public class MembersController : ControllerBase
{
    private readonly ISender _mediator;

    public MembersController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MemberDto>>> GetAll(
        [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetMembersQuery(search, page, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MemberDetailDto>> GetById(int id, CancellationToken ct)
    {
        // IDOR CHECK, worth calling out even though it's a no-op in this single-tenant demo:
        // in HishabPOS, this is exactly where PostgreSQL Row-Level Security kicks in -- the
        // query is scoped to the caller's own tenant automatically, so one member can never
        // fetch /api/members/{id} belonging to a different shop just by guessing an id.
        var result = await _mediator.Send(new GetMemberByIdQuery(id), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateMemberCommand command, CancellationToken ct)
    {
        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>
    /// AUTHORIZATION (not just authentication): only a token with the "Admin" role claim can hit
    /// this. A valid "Staff" token gets 403 Forbidden, not 401 -- a distinction worth stating
    /// explicitly (401 = who are you?, 403 = I know who you are, you're not allowed).
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public IActionResult Delete(int id)
    {
        // Intentionally unimplemented in this demo -- the policy check IS the point being demonstrated.
        return NoContent();
    }
}
