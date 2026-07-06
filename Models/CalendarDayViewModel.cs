namespace ObligatorioApiario.Models
{
    public class CalendarDayViewModel
    {
        public DateTime Date { get; set; }
        public int DayNumber => Date.Day;
        public bool IsOtherMonth { get; set; }
        public List<EventViewModel> Events { get; set; } = new List<EventViewModel>();
    }
}
