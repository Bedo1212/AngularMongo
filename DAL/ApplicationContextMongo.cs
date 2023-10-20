using DAL.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;
using AspNetCore.Identity.MongoDbCore.Extensions;
using AspNetCore.Identity.Mongo;
using AspNetCore.Identity.Mongo.Model;
using Microsoft.AspNetCore.Identity;
using MongoDbGenericRepository;
using System.Data;

namespace DAL
{
    public class ApplicationDbContextMongo  : MongoDbContext
    {
        private readonly IMongoDatabase _database;

      

        public ApplicationDbContextMongo(IMongoDatabase mongoDatabase) : base(mongoDatabase)
        {
            _database = mongoDatabase;
        }

        
        public IMongoCollection<ApplicationUser> Users => _database.GetCollection<ApplicationUser>("Users");
        public IMongoCollection<Customer> Customers => _database.GetCollection<Customer>("Customers");
        public IMongoCollection<ProductCategory> ProductCategories => _database.GetCollection<ProductCategory>("ProductCategories");
        public IMongoCollection<Product> Products => _database.GetCollection<Product>("Products");
        public IMongoCollection<Order> Orders => _database.GetCollection<Order>("Orders");
        public IMongoCollection<OrderDetail> OrderDetails => _database.GetCollection<OrderDetail>("OrderDetails");
        public IMongoCollection<ApplicationRole> Roles => _database.GetCollection<ApplicationRole>("Roles");




    }
}
