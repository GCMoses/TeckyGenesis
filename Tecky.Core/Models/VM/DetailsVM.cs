using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tecky.Core.Models.VM
{
    public class DetailsVM
    {
        public DetailsVM()
        {
            Product = new Product();
        }

        public Product Product { get; set; }
        public bool ExistsInCart { get; set; }
    }
}
