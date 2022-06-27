using Dtm.EFCore.EntityFrameworkContext;
using Dtmcli;
using DtmCommon;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dtm.EFCore.Package
{
    public interface IFullBranchBarrierFactory
    {
        FullBranchBarrier? CreateBranchBarrier(IQueryCollection query, ILogger logger = null);
    }

    public class FullBranchBarrierFactory : IFullBranchBarrierFactory
    {
        private readonly ILogger _logger;
        private readonly DtmOptions _options;
        private readonly DbUtils _dbUtils;
        private readonly IDbContext _dbContext;

        public FullBranchBarrierFactory(
            ILoggerFactory loggerFactory, 
            IOptions<DtmOptions> options, 
            DbUtils dbUtils,
            IDbContext dbContext)
        {
            _logger = loggerFactory.CreateLogger<FullBranchBarrierFactory>();
            _options = options.Value;
            _dbUtils = dbUtils;
            _dbContext = dbContext;
        }

        public FullBranchBarrier? CreateBranchBarrier(IQueryCollection query, ILogger logger = null)
        {
            query.TryGetValue("gid", out var gid);
            if (gid.Any() is false)
                return null;

            if (logger == null)
            {
                logger = _logger;
            }
            query.TryGetValue("branch_id", out var branch_id);
            query.TryGetValue("op", out var op);
            query.TryGetValue("trans_type", out var trans_type);
            var branchBarrier = new FullBranchBarrier(trans_type, gid, branch_id, op, _options, _dbUtils, _dbContext, logger);
            if (branchBarrier.IsInValid())
            {
                throw new DtmException("invalid trans info: " + branchBarrier.ToString());
            }

            return branchBarrier;
        }
    }
}
