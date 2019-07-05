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
using System.Text;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Utilities.Encoders;

namespace Org.BouncyCastle.X509
{
	class PemParser
	{
		private readonly string _header1;
		private readonly string _header2;
		private readonly string _footer1;
		private readonly string _footer2;

		internal PemParser(
			string type)
		{
			_header1 = "-----BEGIN " + type + "-----";
			_header2 = "-----BEGIN X509 " + type + "-----";
			_footer1 = "-----END " + type + "-----";
			_footer2 = "-----END X509 " + type + "-----";
		}

		private string ReadLine(
			Stream inStream)
		{
			int c;
			StringBuilder l = new StringBuilder();

			do
			{
				while (((c = inStream.ReadByte()) != '\r') && c != '\n' && (c >= 0))
				{
					if (c == '\r')
					{
						continue;
					}

					l.Append((char)c);
				}
			}
			while (c >= 0 && l.Length == 0);

			if (c < 0)
			{
				return null;
			}

			return l.ToString();
		}

		internal Asn1Sequence ReadPemObject(
			Stream inStream)
		{
			string line;
			StringBuilder pemBuf = new StringBuilder();

			while ((line = ReadLine(inStream)) != null)
			{
				if (line.StartsWith(_header1) || line.StartsWith(_header2))
				{
					break;
				}
			}

			while ((line = ReadLine(inStream)) != null)
			{
				if (line.StartsWith(_footer1) || line.StartsWith(_footer2))
				{
					break;
				}

				pemBuf.Append(line);
			}

			if (pemBuf.Length != 0)
			{
				Asn1Object o = Asn1Object.FromByteArray(Base64.Decode(pemBuf.ToString()));

				if (!(o is Asn1Sequence))
				{
					throw new IOException("malformed PEM data encountered");
				}

				return (Asn1Sequence) o;
			}

			return null;
		}
	}
}

