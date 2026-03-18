namespace Library.MVC.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Isbn { get; set; }    //int?
        public string Category { get; set; }
        public bool IsAvailable { get; set; }
        

    }
}
