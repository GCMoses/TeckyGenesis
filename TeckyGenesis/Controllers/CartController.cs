using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Tecky.Core.Models;
using Tecky.DataFiles.AppData;
using Tecky.DataFiles.Repo_s.IRepo;
using Microsoft.AspNetCore.Mvc.Rendering;
using Tecky.Core.Models.VM;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authorization;
using TechStaticTools;
using Microsoft.AspNetCore.Http;
using Braintree;

namespace TeckyGenesis.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailSender _emailSender;
        private readonly IAppUserRepo _appUserRepo;
        private readonly IBrainTreeGate _brain;
        private readonly IProductRepo _productRepo;
        private readonly IInquiryHeaderRepo _inquiryHeaderRepo;
        private readonly IInquiryDetailRepo _inquiryDetailRepo;
        private readonly IOrderHeaderRepo _orderHeaderRepo;
        private readonly IOrderDetailRepo _orderDetailRepo;

        [BindProperty]
        public ProductUserVM ProductUserVM { get; set; }

       public CartController(IWebHostEnvironment webHostEnvironment, IEmailSender emailSender,
            IAppUserRepo appUserRepo, IProductRepo productRepo,
            IInquiryHeaderRepo inquiryHeaderRepo, IInquiryDetailRepo inquiryDetailRepo,
            IOrderHeaderRepo orderHeaderRepo, IOrderDetailRepo orderDetailRepo, IBrainTreeGate brain)
        {
            _webHostEnvironment = webHostEnvironment;
            _emailSender = emailSender;
            _appUserRepo = appUserRepo;
            _brain = brain;
            _productRepo = productRepo;
            _inquiryHeaderRepo = inquiryHeaderRepo;
            _inquiryDetailRepo = inquiryDetailRepo;
            _orderHeaderRepo = orderHeaderRepo;
            _orderDetailRepo = orderDetailRepo;
        }
        public IActionResult Index()
        {
            List<ShoppingCart> shoppingCartList = new ();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(StaticFiles.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(StaticFiles.SessionCart).Count() > 0)
            {
                //session exsits
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(StaticFiles.SessionCart);
            }

            List<int> prodInCart = shoppingCartList.Select(i => i.ProductId).ToList();
            IEnumerable<Product> prodListTemp = _productRepo.GetAll(u => prodInCart.Contains(u.Id));
            IList<Product> productList = new List<Product>();

            foreach (var cartObj in shoppingCartList)
            {
                Product prodTemp = prodListTemp.FirstOrDefault(u => u.Id == cartObj.ProductId);
                prodTemp.TempItem = cartObj.Item;
                productList.Add(prodTemp);
            }

            return View(productList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public IActionResult IndexPost(IEnumerable<Product> ProductList)
        {
            List<ShoppingCart> shoppingCartList = new ();
            foreach (Product prod in ProductList)
            {
                shoppingCartList.Add(new ShoppingCart { ProductId = prod.Id, Item = prod.TempItem });
            }

            HttpContext.Session.Set(StaticFiles.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Summary));
        }


        public IActionResult Summary()
        {
            AppUser appUser;

            if (User.IsInRole(StaticFiles.AdminRole))
            {
                if (HttpContext.Session.Get<int>(StaticFiles.SessionInquiryId) != 0)
                {
                    //cart has been loaded using an inquiry
                    InquiryHeader inquiryHeader = _inquiryHeaderRepo.FirstOrDefault(u => u.Id == HttpContext.Session.Get<int>(StaticFiles.SessionInquiryId));
                    appUser = new AppUser()
                    {
                        Email = inquiryHeader.Email,
                        FullName = inquiryHeader.FullName,
                        PhoneNumber = inquiryHeader.PhoneNumber
                    };
                }
                else
                {
                    appUser = new ();
                }
                var gateway = _brain.GetGateway();
                var clientToken = gateway.ClientToken.Generate();
                ViewBag.ClientToken = clientToken;
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                //var userId = User.FindFirstValue(ClaimTypes.Name);

                appUser = _appUserRepo.FirstOrDefault(u => u.Id == claim.Value);
            }



            List<ShoppingCart> shoppingCartList = new ();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(StaticFiles.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(StaticFiles.SessionCart).Count() > 0)
            {
                //session exsits
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(StaticFiles.SessionCart);
            }

            List<int> prodInCart = shoppingCartList.Select(i => i.ProductId).ToList();
            IEnumerable<Product> prodList = _productRepo.GetAll(u => prodInCart.Contains(u.Id));

            ProductUserVM = new ProductUserVM()
            {
                AppUser = appUser,
                ProductList = prodList.ToList()
            };

            foreach (var cartObj in shoppingCartList)
            {
                Product productTemp = _productRepo.FirstOrDefault(u => u.Id == cartObj.ProductId);
                productTemp.TempItem = cartObj.Item;
                ProductUserVM.ProductList.Add(productTemp);
            }


            return View(ProductUserVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPost(IFormCollection collection, ProductUserVM ProductUserVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (User.IsInRole(StaticFiles.AdminRole))
            {
                //we need to create an order
                //var orderTotal = 0.0;
                //foreach(Product prod in ProductUserVM.ProductList)
                //{
                //    orderTotal += prod.Price * prod.TempSqFt;
                //}
                OrderHeader orderHeader = new ()
                {
                    CreatedByUserId = claim.Value,
                    FinalOrderTotal = ProductUserVM.ProductList.Sum(x => x.TempItem * x.Price),
                    City = ProductUserVM.AppUser.City,
                    StreetAddress = ProductUserVM.AppUser.StreetAddress,
                    State = ProductUserVM.AppUser.State,
                    PostalCode = ProductUserVM.AppUser.PostalCode,
                    FullName = ProductUserVM.AppUser.FullName,
                    Email = ProductUserVM.AppUser.Email,
                    PhoneNumber = ProductUserVM.AppUser.PhoneNumber,
                    OrderDate = DateTime.Now,
                    OrderStatus = StaticFiles.StatusPending
                };  
                _orderHeaderRepo.Add(orderHeader);
                _orderHeaderRepo.Save();


                foreach (var prod in ProductUserVM.ProductList)
                {
                    OrderDetail orderDetail = new ()
                    {
                        OrderHeaderId = orderHeader.Id,
                        PricePerItem = prod.Price,
                        Item = prod.TempItem,
                        ProductId = prod.Id
                    };
                    _orderDetailRepo.Add(orderDetail);

                }
                _orderDetailRepo.Save();
                string nonceFromTheClient = collection["payment_method_nonce"];

                var request = new TransactionRequest
                {
                    Amount = Convert.ToDecimal(orderHeader.FinalOrderTotal),
                    PaymentMethodNonce = nonceFromTheClient,
                    OrderId = orderHeader.Id.ToString(),
                    Options = new TransactionOptionsRequest
                    {
                        SubmitForSettlement = true
                    }
                };

                var gateway = _brain.GetGateway();
                Result<Transaction> result = gateway.Transaction.Sale(request);

                if (result.Target.ProcessorResponseText == "Approved")
                {
                    orderHeader.TransactionId = result.Target.Id;
                    orderHeader.OrderStatus = StaticFiles.StatusApproved;
                }
                else
                {
                    orderHeader.OrderStatus = StaticFiles.StatusCancelled;
                }
                _orderHeaderRepo.Save();
                return RedirectToAction(nameof(InquiryConfirmation), new { id = orderHeader.Id });


            }
            else
            {
                //we need to create an inquiry
                var PathToTemplate = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString()
               + "templates" + Path.DirectorySeparatorChar.ToString() +
               "Inquiry.html";

                var subject = "New Inquiry";
                string HtmlBody = "";
                using (StreamReader sr = System.IO.File.OpenText(PathToTemplate))
                {
                    HtmlBody = sr.ReadToEnd();
                }
                //Name: { 0}
                //Email: { 1}
                //Phone: { 2}
                //Products: {3}

                StringBuilder productListSB = new ();
                foreach (var prod in ProductUserVM.ProductList)
                {
                    productListSB.Append($" - Name: { prod.Name} <span style='font-size:14px;'> (ID: {prod.Id})</span><br />");
                }

                string messageBody = string.Format(HtmlBody,
                    ProductUserVM.AppUser.FullName,
                    ProductUserVM.AppUser.Email,
                    ProductUserVM.AppUser.PhoneNumber,
                    productListSB.ToString());

                await _emailSender.SendEmailAsync(StaticFiles.EmailAdmin, subject, messageBody);
                InquiryHeader inquiryHeader = new ()
                {
                    AppUserId = claim.Value,
                    FullName = ProductUserVM.AppUser.FullName,
                    Email = ProductUserVM.AppUser.Email,
                    PhoneNumber = ProductUserVM.AppUser.PhoneNumber,
                    InquiryDate = DateTime.Now

                };
                _inquiryHeaderRepo.Add(inquiryHeader);
                _inquiryHeaderRepo.Save();

                foreach (var prod in ProductUserVM.ProductList)
                {
                    InquiryDetail inquiryDetail = new ()
                    {
                        InquiryHeaderId = inquiryHeader.Id,
                        ProductId = prod.Id,

                    };
                    _inquiryDetailRepo.Add(inquiryDetail);

                }
                _inquiryDetailRepo.Save();
                TempData[StaticFiles.Success] = "Inquiry submitted!";

            }
            
            return RedirectToAction(nameof(InquiryConfirmation));
        }
        public IActionResult InquiryConfirmation(int id = 0)
        {
            OrderHeader orderHeader = _orderHeaderRepo.FirstOrDefault(u => u.Id == id);
            HttpContext.Session.Clear();
            return View(orderHeader);
        }

        public IActionResult Remove(int id)
        {

            List<ShoppingCart> shoppingCartList = new ();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(StaticFiles.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(StaticFiles.SessionCart).Count() > 0)
            {
                //session exsits
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(StaticFiles.SessionCart);
            }

            shoppingCartList.Remove(shoppingCartList.FirstOrDefault(u => u.ProductId == id));
            HttpContext.Session.Set(StaticFiles.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCart(IEnumerable<Product> ProductList)
        {
            List<ShoppingCart> shoppingCartList = new ();
            foreach (Product prod in ProductList)
            {
                shoppingCartList.Add(new ShoppingCart { ProductId = prod.Id, Item = prod.TempItem });
            }

            HttpContext.Session.Set(StaticFiles.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Clear()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}