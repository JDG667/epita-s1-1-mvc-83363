namespace Library.MVC.Models.ViewModels
{
    public class LoanVM
    {
        public Loan Loan { get; set; }
        public IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>? MemberList { get; set; }
        public IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>? BookList { get; set; }
    }
}
