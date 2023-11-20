using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area(areaName: "Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var companyList = _unitOfWork.Company.GetAll().ToList();
           
            return View(companyList);
        }
        public IActionResult Upsert(int? id) //Create or Insert
        {

            if (id == null || id == 0)
            {
                //Create
                
                return View(new Company());

            }
            else
            {

                Company company = _unitOfWork.Company.Get(p => p.Id == id);
                
                return View(company);

            }
            
        }

        [HttpPost]
        public IActionResult Upsert(Company company) //Create or Insert
        {
           
            if (company.Name != null && company.Name.ToLower() == "test")
            {
                ModelState.AddModelError("", "Company Name cannot be test");
            }
            if (ModelState.IsValid)
            {
              
                if ( company.Id == 0)
                {
                    //Create
                    
                    _unitOfWork.Company.Add(company);
                    TempData["success"] = "Company added successfully";

                }
                else
                {
                    //Edit
                    _unitOfWork.Company.Update(company);
                    TempData["success"] = "Company updated successfully";

                }
                _unitOfWork.Save();
                return RedirectToAction("Index", "Company");

            }
            else
            {
               
                return View(company);
            }

        }

      
        #region API Calls

        [HttpGet]
        public IActionResult GetAll()
        {
            var companyList = _unitOfWork.Company.GetAll().ToList();
            return Json(new { data = companyList }
);
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var companyToBeDeleted = _unitOfWork.Company.Get(p => p.Id == id);
            if(companyToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            _unitOfWork.Company.Remove(companyToBeDeleted);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete successful" });



        }
        #endregion
    }
}
