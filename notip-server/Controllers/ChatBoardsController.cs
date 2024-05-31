using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using notip_server.Dto;
using notip_server.Interfaces;
using notip_server.ViewModel.ChatBoard;

namespace notip_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatBoardsController : ControllerBase
    {
        private IChatBoardService _chatBoardService;
        private readonly IHttpContextAccessor _contextAccessor;

        public ChatBoardsController(IChatBoardService chatBoardService, IHttpContextAccessor contextAccessor)
        {
            _chatBoardService = chatBoardService;
            _contextAccessor = contextAccessor;
        }

        [Route("get-history")]
        [HttpGet]
        public async Task<IActionResult> GetHistory()
        {
            ResponseAPI responseAPI = new ResponseAPI();

            try
            {
                string userSession = SystemAuthorization.GetCurrentUser(_contextAccessor);
                Guid.TryParse(userSession, out var userId);
                responseAPI.Data = await _chatBoardService.GetHistory(userId);

                return Ok(responseAPI);
            }
            catch (Exception ex)
            {
                responseAPI.Message = ex.Message;
                return BadRequest(responseAPI);
            }
        }

        [Route("search-group")]
        [HttpGet]
        public async Task<IActionResult> SearchChatGroup(string keySearch)
        {
            ResponseAPI responseAPI = new ResponseAPI();
            try
            {
                string userSession = SystemAuthorization.GetCurrentUser(_contextAccessor);
                Guid.TryParse(userSession, out var userId);
                responseAPI.Data = await _chatBoardService.SearchChatGroup(userId, keySearch);

                return Ok(responseAPI);
            }
            catch (Exception ex)
            {
                responseAPI.Message = ex.Message;
                return BadRequest(responseAPI);
            }
        }
        
        [Route("access-group")]
        [HttpGet]
        public async Task<IActionResult> AccessChatGroup(Guid groupCode)
        {
            ResponseAPI responseAPI = new ResponseAPI();
            try
            {
                string userSession = SystemAuthorization.GetCurrentUser(_contextAccessor);
                Guid.TryParse(userSession, out var userId);
                responseAPI.Data = await _chatBoardService.AccessChatGroup(userId, groupCode);

                return Ok(responseAPI);
            }
            catch (Exception ex)
            {
                responseAPI.Message = ex.Message;
                return BadRequest(responseAPI);
            }
        }

        [Route("get-info")]
        [HttpGet]
        public async Task<IActionResult> GetInfo(Guid groupCode)
        {
            ResponseAPI responseAPI = new ResponseAPI();

            try
            {
                string userSession = SystemAuthorization.GetCurrentUser(_contextAccessor);
                Guid.TryParse(userSession, out var userId);
                responseAPI.Data = await _chatBoardService.GetInfo(userId, groupCode);

                return Ok(responseAPI);
            }
            catch (Exception ex)
            {
                responseAPI.Message = ex.Message;
                return BadRequest(responseAPI);
            }
        }


        [Route("groups")]
        [HttpPost]
        public async Task<IActionResult> AddGroup(AddGroupRequest request)
        {
            ResponseAPI responseAPI = new ResponseAPI();

            try
            {
                string userSession = SystemAuthorization.GetCurrentUser(_contextAccessor);
                Guid.TryParse(userSession, out var userId);
                await _chatBoardService.AddGroup(userId, request);

                return Ok(responseAPI);
            }
            catch (Exception ex)
            {
                responseAPI.Message = ex.Message;
                return BadRequest(responseAPI);
            }
        }

        [Route("update-group-name")]
        [HttpPut]
        public async Task<IActionResult> UpdateGroupName(UpdateGroupNameRequest request)
        {
            ResponseAPI responseAPI = new ResponseAPI();

            try
            {
                await _chatBoardService.UpdateGroupName(request);
                return Ok(responseAPI);
            }
            catch (Exception ex)
            {
                responseAPI.Message = ex.Message;
                return BadRequest(responseAPI);
            }
        }

        [Route("out-group")]
        [HttpDelete]
        public async Task<IActionResult> OutGroup(Guid groupCode)
        {
            ResponseAPI responseAPI = new ResponseAPI();

            try
            {
                string userSession = SystemAuthorization.GetCurrentUser(_contextAccessor);
                Guid.TryParse(userSession, out var userId);
                await _chatBoardService.OutGroup(userId, groupCode);

                return Ok(responseAPI);
            }
            catch (Exception ex)
            {
                responseAPI.Message = ex.Message;
                return BadRequest(responseAPI);
            }
        }

        [Route("update-group-avatar")]
        [HttpPut]
        public async Task<IActionResult> UpdateGroupAvatar(UpdateGroupAvatarRequest request)
        {
            ResponseAPI responseAPI = new ResponseAPI();

            try
            {
                await _chatBoardService.UpdateGroupAvatar(request);

                return Ok(responseAPI);
            }
            catch (Exception ex)
            {
                responseAPI.Message = ex.Message;
                return BadRequest(responseAPI);
            }
        }

        [Route("add-members-to-group")]
        [HttpPost]
        public async Task<IActionResult> AddMembersToGroup(AddMembersToGroupRequest request)
        {
            ResponseAPI responseAPI = new ResponseAPI();
            try
            {
                await _chatBoardService.AddMembersToGroup(request);

                return Ok(responseAPI);
            }
            catch (Exception ex)
            {
                responseAPI.Message = ex.Message;
                return BadRequest(responseAPI);
            }
        }

        [Route("send-message")]
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromQuery] Guid groupCode)
        {
            ResponseAPI responseAPI = new ResponseAPI();

            try
            {
                string jsonMessage = HttpContext.Request.Form["data"]!;
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                };

                MessageDto message = JsonConvert.DeserializeObject<MessageDto>(jsonMessage, settings);
                message.Attachments = Request.Form.Files.ToList();

                string userSession = SystemAuthorization.GetCurrentUser(_contextAccessor);
                Guid.TryParse(userSession, out var userId);
                await _chatBoardService.SendMessage(userId, groupCode, message);

                return Ok(responseAPI);
            }
            catch (Exception ex)
            {
                responseAPI.Message = ex.Message;
                return BadRequest(responseAPI);
            }
        }

        [Route("get-message-by-group/{groupCode}")]
        [HttpGet]
        public async Task<IActionResult> GetMessageByGroup(Guid groupCode)
        {
            ResponseAPI responseAPI = new ResponseAPI();

            try
            {
                string userSession = SystemAuthorization.GetCurrentUser(_contextAccessor);
                Guid.TryParse(userSession, out var userId);
                responseAPI.Data = await _chatBoardService.GetMessageByGroup(userId, groupCode);

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
