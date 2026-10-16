using InterviewPrep.Application.Purchases.Commands.CreatePurchase;
using InterviewPrep.Application.Purchases.Queries.GetPurchaseHistory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterviewPrep.Api.Controllers;

[ApiController]
[Route("api/purchases")]
[Authorize]
public class PurchasesController : ControllerBase
{
    private readonly ISender _mediator;

    public PurchasesController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<CreatePurchaseResult>> Create(CreatePurchaseCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpGet("member/{memberId:int}")]
    public async Task<ActionResult<IReadOnlyList<PurchaseDto>>> GetHistory(int memberId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPurchaseHistoryQuery(memberId), ct);
        return Ok(result);
    }
}
