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
using System.Text;

using Org.BouncyCastle.Utilities.Encoders;

namespace Org.BouncyCastle.Utilities.IO.Pem
{
	public class PemReader
	{
		private const string BeginString = "-----BEGIN ";
		private const string EndString = "-----END ";

		private readonly TextReader reader;

		public PemReader(TextReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException("reader");

			this.reader = reader;
		}

		public TextReader Reader
		{
			get { return reader; }
		}

		/// <returns>
		/// A <see cref="PemObject"/>
		/// </returns>
		/// <exception cref="IOException"></exception>
		public PemObject ReadPemObject()
		{
			string line = reader.ReadLine();

			if (line != null && line.StartsWith(BeginString))
			{
				line = line.Substring(BeginString.Length);
				int index = line.IndexOf('-');
				string type = line.Substring(0, index);

				if (index > 0)
					return LoadObject(type);
			}

			return null;
		}

		private PemObject LoadObject(string type)
		{
			string endMarker = EndString + type;
			IList headers = Platform.CreateArrayList();
			StringBuilder buf = new StringBuilder();

			string line;
			while ((line = reader.ReadLine()) != null
				&& line.IndexOf(endMarker) == -1)
			{
				int colonPos = line.IndexOf(':');

				if (colonPos == -1)
				{
					buf.Append(line.Trim());
				}
				else
				{
					// Process field
					string fieldName = line.Substring(0, colonPos).Trim();

					if (fieldName.StartsWith("X-"))
						fieldName = fieldName.Substring(2);

					string fieldValue = line.Substring(colonPos + 1).Trim();

					headers.Add(new PemHeader(fieldName, fieldValue));
				}
			}

			if (line == null)
			{
				throw new IOException(endMarker + " not found");
			}

			if (buf.Length % 4 != 0)
			{
				throw new IOException("base64 data appears to be truncated");
			}

			return new PemObject(type, headers, Base64.Decode(buf.ToString()));
		}
	}
}
