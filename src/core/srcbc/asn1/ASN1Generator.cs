using System;
using System.Collections;
using System.IO;

namespace Org.BouncyCastle.Asn1
{
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public abstract class Asn1Generator
    {
		private Stream _out;

		protected Asn1Generator(
			Stream outStream)
        {
            _out = outStream;
        }

		protected Stream Out
		{
			get { return _out; }
		}

		public abstract void AddObject(Asn1Encodable obj);

		public abstract Stream GetRawOutputStream();

		public abstract void Close();
    }
}
