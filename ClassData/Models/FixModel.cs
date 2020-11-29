using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NASRData.Models
{
    /// <summary>
    /// Fix Model, Think of this as a template. 
    /// For every fix we want to grab we will take a COPY of this template,
    /// and fill all the information in with the unique data from each individual fix.
    /// </summary>
    public class FixModel
    {
        // Fix ID
        public string Id { get; set; }

        // Fix Latitude
        public string Lat { get; set; }

        // Fix Longitude
        public string Lon { get; set; }

        // Fix Catagory
        public string Catagory { get; set; }
        
        // Fix Use
        public string Use { get; set; }

        // Fix Artcc High
        public string HiArtcc { get; set; }

        // Fix Artcc Low
        public string LoArtcc { get; set; }

        public string Lat_Dec { get; set; }

        public string Lon_Dec { get; set; }
    }
}
