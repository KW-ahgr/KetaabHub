using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domin
{

    public class Person
    {
        public long Id { get; set; }
        public Guid Gid { get; set; } // = Guid.NewGuid();
        public string Name { get; set; }
        public string Description { get; set; }
    }

}
