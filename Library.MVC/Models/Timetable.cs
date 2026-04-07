namespace Library.MVC.Models
{
    public class Timetable
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public Course? Course { get; set; }

        public DayOfWeek Day { get; set; } // Lundi, Mardi, etc.
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? Room { get; set; }
    }
}
