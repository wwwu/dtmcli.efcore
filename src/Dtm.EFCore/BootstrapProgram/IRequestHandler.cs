using Dtm.EFCore.EntityFrameworkContext;
using Dtmcli;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtm.EFCore.BootstrapProgram
{
    public interface IRequestHandler
    {
        Task<object> Query(IQueryCollection query);
    }

    public class RequestHandler : IRequestHandler
    {
        private readonly IDbContext _dbContext;
        private readonly IBranchBarrierFactory _branchBarrierFactory;

        public RequestHandler(IDbContext dbContext,
            IBranchBarrierFactory branchBarrierFactory)
        {
            _dbContext = dbContext;
            _branchBarrierFactory = branchBarrierFactory;
        }

        public async Task<object> Query(IQueryCollection query)
        {
            var bb = _branchBarrierFactory.CreateBranchBarrier(query);
            using var conn = _dbContext.Database.GetDbConnection();
            var res = await bb.QueryPrepared(conn);
            return res;
        }
    }
}
