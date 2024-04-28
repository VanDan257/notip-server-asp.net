using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using notip_server.Dto;
using notip_server.Interfaces;

namespace notip_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CallsController : ControllerBase
    {
        private ICallService _callService;
        private readonly IHttpContextAccessor _contextAccessor;

        public CallsController(ICallService callService, IHttpContextAccessor contextAccessor)
        {
            _callService = callService;
            _contextAccessor = contextAccessor;
        }


        [Route("call/{userCode}")]
        [HttpGet]
        public async Task<IActionResult> Call(string userCode)
        {
            ResponseAPI responeAPI = new ResponseAPI();

            try
            {
                string userSession = SystemAuthorization.GetCurrentUser(_contextAccessor);
                responeAPI.Data = await _callService.Call(userSession, userCode);

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
                string userSession = SystemAuthorization.GetCurrentUser(_contextAccessor);
                responeAPI.Data = await _callService.GetCallHistory(userSession);

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
        public async Task<IActionResult> GetHistoryById(string key)
        {
            ResponseAPI responeAPI = new ResponseAPI();
            try
            {
                string userSession = SystemAuthorization.GetCurrentUser(_contextAccessor);
                responeAPI.Data = await _callService.GetHistoryById(userSession, key);

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
                string userSession = SystemAuthorization.GetCurrentUser(_contextAccessor);
                await _callService.JoinVideoCall(userSession, url);

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
                string userSession = SystemAuthorization.GetCurrentUser(_contextAccessor);
                await _callService.CancelVideoCall(userSession, url);

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
