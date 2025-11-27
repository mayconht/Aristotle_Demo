using Aristotle.Application.Authorization;
using Aristotle.Application.DTOs;
using Aristotle.Application.Exceptions;
using Aristotle.Application.Extensions;
using Aristotle.Application.Services;
using Aristotle.Domain.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aristotle.Controllers;

/// <summary>
///     UserController provides REST API endpoints for user management operations.
/// </summary>
// Some stuff might not make sense here, but it's just a demo API'. I am building JIT and other stuff around this project as a learning experience
[ApiController]
[Authorize]
[Route("api/[controller]")]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;

    /// <summary>
    ///     Initializes a new instance of the UserController class.
    /// </summary>
    /// <param name="userService">The user service for business operations.</param>
    /// <param name="logger">Logger for controller operations.</param>
    /// <param name="mapper">Mapper for DTO conversions.</param>
    public UserController(IUserService userService, ILogger<UserController> logger, IMapper mapper)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <summary>
    ///     Retrieves the current authenticated user and their claims.
    ///     This endpoint always returns the user's claims from the JWT token.
    ///     It attempts to fetch the user entity from the database, but if the user
    ///     doesn't exist yet (e.g., before JIT provisioning), the user property will be null
    ///     while claims will still be available.
    /// </summary>
    /// <returns>The current user (if exists in database) and their JWT claims.</returns>
    /// <response code="200">User and claims retrieved successfully.</response>
    /// <response code="401">Unauthorized: Authentication required.</response>
    /// <exception cref="ServiceOperationException">Thrown when user lookup fails due to infrastructure errors.</exception>
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var externalUserId = User.GetUserId();
        User? user = null;

        // Attempt to fetch user from database, may be null if user hasn't been provisioned yet
        // The exceptions will propagate to GlobalExceptionHandlingMiddleware for proper error handling
        if ( externalUserId != Guid.Empty) user = await _userService.GetUserByExternalUserIdAsync(externalUserId);

        var groups = User.GetGroups();
        var roles = User.Claims.Where(c => c.Type is "roles" or "role").Select(c => c.Value).Distinct().ToList();

        return Ok(new
        {
            claims = new
            {
                sub = User.GetUserId(),
                email = User.GetEmail(),
                name = User.GetName(),
                groups,
                roles,
                all = User.Claims.Select(c => new { c.Type, c.Value })
            },
            user = user == null ? null : _mapper.Map<UserResponseDto>(user)
        });
    }

    /// <summary>
    ///     Retrieves a user by their external user identifier.
    /// </summary>
    /// <param name="externalUserId">The external user identifier of the user to retrieve.</param>
    /// <returns>The user if found, or a 404 Not Found response if the user doesn't exist.</returns>
    /// <response code="200">User retrieved successfully.</response>
    /// <response code="400">Invalid external user ID provided.</response>
    /// <response code="404">User not found.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpGet("external/{externalUserId:guid}")]
    [Authorize(Policy = Policies.RequireAdminRole)]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserByExternalUserId(Guid externalUserId)
    {
        var user = await _userService.GetUserByExternalUserIdAsync(externalUserId);

        if (user == null) return NotFound();

        return Ok(_mapper.Map<UserResponseDto>(user));
    }
    

    /// <summary>
    ///     Retrieves all users from the system.
    /// </summary>
    /// <returns>A collection of all users in the system.</returns>
    /// <response code="200">Users retrieved successfully (includes empty collection).</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpGet]
    [Authorize(Policy = Policies.RequireAdminRole)]
    [ProducesResponseType(typeof(IEnumerable<UserResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllUsers()
    {
        _logger.LogInformation("Received request to get all users");

        var users = await _userService.GetAllUsersAsync();

        _logger.LogInformation("Successfully retrieved {UserCount} users", users.Count());

        // Return OK even if no users exist - an empty collection is still a valid response
        return Ok(_mapper.Map<IEnumerable<UserResponseDto>>(users));
    }

    /// <summary>
    ///     Wipes the entire user database. This action is irreversible and should only be used in development environments.
    ///     Requires Admin role.
    /// </summary>
    /// <returns>204 No Content if the database was wiped successfully.</returns>
    /// <response code="204">Database wiped successfully.</response>
    /// <response code="401">Unauthorized: Authentication required.</response>
    /// <response code="403">Forbidden: Admin role required or action not allowed in current environment.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpDelete("wipe")]
    [Authorize(Policy = Policies.RequireAdminRole)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> WipeDatabase()
    {
        var adminEmail = User.GetEmail();
        var adminId = User.GetUserId();
        var adminGroups = User.GetGroups();
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var isDevelopment = environment == "Development";

        if (!isDevelopment)
        {
            _logger.LogWarning(
                "AUDIT: Database wipe DENIED - Admin: {AdminEmail} (ID: {AdminId}, Groups: [{Groups}], Environment: {Environment})",
                adminEmail,
                adminId,
                string.Join(", ", adminGroups),
                environment);
            return StatusCode(StatusCodes.Status403Forbidden,
                new { Message = "Wiping the database is only allowed in development environments." });
        }

        await _userService.WipeDatabaseAsync();

        _logger.LogWarning(
            "AUDIT: Database wipe COMPLETED - Admin: {AdminEmail} (ID: {AdminId}, Groups: [{Groups}])",
            adminEmail,
            adminId,
            string.Join(", ", adminGroups));

        return NoContent();
    }



}