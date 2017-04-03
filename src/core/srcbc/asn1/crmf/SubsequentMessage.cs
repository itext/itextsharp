using System;

namespace Org.BouncyCastle.Asn1.Crmf
{
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class SubsequentMessage
        : DerInteger
    {
        public static readonly SubsequentMessage encrCert = new SubsequentMessage(0);
        public static readonly SubsequentMessage challengeResp = new SubsequentMessage(1);
    
        private SubsequentMessage(int value)
            : base(value)
        {
        }

        public static SubsequentMessage ValueOf(int value)
        {
            if (value == 0)
                return encrCert;

            if (value == 1)
                return challengeResp;

            throw new ArgumentException("unknown value: " + value, "value");
        }
    }
}
