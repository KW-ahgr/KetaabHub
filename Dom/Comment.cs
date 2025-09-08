using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domin
{
    public class Comment
    {
        public long Id { get; set; }
        public Guid Gid { get; set; } // = Guid.NewGuid();
        public string Title { get; set; }
        public string MainBody { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Status CommentStatus { get; set; } = Status.CHECKING;
        public Guid UserId { get; set; }
        public User? User { get; set; }
        public Guid BookId { get; set; }
        public Book? Book { get; set; }
    }

    public enum Status
    {
        ALLOWED,
        BLOCKED,
        CHECKING
    }

}
