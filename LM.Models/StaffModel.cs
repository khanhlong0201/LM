using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LM.Models
{
    public class StaffModel : Auditable
    {
        public string? StaffCode { get; set; }
        public string? FullName { get; set; }
        public string? StaffType { get; set; }
        public string? StaffTypeName { get; set; }
        public string? Password { get; set; }
        public string? Department { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? PlaceOfOrigin { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
    }
}
