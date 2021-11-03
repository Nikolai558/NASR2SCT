using System.Collections.Generic;
using System.Xml.Serialization;

namespace NASR2SCTDATA.Models
{
    public class Airport
    {
        [XmlAttribute]
        public string ID { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string Elevation { get; set; }

        [XmlAttribute]
        public string MagVar { get; set; }

        [XmlAttribute]
        public string Frequency { get; set; }

        [XmlElement]
        public Location Location { get; set; }

        [XmlElement]
        public Runways Runways { get; set; }


    }

    public class Runways 
    {
        [XmlElement]
        public List<Runway> Runway { get; set; }
    }

    public class Runway 
    {
        [XmlAttribute]
        public string ID { get; set; }

        [XmlAttribute]
        public string Heading { get; set; }

        [XmlAttribute]
        public string Length { get; set; }

        [XmlAttribute]
        public string Width { get; set; }

        [XmlElement]
        public StartLoc StartLoc { get; set; }

        [XmlElement]
        public EndLoc EndLoc { get; set; }
    }

    public class StartLoc 
    {
        [XmlAttribute]
        public string Lon { get; set; }

        [XmlAttribute]
        public string Lat { get; set; }
    }

    public class EndLoc 
    {
        [XmlAttribute]
        public string Lon { get; set; }

        [XmlAttribute]
        public string Lat { get; set; }
    }
}
