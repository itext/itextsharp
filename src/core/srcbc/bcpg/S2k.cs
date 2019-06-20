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
using System.IO;

using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Bcpg
{
	/// <remarks>The string to key specifier class.</remarks>
    public class S2k
        : BcpgObject
    {
        private const int ExpBias = 6;

        public const int Simple = 0;
        public const int Salted = 1;
        public const int SaltedAndIterated = 3;
        public const int GnuDummyS2K = 101;

        internal int type;
        internal HashAlgorithmTag algorithm;
        internal byte[] iv;
        internal int itCount = -1;
        internal int protectionMode = -1;

        internal S2k(
            Stream inStr)
        {
			type = inStr.ReadByte();
            algorithm = (HashAlgorithmTag) inStr.ReadByte();

            //
            // if this happens we have a dummy-S2k packet.
            //
            if (type != GnuDummyS2K)
            {
                if (type != 0)
                {
					iv = new byte[8];
					if (Streams.ReadFully(inStr, iv, 0, iv.Length) < iv.Length)
						throw new EndOfStreamException();

					if (type == 3)
					{
						itCount = inStr.ReadByte();
					}
				}
            }
            else
            {
                inStr.ReadByte(); // G
                inStr.ReadByte(); // N
                inStr.ReadByte(); // U
                protectionMode = inStr.ReadByte(); // protection mode
            }
        }

        public S2k(
            HashAlgorithmTag algorithm)
        {
            this.type = 0;
            this.algorithm = algorithm;
        }

        public S2k(
            HashAlgorithmTag algorithm,
            byte[] iv)
        {
            this.type = 1;
            this.algorithm = algorithm;
            this.iv = iv;
        }

        public S2k(
            HashAlgorithmTag algorithm,
            byte[] iv,
            int itCount)
        {
            this.type = 3;
            this.algorithm = algorithm;
            this.iv = iv;
            this.itCount = itCount;
        }

        public int Type
        {
			get { return type; }
        }

		/// <summary>The hash algorithm.</summary>
        public HashAlgorithmTag HashAlgorithm
        {
			get { return algorithm; }
		}

		/// <summary>The IV for the key generation algorithm.</summary>
        public byte[] GetIV()
        {
            return Arrays.Clone(iv);
        }

		[Obsolete("Use 'IterationCount' property instead")]
        public long GetIterationCount()
        {
            return IterationCount;
        }

		/// <summary>The iteration count</summary>
		public long IterationCount
		{
			get { return (16 + (itCount & 15)) << ((itCount >> 4) + ExpBias); }
		}

		/// <summary>The protection mode - only if GnuDummyS2K</summary>
        public int ProtectionMode
        {
			get { return protectionMode; }
        }

        public override void Encode(
            BcpgOutputStream bcpgOut)
        {
            bcpgOut.WriteByte((byte) type);
            bcpgOut.WriteByte((byte) algorithm);

            if (type != GnuDummyS2K)
            {
                if (type != 0)
                {
                    bcpgOut.Write(iv);
                }

                if (type == 3)
                {
                    bcpgOut.WriteByte((byte) itCount);
                }
            }
            else
            {
                bcpgOut.WriteByte((byte) 'G');
                bcpgOut.WriteByte((byte) 'N');
                bcpgOut.WriteByte((byte) 'U');
                bcpgOut.WriteByte((byte) protectionMode);
            }
        }
    }
}
