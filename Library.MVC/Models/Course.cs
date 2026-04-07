using Library.MVC.Models;
using Serilog.Parsing;


namespace Library.MVC.Models 
{

    public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int BranchId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public Branch Branch { get; set; }
        public List<CourseEnrolment> Enrolments { get; set; }
        public List<Assignment> Assignments { get; set; }
        public List<Exam> Exams { get; set; }

        public int? FacultyProfileId { get; set; } 
        public FacultyProfile? FacultyProfile { get; set; }

        
    }
}
