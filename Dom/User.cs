using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domin;

public class User
{
    public long Id { get; set; }
    public Guid Gid { get; set; } // = Guid.NewGuid();
    public string SureName { get; set; }
    public string UserName { get; set; }
    public string HashPassword { get; set; }
    public string Slt { get; set; }
    public string Role { get; set; }
    public ICollection<Comment> Comments { get; set; }

}
