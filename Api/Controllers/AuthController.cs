using Api.Security;
using Domin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    /// <summary>
    /// Authentication Point (Controller)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly IJwtService _jwt;
        private readonly IRefreshTokenStore _store;
        private readonly JwtOptions _opt;

        public AuthController(IJwtService jwt, IRefreshTokenStore store, Microsoft.Extensions.Options.IOptions<JwtOptions> opt, UserService userservice)
        {
            _jwt = jwt;
            _store = store;
            _opt = opt.Value;
            _userService = userservice;
        }

        /// <summary>
        /// Logins users
        /// </summary>
        /// <param name="req">LoginRequest</param>
        /// <returns>
        /// A LoginResponse with jwt
        /// </returns>
        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest req)
        {
            User user = _userService.FindUserByArgs(req.Username, req.Password);
            if (user is null) return Unauthorized();

            var (access, exp, accessJti) = _jwt.CreateAccessToken(user.Gid.ToString(), req.Username, [user.Role]);
            var (refreshPlain, refreshEntity) = _jwt.CreateRefreshToken(user.Gid.ToString(), Request.Headers.UserAgent.ToString());
            refreshEntity.AccessJti = accessJti;

            _store.Save(refreshEntity);

            return Ok(new LoginResponse(access, exp, refreshPlain));
        }

        /// <summary>
        /// Refreshes accesstoken
        /// </summary>
        /// <param name="req">RefreshRequest</param>
        /// <returns>
        /// LoginResponse with new jwt
        /// </returns>
        [AllowAnonymous]
        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] RefreshRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.RefreshToken))
                return BadRequest("Refresh token is required.");

            var hash = _jwt.Hash(req.RefreshToken);
            var existing = _store.Get(hash);
            if (existing is null) return Unauthorized("Invalid refresh token.");
            if (!existing.IsActive) return Unauthorized("Refresh token is revoked or expired.");

            User user = _userService.FindUserById(existing.UserId);
            if (user is null) return Unauthorized();

            var (access, exp, accessJti) = _jwt.CreateAccessToken(user.Gid.ToString(), user.UserName, [user.Role]);
            var (refreshPlain, refreshEntity) = _jwt.CreateRefreshToken(user.Gid.ToString(), Request.Headers.UserAgent.ToString());
            refreshEntity.AccessJti = accessJti;

            _store.Revoke(existing, refreshEntity.TokenHash, Request.Headers.UserAgent.ToString());
            _store.Save(refreshEntity);

            return Ok(new LoginResponse(access, exp, refreshPlain));
        }

        /// <summary>
        /// Revokes all logins
        /// </summary>
        /// <param name="req">RevokeRequest</param>
        [Authorize]
        [HttpPost("revoke")]
        public IActionResult Revoke([FromBody] RevokeRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.RefreshToken))
                return BadRequest("Refresh token is required.");

            var hash = _jwt.Hash(req.RefreshToken);
            var existing = _store.Get(hash);
            if (existing is null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isOwner = existing.UserId.ToString() == currentUserId;
            var isAdmin = User.IsInRole("Admin");

            if (!isOwner && !isAdmin) return Forbid();

            _store.Revoke(existing, null, Request.Headers.UserAgent.ToString());
            return NoContent();
        }

        /// <summary>
        /// Adds new User - Its testing - shuld be for just admin later
        /// </summary>
        /// <param name="req">CreateUserRequest</param>
        /// <returns>
        /// the id of created user
        /// </returns>
        [AllowAnonymous]  /* For Testing...  Shuld be for just Admin! */
        [HttpPost("AddUser")]
        public IActionResult AddUser([FromBody] CreateUserRequest req)
        {
            Console.WriteLine($"{req.Username}, {req.Password}, {req.Role}");
            if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest("Username and password are required.");
            User result = _userService.CreateUser(req.SureName, req.Username, req.Password, req.Role);
            return Ok(new { Message = "User created", UserId = result.Gid });
        }

        /// <summary>
        /// Deletes a user by admin
        /// </summary>
        /// <param name="id"></param>
        [HttpDelete("RemoveUser/{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult RemoveUser(string id)
        {
            _userService.DeleteUser(id);
            return Ok(new { Message = "User removed" });
        }

    }
}
