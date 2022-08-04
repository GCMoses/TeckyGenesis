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
    public class AppUserRepo : GenRepo<AppUser>, IAppUserRepo
    {
        private readonly ApplicationDbContext _db;
        public AppUserRepo(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

    }
}
