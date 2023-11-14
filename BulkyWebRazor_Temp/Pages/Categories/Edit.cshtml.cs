using BulkyWebRazor_Temp.Data;
using BulkyWebRazor_Temp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor_Temp.Pages.Categories
{
    [BindProperties]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
       // [BindProperty]
        public Category? Category { get; set; }

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }
        public void OnGet(int? id)
        {
            //Category = _context.Categories.Find(Id);
            if (id != 0 && id != null)
            {
                Category = _context.Categories.Find(id);
            } 
            
        }

        public IActionResult OnPost()
        {
            if (Category.Name == Category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "Category name cannot match display order.");
            }
            if (Category.Name != null && Category.Name.ToLower() == "test")
            {
                ModelState.AddModelError("", "Category name cannot be test");
            }
            if (ModelState.IsValid)
            {
                _context.Categories.Update(Category);
                _context.SaveChanges();
                TempData["success"] = "Category updated successfully";
                // return RedirectToAction("Index", "Category");
                return RedirectToPage("Index");

            }
            else
            {
                return Page();
            }
            //_context.Categories.Update(Category);
            //_context.SaveChanges();
            //return RedirectToPage("Index");
        }
    }
}
