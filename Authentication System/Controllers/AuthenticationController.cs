using Authentication_System.DataAccessLayer;
using Authentication_System.Model;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authentication_System.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]

    public class AuthenticationController : ControllerBase
    {
        private readonly ILogger<AuthenticationController> _logger;
        public readonly IAuthenticationDataAccess _authenticationDataAccess;

        public AuthenticationController(ILogger<AuthenticationController> logger, IAuthenticationDataAccess authenticationDataAccess)
        {
            _logger = logger;
            _authenticationDataAccess = authenticationDataAccess;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterUser(RegisterUserRequest request)
        {
            RegisterUserResponse response = new RegisterUserResponse();
            try
            {
                //string userId = "123";
                //string username = await _authenticationDataAccess.GetUsernameById(userId);

                response = await _authenticationDataAccess.RegisterUser(request);
                //_logger.LogInformation("User '{Username}' accessed the 'YourAction' endpoint.", username);

            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return Ok(response);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> UserLogin(UserLoginRequest request)
        {
            UserLoginResponse response = new UserLoginResponse();
            try
            {
                response = await _authenticationDataAccess.UserLogin(request);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AddInformation(AddInformationRequest request)
        {
            AddInformationResponse response = new AddInformationResponse();
            try
            {
                response = await _authenticationDataAccess.AddInformation(request);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return Ok(response);
        }

        [HttpGet]
        [Authorize(Roles = "admin,user")]
        public async Task<IActionResult> GetInformation()
        {
            GetInformationResponse response = new GetInformationResponse();
            try
            {
                response = await _authenticationDataAccess.GetInformation();
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return Ok(response);
        }  
    }
}
