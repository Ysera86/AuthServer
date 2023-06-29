using AuthServer.Core.GenericServices;
using AuthServer.Core.Repositories;
using AuthServer.Core.UnitOfWork;
using AuthServer.Data.Repositories;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using SharedLibrary.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.Service.Services
{
    public class GenericService<TEntity, TDto> : IGenericService<TEntity, TDto> where TEntity : class where TDto : class
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<TEntity> _genericRepository;

        public GenericService(IUnitOfWork unitOfWork, IGenericRepository<TEntity> genericRepository)
        {
            _unitOfWork = unitOfWork;
            _genericRepository = genericRepository;
        }

        public async Task<Response<TDto>> AddAsync(TDto entity)
        {
            var newEntity = ObjectMapper.Mapper.Map<TEntity>(entity);
            await _genericRepository.AddAsync(newEntity);
            await _unitOfWork.CommitAsync();

            var newDto = ObjectMapper.Mapper.Map<TDto>(newEntity);
            return Response<TDto>.Success(newDto, 200);
        }

        public async Task<Response<IEnumerable<TDto>>> GetAllAsync()
        {
            var entities = await _genericRepository.GetAllAsync();
            var dtos = ObjectMapper.Mapper.Map<IEnumerable<TDto>>(entities);
            return Response<IEnumerable<TDto>>.Success(dtos, 200);
        }

        public async Task<Response<TDto>> GetByIdAsync(int id)
        {
            var entity = await _genericRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return Response<TDto>.Fail("Id not found", 404, true);
            }
            var dto = ObjectMapper.Mapper.Map<TDto>(entity);
            return  Response<TDto>.Success(dto, 200);
        }

        public async Task<Response<NoDataDto>> Remove(int id)
        {
            var entity = await _genericRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return Response<NoDataDto>.Fail("Id not found", 404, true);
            }
           _genericRepository.Remove(entity);
            await _unitOfWork.CommitAsync();
            return Response<NoDataDto>.Success(204);
        }

        public async Task<Response<NoDataDto>> Update(TDto dto, int id)
        {
            var entity = await _genericRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return Response<NoDataDto>.Fail("Id not found", 404, true);
            }
            var updateEntity = ObjectMapper.Mapper.Map<TEntity>(dto);
            _genericRepository.Update(updateEntity);
            await _unitOfWork.CommitAsync();
            return Response<NoDataDto>.Success(204);
        }

        public async Task<Response<IEnumerable<TDto>>> Where(Expression<Func<TEntity, bool>> predicate)
        {
            var list = await _genericRepository.Where(predicate).ToListAsync();
            var dtoList = ObjectMapper.Mapper.Map<IEnumerable<TDto>>(list);
            return Response<IEnumerable<TDto>>.Success(dtoList, 200);
             
        }
    }
}
