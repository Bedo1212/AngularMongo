// ---------------------------------------------------
 
 
//
 
// ---------------------------------------------------

using DAL.Models;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DAL.Repositories
{
    public class CustomerRepository : MongoRepository<Customer>, ICustomerRepository
    {
      
      IMongoCollection<Customer> _customerCollection;   

        public CustomerRepository(IMongoDatabase database, string collectionName) : base(database, collectionName)
        {
            //_database = database;
            _customerCollection = database.GetCollection<Customer>("Customer");

            
        }

        public IEnumerable<Customer> GetTopActiveCustomers(int count)
        {
            // Implement logic to get top active customers in MongoDB
            return _customerCollection.Find(FilterDefinition<Customer>.Empty).Limit(count).ToList();
           // throw new NotImplementedException();
        }

        public IEnumerable<Customer> GetAllCustomersData()
        {
            // Implement logic to get all customer data in MongoDB
              return _customerCollection.Find(FilterDefinition<Customer>.Empty).ToList();
           // throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
      /*  public CustomerRepository(ApplicationDbContext context) : base(context)
        { }

        public IEnumerable<Customer> GetTopActiveCustomers(int count)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Customer> GetAllCustomersData()
        {
            return _appContext.Customers
                .Include(c => c.Orders).ThenInclude(o => o.OrderDetails).ThenInclude(d => d.Product)
                .Include(c => c.Orders).ThenInclude(o => o.Cashier)
                .AsSingleQuery()
                .OrderBy(c => c.Name)
                .ToList();
        }

        private ApplicationDbContext _appContext => (ApplicationDbContext)_context;*/
    }
}
