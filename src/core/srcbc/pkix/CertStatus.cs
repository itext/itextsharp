using System;

using Org.BouncyCastle.Utilities.Date;

namespace Org.BouncyCastle.Pkix
{
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class CertStatus
    {
        public const int Unrevoked = 11;

        public const int Undetermined = 12;

        private int status = Unrevoked;

        DateTimeObject revocationDate = null;

        /// <summary>
        /// Returns the revocationDate.
        /// </summary>
         public DateTimeObject RevocationDate
        {
            get { return revocationDate; }
            set { this.revocationDate = value; }
        }

		/// <summary>
        /// Returns the certStatus.
        /// </summary>
        public int Status
        {
            get { return status; }
            set { this.status = value; }
        }
    }
}
