using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeckyGenesis.Models.VM
{
    public class ProductUserVM
    {
        public ProductUserVM()
        {
            ProductList = new List<Product>();
        }

        public AppUser AppUser { get; set; }
        public IList<Product> ProductList { get; set; }
    }
}