using Dtm.EFCore.BootstrapProgram;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Dtm.EFCore
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder DtmQueryPreparedRegister(this IApplicationBuilder builder)
        {
            var dtmOptionsExt = builder.ApplicationServices.GetService<IOptions<DtmOptionsExt>>();

            builder.Map(dtmOptionsExt.Value.QueryPreparedPath, b =>
            {
                b.Run(async context =>
                {
                    var handler = context.RequestServices.GetService<IRequestHandler>();
                    var res = await handler.Query(context.Request.Query);
                    await context.Response.WriteAsync($"{{dtm_result:\"{res}\"}});");
                });
            });
            return builder;
        }
    }
}
