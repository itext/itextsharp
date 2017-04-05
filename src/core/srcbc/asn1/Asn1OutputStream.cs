using System;
using System.IO;

namespace Org.BouncyCastle.Asn1
{
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class Asn1OutputStream
        : DerOutputStream
    {
        public Asn1OutputStream(Stream os) : base(os)
        {
        }

		[Obsolete("Use version taking an Asn1Encodable arg instead")]
        public override void WriteObject(
            object    obj)
        {
            if (obj == null)
            {
                WriteNull();
            }
            else if (obj is Asn1Object)
            {
                ((Asn1Object)obj).Encode(this);
            }
            else if (obj is Asn1Encodable)
            {
                ((Asn1Encodable)obj).ToAsn1Object().Encode(this);
            }
            else
            {
                throw new IOException("object not Asn1Encodable");
            }
        }
    }
}
