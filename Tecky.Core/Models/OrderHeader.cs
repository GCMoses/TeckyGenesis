using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tecky.Core.Models
{
    public class OrderHeader
    {
        [Key]
        public int Id { get; set; }

        public string CreatedByUserId { get; set; }
        [ForeignKey("CreatedByUserId")]
        public AppUser CreatedBy { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        public DateTime ShippingDate { get; set; }
        [Required]
        public double FinalOrderTotal { get; set; }
        public string OrderStatus { get; set; }
        public DateTime PaymentDate { get; set; }
        public string TransactionId { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }
        [Required]
        public string StreetAddress { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        [DataType(DataType.PostalCode)]
        public string PostalCode { get; set; }
        [Required]
        public string FullName { get; set; }
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}