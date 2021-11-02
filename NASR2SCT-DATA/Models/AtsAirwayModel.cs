using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassData.Models
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
