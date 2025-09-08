using Domin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.DTO
{
    /// <summary>
    /// BookDTO
    /// </summary>
    public class BookDTO
    {
        /// <summary> id </summary>
        public Guid Gid { get; set; }

        /// <summary> Title </summary>
        public string Title { get; set; }

        /// <summary> Description </summary>
        public string Description { get; set; }

        /// <summary> authors </summary>
        public ICollection<PersonDTO> Authors { get; set; } = new List<PersonDTO>();

        /// <summary> translators </summary>
        public ICollection<PersonDTO> Translators { get; set; } = new List<PersonDTO>();

        /// <summary> categury id </summary>
        public Guid CategoryId { get; set; }

        /// <summary> comments </summary>
        public ICollection<CommentDTO> Comments { get; set; } = new List<CommentDTO>();

        /// <summary> pictures </summary>
        public ICollection<String> Pics { get; set; } = new List<String>();
    }
}
