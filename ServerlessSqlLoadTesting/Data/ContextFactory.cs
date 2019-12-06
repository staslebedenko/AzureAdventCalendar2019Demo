using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Data.SqlClient;

namespace ServerlessSqlLoadTesting
{
    public class ContextFactory : IDesignTimeDbContextFactory<DemoDbContext>
    {
        public DemoDbContext CreateDbContext(string[] args)
        {
            var sqlString = Environment.GetEnvironmentVariable("SqlConnectionString");
            var password = Environment.GetEnvironmentVariable("SqlConnectionPassword");
            var builder = new SqlConnectionStringBuilder(sqlString) { Password = password };

            var optionsBuilder = new DbContextOptionsBuilder<DemoDbContext>();
            optionsBuilder.UseSqlServer(builder.ConnectionString);

            return new DemoDbContext(optionsBuilder.Options);
        }
    }
}
