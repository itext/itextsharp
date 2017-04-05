using System;

namespace Org.BouncyCastle.Asn1
{
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class DerOctetString
        : Asn1OctetString
    {
		/// <param name="str">The octets making up the octet string.</param>
        public DerOctetString(
			byte[] str)
			: base(str)
        {
        }

		public DerOctetString(
			Asn1Encodable obj)
			: base(obj)
        {
        }

        internal override void Encode(
            DerOutputStream derOut)
        {
            derOut.WriteEncoded(Asn1Tags.OctetString, str);
        }

		internal static void Encode(
			DerOutputStream	derOut,
			byte[]			bytes,
			int				offset,
			int				length)
		{
			derOut.WriteEncoded(Asn1Tags.OctetString, bytes, offset, length);
		}
	}
}
