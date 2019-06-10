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
using System;

namespace Org.BouncyCastle.Asn1.Gnu
{
	public abstract class GnuObjectIdentifiers
	{
		public static readonly DerObjectIdentifier Gnu					= new DerObjectIdentifier("1.3.6.1.4.1.11591.1"); // GNU Radius
		public static readonly DerObjectIdentifier GnuPG				= new DerObjectIdentifier("1.3.6.1.4.1.11591.2"); // GnuPG (Ägypten)
		public static readonly DerObjectIdentifier Notation				= new DerObjectIdentifier("1.3.6.1.4.1.11591.2.1"); // notation
		public static readonly DerObjectIdentifier PkaAddress			= new DerObjectIdentifier("1.3.6.1.4.1.11591.2.1.1"); // pkaAddress
		public static readonly DerObjectIdentifier GnuRadar				= new DerObjectIdentifier("1.3.6.1.4.1.11591.3"); // GNU Radar
		public static readonly DerObjectIdentifier DigestAlgorithm		= new DerObjectIdentifier("1.3.6.1.4.1.11591.12"); // digestAlgorithm
		public static readonly DerObjectIdentifier Tiger192				= new DerObjectIdentifier("1.3.6.1.4.1.11591.12.2"); // TIGER/192
		public static readonly DerObjectIdentifier EncryptionAlgorithm	= new DerObjectIdentifier("1.3.6.1.4.1.11591.13"); // encryptionAlgorithm
		public static readonly DerObjectIdentifier Serpent				= new DerObjectIdentifier("1.3.6.1.4.1.11591.13.2"); // Serpent
		public static readonly DerObjectIdentifier Serpent128Ecb		= new DerObjectIdentifier("1.3.6.1.4.1.11591.13.2.1"); // Serpent-128-ECB
		public static readonly DerObjectIdentifier Serpent128Cbc		= new DerObjectIdentifier("1.3.6.1.4.1.11591.13.2.2"); // Serpent-128-CBC
		public static readonly DerObjectIdentifier Serpent128Ofb		= new DerObjectIdentifier("1.3.6.1.4.1.11591.13.2.3"); // Serpent-128-OFB
		public static readonly DerObjectIdentifier Serpent128Cfb		= new DerObjectIdentifier("1.3.6.1.4.1.11591.13.2.4"); // Serpent-128-CFB
		public static readonly DerObjectIdentifier Serpent192Ecb		= new DerObjectIdentifier("1.3.6.1.4.1.11591.13.2.21"); // Serpent-192-ECB
		public static readonly DerObjectIdentifier Serpent192Cbc		= new DerObjectIdentifier("1.3.6.1.4.1.11591.13.2.22"); // Serpent-192-CBC
		public static readonly DerObjectIdentifier Serpent192Ofb		= new DerObjectIdentifier("1.3.6.1.4.1.11591.13.2.23"); // Serpent-192-OFB
		public static readonly DerObjectIdentifier Serpent192Cfb		= new DerObjectIdentifier("1.3.6.1.4.1.11591.13.2.24"); // Serpent-192-CFB
		public static readonly DerObjectIdentifier Serpent256Ecb		= new DerObjectIdentifier("1.3.6.1.4.1.11591.13.2.41"); // Serpent-256-ECB
		public static readonly DerObjectIdentifier Serpent256Cbc		= new DerObjectIdentifier("1.3.6.1.4.1.11591.13.2.42"); // Serpent-256-CBC
		public static readonly DerObjectIdentifier Serpent256Ofb		= new DerObjectIdentifier("1.3.6.1.4.1.11591.13.2.43"); // Serpent-256-OFB
		public static readonly DerObjectIdentifier Serpent256Cfb		= new DerObjectIdentifier("1.3.6.1.4.1.11591.13.2.44"); // Serpent-256-CFB
		public static readonly DerObjectIdentifier Crc					= new DerObjectIdentifier("1.3.6.1.4.1.11591.14"); // CRC algorithms
		public static readonly DerObjectIdentifier Crc32				= new DerObjectIdentifier("1.3.6.1.4.1.11591.14.1"); // CRC 32
	}
}
