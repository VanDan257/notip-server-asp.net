using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using notip_server.Dto;
using notip_server.Interfaces;
using notip_server.ViewModel.Friend;
using notip_server.ViewModel.User;

namespace notip_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FriendController : ControllerBase
    {
        private readonly IFriendService friendService;
        private readonly IUserService _userService;

        public FriendController(IFriendService friendService, IHttpContextAccessor contextAccessor, IUserService userService)
        {
            this.friendService = friendService;
            _userService = userService;
        }

        [HttpGet("get-list-friend")]
        public async Task<IActionResult> GetListFriend(GetContactRequest request)
        {
            ResponseAPI responseAPI = new ResponseAPI();

            try
            {
                var userSession = await _userService.GetCurrentUserAsync();
                responseAPI.Data = await friendService.GetListFriend(userSession.Id, request);

                return Ok(responseAPI);
            }
            catch (Exception ex)
            {
                responseAPI.Message = ex.Message;
                return BadRequest(responseAPI);
            }
        }
        
        [HttpGet("get-list-friend-invite")]
        public async Task<IActionResult> GetListFriendInvite([FromQuery]GetContactRequest request)
        {
            ResponseAPI responseAPI = new ResponseAPI();

            try
            {
                var userSession = await _userService.GetCurrentUserAsync();
                responseAPI.Data = await friendService.GetListFriendInvite(userSession.Id, request);

                return Ok(responseAPI);
            }
            catch (Exception ex)
            {
                responseAPI.Message = ex.Message;
                return BadRequest(responseAPI);
            }
        }

        [Route("send-friend-request")]
        [HttpPost]
        public async Task<IActionResult> SendFriendRequest(FriendRequest receiver)
        {
            ResponseAPI responseAPI = new ResponseAPI();

            try
            {
                var userSession = await _userService.GetCurrentUserAsync();
                await friendService.SendFriendRequest(userSession.Id, receiver.UserCode);

                return Ok(responseAPI);
            }
            catch (Exception ex)
            {
                responseAPI.Message = ex.Message;
                return BadRequest(responseAPI);
            }
        }

        [Route("accept-friend-request")]
        [HttpPatch]
        public async Task<IActionResult> AcceptFriendRequest(FriendRequest receiver)
        {
            ResponseAPI responseAPI = new ResponseAPI();

            try
            {
                var userSession = await _userService.GetCurrentUserAsync();
                await friendService.AcceptFriendRequest(userSession.Id, receiver.UserCode);

                return Ok();
            }
            catch (Exception ex)
            {
                responseAPI.Message = ex.Message;
                return BadRequest(responseAPI);
            }
        }

        [Route("cancel-friend-request")]
        [HttpPatch]
        public async Task<IActionResult> CancelFriendRequest(FriendRequest receiver)
        {
            ResponseAPI responseAPI = new ResponseAPI();

            try
            {
                var userSession = await _userService.GetCurrentUserAsync();
                await friendService.CancelFriendRequest(userSession.Id, receiver.UserCode);

                return Ok();
            }
            catch (Exception ex)
            {
                responseAPI.Message = ex.Message;
                return BadRequest(responseAPI);
            }
        }

        [Route("block-user")]
        [HttpPost]
        public async Task<IActionResult> BlockUser(FriendRequest receiver)
        {
            ResponseAPI responseAPI = new ResponseAPI();

            try
            {
                var userSession = await _userService.GetCurrentUserAsync();
                await friendService.BlockUser(userSession.Id, receiver.UserCode);

                return Ok(responseAPI);
            }
            catch (Exception ex)
            {
                responseAPI.Message = ex.Message;
                return BadRequest(responseAPI);
            }
        }

        [Route("unblock-user")]
        [HttpPatch]
        public async Task<IActionResult> UnBlockUser(FriendRequest receiver)
        {
            ResponseAPI responseAPI = new ResponseAPI();

            try
            {
                var userSession = await _userService.GetCurrentUserAsync();
                await friendService.UnBlockUser(userSession.Id, receiver.UserCode);

                return Ok();
            }
            catch (Exception ex)
            {
                responseAPI.Message = ex.Message;
                return BadRequest(responseAPI);
            }
        }

    }
}
