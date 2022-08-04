using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tecky.Core.Models;

namespace Tecky.DataFiles.Repo_s.IRepo
{
    public interface IApplicationTypeRepo : IGenRepo<ApplicationType>
    {
        void Update(ApplicationType obj);
    }
}