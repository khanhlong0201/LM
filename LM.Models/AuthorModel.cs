using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LM.Models
{
    public class AuthorModel : Auditable
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Vui lòng điền Tác giả")]
        public string? AuthorName { get; set; }
        public string? Description { get; set; }
    }
}
