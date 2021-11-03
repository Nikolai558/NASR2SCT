using System.Xml.Serialization;

namespace NASR2SCTDATA.Models
{
    //public class Waypoints
    //{
    //    [XmlArray]
    //    public Waypoint[] waypoints;
    //}

    public class Waypoint
    {
        [XmlAttribute]
        public string Type { get; set; }

        [XmlAttribute]
        public string ID { get; set; }

        [XmlElement]
        public Location Location { get; set; }
    }

    public class Location
    {
        [XmlAttribute]
        public string Lon { get; set; }

        [XmlAttribute]
        public string Lat { get; set; }
    }
}
