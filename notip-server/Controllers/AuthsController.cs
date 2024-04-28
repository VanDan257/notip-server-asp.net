using Microsoft.AspNetCore.Mvc;
using notip_server.Dto;
using notip_server.Models;
using notip_server.Interfaces;
using notip_server.Service;
using notip_server.Interfaces;
using System;
using notip_server.ViewModel.Auth;

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

        //[HttpGet("img")]
        //public async Task<IActionResult> DownloadImage(string key)
        //{
        //    try
        //    {
        //        BlobDto result = await _azureStorage.DownloadAsync(key);

        //        return File(result.Content, result.ContentType);
        //        //string path = Path.Combine(_webHostEnvironment.ContentRootPath, key);
        //        //var image = System.IO.File.OpenRead(path);

        //        //return File(image, "image/*");
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest();
        //    }
        //}

        //[HttpGet("file")]
        //public async Task<IActionResult> DownloadFile(string key)
        //{
        //    ResponseAPI responseAPI = new ResponseAPI();

        //    try
        //    {
        //        //string path = Path.Combine(_webHostEnvironment.ContentRootPath, key);
        //        //Stream stream = new FileStream(path, FileMode.Open);
        //        responseAPI.Data = "";
        //        BlobDto result = await _azureStorage.DownloadAsync(key);

        //        return File(result.Content, result.ContentType, result.Name);
        //        //return File(stream, "application/octet-stream", key);
        //    }
        //    catch (Exception ex)
        //    {
        //        responseAPI.Message = ex.Message;
        //        return BadRequest(responseAPI);
        //    }
        //}

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
