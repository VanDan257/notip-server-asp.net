using Microsoft.AspNetCore.Mvc;
using notip_server.Dto;
using notip_server.Models;
using notip_server.Interfaces;
using notip_server.Service;
using notip_server.Interfaces;
using System;
using notip_server.ViewModel.Auth;
using Microsoft.Extensions.Hosting;

namespace notip_server.Controllers
{
    [Route("api")]
    [ApiController]
    public class AuthsController : ControllerBase
    {
        private IAuthService _authService;
        private IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _contextAccessor;

        public AuthsController(IAuthService authService,IWebHostEnvironment webHostEnvironment, IHttpContextAccessor contextAccessor)
        {
            _authService = authService;
            _webHostEnvironment = webHostEnvironment;
            _contextAccessor = contextAccessor;
        }


        [Route("auths/login")]
        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            ResponseAPI responseAPI = new ResponseAPI();
            try
            {
                AccessToken accessToken = await _authService.Login(request);
                responseAPI.Data = accessToken;
                
                return Ok(responseAPI);
            }
            catch (Exception ex)
            {
                responseAPI.Message = ex.Message;
                return BadRequest(ex.Message);
            }
        }

        [Route("auths/sign-up")]
        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpRequest request)
        {
            ResponseAPI responseAPI = new ResponseAPI();

            try
            {
                await _authService.SignUp(request);
                return Ok(responseAPI);
            }
            catch (Exception ex)
            {
                responseAPI.Message = ex.Message;
                return BadRequest(responseAPI);
            }
        }

        [HttpGet("file")]
        public async Task<IActionResult> DownloadFile(string path)
        {
            ResponseAPI responseAPI = new ResponseAPI();

            try
            {
                string filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot", path);
                if (!System.IO.File.Exists(filePath))
                    return NotFound("File not found.");

                var fileName = filePath.Split("/");
                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

                return File(fileStream, "application/octet-stream", fileName[fileName.Length - 1]);
            }
            catch (Exception ex)
            {
                responseAPI.Message = ex.Message;
                return BadRequest(responseAPI);
            }
        }

        [Route("post-hubconnection")]
        [HttpPost]
        public async Task<IActionResult> PutHubConnection(string key)
        {
            ResponseAPI responseAPI = new ResponseAPI();

            try
            {
                string userSession = SystemAuthorization.GetCurrentUser(_contextAccessor);
                await _authService.PutHubConnection(userSession, key);

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
