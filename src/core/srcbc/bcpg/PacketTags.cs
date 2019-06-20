/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2019 iText Group NV
    Authors: iText Software.

This program is free software; you can redistribute it and/or modify it under the terms of the GNU Affero General Public License version 3 as published by the Free Software Foundation with the addition of the following permission added to Section 15 as permitted in Section 7(a): FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY iText Group NV, iText Group NV DISCLAIMS THE WARRANTY OF NON INFRINGEMENT OF THIRD PARTY RIGHTS.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more details.
You should have received a copy of the GNU Affero General Public License along with this program; if not, see http://www.gnu.org/licenses or write to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA, 02110-1301 USA, or download the license from the following URL:

http://itextpdf.com/terms-of-use/

The interactive user interfaces in modified source and object code versions of this program must display Appropriate Legal Notices, as required under Section 5 of the GNU Affero General Public License.

In accordance with Section 7(b) of the GNU Affero General Public License, a covered work must retain the producer line in every PDF that is created or manipulated using iText.

You can be released from the requirements of the license by purchasing a commercial license. Buying such a license is mandatory as soon as you develop commercial activities involving the iText software without disclosing the source code of your own applications.
These activities include: offering paid services to customers as an ASP, serving PDFs on the fly in a web application, shipping iText with a closed source product.

For more information, please contact iText Software Corp. at this address: sales@itextpdf.com */
namespace Org.BouncyCastle.Bcpg
{
	/// <remarks>Basic PGP packet tag types.</remarks>
    public enum PacketTag
    {
        Reserved =  0,								//  Reserved - a packet tag must not have this value
        PublicKeyEncryptedSession = 1,				// Public-Key Encrypted Session Key Packet
        Signature = 2,								// Signature Packet
        SymmetricKeyEncryptedSessionKey = 3,		// Symmetric-Key Encrypted Session Key Packet
        OnePassSignature = 4,						// One-Pass Signature Packet
        SecretKey = 5,								// Secret Key Packet
        PublicKey = 6,								// Public Key Packet
        SecretSubkey = 7,							// Secret Subkey Packet
        CompressedData = 8,							// Compressed Data Packet
        SymmetricKeyEncrypted = 9,					// Symmetrically Encrypted Data Packet
        Marker = 10,								// Marker Packet
        LiteralData = 11,							// Literal Data Packet
        Trust = 12,									// Trust Packet
        UserId = 13,								// User ID Packet
        PublicSubkey = 14,							// Public Subkey Packet
        UserAttribute = 17,							// User attribute
        SymmetricEncryptedIntegrityProtected = 18,	// Symmetric encrypted, integrity protected
        ModificationDetectionCode = 19,				// Modification detection code

        Experimental1 = 60,							// Private or Experimental Values
        Experimental2 = 61,
        Experimental3 = 62,
        Experimental4 = 63
    }
}
