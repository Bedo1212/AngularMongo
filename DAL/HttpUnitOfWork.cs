// ---------------------------------------------------
 
 
//
 
// ---------------------------------------------------

using DAL.Core;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Linq;

namespace DAL
{
    public class HttpUnitOfWork : UnitOfWork
    {
        //   public HttpUnitOfWork(ApplicationDbContextMongo context, IHttpContextAccessor httpAccessor) : base(context)
        //  {
        //      context.CurrentUserId = httpAccessor.HttpContext?.User.FindFirst(ClaimConstants.Subject)?.Value?.Trim();
        //   }
        public HttpUnitOfWork(IMongoDatabase database) : base(database)
        {
        }
    }
}
