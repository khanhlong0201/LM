using System.ComponentModel.DataAnnotations;

namespace LM.Models;

public class ReservationModel : Auditable
{
    public int ReservationId { get; set; }
    public DateTime ExpectedPickupDate { get; set; }
    public string Status { get; set; }
    public int UserId { get; set; }
    public int BookId { get; set; }
}

