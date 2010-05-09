using System;
using System.IO;

using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Cms
{
	internal class TeeInputStream
		: BaseInputStream
	{
		private readonly Stream input;
		// FIXME Need internal access in RecipientInformation atm
		internal readonly Stream output;

		internal TeeInputStream(Stream input, Stream output)
		{
			this.input = input;
			this.output = output;
		}

		public override int Read(byte[] buf, int off, int len)
		{
			int i = input.Read(buf, off, len);

			if (i > 0)
			{
				output.Write(buf, off, i);
			}

			return i;
		}

		public override int ReadByte()
		{
			int i = input.ReadByte();

			if (i >= 0)
			{
				output.WriteByte((byte)i);
			}

			return i;
		}

		public override void Close()
		{
			this.input.Close();
			this.output.Close();
		}
	}
}
