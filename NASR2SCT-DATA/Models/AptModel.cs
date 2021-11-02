using System.Collections.Generic;

namespace ClassData.Models
{
    /// <summary>
    /// Airport Model - Template for all the information we need for FAA Airports.
    /// </summary>
    public class AptModel
    {
        // Airport Type, ie. Airport, Helipad, etc.
        public string Type { get; set; }

        // FAA ID 3-LD
        public string Id { get; set; }

        // Name of Landing Facility 
        public string Name { get; set; }

        // Facility Latitude
        public string Lat { get; set; }

        // Facility Longitude
        public string Lon { get; set; }

        // Facility Elevation
        public string Elv { get; set; }

        // Facility Responsible ARTCC
        public string ResArtcc { get; set; }

        // Facility Status. ie. "O"  - OPERATIONAL, "CP" - CLOSED PERMANENTLY, "CI" - CLOSED INDEFINITELY
        public string Status { get; set; }

        // Towered Facility ie. "Y" - Yes, "N" - No
        public string Twr { get; set; }

        // Facility CTAF, if one is available
        public string Ctaf { get; set; }

        // Facility ICAO if one is available.
        public string Icao { get; set; }

        public string Lat_Dec { get; set; }

        public string Lon_Dec { get; set; }

        public string magVariation { get; set; }
        
        public List<RunwayModel> Runways { get; set; }

    }
}
