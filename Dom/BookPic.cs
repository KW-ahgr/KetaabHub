using Domin;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domin;

public class BookPic
{
    public long Id { get; set; }
    public Guid Gid { get; set; } // = Guid.NewGuid();
    public byte[] Content { get; set; } = default!;
}
