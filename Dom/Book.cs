using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace Domin
{
    public class Book : IComparable<Book>
    {
        public long Id { get; set; }
        public Guid Gid { get; set; } // = Guid.NewGuid();
        public string Title { get; set; }
        public string Description { get; set; }
        public ICollection<Person> Authors { get; set; } = new List<Person>();
        public ICollection<Person> Translators { get; set; } = new List<Person>();
        public Guid CategoryId { get; set; }
        public BookCat Category { get; set; }
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<String> Pics { get; set; } = new List<String>();

        public int CompareTo(Book? other)
        {
            return Title.CompareTo(other?.Title);
        }
    }

}
