using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area(areaName: "Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
		private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public int MyProperty { get; set; }

        public UserController(IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
			_unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult RoleManagement(string id)
        {
          //  var userFromDb = _context.ApplicationUsers.Include(u => u.Company).FirstOrDefault(u => u.Id == id);
            var userFromDb = _unitOfWork.ApplicationUser.Get(filter: u  => u.Id == id, includeProperties: "Company");
            if (userFromDb == null)
            {
                return NotFound();
            }

            //var roles = _context.Roles.ToList();
            //var userRoles = _context.UserRoles.ToList();

            var roles = _roleManager.Roles.ToList();
            //var userRoles = _roleManager.rol

            var userVM = new UserVM
            {
                User = userFromDb,
                Companies = _unitOfWork.Company.GetAll().Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList(),

                Roles = roles.Select(r => new SelectListItem
                {
                    Value = r.Id,
                    Text = r.Name
                }).ToList()



            };
            //var roleId = userRoles.FirstOrDefault(r => r.UserId == id).RoleId;

            var roleId = _userManager.GetRolesAsync(userFromDb).GetAwaiter().GetResult().FirstOrDefault();
            userVM.User.Role = roleId;
            var numericRoleId = roles.FirstOrDefault(r => r.Name == roleId).Id;
            userVM.RoleId = numericRoleId;

            return View(userVM);
        }

        [HttpPost]
        public IActionResult RoleManagement(UserVM userVM)
        {

            var userFromDb = _unitOfWork.ApplicationUser.Get(filter: u => u.Id == userVM.User.Id, includeProperties: "Company", flag:true);
            if (userFromDb == null)
            {
                return NotFound();
            }

            var roles = _roleManager.Roles.ToList(); 
            var companyRole = roles.FirstOrDefault(r => r.Name == SD.Role_Company);



            var roleId = _userManager.GetRolesAsync(userFromDb).GetAwaiter().GetResult().FirstOrDefault();
            var oldRole = roles.FirstOrDefault(r => r.Name == roleId);
            var newRole = roles.FirstOrDefault(r => r.Id == userVM.RoleId);
            //userRole.RoleId = userVM.RoleId;

            
            if(companyRole.Id == userVM.RoleId) {
                userFromDb.CompanyId = userVM.User.CompanyId;
            }
            else
            {
                if(userFromDb.CompanyId != null)
                {
                    userFromDb.CompanyId = null;
                }
            }



            _unitOfWork.Save();
            // _userManager.RemoveFromRoleAsync(userFromDb)

            

            if(oldRole.Name != newRole.Name)
            {
                _userManager.RemoveFromRoleAsync(userFromDb, oldRole.Name).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(userFromDb, newRole.Name).GetAwaiter().GetResult();
            }
            

            TempData["success"] = "User permission updated successfully";
            return RedirectToAction("Index");
        }



        #region API Calls

        [HttpGet]
        public IActionResult GetAll()
        {
            var userList = _unitOfWork.ApplicationUser.GetAll(includeProperties:"Company");
            var roles = _roleManager.Roles.ToList();
            //var userRoles = _context.UserRoles.ToList();

            foreach (var user in userList)
            {
                var roleId = _userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault();
                //var roleName = roles.FirstOrDefault(r => r.Id == roleId).Name;
                user.Role = roleId;
            }

            return Json(new { data = userList }
);
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody]string id)
        {
            

            //var user = _context.ApplicationUsers.FirstOrDefault(u => u.Id == id);
            var user = _unitOfWork.ApplicationUser.Get(filter: u => u.Id == id, flag:true);
            if (user == null)
            {
                return Json(new { success = false, message = "Error while locking/unlocking" });
            }
            if(user.LockoutEnd != null && user.LockoutEnd > DateTime.Now)
            {
                user.LockoutEnd = DateTime.Now;
            }
            else
            {
                user.LockoutEnd = DateTime.Now.AddDays(30);
            }

            _unitOfWork.Save();


            return Json(new { success = true, message = "Operation successful" });



        }
        #endregion
    }
}
