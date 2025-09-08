using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.DTO
{
    /// <summary>
    /// UserDTO
    /// </summary>
    public class UserDTO
    {
        /// <summary> id </summary>
        public Guid Gid { get; set; }

        /// <summary> Sure-Name </summary>
        public string SureName { get; set; }

        /// <summary> User-Name </summary>
        public string UserName { get; set; }
    }
}
