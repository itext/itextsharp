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

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities.IO;
using Org.BouncyCastle.Utilities.Zlib;

namespace Org.BouncyCastle.Cms
{
	/**
	* General class for generating a compressed CMS message stream.
	* <p>
	* A simple example of usage.
	* </p>
	* <pre>
	*      CMSCompressedDataStreamGenerator gen = new CMSCompressedDataStreamGenerator();
	*
	*      Stream cOut = gen.Open(outputStream, CMSCompressedDataStreamGenerator.ZLIB);
	*
	*      cOut.Write(data);
	*
	*      cOut.Close();
	* </pre>
	*/
	public class CmsCompressedDataStreamGenerator
	{
		public const string ZLib = "1.2.840.113549.1.9.16.3.8";

		private int _bufferSize;
		
		/**
		* base constructor
		*/
		public CmsCompressedDataStreamGenerator()
		{
		}

		/**
		* Set the underlying string size for encapsulated data
		*
		* @param bufferSize length of octet strings to buffer the data.
		*/
		public void SetBufferSize(
			int bufferSize)
		{
			_bufferSize = bufferSize;
		}

		public Stream Open(
			Stream	outStream,
			string	compressionOID)
		{
			return Open(outStream, CmsObjectIdentifiers.Data.Id, compressionOID);
		}

		public Stream Open(
			Stream	outStream,
			string	contentOID,
			string	compressionOID)
		{
			BerSequenceGenerator sGen = new BerSequenceGenerator(outStream);

			sGen.AddObject(CmsObjectIdentifiers.CompressedData);

			//
			// Compressed Data
			//
			BerSequenceGenerator cGen = new BerSequenceGenerator(
				sGen.GetRawOutputStream(), 0, true);

			// CMSVersion
			cGen.AddObject(new DerInteger(0));

			// CompressionAlgorithmIdentifier
			cGen.AddObject(new AlgorithmIdentifier(new DerObjectIdentifier(ZLib)));

			//
			// Encapsulated ContentInfo
			//
			BerSequenceGenerator eiGen = new BerSequenceGenerator(cGen.GetRawOutputStream());

			eiGen.AddObject(new DerObjectIdentifier(contentOID));

			Stream octetStream = CmsUtilities.CreateBerOctetOutputStream(
				eiGen.GetRawOutputStream(), 0, true, _bufferSize);

			return new CmsCompressedOutputStream(
				new ZOutputStream(octetStream, JZlib.Z_DEFAULT_COMPRESSION), sGen, cGen, eiGen);
		}

		private class CmsCompressedOutputStream
			: BaseOutputStream
		{
			private ZOutputStream _out;
			private BerSequenceGenerator _sGen;
			private BerSequenceGenerator _cGen;
			private BerSequenceGenerator _eiGen;

			internal CmsCompressedOutputStream(
				ZOutputStream			outStream,
				BerSequenceGenerator	sGen,
				BerSequenceGenerator	cGen,
				BerSequenceGenerator	eiGen)
			{
				_out = outStream;
				_sGen = sGen;
				_cGen = cGen;
				_eiGen = eiGen;
			}

			public override void WriteByte(
				byte b)
			{
				_out.WriteByte(b);
			}

			public override void Write(
				byte[]	bytes,
				int		off,
				int		len)
			{
				_out.Write(bytes, off, len);
			}

			public override void Close()
			{
				_out.Close();

				// TODO Parent context(s) should really be be closed explicitly

				_eiGen.Close();
				_cGen.Close();
				_sGen.Close();
				base.Close();
			}
		}
	}
}
