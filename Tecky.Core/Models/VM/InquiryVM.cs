using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tecky.Core.Models.VM
{
    public class InquiryVM
    {
        public InquiryHeader InquiryHeader { get; set; }
        public IEnumerable<InquiryDetail> InquiryDetail { get; set; }
    }
}