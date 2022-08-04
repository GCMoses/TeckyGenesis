using Braintree;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechStaticTools;
using Tecky.Core.Models;
using Tecky.Core.Models.VM;
using Tecky.DataFiles.Repo_s.IRepo;

namespace TeckyGenesis.Controllers
{
    [Authorize(Roles = StaticFiles.AdminRole)]
    public class OrderController : Controller
    {
        private readonly IOrderHeaderRepo _orderHeaderRepo;
        private readonly IOrderDetailRepo _orderDetailRepo;
        private readonly IBrainTreeGate _brain;

        [BindProperty]
        public OrderVM OrderVM { get; set; }
        public OrderController(
        IOrderHeaderRepo orderHeaderRepo, IOrderDetailRepo orderDetailRepo, IBrainTreeGate brain)
        {
            _brain = brain;
            _orderDetailRepo = orderDetailRepo;
            _orderHeaderRepo = orderHeaderRepo;
        }


        public IActionResult Index(string searchName = null, string searchEmail = null, string searchPhone = null, string Status = null)
        {
            OrderListVM orderListVM = new ()
            {
                OrderHList = _orderHeaderRepo.GetAll(),
                StatusList = StaticFiles.listStatus.ToList().Select(i => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = i,
                    Value = i
                })
            };


            if (!string.IsNullOrEmpty(searchName))
            {
                orderListVM.OrderHList = orderListVM.OrderHList.Where(u => u.FullName.ToLower().Contains(searchName.ToLower()));
            }
            if (!string.IsNullOrEmpty(searchEmail))
            {
                orderListVM.OrderHList = orderListVM.OrderHList.Where(u => u.Email.ToLower().Contains(searchEmail.ToLower()));
            }
            if (!string.IsNullOrEmpty(searchPhone))
            {
                orderListVM.OrderHList = orderListVM.OrderHList.Where(u => u.PhoneNumber.ToLower().Contains(searchPhone.ToLower()));
            }
            if (!string.IsNullOrEmpty(Status) && Status != "--Order Status--")
            {
                orderListVM.OrderHList = orderListVM.OrderHList.Where(u => u.OrderStatus.ToLower().Contains(Status.ToLower()));
            }

            return View(orderListVM);
        }

        public IActionResult Details(int id)
        {
            OrderVM = new OrderVM()
            {
                OrderHeader = _orderHeaderRepo.FirstOrDefault(u => u.Id == id),
                OrderDetail = _orderDetailRepo.GetAll(o => o.OrderHeaderId == id, includeProperties: "Product")
            };

            return View(OrderVM);
        }


        [HttpPost]
        public IActionResult StartProcessing()
        {
            OrderHeader orderHeader = _orderHeaderRepo.FirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeader.OrderStatus = StaticFiles.StatusInProcess;
            _orderHeaderRepo.Save();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult ShipOrder()
        {
            OrderHeader orderHeader = _orderHeaderRepo.FirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeader.OrderStatus = StaticFiles.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;
            _orderHeaderRepo.Save();
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public IActionResult CancelOrder()
        {
            OrderHeader orderHeader = _orderHeaderRepo.FirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            var gateway = _brain.GetGateway();
            Transaction transaction = gateway.Transaction.Find(orderHeader.TransactionId);

            if (transaction.Status == TransactionStatus.AUTHORIZED || transaction.Status == TransactionStatus.SUBMITTED_FOR_SETTLEMENT)
            {
                //no refund
                Result<Transaction> resultvoid = gateway.Transaction.Void(orderHeader.TransactionId);
            }
            else
            {
                //refund
                Result<Transaction> resultRefund = gateway.Transaction.Refund(orderHeader.TransactionId);
            }
            orderHeader.OrderStatus = StaticFiles.StatusRefunded;
            _orderHeaderRepo.Save();
            return RedirectToAction(nameof(Index));
        }
    }
}
