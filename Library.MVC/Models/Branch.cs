namespace Library.MVC.Models
{
    public class Branch
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }

        public List<Course> Courses { get; set; }
    }
}