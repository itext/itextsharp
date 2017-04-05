using System;
using System.IO;

namespace Org.BouncyCastle.Bcpg
{
	/// <remarks>Basic packet for an experimental packet.</remarks>
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class ExperimentalPacket
        : ContainedPacket //, PublicKeyAlgorithmTag
    {
        private readonly PacketTag	tag;
        private readonly byte[]		contents;

		internal ExperimentalPacket(
            PacketTag		tag,
            BcpgInputStream	bcpgIn)
        {
            this.tag = tag;

			this.contents = bcpgIn.ReadAll();
        }

		public PacketTag Tag
        {
			get { return tag; }
        }

		public byte[] GetContents()
        {
			return (byte[]) contents.Clone();
        }

		public override void Encode(
            BcpgOutputStream bcpgOut)
        {
            bcpgOut.WritePacket(tag, contents, true);
        }
    }
}
