using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassData.Models
{
    public class BoundryModel
    {
        public string Identifier { get; set; }

        public string Type { get; set; }

        public List<ArbModel> AllPoints { get; set; }
    }
}
