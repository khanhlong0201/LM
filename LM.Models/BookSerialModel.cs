using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LM.Models
{
    public class BookSerialModel : Auditable
    {
        public int Id { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "Vui lòng chọn Sách")]
        public int BookID { get; set; }

        [Required(ErrorMessage = "Vui lòng điền mã sách/serial")]
        public string? SerialNumber { get; set; }
        public string? NoteForAll { get; set; }
        public string? BookName { get; set; }
        public bool IsActive { get; set; }
    }
}
