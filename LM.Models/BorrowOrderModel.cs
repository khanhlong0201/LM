using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LM.Models
{
    public class BorrowOrderModel : Auditable
    {
        public string? VoucherNo { get; set; }
        public string? StaffCode { get; set; }
        public string? FullName { get; set; }
        public string? Description { get; set; }
        public string? StatusCode { get; set; }
        public string? StatusName { get; set; }
        public DateTime DocDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? PromiseDate { get; set; }
        public string? TypeBO { get; set; }
    }
}
