﻿using Bulky.DataAccess.Data;
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

        public T Get(Expression<Func<T, bool>> filter, string? includeProperties = null, bool flag = false)
        {
            //throw new NotImplementedException();
            IQueryable<T> query;
            if (flag)
            {
                query = DbSet;
            }
            else
            {
                query = DbSet.AsNoTracking();

            }
            query = query.Where(filter);
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var property in includeProperties
                    .Split(",", StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(property);
                }

            }
            //var user1 = _context.Users.FirstOrDefault(u=> u.Id == "eca161e1-7c7b-4748-8d70-dd50d08c64dd");
            //var user2 = _context.ApplicationUsers.FirstOrDefault(u => u.Id == "eca161e1-7c7b-4748-8d70-dd50d08c64dd");
			return query.FirstOrDefault();
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter, string? includeProperties = null)
        {
            // throw new NotImplementedException();

            //include properties like Category,Cover
            IQueryable<T> query = DbSet;
            if(filter != null)
            {
                query = query.Where(filter);
            }

            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var property in includeProperties
                    .Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(property);
                }

            }
           
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
