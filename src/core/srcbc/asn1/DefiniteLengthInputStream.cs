using System;
using System.IO;

using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Asn1
{
    class DefiniteLengthInputStream
        : LimitedInputStream
    {
		private static readonly byte[] EmptyBytes = new byte[0];

		private int _length;

        internal DefiniteLengthInputStream(
            Stream	inStream,
            int		length)
            : base(inStream)
        {
			if (length < 0)
				throw new ArgumentException("negative lengths not allowed", "length");

			this._length = length;

			if (length == 0)
			{
				SetParentEofDetect(true);
			}
        }

		public override int ReadByte()
        {
			if (_length == 0)
				return -1;

			int b = _in.ReadByte();

			if (b < 0)
				throw new EndOfStreamException();

			if (--_length == 0)
			{
				SetParentEofDetect(true);
			}

			return b;
        }

		public override int Read(
			byte[]	buf,
			int		off,
			int		len)
		{
			if (_length == 0)
				return 0;

			int toRead = System.Math.Min(len, _length);
			int numRead = _in.Read(buf, off, toRead);

			if (numRead < 1)
				throw new EndOfStreamException();

			if ((_length -= numRead) == 0)
			{
				SetParentEofDetect(true);
			}

			return numRead;
		}

		internal byte[] ToArray()
		{
			if (_length == 0)
				return EmptyBytes;

			byte[] bytes = new byte[_length];
			if (Streams.ReadFully(_in, bytes) < _length)
				throw new EndOfStreamException();
			_length = 0;
			SetParentEofDetect(true);
			return bytes;
		}
    }
}
