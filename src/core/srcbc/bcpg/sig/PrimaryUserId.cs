using System;



namespace Org.BouncyCastle.Bcpg.Sig
{
    /**
    * packet giving whether or not the signature is signed using the primary user ID for the key.
    */
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class PrimaryUserId
        : SignatureSubpacket
    {
        private static byte[] BooleanToByteArray(
            bool    val)
        {
            byte[]    data = new byte[1];

            if (val)
            {
                data[0] = 1;
                return data;
            }
            else
            {
                return data;
            }
        }

        public PrimaryUserId(
            bool    critical,
            byte[]     data)
            : base(SignatureSubpacketTag.PrimaryUserId, critical, data)
        {
        }

        public PrimaryUserId(
            bool    critical,
            bool    isPrimaryUserId)
            : base(SignatureSubpacketTag.PrimaryUserId, critical, BooleanToByteArray(isPrimaryUserId))
        {
        }

        public bool IsPrimaryUserId()
        {
            return data[0] != 0;
        }
    }
}
