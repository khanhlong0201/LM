using System.ComponentModel.DataAnnotations;

namespace LM.Models
{
    public class LocationModel : Auditable
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Vui lòng điền Vị trí sách")]
        public string? LocationName { get; set; }
        public string? Description { get; set; }
    }
}
