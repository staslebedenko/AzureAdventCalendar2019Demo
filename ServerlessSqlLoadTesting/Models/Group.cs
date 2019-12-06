using System.Collections.Generic;

namespace ServerlessSqlLoadTesting
{
    public class Group
    {
        public Group()
        {
            this.Persons = new HashSet<Person>();
        }

        public ICollection<Person> Persons { get; set; }

        public string Id { get; set; }
        public string Name { get; set; }
    }
}
