using Microsoft.EntityFrameworkCore;

namespace ServerlessSqlLoadTesting
{
    public class ContextConfig
    {
        public static void Configure(ModelBuilder modelBuilder)
        {
            var person = modelBuilder.Entity<Person>();
            person.Property(x => x.Id).ValueGeneratedOnAdd();
            person.Property(x => x.Name);
            person.HasOne(x => x.Group)
                .WithMany(x => x.Persons)
                .OnDelete(DeleteBehavior.Restrict);

            var group = modelBuilder.Entity<Group>();
            person.Property(x => x.Id).ValueGeneratedOnAdd();
            person.Property(x => x.Name);
        }
    }
}
