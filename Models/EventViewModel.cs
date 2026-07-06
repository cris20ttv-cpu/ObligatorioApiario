namespace ObligatorioApiario.Models
{
    public class EventViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Type { get; set; } = "yellow"; // "yellow" or "red"
        public string? IconClass { get; set; } 
    }
}
