using Dtm.EFCore.EntityFrameworkContext;
using Dtmcli;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtm.EFCore.Package
{
    public interface IFullDtmTransFactory : IDtmTransFactory
    {
        Saga NewSaga();

        FullMsg NewFullMsg();

        FullMsg NewFullMsg(string gid);
    }

    public class FullDtmTransFactory : IFullDtmTransFactory
    {
        private readonly IDtmClient _cient;
        private readonly IBranchBarrierFactory _branchBarrierFactory;
        private readonly IDbContext _dbContext;
        private readonly IOptions<DtmOptionsExt> _options;

        public FullDtmTransFactory(IDtmClient client,
            IBranchBarrierFactory branchBarrierFactory,
            IDbContext dbContext,
            IOptions<DtmOptionsExt> options)
        {
            _cient = client;
            _branchBarrierFactory = branchBarrierFactory;
            _dbContext = dbContext;
            _options = options;
        }

        #region Original

        public Msg NewMsg(string gid)
        {
            return new Msg(_cient, _branchBarrierFactory, gid);
        }

        public Saga NewSaga(string gid)
        {
            return new Saga(_cient, gid);
        }

        #endregion

        public Saga NewSaga()
        {
            var gid = _cient.GenGid(default).Result;
            return NewSaga(gid);
        }

        public FullMsg NewFullMsg()
        {
            var gid = _cient.GenGid(default).Result;
            return NewFullMsg(gid);
        }

        public FullMsg NewFullMsg(string gid)
        {
            var queryPreparedUrl = $"http://{_options.Value.HostName}{_options.Value.QueryPreparedPath}";
            return new FullMsg(_cient, _branchBarrierFactory, _dbContext, gid, queryPreparedUrl);
        }
    }
}
