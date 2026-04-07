namespace Library.MVC.Models
{
    public class AttendanceRecord
    {
        public int Id { get; set; }
        public int CourseEnrolmentId { get; set; }
        public DateTime Date { get; set; }
        public bool Present { get; set; }

        public CourseEnrolment Enrolment { get; set; }
    }
}