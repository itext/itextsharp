using System;

namespace Org.BouncyCastle.Bcpg.OpenPgp
{
	/// <remarks>Key flag values for the KeyFlags subpacket.</remarks>
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public abstract class PgpKeyFlags
    {
        public const int CanCertify = 0x01; // This key may be used to certify other keys.
        public const int CanSign = 0x02; // This key may be used to sign data.
        public const int CanEncryptCommunications = 0x04; // This key may be used to encrypt communications.
        public const int CanEncryptStorage = 0x08; // This key may be used to encrypt storage.
        public const int MaybeSplit = 0x10; // The private component of this key may have been split by a secret-sharing mechanism.
        public const int MaybeShared = 0x80; // The private component of this key may be in the possession of more than one person.
    }
}
