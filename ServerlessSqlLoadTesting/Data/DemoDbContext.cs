using Microsoft.EntityFrameworkCore;

namespace ServerlessSqlLoadTesting
{
    public class DemoDbContext : DbContext
    {
        public DemoDbContext(DbContextOptions<DemoDbContext> options) : base(options)
        {
        }

        public DbSet<Person> Person { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ContextConfig.Configure(modelBuilder);
            base.OnModelCreating(modelBuilder);
        }

        /* open the command line via CMD, install tools (if there are none present)
           * dotnet tool install --global dotnet-ef --version 2.2
            * dotnet ef migrations add InitialCreate --context FunctionDbContext
            * dotnet ef database update
           */
    }
}
