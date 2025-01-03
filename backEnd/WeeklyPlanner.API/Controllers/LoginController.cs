using Microsoft.AspNetCore.Mvc;
using WeeklyPlanner.Model.Entities;
using WeeklyPlanner.Model.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace WeeklyPlanner.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly LoginRepository _loginRepository;
        private readonly RoomieRepository _roomieRepository;

        public LoginController(LoginRepository loginRepository, RoomieRepository roomieRepository)
        {
            _loginRepository = loginRepository ?? throw new ArgumentNullException(nameof(loginRepository));
            _roomieRepository = roomieRepository ?? throw new ArgumentNullException(nameof(roomieRepository));
        }
        
        [AllowAnonymous]
        [HttpGet("test")]
        public ActionResult<string> Test()
        {
            return "LoginRepository resolved successfully!";
        }

        [AllowAnonymous]
        [HttpPost("sign-up")]
        public ActionResult Signup([FromBody] SignupRequest signupRequest)
        {
            if (signupRequest == null || string.IsNullOrWhiteSpace(signupRequest.Email) ||
                string.IsNullOrWhiteSpace(signupRequest.PasswordHash) || signupRequest.RoomieNames == null)
            {
                return BadRequest("Invalid signup data.");
            }

            // Check if email already exists
            var existingLogin = _loginRepository.GetLoginByEmail(signupRequest.Email);
            if (existingLogin != null)
            {
                return Conflict("An account with this email already exists."); // HTTP 409 Conflict
            }

            try
            {
                // Create login
                var login = new Login
                {
                    Email = signupRequest.Email,
                    PasswordHash = signupRequest.PasswordHash
                };

                var loginId = _loginRepository.CreateLogin(login);
                if (loginId <= 0)
                {
                    return StatusCode(500, "Failed to create login.");
                }

                // Create associated roomies
                foreach (var roomieName in signupRequest.RoomieNames)
                {
                    var roomie = new Roomie
                    {
                        roomiename = roomieName,
                        loginid = loginId // Associate roomie with the login
                    };

                    if (!_roomieRepository.AddRoomie(roomie))
                    {
                        return StatusCode(500, $"Failed to create roomie: {roomieName}");
                    }
                }

                // Include loginId in the response
                return Ok(new { success = true, message = "Signup successful.", loginId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        [AllowAnonymous]
        [HttpPost("login")]
        public ActionResult Login([FromBody] Login credentials)
        {
            var login = _loginRepository.GetLoginByEmail(credentials.Email);
            if (login == null || login.PasswordHash != credentials.PasswordHash)
            {
                return Unauthorized("Invalid username or password.");
            }


            // Return success response with loginId and basic info
            return Ok(new { Message = "Login successful", Email = login.Email, LoginId = login.LoginId });
        }

        [AllowAnonymous]
        [HttpGet("status")]
        public ActionResult<string> Status()
        {
            return Ok("Login system is operational.");
        }

        // GET: api/login
        [AllowAnonymous]
        [HttpGet]
        public ActionResult<IEnumerable<Login>> GetAllLogins()
        {
            try
            {
                var logins = _loginRepository.GetAllLogins();
                return Ok(logins);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/login/{loginid}
        [AllowAnonymous]
        [HttpGet("{loginid}")]
        public ActionResult<Login> GetLoginById([FromRoute] int loginid)
        {
            try
            {
                var login = _loginRepository.GetLoginById(loginid);
                if (login == null)
                {
                    return NotFound($"Login with ID {loginid} not found.");
                }

                return Ok(login);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/login
        [AllowAnonymous]
        [HttpPost]
        public ActionResult CreateLogin([FromBody] Login login)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var loginId = _loginRepository.CreateLogin(login);
                if (loginId > 0)
                {
                    return Ok("Login created successfully.");
                }

                return BadRequest("Failed to create login.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/login/{loginid}
        [AllowAnonymous]
        [HttpPut("{loginid}")]
        public ActionResult UpdateLogin([FromRoute] int loginid, [FromBody] Login login)
        {
            if (!ModelState.IsValid || login == null || loginid != login.LoginId)
            {
                return BadRequest("Invalid login data or ID mismatch.");
            }

            try
            {
                var result = _loginRepository.UpdateLogin(login);
                if (result)
                {
                    return Ok("Login updated successfully.");
                }

                return BadRequest("Failed to update login.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/login/{loginid}
        [AllowAnonymous]
        [HttpDelete("{loginid}")]
        public ActionResult DeleteLogin([FromRoute] int loginid)
        {
            try
            {
                var result = _loginRepository.DeleteLogin(loginid);
                if (result)
                {
                    return NoContent();
                }

                return BadRequest("Failed to delete login.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

    public class SignupRequest
    {
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public required List<string> RoomieNames { get; set; }
    }
}