using Domin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.DTO
{
    /// <summary>
    /// CommentDTO
    /// </summary>
    public class CommentDTO
    {
        /// <summary> id </summary>
        public Guid Gid { get; set; }

        /// <summary> Title </summary>
        public string Title { get; set; }

        /// <summary> Main Text Body </summary>
        public string MainBody { get; set; }

        /// <summary> Created Date </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary> Comment Status </summary>
        public Status CommentStatus { get; set; } = Status.CHECKING;

        /// <summary> User Id </summary>
        public Guid UserId { get; set; }

        /// <summary> Book Id </summary>
        public Guid BookId { get; set; }
    }
}
