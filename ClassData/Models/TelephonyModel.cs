using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassData.Models
{
    public class TelephonyModel
    {
        public string ThreeLD { get; set; }

        public string Telephony { get; set; }

        /// <summary>
        /// Telephony no special characters.
        /// </summary>
        public string TelephonyAltered { get; set; }
    }
}
