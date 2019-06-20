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

using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls
{
	/// <remarks>
	/// A generic TLS MAC implementation, which can be used with any kind of
	/// IDigest to act as an HMAC.
	/// </remarks>
	public class TlsMac
	{
		protected long seqNo;
		protected byte[] secret;
		protected HMac mac;

		/**
		* Generate a new instance of an TlsMac.
		*
		* @param digest    The digest to use.
		* @param key_block A byte-array where the key for this mac is located.
		* @param offset    The number of bytes to skip, before the key starts in the buffer.
		* @param len       The length of the key.
		*/
		public TlsMac(
			IDigest	digest,
			byte[]	key_block,
			int		offset,
			int		len)
		{
			this.seqNo = 0;

			KeyParameter param = new KeyParameter(key_block, offset, len);

			this.secret = Arrays.Clone(param.GetKey());

			this.mac = new HMac(digest);
			this.mac.Init(param);
		}

		/**
		 * @return the MAC write secret
		 */
		public virtual byte[] GetMacSecret()
		{
			return this.secret;
		}

		/**
		 * @return the current write sequence number
		 */
		public virtual long SequenceNumber
		{
			get { return this.seqNo; }
		}

		/**
		 * Increment the current write sequence number
		 */
		public virtual void IncSequenceNumber()
		{
			this.seqNo++;
		}

		/**
		* @return The Keysize of the mac.
		*/
		public virtual int Size
		{
			get { return mac.GetMacSize(); }
		}

		/**
		* Calculate the mac for some given data.
		* <p/>
		* TlsMac will keep track of the sequence number internally.
		*
		* @param type    The message type of the message.
		* @param message A byte-buffer containing the message.
		* @param offset  The number of bytes to skip, before the message starts.
		* @param len     The length of the message.
		* @return A new byte-buffer containing the mac value.
		*/
		public virtual byte[] CalculateMac(ContentType type, byte[] message, int offset, int len)
		{
            //bool isTls = context.ServerVersion.FullVersion >= ProtocolVersion.TLSv10.FullVersion;
            bool isTls = true;

            byte[] macHeader = new byte[isTls ? 13 : 11];
			TlsUtilities.WriteUint64(seqNo++, macHeader, 0);
			TlsUtilities.WriteUint8((byte)type, macHeader, 8);
            if (isTls)
            {
                TlsUtilities.WriteVersion(macHeader, 9);
            }
			TlsUtilities.WriteUint16(len, macHeader, 11);

            mac.BlockUpdate(macHeader, 0, macHeader.Length);
			mac.BlockUpdate(message, offset, len);
			return MacUtilities.DoFinal(mac);
		}

        public virtual byte[] CalculateMacConstantTime(ContentType type, byte[] message, int offset, int len,
            int fullLength, byte[] dummyData)
        {
            // Actual MAC only calculated on 'len' bytes
            byte[] result = CalculateMac(type, message, offset, len);

            //bool isTls = context.ServerVersion.FullVersion >= ProtocolVersion.TLSv10.FullVersion;
            bool isTls = true;

            // ...but ensure a constant number of complete digest blocks are processed (per 'fullLength')
            if (isTls)
            {
                // TODO Currently all TLS digests use a block size of 64, a suffix (length field) of 8, and padding (1+)
                int db = 64, ds = 8;

                int L1 = 13 + fullLength;
                int L2 = 13 + len;

                // How many extra full blocks do we need to calculate?
                int extra = ((L1 + ds) / db) - ((L2 + ds) / db);

                while (--extra >= 0)
                {
                    mac.BlockUpdate(dummyData, 0, db);
                }

                // One more byte in case the implementation is "lazy" about processing blocks
                mac.Update(dummyData[0]);
                mac.Reset();
            }

            return result;
        }
	}
}
