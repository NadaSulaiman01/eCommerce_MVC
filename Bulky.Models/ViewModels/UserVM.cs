using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Models.ViewModels
{
    public class UserVM
    {
        [Required]
        public ApplicationUser User { get; set; }
        [Required]
        [DisplayName("Role")]
        public string RoleId { get; set; }
        [ValidateNever]

        public List<SelectListItem> Roles { get; set; }
        [ValidateNever]
        [DisplayName("Company")]

        public List<SelectListItem> Companies { get; set; }
    }
}
