namespace NASR2SCTDATA.Models
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
