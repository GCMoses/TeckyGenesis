using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Tecky.DataFiles.AppData;
using Tecky.Core.Models;
using Tecky.Core.Models.VM;
using TechStaticTools;
using Tecky.DataFiles.Repo_s.IRepo;

namespace TeckyGenesis.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductRepo _productRepo;
        private readonly ICategoryRepo _categoryRepo;



        public HomeController(ILogger<HomeController> logger, IProductRepo productRepo,
            ICategoryRepo categoryRepo)
        {
            _logger = logger;
            _productRepo = productRepo;
            _categoryRepo = categoryRepo;
        }

        public IActionResult Index()
        {

            HomeVM homeVM = new()
            {
                Products = _productRepo.GetAll(includeProperties: "Category,ApplicationType"),
                Categories = _categoryRepo.GetAll()
            };
            return View(homeVM);
        }
        public IActionResult Details(int id)
        {

            List<ShoppingCart> shoppingCartList = new ();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(StaticFiles.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(StaticFiles.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(StaticFiles.SessionCart);
            }

            DetailsVM DetailsVM = new()
            {
                Product = _productRepo.FirstOrDefault(u => u.Id == id, includeProperties: "Category,ApplicationType"),
                ExistsInCart = false
            };

            foreach (var item in shoppingCartList)
            {
                if (item.ProductId == id)
                {
                    DetailsVM.ExistsInCart = true;
                }
            }


            return View(DetailsVM);
        }


        [HttpPost, ActionName("Details")]
        public IActionResult DetailsPost(int id, DetailsVM detailsVM)
        {
            List<ShoppingCart> shoppingCartList = new();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(StaticFiles.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(StaticFiles.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(StaticFiles.SessionCart);
            }
            shoppingCartList.Add(new ShoppingCart { ProductId = id, Item = detailsVM.Product.TempItem });
            HttpContext.Session.Set(StaticFiles.SessionCart, shoppingCartList);
            TempData[StaticFiles.Success] = "Item added to cart!";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult RemoveFromCart(int id)
        {
            List<ShoppingCart> shoppingCartList = new();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(StaticFiles.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(StaticFiles.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(StaticFiles.SessionCart);
            }

            var itemToRemove = shoppingCartList.SingleOrDefault(r => r.ProductId == id);
            if (itemToRemove != null)
            {
                shoppingCartList.Remove(itemToRemove);
            }

            HttpContext.Session.Set(StaticFiles.SessionCart, shoppingCartList);
            TempData[StaticFiles.Success] = "Item removed!";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
