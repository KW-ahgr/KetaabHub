using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.DTO
{
    /// <summary>
    /// BookPicDTO
    /// </summary>
    public class BookPicDTO
    {
        /// <summary> id </summary>
        public Guid Gid { get; set; }

        /// <summary> data content </summary>
        public byte[] Content { get; set; } = default!;
    }
}
