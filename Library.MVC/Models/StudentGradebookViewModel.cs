namespace Library.MVC.Models
{
    public class StudentGradebookViewModel
    {
        public StudentProfile Student { get; set; }
        public List<CourseEnrolment> Enrolments { get; set; }
    }
}
