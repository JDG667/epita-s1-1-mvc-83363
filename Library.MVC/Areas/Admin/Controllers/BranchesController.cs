using Library.MVC.Data;
using Library.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Area("Admin")]
public class BranchesController : Controller
{
    private readonly ApplicationDbContext _context;

    public BranchesController(ApplicationDbContext context) => _context = context;

    // Affiche la liste
    public async Task<IActionResult> Index() => View(await _context.Branches.ToListAsync());

    // Formulaire de création
    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(Branch branch)
    {
        _context.Add(branch);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // Formulaire d'édition
    public async Task<IActionResult> Edit(int id) => View(await _context.Branches.FindAsync(id));

    [HttpPost]
    public async Task<IActionResult> Edit(Branch branch)
    {
        _context.Update(branch);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // Suppression directe
    public async Task<IActionResult> Delete(int id)
    {
        var branch = await _context.Branches.FindAsync(id);
        if (branch != null) _context.Branches.Remove(branch);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}