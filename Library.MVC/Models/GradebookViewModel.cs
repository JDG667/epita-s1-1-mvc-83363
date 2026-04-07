using Library.MVC.Models;


namespace Library.MVC.Models
{
    public class GradebookViewModel
    {
        public Course Course { get; set; }
        public List<StudentProfile> Students { get; set; }
        public List<Assignment> Assignments { get; set; }
        public List<Exam> Exams { get; set; }
    }
}