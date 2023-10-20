// ---------------------------------------------------
 
 
//
 
// ---------------------------------------------------

using DAL.Repositories;
using DAL.Repositories.Interfaces;
using MongoDB.Driver;
using System;
using System.Linq;

namespace DAL
{
    public class UnitOfWork : IUnitOfWork
    {

        private readonly IMongoDatabase _database;
        private ICustomerRepository _customers;
        private IProductRepository _products;
        private IOrdersRepository _orders;

        public UnitOfWork(IMongoDatabase database)
        {
            _database = database;
        }

        public ICustomerRepository Customers
        {
            get
            {
                return _customers ??= new CustomerRepository(_database, "CustomerCollectionName");
            }
        }

        public IProductRepository Products
        {
            get
            {
                return null; // _products ??= new ProductMongoRepository(_database, "ProductCollectionName");
            }
        }

        public IOrdersRepository Orders
        {
            get
            {
                return null; //_orders ??= new OrdersMongoRepository(_database, "OrdersCollectionName");
            }
        }

        public int SaveChanges()
        {
            // In MongoDB, there is no direct "SaveChanges" method as in Entity Framework.
            // You can handle transactions, if needed, or return a value that makes sense for your use case.
            throw new NotImplementedException("Implement your MongoDB SaveChanges logic here.");
        }

        /* private readonly ApplicationDbContext _context;
         private ICustomerRepository _customers;
         private IProductRepository _products;
         private IOrdersRepository _orders;

         public UnitOfWork(ApplicationDbContext context)
         {
             _context = context;
         }

         public ICustomerRepository Customers
         {
             get
             {
                 _customers ??= new CustomerRepository(_context);

                 return _customers;
             }
         }

         public IProductRepository Products
         {
             get
             {
                 _products ??= new ProductRepository(_context);

                 return _products;
             }
         }

         public IOrdersRepository Orders
         {
             get
             {
                 _orders ??= new OrdersRepository(_context);

                 return _orders;
             }
         }

         public int SaveChanges()
         {
             return _context.SaveChanges();
         }
        */
    }
}
