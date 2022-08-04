using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Tecky.Core.Models;
using Tecky.DataFiles.AppData;
using Microsoft.AspNetCore.Mvc.Rendering;
using Tecky.Core.Models.VM;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using TechStaticTools;
using Tecky.DataFiles.Repo_s.IRepo;

namespace TeckyGenesis.Controllers
{
    [Authorize(Roles = StaticFiles.AdminRole)]
    public class ProductController : Controller
    {
        private readonly IProductRepo _productRepo;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IProductRepo productRepo, IWebHostEnvironment webHostEnvironment)
        {
            _productRepo = productRepo;
            _webHostEnvironment = webHostEnvironment;
        }


        public IActionResult Index()
        {
            IEnumerable<Product> objList = _productRepo.GetAll(includeProperties: "Category,ApplicationType");

            //foreach(var obj in objList)
            //{
            //    obj.Category = _db.Category.FirstOrDefault(u => u.Id == obj.CategoryId);
            //    obj.ApplicationType = _db.ApplicationType.FirstOrDefault(u => u.Id == obj.ApplicationTypeId);
            //};

            return View(objList);
        }


        //GET - UPSERT
        public IActionResult Upsert(int? id)
        {

            //IEnumerable<SelectListItem> CategoryDropDown = _db.Category.Select(i => new SelectListItem
            //{
            //    Text = i.Name,
            //    Value = i.Id.ToString()
            //});

            ////ViewBag.CategoryDropDown = CategoryDropDown;
            //ViewData["CategoryDropDown"] = CategoryDropDown;

            //Product product = new Product();

            ProductVM productVM = new ()
            {
                Product = new (),
                CategorySelectList = _productRepo.GetAllDropdownList(StaticFiles.CategoryName),
                ApplicationTypeSelectList = _productRepo.GetAllDropdownList(StaticFiles.ApplicationTypeName),
            };

            if (id == null)
            {
                //this is for create
                return View(productVM);
            }
            else
            {
                productVM.Product = _productRepo.Find(id.GetValueOrDefault());
                if (productVM.Product == null)
                {
                    return NotFound();
                }
                return View(productVM);
            }
        }


        //POST - UPSERT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                string webRootPath = _webHostEnvironment.WebRootPath;

                if (productVM.Product.Id == 0)
                {
                    //Creating
                    string upload = webRootPath + StaticFiles.ImagePath;
                    string fileName = Guid.NewGuid().ToString();
                    string extension = Path.GetExtension(files[0].FileName);

                    using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }

                    productVM.Product.Image = fileName + extension;

                    _productRepo.Add(productVM.Product);
                }
                else
                {
                    //updating
                    var objFromDb = _productRepo.FirstOrDefault(u => u.Id == productVM.Product.Id, isTracking: false);

                    if (files.Count > 0)
                    {
                        string upload = webRootPath + StaticFiles.ImagePath;
                        string fileName = Guid.NewGuid().ToString();
                        string extension = Path.GetExtension(files[0].FileName);

                        var oldFile = Path.Combine(upload, objFromDb.Image);

                        if (System.IO.File.Exists(oldFile))
                        {
                            System.IO.File.Delete(oldFile);
                        }

                        using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                        {
                            files[0].CopyTo(fileStream);
                        }

                        productVM.Product.Image = fileName + extension;
                    }
                    else
                    {
                        productVM.Product.Image = objFromDb.Image;
                    }
                   _productRepo.Update(productVM.Product);
                }
                TempData[StaticFiles.Success] = "Product updated!";

                _productRepo.Save();
                return RedirectToAction("Index");
            }
            productVM.CategorySelectList = _productRepo.GetAllDropdownList(StaticFiles.CategoryName);
            productVM.ApplicationTypeSelectList = _productRepo.GetAllDropdownList(StaticFiles.ApplicationTypeName);

            return View(productVM);

        }



        //GET - DELETE
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Product product = _productRepo.FirstOrDefault(u => u.Id == id, includeProperties: "Category,ApplicationType");
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        //POST - DELETE
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var obj = _productRepo.Find(id.GetValueOrDefault());
            if (obj == null)
            {
                return NotFound();
            }

            string upload = _webHostEnvironment.WebRootPath + StaticFiles.ImagePath;
            var oldFile = Path.Combine(upload, obj.Image);

            if (System.IO.File.Exists(oldFile))
            {
                System.IO.File.Delete(oldFile);
            }
            _productRepo.Remove(obj);
            _productRepo.Save();
            TempData[StaticFiles.Success] = "Product removed!";
            return RedirectToAction("Index");


        }

    }
}