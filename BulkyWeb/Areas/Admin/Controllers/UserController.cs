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
		private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public int MyProperty { get; set; }

        public UserController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
			_context = context;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult RoleManagement(string id)
        {
            var userFromDb = _context.ApplicationUsers.Include(u => u.Company).FirstOrDefault(u => u.Id == id);
            if (userFromDb == null)
            {
                return NotFound();
            }

            var roles = _context.Roles.ToList();
            var userRoles = _context.UserRoles.ToList();

            var userVM = new UserVM
            {
                User = userFromDb,
                Companies = _context.Companies.Select(c => new SelectListItem
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
            var roleId = userRoles.FirstOrDefault(r => r.UserId == id).RoleId;
            userVM.User.Role = roles.FirstOrDefault(r => r.Id == roleId).Name;
            userVM.RoleId = roleId;

            return View(userVM);
        }

        [HttpPost]
        public IActionResult RoleManagement(UserVM userVM)
        {

            var userFromDb = _context.ApplicationUsers.Include(u => u.Company).FirstOrDefault(u => u.Id == userVM.User.Id);
            if (userFromDb == null)
            {
                return NotFound();
            }

            var roles = _context.Roles.ToList();
            var companyRole = roles.FirstOrDefault(r => r.Name == SD.Role_Company);
            


            var userRole = _context.UserRoles.FirstOrDefault(ur => ur.UserId == userVM.User.Id);
            var oldRole = roles.FirstOrDefault(r => r.Id == userRole.RoleId);
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



            _context.SaveChanges();
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
            var userList = _context.ApplicationUsers.Include(u => u.Company).ToList();
            var roles = _context.Roles.ToList();
            var userRoles = _context.UserRoles.ToList();

            foreach (var user in userList)
            {
                var roleId = userRoles.FirstOrDefault(r => r.UserId == user.Id).RoleId;
                var roleName = roles.FirstOrDefault(r => r.Id == roleId).Name;
                user.Role = roleName;
            }

            return Json(new { data = userList }
);
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody]string id)
        {
            

            var user = _context.ApplicationUsers.FirstOrDefault(u => u.Id == id);
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

            _context.SaveChanges();


            return Json(new { success = true, message = "Operation successful" });



        }
        #endregion
    }
}
