using System.ComponentModel.DataAnnotations;

namespace LM.Models;

public class PublisherModel : Auditable
{
    public int PublisherId { get; set; }
    public string PublisherName { get; set; }
    public string Description { get; set; }
    public string Address { get; set; }
    public string Phone { get; set; }
}

