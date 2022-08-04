using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Tecky.Core.Models;
using Tecky.DataFiles.AppData;
using Tecky.DataFiles.Repo_s.IRepo;

namespace Tecky.DataFiles.Repo_s.GenRepo
{
    public class CategoryRepo : GenRepo<Category>, ICategoryRepo
    {
        private readonly ApplicationDbContext _db;
        public CategoryRepo(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Category obj)
        {
            var objFromDb = base.FirstOrDefault(u => u.Id == obj.Id);
            if (objFromDb != null)
            {
                objFromDb.Name = obj.Name;
                objFromDb.DisplayOrder = obj.DisplayOrder;
            }
        }
    }
}