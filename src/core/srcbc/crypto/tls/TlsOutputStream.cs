using System;

using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Crypto.Tls
{
	/// <remarks>An output Stream for a TLS 1.0 connection.</remarks>
	internal class TlsOutputStream
		: BaseOutputStream
	{
		private readonly TlsProtocolHandler handler;

		internal TlsOutputStream(
			TlsProtocolHandler handler)
		{
			this.handler = handler;
		}

		public override void Write(
			byte[]	buf,
			int		offset,
			int		len)
		{
			this.handler.WriteData(buf, offset, len);
		}

		public override void WriteByte(byte b)
		{
			this.Write(b);
		}

		public override void Close()
		{
			handler.Close();
			base.Close();
		}

		public override void Flush()
		{
			handler.Flush();
		}
	}
}