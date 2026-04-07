using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.MVC.Models
{
    public class StudentProfile
    {
        public int Id { get; set; }
        public string IdentityUserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public int StudentNumber { get; set; }
        public DateTime DateOfBirth { get; set; }

        public List<CourseEnrolment> Enrolments { get; set; }
        public List<AssignmentResult> AssignmentResults { get; set; }
        public List<ExamResult> ExamResults { get; set; }
    }
}