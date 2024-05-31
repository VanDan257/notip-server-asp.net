using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using notip_server.Dto;
using notip_server.Interfaces;
using notip_server.Models;
using notip_server.ViewModel.User;
using System.Net.WebSockets;

namespace notip_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private IUserService _userService;
        private readonly IHttpContextAccessor _contextAccessor;

        public UsersController(IUserService userService, IHttpContextAccessor contextAccessor, UserManager<User> userManager)
        {
            _userService = userService;
            _contextAccessor = contextAccessor;
            _userManager = userManager;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            ResponseAPI responseAPI = new ResponseAPI();

            try
            {

                string userSession = SystemAuthorization.GetCurrentUser(_contextAccessor);
                Guid.TryParse(userSession, out var userId);
                responseAPI.Data = await _userService.GetProfile(userId);

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
                Guid.TryParse(userSession, out var userId);
                responseAPI.Data = await _userService.UpdateProfile(userId, request);

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
            ResponseAPI responseAPI = new ResponseAPI();
            try
            {
                string userSession = SystemAuthorization.GetCurrentUser(_contextAccessor);
                Guid.TryParse(userSession, out var userId);
                var user = await _userService.GetContact(userId, request);
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
