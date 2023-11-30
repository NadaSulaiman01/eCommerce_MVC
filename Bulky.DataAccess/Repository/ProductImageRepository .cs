using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository
{
    public class ProductImageRepository : Repository<ProductImage>, IProductImageRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductImageRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        //public void save()
        //{
        //    //throw new NotImplementedException();
        //    _context.SaveChanges();
        //}

        public void update(ProductImage productImage)
        {
            //throw new NotImplementedException();
            _context.Update(productImage);
        }
    }
}
