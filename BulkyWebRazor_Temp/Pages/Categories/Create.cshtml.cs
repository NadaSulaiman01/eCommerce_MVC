using BulkyWebRazor_Temp.Data;
using BulkyWebRazor_Temp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor_Temp.Pages.Categories
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        [BindProperty]
        public Category Category { get; set; }

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }
        public void OnGet()
        {
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
                _context.Categories.Add(Category);
                _context.SaveChanges();
                 TempData["success"] = "Category created successfully";
                // return RedirectToAction("Index", "Category");
                return RedirectToPage("Index");

            }
            else
            {
                return Page();
            }
        }
    }
}
