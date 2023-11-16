using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyWeb.Areas.Admin.Controllers
{
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            var productList = _unitOfWork.Product.GetAll("Category").ToList();
           
            return View(productList);
        }
        public IActionResult Upsert(int? id) //Create or Insert
        {
            IEnumerable<SelectListItem> categoryList = _unitOfWork.Category.GetAll().Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString()
            });
            var productVM = new ProductVM
            {
                CategoryList = categoryList,
                Product = new Product()
                
            };
            //ViewBag.CategoryList = categoryList;
            //ViewData["CategoryList"] = categoryList;
            if (id == null || id == 0)
            {
                //Create
                
                return View(productVM);

            }
            else
            {

                productVM.Product = _unitOfWork.Product.Get(p => p.Id == id);
                
                return View(productVM);

            }
            
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file) //Create or Insert
        {
           
            if (productVM.Product.Title != null && productVM.Product.Title.ToLower() == "test")
            {
                ModelState.AddModelError("", "Product title cannot be test");
            }
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if(file != null) {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product");
                    if(!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        //get path for old image (if exists) to delete it
                        var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));
                        if(System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    using (var fileStream = new FileStream(Path.Combine(productPath,fileName),FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    productVM.Product.ImageUrl = @"\images\product\" + fileName;
                }
                if ( productVM.Product.Id == 0)
                {
                    //Create
                    
                    _unitOfWork.Product.Add(productVM.Product);
                    TempData["success"] = "Product added successfully";

                }
                else
                {
                    //Edit
                    _unitOfWork.Product.Update(productVM.Product);
                    TempData["success"] = "Product updated successfully";

                }
                _unitOfWork.Save();
                return RedirectToAction("Index", "Product");

            }
            else
            {
                productVM.CategoryList = _unitOfWork.Category.GetAll().Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                });

                return View(productVM);
            }

        }

        //public IActionResult Edit(int? id)
        //{
        //    if (id == 0 || id == null)
        //    {
        //        return NotFound();
        //    }
        //    var product = _unitOfWork.Product.Get(c => c.Id == id);
        //    if (product == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(product);
        //}

        //[HttpPost]
        //public IActionResult Edit(Product product)
        //{

        //    if (product.Title != null && product.Title.ToLower() == "test")
        //    {
        //        ModelState.AddModelError("", "Product title cannot be test");
        //    }
        //    if (ModelState.IsValid)
        //    {
        //        _unitOfWork.Product.Update(product);
        //        _unitOfWork.Save();
        //        TempData["success"] = "Product updated successfully";
        //        return RedirectToAction("Index", "Product");

        //    }
        //    else
        //    {
        //        return View();
        //    }

        //}

        //public IActionResult Delete(int? id)
        //{
        //    if (id == 0 || id == null)
        //    {
        //        return NotFound();
        //    }
        //    var product = _unitOfWork.Product.Get(c => c.Id == id);
        //    if (product == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(product);
        //}

        //[HttpPost, ActionName("Delete")]
        //public IActionResult DeletePost(int? id)
        //{
        //    var product = _unitOfWork.Product.Get(c => c.Id == id);
        //    if (product == null)
        //    {
        //        return NotFound();
        //    }
        //    _unitOfWork.Product.Remove(product);
        //    _unitOfWork.Save();
        //    TempData["success"] = "Product deleted successfully";
        //    return RedirectToAction("Index", "Product");




        //}
        #region API Calls

        [HttpGet]
        public IActionResult GetAll()
        {
            var productList = _unitOfWork.Product.GetAll("Category").ToList();
            return Json(new { data = productList }
);
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDeleted = _unitOfWork.Product.Get(p => p.Id == id);
            if(productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, productToBeDeleted.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }
            _unitOfWork.Product.Remove(productToBeDeleted);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete successful" });



        }
        #endregion
    }
}
