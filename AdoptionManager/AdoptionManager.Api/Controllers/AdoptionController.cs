using System;
using System.Threading.Tasks;
using AdoptionManager.Application.Commands;
using AdoptionManager.Application.DTOs;
using AdoptionManager.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AdoptionManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdoptionController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdoptionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<ApplicationDto>> SubmitApplication([FromBody] SubmitApplicationCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetApplication), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApplicationDto>> GetApplication(Guid id)
    {
        var result = await _mediator.Send(new GetApplicationQuery(id));
        if (result == null)
            return NotFound();

        return Ok(result);
    }
}