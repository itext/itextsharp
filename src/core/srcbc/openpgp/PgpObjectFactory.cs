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
using System.Collections;
using System.IO;

using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Bcpg.OpenPgp
{
	/// <remarks>
    /// General class for reading a PGP object stream.
    /// <p>
    /// Note: if this class finds a PgpPublicKey or a PgpSecretKey it
    /// will create a PgpPublicKeyRing, or a PgpSecretKeyRing for each
    /// key found. If all you are trying to do is read a key ring file use
    /// either PgpPublicKeyRingBundle or PgpSecretKeyRingBundle.</p>
	/// </remarks>
	public class PgpObjectFactory
    {
        private readonly BcpgInputStream bcpgIn;

		public PgpObjectFactory(
            Stream inputStream)
        {
            this.bcpgIn = BcpgInputStream.Wrap(inputStream);
        }

        public PgpObjectFactory(
            byte[] bytes)
            : this(new MemoryStream(bytes, false))
        {
        }

		/// <summary>Return the next object in the stream, or null if the end is reached.</summary>
		/// <exception cref="IOException">On a parse error</exception>
        public PgpObject NextPgpObject()
        {
            PacketTag tag = bcpgIn.NextPacketTag();

            if ((int) tag == -1) return null;

            switch (tag)
            {
                case PacketTag.Signature:
                {
                    IList l = Platform.CreateArrayList();

                    while (bcpgIn.NextPacketTag() == PacketTag.Signature)
                    {
                        try
                        {
                            l.Add(new PgpSignature(bcpgIn));
                        }
                        catch (PgpException e)
                        {
                            throw new IOException("can't create signature object: " + e);
                        }
                    }

                    PgpSignature[] sigs = new PgpSignature[l.Count];
                    for (int i = 0; i < l.Count; ++i)
                    {
                        sigs[i] = (PgpSignature)l[i];
                    }
					return new PgpSignatureList(sigs);
                }
                case PacketTag.SecretKey:
                    try
                    {
                        return new PgpSecretKeyRing(bcpgIn);
                    }
                    catch (PgpException e)
                    {
                        throw new IOException("can't create secret key object: " + e);
                    }
                case PacketTag.PublicKey:
                    return new PgpPublicKeyRing(bcpgIn);
				// TODO Make PgpPublicKey a PgpObject or return a PgpPublicKeyRing
//				case PacketTag.PublicSubkey:
//					return PgpPublicKeyRing.ReadSubkey(bcpgIn);
                case PacketTag.CompressedData:
                    return new PgpCompressedData(bcpgIn);
                case PacketTag.LiteralData:
                    return new PgpLiteralData(bcpgIn);
                case PacketTag.PublicKeyEncryptedSession:
                case PacketTag.SymmetricKeyEncryptedSessionKey:
                    return new PgpEncryptedDataList(bcpgIn);
                case PacketTag.OnePassSignature:
                {
                    IList l = Platform.CreateArrayList();

                    while (bcpgIn.NextPacketTag() == PacketTag.OnePassSignature)
                    {
                        try
                        {
                            l.Add(new PgpOnePassSignature(bcpgIn));
                        }
                        catch (PgpException e)
                        {
							throw new IOException("can't create one pass signature object: " + e);
						}
                    }

                    PgpOnePassSignature[] sigs = new PgpOnePassSignature[l.Count];
                    for (int i = 0; i < l.Count; ++i)
                    {
                        sigs[i] = (PgpOnePassSignature)l[i];
                    }
					return new PgpOnePassSignatureList(sigs);
                }
                case PacketTag.Marker:
                    return new PgpMarker(bcpgIn);
                case PacketTag.Experimental1:
                case PacketTag.Experimental2:
                case PacketTag.Experimental3:
                case PacketTag.Experimental4:
					return new PgpExperimental(bcpgIn);
            }

            throw new IOException("unknown object in stream " + bcpgIn.NextPacketTag());
        }

		[Obsolete("Use NextPgpObject() instead")]
		public object NextObject()
		{
			return NextPgpObject();
		}

		/// <summary>
		/// Return all available objects in a list.
		/// </summary>
		/// <returns>An <c>IList</c> containing all objects from this factory, in order.</returns>
		public IList AllPgpObjects()
		{
            IList result = Platform.CreateArrayList();
			PgpObject pgpObject;
			while ((pgpObject = NextPgpObject()) != null)
			{
				result.Add(pgpObject);
			}
			return result;
		}
	}
}
