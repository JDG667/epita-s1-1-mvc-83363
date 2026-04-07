namespace Library.MVC.Models
{
    public class ExamResult
    {
        public int Id { get; set; }
        public int ExamId { get; set; }
        public int StudentProfileId { get; set; }
        public int Score { get; set; }
        public string Grade { get; set; }


        public Exam Exam { get; set; }
        public StudentProfile Student { get; set; }
    }
}