namespace NASR2SCTDATA.Models
{
    /// <summary>
    /// VOR Model - Holds all information we need about each VOR
    /// </summary>
    public class VORModel
    {
        // VOR Id
        public string Id { get; set; }

        // VOR Frequency
        public string Freq { get; set; }

        // VOR Latitude
        public string Lat { get; set; }

        // VOR Longitude
        public string Lon { get; set; }

        // VOR Name
        public string Name { get; set; }

        // VOR Type
        public string Type { get; set; }

        public string Dec_Lat { get; set; }

        public string Dec_Lon { get; set; }
    }
}
