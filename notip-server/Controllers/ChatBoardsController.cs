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
        private readonly IUserService _userService;
        private IChatBoardService _chatBoardService;
        private readonly IHttpContextAccessor _contextAccessor;

        public ChatBoardsController(IChatBoardService chatBoardService, IHttpContextAccessor contextAccessor, IUserService userService)
        {
            _chatBoardService = chatBoardService;
            _contextAccessor = contextAccessor;
            _userService = userService;
        }

        [Route("get-history")]
        [HttpGet]
        public async Task<IActionResult> GetHistory()
        {
            ResponseAPI responseAPI = new ResponseAPI();

            try
            {
                var userSession = await _userService.GetCurrentUserAsync();
                responseAPI.Data = await _chatBoardService.GetHistory(userSession.Id);

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
                var userSession = await _userService.GetCurrentUserAsync();
                responseAPI.Data = await _chatBoardService.SearchChatGroup(userSession.Id, keySearch);

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
                var userSession = await _userService.GetCurrentUserAsync();
                responseAPI.Data = await _chatBoardService.AccessChatGroup(userSession.Id, groupCode);

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
                var userSession = await _userService.GetCurrentUserAsync();
                responseAPI.Data = await _chatBoardService.GetInfo(userSession.Id, groupCode);

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
                var userSession = await _userService.GetCurrentUserAsync();
                await _chatBoardService.AddGroup(userSession.Id, request);

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
                var userSession = await _userService.GetCurrentUserAsync();
                await _chatBoardService.OutGroup(userSession.Id, groupCode);

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

                var userSession = await _userService.GetCurrentUserAsync();
                await _chatBoardService.SendMessage(userSession.Id, groupCode, message);

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
                var userSession = await _userService.GetCurrentUserAsync();
                responseAPI.Data = await _chatBoardService.GetMessageByGroup(userSession.Id, groupCode);

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
