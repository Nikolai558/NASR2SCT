namespace NASR2SCTDATA.Models
{
    /// <summary>
    /// Weather Station Model
    /// </summary>
    public class WxStationModel
    {
        // Weather station ID
        public string Id { get; set; }

        // Weather Station Name
        public string Name { get; set; }

        public string Lat_N_S { get; set; }

        public string LatDeg { get; set; }

        public string LatMin { get; set; }

        public string LatSec { get; set; }

        public string Lon_E_W { get; set; }

        public string LonDeg { get; set; }

        public string LonMin { get; set; }

        public string LonSec { get; set; }

        // Weather Station Latitude
        public string Lat { get { return $"{Lat_N_S}{LatDeg}.{LatMin}.{LatSec}.000"; } }

        // Weather Station Longitude
        public string Lon { get { return $"{Lon_E_W}{LonDeg}.{LonMin}.{LonSec}.000"; } }

        public string LatCorrect { get; set; }

        public string LonCorrect { get; set; }

        public string Type { get; set; }

        // This is used for VRC. We have to define a color for it to display on screen.
        public string ColorDefine { get; set; }

        public string Dec_Lat { get; set; }

        public string Dec_Lon { get; set; }
    }
}
