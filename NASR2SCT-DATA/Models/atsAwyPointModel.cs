using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassData.Models
{
    public class atsAwyPointModel
    {
        public string AirwayId { get; set; }

        public string PointId { get; set; }

        public string Type { get; set; }

        public string Lat { get; set; }

        public string Lon { get; set; }

        public string Name { get; set; }

        public bool GapAfter { get; set; }

        public bool BorderAfter { get; set; }

        public bool EndOfAirway { get; set; }

        public string Dec_Lat { get; set; }

        public string Dec_Lon { get; set; }
    }
}
