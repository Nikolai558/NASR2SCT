using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassData.Models
{
    public class AirwayModel
    {
        public string Id { get; set; }

        public List<AwyPointModel> AwyPoints { get; set; }
    }
}
