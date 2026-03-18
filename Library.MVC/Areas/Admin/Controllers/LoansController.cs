using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Library.MVC.Data;
using Library.MVC.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Library.MVC.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Library.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class LoansController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoansController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Loans
        public async Task<IActionResult> Index()
        {
            var loans = await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Member)
                .ToListAsync();

            return View(loans); 
        }

        [HttpGet]
        public IActionResult Create()
        {
            var loanVM = new LoanVM
            {
                Loan = new Loan 
                { 
                    LoanDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(14),

                },
                
                MemberList = _context.Members.Select(m => new SelectListItem
                {
                    Text = m.FullName,
                    Value = m.Id.ToString()
                }),
                
                BookList = _context.Books
                    .Where(b => !_context.Loans.Any(l => l.BookId == b.Id && l.ReturnedDate == null))
                    .Select(b => new SelectListItem
                    {
                        Text = b.Title,
                        Value = b.Id.ToString()
                    })
            };
            return View(loanVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LoanVM obj)
        {
            
            if (obj.Loan.BookId > 0 && obj.Loan.MemberId > 0)
            {
            
                _context.Loans.Add(obj.Loan);
                await _context.SaveChangesAsync();

            
                return RedirectToAction(nameof(Index));
            }

            
            
            obj.MemberList = _context.Members.Select(m => new SelectListItem
            {
                Text = m.FullName,
                Value = m.Id.ToString()
            });

            obj.BookList = _context.Books
                .Where(b => !_context.Loans.Any(l => l.BookId == b.Id && l.ReturnedDate == null))
                .Select(b => new SelectListItem
                {
                    Text = b.Title,
                    Value = b.Id.ToString()
                });

            return View(obj);
        }

        // POST: /Admin/Loans/MarkReturned/5
        [HttpPost]
        [ValidateAntiForgeryToken] 
        [ActionName("MarkAsReturned")] 
        public async Task<IActionResult> MarkAsReturned(int id)
        {
            var loan = await _context.Loans.FindAsync(id);
            if (loan == null) return NotFound();

            loan.ReturnedDate = DateTime.Now;

            
            
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReturned()
        {
         
            var returnedLoans = _context.Loans.Where(l => l.ReturnedDate != null);

            if (returnedLoans.Any())
            {
                _context.Loans.RemoveRange(returnedLoans); 
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}