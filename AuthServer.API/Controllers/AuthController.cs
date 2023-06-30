using AuthServer.Core.Dtos;
using AuthServer.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace AuthServer.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : CustomBaseController
    {
        private readonly IAuthenticationService _authenticationService;
        public AuthController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        // api/auth/createtoken
        [HttpPost]
        public async Task<IActionResult> CreateToken(LoginDto loginDto)
        {
            var result = await _authenticationService.CreateTokenAsync(loginDto);

            // Yapmaya gerek kalmadı ActionResultInstance döndürerek
            //switch (result.StatusCode)
            //{
            //    case 200:
            //        return Ok(result);
            //        break;
            //    case 404:
            //        return NotFound(result);
            //        break;
            //          .
            //          .
            //          .
            //}
            return ActionResultInstance(result);                            
        }

        [HttpPost]
        public  IActionResult CreateTokenByClient(ClientLoginDto  clientLoginDto)
        {
            var result =  _authenticationService.CreateTokenByClient(clientLoginDto);

            // Yapmaya gerek kalmadı ActionResultInstance döndürerek
            //switch (result.StatusCode)
            //{
            //    case 200:
            //        return Ok(result);
            //        break;
            //    case 404:
            //        return NotFound(result);
            //        break;
            //          .
            //          .
            //          .
            //}
            return ActionResultInstance(result);
        }

        // post put gibi http methodlu fonksiyonlarda mutlaka class alın.
        [HttpPost]
        public async Task<IActionResult> RevokeRefreshToken(RefreshTokenDto refreshTokenDto) 
        {
            var result =  await _authenticationService.RevokeRefreshTokenAsync(refreshTokenDto.Token);

            return ActionResultInstance(result);
        }


        [HttpPost]
        public async Task<IActionResult> CreateTokenByRefreshToken(RefreshTokenDto refreshTokenDto)
        {
            var result = await _authenticationService.CreateTokenByRefreshTokenAsync(refreshTokenDto.Token);

            return ActionResultInstance(result);
        }
    }
}
