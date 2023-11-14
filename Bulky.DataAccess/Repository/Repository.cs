using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        internal DbSet<T> DbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            this.DbSet = _context.Set<T>();
        }
        public void Add(T entity)
        {
            //throw new NotImplementedException();
            DbSet.Add(entity);
        }

        public T Get(Expression<Func<T, bool>> filter)
        {
            //throw new NotImplementedException();
            IQueryable<T> query = DbSet;
            query = query.Where(filter);
            return query.FirstOrDefault();
        }

        public IEnumerable<T> GetAll()
        {
            // throw new NotImplementedException();
            IQueryable<T> query = DbSet;
            return query.ToList();
        }

        public void Remove(T entity)
        {
            //throw new NotImplementedException();
            DbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            //throw new NotImplementedException();
            DbSet.RemoveRange(entities);
        }
    }
}
