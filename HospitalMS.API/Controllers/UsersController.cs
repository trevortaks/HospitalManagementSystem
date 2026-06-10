using HospitalMS.Business.Models;
using HospitalMS.Business.Services;
using HospitalMS.Common.Auth;
using HospitalMS.Common.Constants;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.API.Controllers;

[ApiController]
[Route("api/users")]
[RoleBasedAuth(UserRoles.Admin)]
public sealed class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var users = await userService.GetAllAsync(cancellationToken);
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await userService.GetByIdAsync(id, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPut("{id:guid}/profile")]
    public async Task<IActionResult> UpdateProfile(Guid id, [FromBody] UpdateUserProfileRequest request, CancellationToken cancellationToken)
    {
        try { return Ok(await userService.UpdateProfileAsync(id, request, cancellationToken)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPost("{id:guid}/toggle-active")]
    public async Task<IActionResult> ToggleActive(Guid id, CancellationToken cancellationToken)
    {
        try { return Ok(await userService.ToggleActiveAsync(id, cancellationToken)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }
}
