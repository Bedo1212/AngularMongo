// ---------------------------------------------------
 
 
//
 
// ---------------------------------------------------

using DAL.Models;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace DAL.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(DbContext context) : base(context)
        { }

       // private ApplicationDbContext _appContext => (ApplicationDbContext)_context;
    }
}
