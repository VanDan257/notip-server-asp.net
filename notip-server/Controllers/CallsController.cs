using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using notip_server.Dto;
using notip_server.Interfaces;
using notip_server.Service;

namespace notip_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CallsController : ControllerBase
    {
        private ICallService _callService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IUserService _userService;

        public CallsController(ICallService callService, IHttpContextAccessor contextAccessor, IUserService userService)
        {
            _callService = callService;
            _contextAccessor = contextAccessor;
            _userService = userService;
        }


        [Route("call/{userCode}")]
        [HttpGet]
        public async Task<IActionResult> Call(Guid userCode)
        {
            ResponseAPI responeAPI = new ResponseAPI();

            try
            {
                var userSession = await _userService.GetCurrentUserAsync();
                responeAPI.Data = await _callService.Call(userSession.Id, userCode);

                return Ok(responeAPI);
            }
            catch (Exception ex)
            {
                responeAPI.Message = ex.Message;
                return BadRequest(responeAPI);
            }
        }

        [Route("get-history")]
        [HttpGet]
        public async Task<IActionResult> GetHistory()
        {
            ResponseAPI responeAPI = new ResponseAPI();

            try
            {
                var userSession = await _userService.GetCurrentUserAsync();
                responeAPI.Data = await _callService.GetCallHistory(userSession.Id);

                return Ok(responeAPI);
            }
            catch (Exception ex)
            {
                responeAPI.Message = ex.Message;
                return BadRequest(responeAPI);
            }
        }

        [Route("get-history/{key}")]
        [HttpGet]
        public async Task<IActionResult> GetHistoryById(Guid key)
        {
            ResponseAPI responeAPI = new ResponseAPI();
            try
            {
                var userSession = await _userService.GetCurrentUserAsync();
                responeAPI.Data = await _callService.GetHistoryById(userSession.Id, key);

                return Ok(responeAPI);
            }
            catch (Exception ex)
            {
                responeAPI.Message = ex.Message;
                return BadRequest(responeAPI);
                
            }
        }


        [Route("join-video-call")]
        [HttpGet]
        public async Task<IActionResult> JoinVideoCall(string url)
        {
            ResponseAPI responeAPI = new ResponseAPI();

            try
            {
                var userSession = await _userService.GetCurrentUserAsync();
                await _callService.JoinVideoCall(userSession.Id, url);

                return Ok(responeAPI);
            }
            catch (Exception ex)
            {
                responeAPI.Message = ex.Message;
                return BadRequest(responeAPI);
            }
        }


        [Route("cancel-video-call")]
        [HttpGet]
        public async Task<IActionResult> CancelVideoCall(string url)
        {
            ResponseAPI responeAPI = new ResponseAPI();

            try
            {
                var userSession = await _userService.GetCurrentUserAsync();
                await _callService.CancelVideoCall(userSession.Id, url);

                return Ok(responeAPI);
            }
            catch (Exception ex)
            {
                responeAPI.Message = ex.Message;
                return BadRequest(responeAPI);
            }
        }
    }
}
