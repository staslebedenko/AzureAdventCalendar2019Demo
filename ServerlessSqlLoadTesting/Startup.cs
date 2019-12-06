using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

using ServerlessSqlLoadTesting;
using System.Data.SqlClient;

[assembly: FunctionsStartup(typeof(Startup))]

namespace ServerlessSqlLoadTesting
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging(options =>
                {
                    options.AddFilter("ServerlessSqlLoadTesting", LogLevel.Information);
                });

            var sqlString = Environment.GetEnvironmentVariable("SqlConnectionString");
            var password = Environment.GetEnvironmentVariable("SqlConnectionPassword");
            var conBuilder = new SqlConnectionStringBuilder(sqlString) { Password = password };

            //builder.Services.AddDbContextPool<FunctionDbContext>(
            builder.Services.AddDbContext<DemoDbContext>(
                options =>
                    {
                        if (!string.IsNullOrEmpty(conBuilder.ConnectionString))
                        {
                            options.UseSqlServer(conBuilder.ConnectionString, providerOptions => providerOptions.EnableRetryOnFailure());
                        }
                    });
        }

    }
}
