using Dtm.EFCore.EntityFrameworkContext;
using Dtmcli;
using Microsoft.EntityFrameworkCore;

namespace Dtm.EFCore.Package
{
    public class FullMsg : Msg
    {
        private readonly IDbContext _dbContext;
        private readonly string _queryPreparedUrl;

        public FullMsg(IDtmClient dtmHttpClient, IBranchBarrierFactory branchBarrierFactory, IDbContext dbContext, string gid, string queryPreparedUrl) 
            : base(dtmHttpClient, branchBarrierFactory, gid)
        {
            _dbContext = dbContext;
            _queryPreparedUrl = queryPreparedUrl;
        }

        //for mock
        public FullMsg()
            : base(default, default, default)
        {
            
        }

        public virtual async Task DoAndSubmitDB(Func<Task> busiCall, CancellationToken cancellationToken = default)
        {
            using var conn = _dbContext.Database.GetDbConnection();
            await base.DoAndSubmitDB(_queryPreparedUrl, conn, async trans =>
            {
                await _dbContext.Database.UseTransactionAsync(trans);
                await busiCall();
            }, cancellationToken);
        }

        public virtual async Task Prepare(CancellationToken cancellationToken = default)
        {
            await base.Prepare(_queryPreparedUrl, cancellationToken);
        }
    }
}
