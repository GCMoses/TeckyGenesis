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
    public class InquiryHeaderRepo : GenRepo<InquiryHeader>, IInquiryHeaderRepo
    {
        private readonly ApplicationDbContext _db;
        public InquiryHeaderRepo(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(InquiryHeader obj)
        {
            _db.InquiryHeader.Update(obj);
        }
    }
}
