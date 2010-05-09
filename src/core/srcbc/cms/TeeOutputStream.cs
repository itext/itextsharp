using System;
using System.Diagnostics;
using System.IO;

using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Cms
{
	internal class TeeOutputStream
		: BaseOutputStream
	{
		private readonly Stream s1, s2;

		internal TeeOutputStream(Stream dataOutputStream, Stream digStream)
		{
			Debug.Assert(dataOutputStream.CanWrite);
			Debug.Assert(digStream.CanWrite);

			this.s1 = dataOutputStream;
			this.s2 = digStream;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			s1.Write(buffer, offset, count);
			s2.Write(buffer, offset, count);
		}

		public override void WriteByte(byte b)
		{
			s1.WriteByte(b);
			s2.WriteByte(b);
		}

		public override void Close()
		{
			s1.Close();
			s2.Close();
		}
	}
}
