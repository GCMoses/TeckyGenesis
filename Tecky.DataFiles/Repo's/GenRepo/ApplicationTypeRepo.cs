using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tecky.Core.Models;
using Tecky.DataFiles.AppData;
using Tecky.DataFiles.Repo_s.IRepo;

namespace Tecky.DataFiles.Repo_s.GenRepo
{
    public class ApplicationTypeRepo : GenRepo<ApplicationType>, IApplicationTypeRepo
    {
        private readonly ApplicationDbContext _db;
        public ApplicationTypeRepo(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(ApplicationType obj)
        {
            var objFromDb = base.FirstOrDefault(u => u.Id == obj.Id);
            if (objFromDb != null)
            {
                objFromDb.Name = obj.Name;
            }
        }
    }
}