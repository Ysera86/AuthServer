using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Dtos;
using System.Linq;
using System.Security.Claims;

namespace MiniApp1.API.Controllers
{
    //[Authorize(Roles = "admin2")] // olmayan bir yetki : 403 forbidden
    [Authorize(Roles = "admin")]
    //[Authorize(Roles = "admin,manager")]
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        [Authorize(Policy = "MuglaPolicy")]
        [Authorize(Roles = "admin", Policy = "AgePolicy")]
        [HttpGet]
        public IActionResult GetStock()
        {
            var userName = HttpContext.User.Identity.Name;
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            // dbde userId ve ya userName göre stok bilgileri çek

            // stock
            // Id, Quantity, Category, User etc

            return Ok($"Stock işlemleri : userName : {userName} - userId : {userId}");
        }
    }
}
