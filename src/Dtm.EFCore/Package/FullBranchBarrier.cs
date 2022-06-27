using Dtm.EFCore.EntityFrameworkContext;
using DtmCommon;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dtm.EFCore.Package
{
    public class FullBranchBarrier : BranchBarrier
    {
        private readonly IDbContext _dbContext;

        public FullBranchBarrier(string transType, string gid, string branchID, string op, DtmOptions options, DbUtils utils, IDbContext dbContext, ILogger logger = null)
            : base(transType, gid, branchID, op, options, utils, logger)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 子事务屏障 https://dtm.pub/practice/barrier.html
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public virtual async Task Barrier(Func<Task> func)
        {
            using var conn = _dbContext.Database.GetDbConnection();
            await base.Call(conn, async trans =>
            {
                await _dbContext.Database.UseTransactionAsync(trans);
                await func();
            });
        }
    }
}
