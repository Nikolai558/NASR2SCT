using System.Collections.Generic;

namespace NASR2SCTDATA.Models
{
    /// <summary>
    /// Model for our ATS Airway Routes.
    /// </summary>
    public class AtsAirwayModel
    {
        // Ats Id
        public string Id { get; set; }

        // Ats Airway Points.
        public List<atsAwyPointModel> atsAwyPoints { get; set; }
    }
}
