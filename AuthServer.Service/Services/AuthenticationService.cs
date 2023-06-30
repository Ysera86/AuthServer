using AuthServer.Core.Configuration;
using AuthServer.Core.Dtos;
using AuthServer.Core.GenericServices;
using AuthServer.Core.Models;
using AuthServer.Core.Repositories;
using AuthServer.Core.Services;
using AuthServer.Core.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedLibrary.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.Service.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly List<Client> _clients;
        private readonly ITokenService _tokenService;
        private readonly UserManager<UserApp> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<UserRefreshToken> _repository;
        public AuthenticationService(IOptions<List<Client>> optionsClient, ITokenService tokenService, UserManager<UserApp> userManager, IUnitOfWork unitOfWork, IGenericRepository<UserRefreshToken> repository)
        {
            _clients = optionsClient.Value;
            _tokenService = tokenService;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _repository = repository;
        }

        public async Task<Response<TokenDto>> CreateTokenAsync(LoginDto loginDto)
        {
            if (loginDto == null)
            {
                throw new ArgumentNullException(nameof(loginDto));
            }

            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
                return Response<TokenDto>.Fail("Email or Password error", 400, true); // vague message for malevolent users

            if (!await _userManager.CheckPasswordAsync(user, loginDto.Password))
                return Response<TokenDto>.Fail("Email or Password error", 400, true); // vague message for malevolent users

            var token = _tokenService.CreateToken(user);
            var userRefreshToken = await _repository.Where(x => x.UserId == user.Id).SingleOrDefaultAsync();
            if (userRefreshToken == null)
                await _repository.AddAsync(new UserRefreshToken { UserId = user.Id, Code = token.RefreshToken, Expiration = token.RefreshTokenExpiration });
            else
            {
                userRefreshToken.Code = token.RefreshToken;
                userRefreshToken.Expiration = token.RefreshTokenExpiration;
            }

            await _unitOfWork.CommitAsync();
            return Response<TokenDto>.Success(token, 200);
        }

        public Response<ClientTokenDto> CreateTokenByClient(ClientLoginDto clientLoginDto)
        {
            var client = _clients.SingleOrDefault(x => x.Id == clientLoginDto.ClientId && x.Secret == clientLoginDto.ClientSecret);
            if (client == null)
                return Response<ClientTokenDto>.Fail("Client or Client secret not found", 400, true); // vague message for malevolent users

            var token = _tokenService.CreateTokenByClient(client);
            return Response<ClientTokenDto>.Success(token, 200);
        }

        public async Task<Response<TokenDto>> CreateTokenByRefreshTokenAsync(string refreshToken)
        {
            var refreshTokenFromDb = await _repository.Where(x => x.Code == refreshToken).SingleOrDefaultAsync();
            if (refreshTokenFromDb == null)
                return Response<TokenDto>.Fail("Refreshtoken not found", 404, true);

            var user = await _userManager.FindByIdAsync(refreshTokenFromDb.UserId);
            if (user == null)
                return Response<TokenDto>.Fail("User not found", 404, true);

            var tokenDto = _tokenService.CreateToken(user);
            refreshTokenFromDb.Code = tokenDto.RefreshToken;
            refreshTokenFromDb.Expiration = tokenDto.RefreshTokenExpiration;

            await _unitOfWork.CommitAsync();

            return Response<TokenDto>.Success(tokenDto, 200);
        }

        public async Task<Response<NoDataDto>> RevokeRefreshTokenAsync(string refreshToken)
        {
            var refreshTokenFromDb = await _repository.Where(x => x.Code == refreshToken).SingleOrDefaultAsync();
            if (refreshTokenFromDb == null)
                return Response<NoDataDto>.Fail("Refreshtoken not found", 404, true);

            _repository.Remove(refreshTokenFromDb);
            await _unitOfWork.CommitAsync();

            return Response<NoDataDto>.Success(200);
        }
    }
}
