using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using notip_server.Dto;
using notip_server.Interfaces;
using notip_server.ViewModel.User;

namespace notip_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;
        private readonly IHttpContextAccessor _contextAccessor;

        public UsersController(IUserService userService, IHttpContextAccessor contextAccessor)
        {
            _userService = userService;
            _contextAccessor = contextAccessor;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            ResponseAPI responseAPI = new ResponseAPI();

            try
            {
                string userSession = SystemAuthorization.GetCurrentUser(_contextAccessor);
                responseAPI.Data = await _userService.GetProfile(userSession);

                return Ok(responseAPI);
            }
            catch (Exception ex)
            {
                responseAPI.Message = ex.Message;
                return BadRequest(responseAPI);
            }
        }


        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request)
        {
            ResponseAPI responseAPI = new ResponseAPI();

            try
            {
                string userSession = SystemAuthorization.GetCurrentUser(_contextAccessor);
                responseAPI.Data = await _userService.UpdateProfile(userSession, request);

                return Ok(responseAPI);
            }
            catch (Exception ex)
            {
                responseAPI.Message = ex.Message;
                return BadRequest(responseAPI);
            }
        }

        [HttpPost("contacts")]
        public async Task<IActionResult> AddContact(UserDto user)
        {
            ResponseAPI responseAPI = new ResponseAPI();
            try
            {
                string userSession = SystemAuthorization.GetCurrentUser(_contextAccessor);
                await _userService.AddContact(userSession, user);
                return Ok(responseAPI);
            }
            catch (Exception ex)
            {
                responseAPI.Message = ex.Message;
                return BadRequest(responseAPI);
            }
        }

        [HttpGet("get-contact")]
        public async Task<IActionResult> GetContact([FromQuery] GetContactRequest request)
        {
            string userSession = SystemAuthorization.GetCurrentUser(_contextAccessor);
            ResponseAPI responseAPI = new ResponseAPI();
            try
            {
                var user = await _userService.GetContact(userSession, request);
                responseAPI.Data = user;

                return Ok(responseAPI);
            }
            catch (Exception ex)
            {
                responseAPI.Message = ex.Message;
                return BadRequest(responseAPI);
            }
        }
    }
}
