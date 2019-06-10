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

using Org.BouncyCastle.Utilities.Encoders;

namespace Org.BouncyCastle.Utilities.IO.Pem
{
	/**
	* A generic PEM writer, based on RFC 1421
	*/
	public class PemWriter
	{
		private const int LineLength = 64;

		private readonly TextWriter	writer;
		private readonly int		nlLength;
		private char[]				buf = new char[LineLength];
		
		/**
		 * Base constructor.
		 *
		 * @param out output stream to use.
		 */
		public PemWriter(TextWriter writer)
		{
			if (writer == null)
				throw new ArgumentNullException("writer");

			this.writer = writer;
			this.nlLength = Platform.NewLine.Length;
		}

		public TextWriter Writer
		{
			get { return writer; }
		}

		/**
		 * Return the number of bytes or characters required to contain the
		 * passed in object if it is PEM encoded.
		 *
		 * @param obj pem object to be output
		 * @return an estimate of the number of bytes
		 */
		public int GetOutputSize(PemObject obj)
		{
			// BEGIN and END boundaries.
			int size = (2 * (obj.Type.Length + 10 + nlLength)) + 6 + 4;

			if (obj.Headers.Count > 0)
			{
				foreach (PemHeader header in obj.Headers)
				{
					size += header.Name.Length + ": ".Length + header.Value.Length + nlLength;
				}

				size += nlLength;
			}

			// base64 encoding
			int dataLen = ((obj.Content.Length + 2) / 3) * 4;

			size += dataLen + (((dataLen + LineLength - 1) / LineLength) * nlLength);

			return size;
		}

		public void WriteObject(PemObjectGenerator objGen)
		{
			PemObject obj = objGen.Generate();

			WritePreEncapsulationBoundary(obj.Type);

			if (obj.Headers.Count > 0)
			{
				foreach (PemHeader header in obj.Headers)
				{
					writer.Write(header.Name);
					writer.Write(": ");
					writer.WriteLine(header.Value);
				}

				writer.WriteLine();
			}

			WriteEncoded(obj.Content);
			WritePostEncapsulationBoundary(obj.Type);
		}

		private void WriteEncoded(byte[] bytes)
		{
			bytes = Base64.Encode(bytes);

			for (int i = 0; i < bytes.Length; i += buf.Length)
			{
				int index = 0;
				while (index != buf.Length)
				{
					if ((i + index) >= bytes.Length)
						break;

					buf[index] = (char)bytes[i + index];
					index++;
				}
				writer.WriteLine(buf, 0, index);
			}
		}

		private void WritePreEncapsulationBoundary(string type)
		{
			writer.WriteLine("-----BEGIN " + type + "-----");
		}

		private void WritePostEncapsulationBoundary(string type)
		{
			writer.WriteLine("-----END " + type + "-----");
		}
	}
}
