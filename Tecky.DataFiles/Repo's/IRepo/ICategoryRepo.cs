using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Tecky.Core.Models;

namespace Tecky.DataFiles.Repo_s.IRepo
{
    public interface ICategoryRepo : IGenRepo<Category>
    {
        void Update(Category obj);
    }
}


