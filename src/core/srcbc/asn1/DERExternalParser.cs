using System;
using System.IO;

namespace Org.BouncyCastle.Asn1
{
	[Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class DerExternalParser
		: Asn1Encodable
	{
		private readonly Asn1StreamParser _parser;

		public DerExternalParser(Asn1StreamParser parser)
		{
			this._parser = parser;
		}

		public IAsn1Convertible ReadObject()
		{
			return _parser.ReadObject();
		}

		public override Asn1Object ToAsn1Object()
		{
			return new DerExternal(_parser.ReadVector());
		}
	}
}
