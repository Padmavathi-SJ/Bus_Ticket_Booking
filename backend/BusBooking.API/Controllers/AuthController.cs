using BusBooking.Application.Auth.Commands.Login;
using BusBooking.Application.Auth.Commands.RegisterCustomer;
using BusBooking.Application.Auth.Commands.RegisterOperator;
using BusBooking.Application.Auth.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BusBooking.API.Controllers;

/// <summary>
/// Authentication Controller - Handles login and registration
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Admin login
    /// </summary>
    [HttpPost("admin-login")]
    public async Task<ActionResult<AuthResponseDto>> AdminLogin([FromBody] LoginCommand command)
    {
        command.Role = BusBooking.Domain.Enums.UserRole.Admin;
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    /// <summary>
    /// Customer/User login
    /// </summary>
    [HttpPost("user-login")]
    public async Task<ActionResult<AuthResponseDto>> UserLogin([FromBody] LoginCommand command)
    {
        command.Role = BusBooking.Domain.Enums.UserRole.Customer;
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    /// <summary>
    /// Bus Operator login
    /// </summary>
    [HttpPost("operator-login")]
    public async Task<ActionResult<AuthResponseDto>> OperatorLogin([FromBody] LoginCommand command)
    {
        command.Role = BusBooking.Domain.Enums.UserRole.BusOperator;
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    /// <summary>
    /// Register a new customer
    /// </summary>
    [HttpPost("user-register")]
    public async Task<ActionResult> RegisterCustomer([FromBody] RegisterCustomerCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { message = "Registration successful" });
    }

    /// <summary>
    /// Register a new bus operator (requires admin approval)
    /// </summary>
    [HttpPost("operator-register")]
    public async Task<ActionResult> RegisterOperator([FromBody] RegisterOperatorCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { message = "Operator registration successful. Waiting for admin approval." });
    }
}