using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.DTO
{
    /// <summary>
    /// PersonDTO
    /// </summary>
    public class PersonDTO
    {
        /// <summary> id </summary>
        public Guid Gid { get; set; }

        /// <summary> Name </summary>
        public string Name { get; set; }

        /// <summary> Description </summary>
        public string Description { get; set; }
    }

}
