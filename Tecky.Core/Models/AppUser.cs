using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Tecky.Core.Models
{
    public class AppUser : IdentityUser
    {
        [Required]
        public string FullName { get; set; }

        [NotMapped]
        public string StreetAddress { get; set; }
        [NotMapped]
        public string City { get; set; }
        [NotMapped]
        [DisplayName("State/Province")]
        public string State { get; set; }
        [NotMapped]
        public string PostalCode { get; set; }

    }
}
