using System;
using System.IO;
namespace Org.BouncyCastle.Bcpg
{
	/// <remarks>Basic packet for a PGP public subkey</remarks>
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class PublicSubkeyPacket
        : PublicKeyPacket
    {
        internal PublicSubkeyPacket(
            BcpgInputStream bcpgIn)
			: base(bcpgIn)
        {
        }

		/// <summary>Construct a version 4 public subkey packet.</summary>
        public PublicSubkeyPacket(
            PublicKeyAlgorithmTag	algorithm,
            DateTime				time,
            IBcpgKey				key)
            : base(algorithm, time, key)
        {
        }

		public override void Encode(
            BcpgOutputStream bcpgOut)
        {
            bcpgOut.WritePacket(PacketTag.PublicSubkey, GetEncodedContents(), true);
        }
    }
}
