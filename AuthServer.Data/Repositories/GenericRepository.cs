using AuthServer.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.Data.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        private readonly DbContext _context;
        private readonly DbSet<TEntity> _dbSet;

        public GenericRepository(AppDbContext appDbContext)
        {
            _context = appDbContext;
            _dbSet = appDbContext.Set<TEntity>();
        }

        public async Task AddAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity); // şuan memorye eklendi, serviste UnitOfWork.Commit çağrılınca dbye yansicak
        }

        /// <summary>
        /// Burada direk tüm datalatı çektiğimiz için sorguya bişey eklenmez. IQueryable gerek yok tüm dataları direk ToList ile çekmek mantıklı
        /// RoList ile çekilen datada orderby çaıştırmak memoryye TÜM datalatı çekip memorydeki tüm dataları sorgulamak demek.
        /// IQueryable ile çekmek ise, memory üzernde sorgu yazıp ToList olana kadar dbden memorye data çekmemek demek, dbden tüm filtrelerle son anda çekmek demek, bu da performanslı ve etkili
        /// Prodda geriye IQueryable dönmeli
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            // _dbSet.AsQueryable()
            return await _dbSet.ToListAsync();
        }

        public async Task<TEntity> GetByIdAsync(int id)
        {
            // find pk üzerinden arama yapar
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                _context.Entry(entity).State = EntityState.Detached; // takip etmesin, track etmesin ef memoryde. çnk sadece okuyoruz
            }
            return entity;
        }

        public void Remove(TEntity entity)
        {
            _dbSet.Remove(entity);
        }

        public TEntity Update(TEntity entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            return entity;
        }

        /// <summary>
        /// Dönen data üzerinde orderby vs daha çalıştırabilirim istersem servis katmanında, ToList ekleyene kadar dbde çalıştırmayacağı için daha mantıklı
        /// Yazılmış olan tüm sorgular memoryde hazırlanıp ToList ile dbde sorgulayacağı için daha mantıklı
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        {
            return _dbSet.Where(predicate);
        }
    }
}
