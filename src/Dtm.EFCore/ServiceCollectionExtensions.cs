using Dtm.EFCore;
using Dtm.EFCore.BootstrapProgram;
using Dtm.EFCore.EntityFrameworkContext;
using Dtm.EFCore.Package;
using Dtmcli;
using DtmCommon;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dtm.EFCore
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDtmcli<TContext>(this IServiceCollection services, Action<DtmOptionsExt> setupAction)
            where TContext : DbContext
        {
            if (setupAction == null)
                throw new ArgumentNullException(nameof(setupAction));

            services.AddOptions();
            services.Configure(setupAction);
            services.AddDtmcli((Action<DtmOptions>)setupAction);
            return AddDtmCore<TContext>(services);
        }

        public static IServiceCollection AddDtmcli<TContext>(this IServiceCollection services, IConfiguration configuration, string sectionName = "dtm")
            where TContext : DbContext
        {
            services.Configure<DtmOptionsExt>(configuration.GetSection(sectionName));
            services.AddDtmcli(configuration);
            return AddDtmCore<TContext>(services);
        }

        private static IServiceCollection AddDtmCore<TContext>(IServiceCollection services)
            where TContext : DbContext
        {
            services.AddScoped<IRequestHandler, RequestHandler>();
            services.AddScoped<IDbContext, EFCoreContext<TContext>>();
            services.AddHostedService<CreateTableService<TContext>>();
            services.AddScoped<IFullDtmTransFactory, FullDtmTransFactory>();
            services.AddScoped<IFullBranchBarrierFactory, FullBranchBarrierFactory>();

            return services;
        }
    }
}