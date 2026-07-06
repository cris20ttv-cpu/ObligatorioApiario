namespace ObligatorioApiario.Models
{
    public class CalendarViewModel
    {
        public int CurrentYear { get; set; }
        public int CurrentMonth { get; set; }
        public string MonthName { get; set; } = string.Empty;

        public int PreviousYear { get; set; }
        public int PreviousMonth { get; set; }
        public int NextYear { get; set; }
        public int NextMonth { get; set; }

        public List<CalendarDayViewModel> Days { get; set; } = new List<CalendarDayViewModel>();
    }
}
