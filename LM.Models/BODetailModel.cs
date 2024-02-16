using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LM.Models
{
    public class BODetailModel
    {
        public int Id { get; set; }
        public int BookSerialId { get; set; }
        public string? VoucherNo { get; set; }
        public string? StatusCode { get; set; }
        public string? NoteForAll { get; set; }
    }
}
