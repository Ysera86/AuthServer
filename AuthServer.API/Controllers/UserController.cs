using AuthServer.Core.Dtos;
using AuthServer.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AuthServer.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : CustomBaseController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // api/user
        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserDto createUserDto)
        {
            return ActionResultInstance(await _userService.CreateUserAsync(createUserDto));
        }

        // api/user
        [Authorize] // bu method mutlaka token istiyor.
        [HttpGet]
        public async Task<IActionResult> GetUser()
        {
            // token içinden Name claimi buluyor : userApp.UserName
            // eğer bu şekilde düzgün vermeseydik 
            // HttpContext.User.Claims.Where(x => x.Type == "UserName").FirstOrDefault(); vs yapmam gerekirdi burada.
            return ActionResultInstance(await _userService.GetUserByNameAsync(HttpContext.User.Identity.Name));
        }
    }
}
