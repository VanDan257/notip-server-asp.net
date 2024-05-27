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
        private readonly IHttpContextAccessor _contextAccessor;

        public FriendController(IFriendService friendService, IHttpContextAccessor contextAccessor)
        {
            this.friendService = friendService;
            _contextAccessor = contextAccessor;
        }

        [HttpGet("get-list-friend")]
        public async Task<IActionResult> GetListFriend(GetContactRequest request)
        {
            ResponseAPI responseAPI = new ResponseAPI();

            try
            {
                string userSession = SystemAuthorization.GetCurrentUser(_contextAccessor);
                await friendService.GetListFriend(userSession, request);

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
                string userSession = SystemAuthorization.GetCurrentUser(_contextAccessor);
                await friendService.GetListFriendInvite(userSession, request);

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
                string userSession = SystemAuthorization.GetCurrentUser(_contextAccessor);
                await friendService.SendFriendRequest(userSession, receiver.UserCode);

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
                string userSession = SystemAuthorization.GetCurrentUser(_contextAccessor);
                await friendService.AcceptFriendRequest(userSession, receiver.UserCode);

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
                string userSession = SystemAuthorization.GetCurrentUser(_contextAccessor);
                await friendService.CancelFriendRequest(userSession, receiver.UserCode);

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
                string userSession = SystemAuthorization.GetCurrentUser(_contextAccessor);
                await friendService.BlockUser(userSession, receiver.UserCode);

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
                string userSession = SystemAuthorization.GetCurrentUser(_contextAccessor);
                await friendService.UnBlockUser(userSession, receiver.UserCode);

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
