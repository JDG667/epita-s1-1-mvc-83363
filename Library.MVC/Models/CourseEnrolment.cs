namespace Library.MVC.Models
{
    public class CourseEnrolment
    {
        public int Id { get; set; }
        public int StudentProfileId { get; set; }
        public int CourseId { get; set; }
        public DateTime EnrolDate { get; set; }
        public string Status { get; set; }

        public StudentProfile Student { get; set; }
        public Course Course { get; set; }
        public List<AttendanceRecord> AttendanceRecords { get; set; }
    }
}