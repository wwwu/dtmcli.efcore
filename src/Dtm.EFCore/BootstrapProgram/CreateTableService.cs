using DtmCommon;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dtm.EFCore.BootstrapProgram
{
    internal class CreateTableService<TDbContext> : BackgroundService
        where TDbContext : DbContext
    {
        private readonly ILogger<CreateTableService<TDbContext>> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptions<DtmOptionsExt> _dtmOptionsExt;

        public CreateTableService(
            ILogger<CreateTableService<TDbContext>> logger,
            IServiceProvider serviceProvider,
            IOptions<DtmOptionsExt> dtmOptionsExt)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _dtmOptionsExt = dtmOptionsExt;
        }

        internal const string _mysqlScrpit = @"
create table if not exists {0}(
  id bigint(22) PRIMARY KEY AUTO_INCREMENT,
  trans_type varchar(45) default '',
  gid varchar(128) default '',
  branch_id varchar(128) default '',
  op varchar(45) default '',
  barrier_id varchar(45) default '',
  reason varchar(45) default '' comment 'the branch type who insert this record',
  create_time datetime DEFAULT now(),
  update_time datetime DEFAULT now(),
  key(create_time),
  key(update_time),
  UNIQUE key(gid, branch_id, op, barrier_id)
);";

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var opt = _dtmOptionsExt.Value;
            var sql = opt.DBType switch
            {
                "mysql" => string.Format(_mysqlScrpit, opt.BarrierTableName),
                _ => throw new ArgumentException("invalid argument", nameof(DtmOptionsExt.DBType))
            };

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
                var affectedRows = await dbContext.Database.ExecuteSqlRawAsync(sql, stoppingToken);
                if (affectedRows > 0)
                    _logger.LogInformation("barrier table '{name}' created successfully", opt.BarrierTableName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "barrier table '{name}' creation failed", opt.BarrierTableName);
                throw;
            }
        }
    }
}
