using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domin
{
    public class BookCat
    {
        public long Id { get; set; }
        public Guid Gid { get; set; } // = Guid.NewGuid();
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<Book> Books { get; set; }
    }
}
