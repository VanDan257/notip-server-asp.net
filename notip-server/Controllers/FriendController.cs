using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using notip_server.Dto;
using notip_server.Interfaces;

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

        [Route("send-friend-request")]
        [HttpPost]
        public async Task<IActionResult> SendFriendRequest(string receiverCode)
        {
            ResponseAPI responseAPI = new ResponseAPI();

            try
            {
                string userSession = SystemAuthorization.GetCurrentUser(_contextAccessor);
                await friendService.SendFriendRequest(userSession, receiverCode);

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
        public async Task<IActionResult> AcceptFriendRequest(int requestId)
        {
            ResponseAPI responseAPI = new ResponseAPI();

            try
            {
                await friendService.AcceptFriendRequest(requestId);

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
        public async Task<IActionResult> CancelFriendRequest(int requestId)
        {
            ResponseAPI responseAPI = new ResponseAPI();

            try
            {
                await friendService.CancelFriendRequest(requestId);

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
        public async Task<IActionResult> BlockUser(string receiverCode)
        {
            ResponseAPI responseAPI = new ResponseAPI();

            try
            {
                string userSession = SystemAuthorization.GetCurrentUser(_contextAccessor);
                await friendService.BlockUser(userSession, receiverCode);

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
        public async Task<IActionResult> UnBlockUser(int requestId)
        {
            ResponseAPI responseAPI = new ResponseAPI();

            try
            {
                await friendService.UnBlockUser(requestId);

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
