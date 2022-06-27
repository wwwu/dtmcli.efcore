using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtm.EFCore.EntityFrameworkContext
{
    public interface IDbContext
    {
        public DatabaseFacade Database { get; }
    }

    public class EFCoreContext<TContext> : IDbContext
        where TContext : DbContext
    {
        private readonly TContext _dbContext;

        public EFCoreContext(TContext dbContext)
        {
            _dbContext = dbContext;
        }

        public DatabaseFacade Database => _dbContext.Database;
    }
}
