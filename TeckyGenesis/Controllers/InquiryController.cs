using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Tecky.Core.Models;
using Microsoft.AspNetCore.Authorization;
using TechStaticTools;
using Tecky.DataFiles.AppData;
using Tecky.DataFiles.Repo_s.IRepo;
using Tecky.Core.Models.VM;

namespace TeckyGenesis.Controllers
{
    [Authorize(Roles = StaticFiles.AdminRole)]
    public class InquiryController : Controller
    {
        private readonly IInquiryHeaderRepo _inquiryHeaderRepo;
        private readonly IInquiryDetailRepo _inquiryDetailRepo;

        [BindProperty]
        public InquiryVM InquiryVM { get; set; }


        public InquiryController(IInquiryDetailRepo inquiryDetailRepo,
            IInquiryHeaderRepo inquiryHeaderRepo)
        {
            _inquiryDetailRepo = inquiryDetailRepo;
            _inquiryHeaderRepo = inquiryHeaderRepo;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            InquiryVM = new InquiryVM()
            {
                InquiryHeader = _inquiryHeaderRepo.FirstOrDefault(u => u.Id == id),
                InquiryDetail = _inquiryDetailRepo.GetAll(u => u.InquiryHeaderId == id, includeProperties: "Product")
            };
            return View(InquiryVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details()
        {
            List<ShoppingCart> shoppingCartList = new ();
            InquiryVM.InquiryDetail = _inquiryDetailRepo.GetAll(u => u.InquiryHeaderId == InquiryVM.InquiryHeader.Id);

            foreach (var detail in InquiryVM.InquiryDetail)
            {
                ShoppingCart shoppingCart = new ()
                {
                    ProductId = detail.ProductId,
                    Item = 1
                };
                shoppingCartList.Add(shoppingCart);
            }
            HttpContext.Session.Clear();
            HttpContext.Session.Set(StaticFiles.SessionCart, shoppingCartList);
            HttpContext.Session.Set(StaticFiles.SessionInquiryId, InquiryVM.InquiryHeader.Id);
            return RedirectToAction("Index", "Cart");
        }

        [HttpPost]
        public IActionResult Delete()
        {
            InquiryHeader inquiryHeader = _inquiryHeaderRepo.FirstOrDefault(u => u.Id == InquiryVM.InquiryHeader.Id);
            IEnumerable<InquiryDetail> inquiryDetails = _inquiryDetailRepo.GetAll(u => u.InquiryHeaderId == InquiryVM.InquiryHeader.Id);

            _inquiryDetailRepo.RemoveRange(inquiryDetails);
            _inquiryHeaderRepo.Remove(inquiryHeader);
            _inquiryHeaderRepo.Save();
            TempData[StaticFiles.Success] = "Inquiry removed!";

            return RedirectToAction(nameof(Index));
        }


        #region API CALLS
        [HttpGet]
        public IActionResult GetInquiryList()
        {
            return Json(new { data = _inquiryHeaderRepo.GetAll() });
        }

        #endregion
    }
}