namespace NASR2SCTDATA.Models
{
    /// <summary>
    /// NDB Model, Think of this as a template. 
    /// For every NDB we want to grab we will take a COPY of this template,
    /// and fill all the information in with the unique data from each individual NDB.
    /// </summary>
    public class NDBModel
    {
        // NDB Id
        public string Id { get; set; }

        // NDB Frequency
        public string Freq { get; set; }

        // NDB Latitude
        public string Lat { get; set; }

        // NDB Longitude
        public string Lon { get; set; }

        // NDB Name
        public string Name { get; set; }

        // NDB Type
        public string Type { get; set; }

        public string Dec_Lat { get; set; }

        public string Dec_Lon { get; set; }
    }
}
