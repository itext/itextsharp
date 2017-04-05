using System;
using System.Diagnostics;
using System.IO;

namespace Org.BouncyCastle.Utilities.IO
{
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class TeeOutputStream
		: BaseOutputStream
	{
		private readonly Stream output, tee;

		public TeeOutputStream(Stream output, Stream tee)
		{
			Debug.Assert(output.CanWrite);
			Debug.Assert(tee.CanWrite);

			this.output = output;
			this.tee = tee;
		}

		public override void Close()
		{
			output.Close();
			tee.Close();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			output.Write(buffer, offset, count);
			tee.Write(buffer, offset, count);
		}

		public override void WriteByte(byte b)
		{
			output.WriteByte(b);
			tee.WriteByte(b);
		}
	}
}
