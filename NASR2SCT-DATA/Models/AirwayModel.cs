using System.Collections.Generic;

namespace NASR2SCTDATA.Models
{
    public class AirwayModel
    {
        public string Id { get; set; }

        public List<AwyPointModel> AwyPoints { get; set; }
    }
}
