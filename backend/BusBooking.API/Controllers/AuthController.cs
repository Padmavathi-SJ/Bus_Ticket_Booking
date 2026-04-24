using BusBooking.Application.Auth.Commands.Login;
using BusBooking.Application.Auth.Commands.RegisterCustomer;
using BusBooking.Application.Auth.Commands.RegisterOperator;
using BusBooking.Application.Auth.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BusBooking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("admin-login")]
    public async Task<ActionResult<AuthResponseDto>> AdminLogin([FromBody] LoginCommand command)
    {
        command.Role = BusBooking.Domain.Enums.UserRole.Admin;
        return await ProcessLogin(command);
    }

    [HttpPost("user-login")]
    public async Task<ActionResult<AuthResponseDto>> UserLogin([FromBody] LoginCommand command)
    {
        command.Role = BusBooking.Domain.Enums.UserRole.Customer;
        return await ProcessLogin(command);
    }

    [HttpPost("operator-login")]
    public async Task<ActionResult<AuthResponseDto>> OperatorLogin([FromBody] LoginCommand command)
    {
        command.Role = BusBooking.Domain.Enums.UserRole.BusOperator;
        return await ProcessLogin(command);
    }

    private async Task<ActionResult<AuthResponseDto>> ProcessLogin(LoginCommand command)
    {
        try
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("user-register")]
    public async Task<ActionResult> RegisterCustomer([FromBody] RegisterCustomerCommand command)
    {
        try
        {
            await _mediator.Send(command);
            return Ok(new { message = "Registration successful" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("operator-register")]
    public async Task<ActionResult> RegisterOperator([FromBody] RegisterOperatorCommand command)
    {
        try
        {
            await _mediator.Send(command);
            return Ok(new { message = "Operator registration successful. Waiting for admin approval." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
