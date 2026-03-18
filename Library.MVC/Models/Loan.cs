namespace Library.MVC.Models
{
    public class Loan
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public int MemberId { get; set; }
        public DateTime LoanDate{ get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnedDate { get; set; }

        //Navigation properties (Id from here are foreign keys)

        public Book Book { get; set; }
        public Member Member { get; set; }

        
    }
}
