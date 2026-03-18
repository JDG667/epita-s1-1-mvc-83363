using Microsoft.AspNetCore.Mvc;
using Library.MVC.Data; 
using Library.MVC.Models; 
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Library.MVC.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BooksController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string searchString, string category, string availability, string sortOrder)
        {
         
            ViewData["CurrentSort"] = sortOrder;
            ViewData["TitleSortParm"] = string.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewData["AuthorSortParm"] = sortOrder == "Author" ? "author_desc" : "Author";

            IQueryable<Book> booksQuery = _context.Books;

         
            if (!string.IsNullOrEmpty(searchString))
            {
                booksQuery = booksQuery.Where(b => b.Title.Contains(searchString) || b.Author.Contains(searchString));
            }
            if (!string.IsNullOrEmpty(category))
            {
                booksQuery = booksQuery.Where(b => b.Category == category);
            }

         
         
            var activeLoanedIds = await _context.Loans
                .Where(l => l.ReturnedDate == null)
                .Select(l => l.BookId)
                .ToListAsync();

         
            if (!string.IsNullOrEmpty(availability))
            {
                if (availability == "Available")
                {
                    booksQuery = booksQuery.Where(b => !activeLoanedIds.Contains(b.Id));
                }
                else if (availability == "OnLoan")
                {
                    booksQuery = booksQuery.Where(b => activeLoanedIds.Contains(b.Id));
                }
            }

         
            booksQuery = sortOrder switch
            {
                "title_desc" => booksQuery.OrderByDescending(b => b.Title),
                "Author" => booksQuery.OrderBy(b => b.Author),
                "author_desc" => booksQuery.OrderByDescending(b => b.Author),
                _ => booksQuery.OrderBy(b => b.Title),
            };

         
            var books = await booksQuery.AsNoTracking().ToListAsync();

         
            foreach (var book in books)
            {
                book.IsAvailable = !activeLoanedIds.Contains(book.Id);
            }

         
            var categories = await _context.Books.Select(b => b.Category).Where(c => c != null).Distinct().ToListAsync();
            ViewData["Categories"] = new SelectList(categories);
            ViewBag.CurrentSearch = searchString;

            return View(books);
        }

        // GET: Admin/Books/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }



        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book)
        {
            if (ModelState.IsValid)
            {
                _context.Books.Add(book);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Books/Edit/5
        public IActionResult Edit(int id)
        {
            var book = _context.Books.Find(id);
            if (book == null) return NotFound();
            return View(book);
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Book book)
        {
            if (ModelState.IsValid)
            {
                _context.Books.Update(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }
        // GET: Admin/Books/Delete/5
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var book = _context.Books.Find(id);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        // POST: Admin/Books/Delete/5
        [HttpPost, ActionName("Delete")] 
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) 
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}