using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var categoryList = _unitOfWork.Category.GetAll().ToList();
            return View(categoryList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "Category name cannot match display order.");
            }
            if (category.Name != null && category.Name.ToLower() == "test")
            {
                ModelState.AddModelError("", "Category name cannot be test");
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Add(category);
                _unitOfWork.Save();
                TempData["success"] = "The category has been added successfully";
                return RedirectToAction("Index", "Category");

            }
            else
            {
                return View();
            }
           
        }

        public IActionResult Edit(int? id)
        {
            if (id == 0 || id == null)
            {
                return NotFound();
            }
            var category = _unitOfWork.Category.Get(c => c.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "Category name cannot match display order.");
            }
            if (category.Name != null && category.Name.ToLower() == "test")
            {
                ModelState.AddModelError("", "Category name cannot be test");
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.update(category);
                _unitOfWork.Save();
                TempData["success"] = "The category has been updated successfully";
                return RedirectToAction("Index", "Category");

            }
            else
            {
                return View();
            }

        }

        public IActionResult Delete(int? id)
        {
            if (id == 0 || id == null)
            {
                return NotFound();
            }
            var category = _unitOfWork.Category.Get(c => c.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            var category = _unitOfWork.Category.Get(c => c.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            _unitOfWork.Category.Remove(category);
            _unitOfWork.Save();
            TempData["success"] = "The category has been deleted successfully";
            return RedirectToAction("Index", "Category");




        }
    }
}
