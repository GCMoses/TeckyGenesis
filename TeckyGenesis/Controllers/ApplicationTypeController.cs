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

namespace TeckyGenesis.Controllers
{
    [Authorize(Roles = StaticFiles.AdminRole)]
    public class ApplicationTypeController : Controller
{
    private readonly IApplicationTypeRepo _applicationTypeRepo;

    public ApplicationTypeController(IApplicationTypeRepo applicationTypeRepo)
    {
        _applicationTypeRepo = applicationTypeRepo;
    }


    public IActionResult Index()
    {
        IEnumerable<ApplicationType> objList = _applicationTypeRepo.GetAll();
        return View(objList);
    }


    //GET - CREATE
    public IActionResult Create()
    {
        return View();
    }


    //POST - CREATE
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(ApplicationType obj)
    {
            if (ModelState.IsValid)
            {
                _applicationTypeRepo.Add(obj);
                _applicationTypeRepo.Save();
                TempData[StaticFiles.Success] = "ApplicationType created!";
                return RedirectToAction("Index");
            }
            return View(obj);

        }


        //GET - EDIT
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _applicationTypeRepo.Find(id.GetValueOrDefault());
            if (obj == null)
            {
                return NotFound();
            }

            return View(obj);
        }

        //POST - EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ApplicationType obj)
        {
            if (ModelState.IsValid)
            {
                _applicationTypeRepo.Update(obj);
                _applicationTypeRepo.Save();
                TempData[StaticFiles.Success] = "ApplicationType updated!";
                return RedirectToAction("Index");
            }
            return View(obj);

        }

        //GET - DELETE
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _applicationTypeRepo.Find(id.GetValueOrDefault());
            if (obj == null)
            {
                return NotFound();
            }

            return View(obj);
        }

        //POST - DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var obj = _applicationTypeRepo.Find(id.GetValueOrDefault());
            if (obj == null)
            {
                return NotFound();
            }
            _applicationTypeRepo.Remove(obj);
            _applicationTypeRepo.Save();
            TempData[StaticFiles.Success] = "ApplicationType deleted!";
            return RedirectToAction("Index");


        }

    }
}
