﻿using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        //public void save()
        //{
        //    //throw new NotImplementedException();
        //    _context.SaveChanges();
        //}

        public void update(Category category)
        {
            //throw new NotImplementedException();
            _context.Update(category);
        }
    }
}
