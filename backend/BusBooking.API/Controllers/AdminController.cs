using BusBooking.Application.Admin.Commands.ApproveOperator;
using BusBooking.Application.Admin.Commands.RejectOperator;
using BusBooking.Application.Admin.Queries.GetPendingOperators;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BusBooking.Domain.Enums;

namespace BusBooking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")] // Only admins can access these endpoints
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("operator-requests")]
    public async Task<ActionResult> GetPendingOperators()
    {
        var result = await _mediator.Send(new GetPendingOperatorsQuery());
        return Ok(result);
    }

    [HttpGet("operators")]
    public async Task<ActionResult> GetAllOperators([FromQuery] OperatorStatus? status)
    {
        var result = await _mediator.Send(new BusBooking.Application.Admin.Queries.GetOperators.GetOperatorsQuery(status));
        return Ok(result);
    }

    [HttpPost("approve-operator/{id}")]
    public async Task<ActionResult> ApproveOperator(Guid id)
    {
        var result = await _mediator.Send(new ApproveOperatorCommand(id));
        if (!result) return NotFound();
        return Ok(new { message = "Operator approved successfully" });
    }

    [HttpPost("reject-operator/{id}")]
    public async Task<ActionResult> RejectOperator(Guid id, [FromBody] string reason)
    {
        var result = await _mediator.Send(new RejectOperatorCommand(id, reason));
        if (!result) return NotFound();
        return Ok(new { message = "Operator rejected successfully" });
    }

    [HttpPost("enable-operator/{id}")]
    public async Task<ActionResult> EnableOperator(Guid id)
    {
        var result = await _mediator.Send(new BusBooking.Application.Admin.Commands.EnableOperator.EnableOperatorCommand(id));
        if (!result) return NotFound();
        return Ok(new { message = "Operator enabled successfully" });
    }

    [HttpPost("disable-operator/{id}")]
    public async Task<ActionResult> DisableOperator(Guid id)
    {
        var result = await _mediator.Send(new BusBooking.Application.Admin.Commands.DisableOperator.DisableOperatorCommand(id));
        if (!result) return NotFound();
        return Ok(new { message = "Operator disabled successfully" });
    }

    // --- Station Management ---
    [HttpGet("stations")]
    public async Task<ActionResult> GetStations()
    {
        var result = await _mediator.Send(new BusBooking.Application.Stations.Queries.GetStations.GetStationsQuery());
        return Ok(result);
    }

    [HttpPost("stations")]
    public async Task<ActionResult> CreateStation([FromBody] BusBooking.Application.Stations.Commands.CreateStation.CreateStationCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { id = result, message = "Station created successfully" });
    }

    // --- Route Management ---
    [HttpGet("routes")]
    public async Task<ActionResult> GetRoutes()
    {
        var result = await _mediator.Send(new BusBooking.Application.Routes.Queries.GetRoutes.GetRoutesQuery());
        return Ok(result);
    }

    [HttpPost("routes")]
    public async Task<ActionResult> CreateRoute([FromBody] BusBooking.Application.Routes.Commands.CreateRoute.CreateRouteCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { id = result, message = "Route created successfully" });
    }

    // --- Bus Management ---
    [HttpGet("fleet/requests")]
    public async Task<ActionResult> GetBusRequests()
    {
        var result = await _mediator.Send(new BusBooking.Application.Admin.Queries.GetPendingBuses.GetPendingBusesQuery());
        return Ok(result);
    }

    [HttpGet("fleet/active")]
    public async Task<ActionResult> GetActiveBuses()
    {
        var result = await _mediator.Send(new BusBooking.Application.Admin.Queries.GetBuses.GetBusesQuery());
        return Ok(result.Where(b => b.Status == BusStatus.Approved && b.IsAvailable).ToList());
    }

    [HttpGet("fleet/history")]
    public async Task<ActionResult> GetAllHistoryBuses()
    {
        var result = await _mediator.Send(new BusBooking.Application.Admin.Queries.GetBuses.GetBusesQuery());
        return Ok(result);
    }

    [HttpPost("approve-bus/{id}")]
    public async Task<ActionResult> ApproveBus(Guid id)
    {
        var result = await _mediator.Send(new BusBooking.Application.Admin.Commands.ApproveBus.ApproveBusCommand(id));
        if (!result) return NotFound();
        return Ok(new { message = "Bus approved successfully" });
    }

    [HttpPost("reject-bus/{id}")]
    public async Task<ActionResult> RejectBus(Guid id, [FromBody] string reason)
    {
        var result = await _mediator.Send(new BusBooking.Application.Admin.Commands.RejectBus.RejectBusCommand(id, reason));
        if (!result) return NotFound();
        return Ok(new { message = "Bus rejected successfully" });
    }

    [HttpPost("add-bus")]
    public async Task<ActionResult> AddBus([FromBody] BusBooking.Application.Admin.Commands.AddBus.AdminAddBusCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { id = result, message = "Bus added successfully" });
    }

    // --- Dashboard Statistics ---
    [HttpGet("dashboard/stats")]
    public async Task<ActionResult> GetDashboardStats()
    {
        var result = await _mediator.Send(new BusBooking.Application.Admin.Queries.GetDashboardStats.GetDashboardStatsQuery());
        return Ok(result);
    }

    // --- Revenue Management ---
    [HttpGet("revenue")]
    public async Task<ActionResult> GetRevenue([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var result = await _mediator.Send(new BusBooking.Application.Admin.Queries.GetRevenue.GetRevenueQuery(startDate, endDate));
        return Ok(result);
    }
}
