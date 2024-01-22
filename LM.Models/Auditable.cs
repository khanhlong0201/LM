namespace LM.Models
{
    public interface IAuditable
    {
        DateTime? DateCreate { get; set; }
        int? UserCreate { get; set; }
        DateTime? DateUpdate { get; set; }
        int? UserUpdate { get; set; }
        bool IsDeleted { get; set; }
        string? ReasonDelete { get; set; }
        string? UserNameCreate { get; set; }
        string? UserNameUpdate { get; set; }
    }

    public abstract class Auditable : IAuditable
    {
        public DateTime? DateCreate { get; set; }
        public int? UserCreate { get; set; }
        public DateTime? DateUpdate { get; set; }
        public int? UserUpdate { get; set; }
        public bool IsDeleted { get; set; }
        public string? ReasonDelete { get; set; }
        public string? UserNameCreate { get; set; }
        public string? UserNameUpdate { get; set; }

    }
}