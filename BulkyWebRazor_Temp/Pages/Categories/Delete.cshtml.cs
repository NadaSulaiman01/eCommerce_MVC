using BulkyWebRazor_Temp.Data;
using BulkyWebRazor_Temp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor_Temp.Pages.Categories
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        [BindProperty]
        public Category? Category { get; set; }

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }
        public void OnGet(int? Id)
        {
            //Category = _context.Categories.Find(Id);
            if (Id != 0 && Id != null)
            {
                Category = _context.Categories.Find(Id);
            }

        }

        public IActionResult OnPost()
        {
            if(Category != null) { 
            _context.Categories.Remove(Category);
            _context.SaveChanges();
                TempData["success"] = "Category deleted successfully";
                return RedirectToPage("Index");
            }
            else
            {
                return Page();
            }
        }
    }
}
