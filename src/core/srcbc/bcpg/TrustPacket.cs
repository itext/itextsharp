using System;
using System.IO;

namespace Org.BouncyCastle.Bcpg
{
	/// <summary>Basic type for a trust packet.</summary>
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class TrustPacket
        : ContainedPacket
    {
        private readonly byte[] levelAndTrustAmount;

		public TrustPacket(
            BcpgInputStream bcpgIn)
        {
            MemoryStream bOut = new MemoryStream();

			int ch;
            while ((ch = bcpgIn.ReadByte()) >= 0)
            {
                bOut.WriteByte((byte) ch);
            }

			levelAndTrustAmount = bOut.ToArray();
        }

		public TrustPacket(
            int trustCode)
        {
			this.levelAndTrustAmount = new byte[]{ (byte) trustCode };
        }

		public byte[] GetLevelAndTrustAmount()
		{
			return (byte[]) levelAndTrustAmount.Clone();
		}

		public override void Encode(
            BcpgOutputStream bcpgOut)
        {
            bcpgOut.WritePacket(PacketTag.Trust, levelAndTrustAmount, true);
        }
    }
}
