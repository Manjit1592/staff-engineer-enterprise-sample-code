using Microsoft.Extensions.Hosting;
using Serilog;

namespace Enterprise.Logging.Extensions;

public static class LoggingExtensions
{
    public static void ConfigureEnterpriseLogging(this IHostBuilder hostBuilder, string serviceName)
    {
        hostBuilder.UseSerilog((context, configuration) =>
        {
            configuration
                .Enrich.WithProperty("Service", serviceName)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Seq("http://enterprise_seq:80");
        });
    }
}