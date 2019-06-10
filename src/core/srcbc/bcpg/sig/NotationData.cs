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

namespace Org.BouncyCastle.Bcpg.Sig
{
	/**
	* Class provided a NotationData object according to
	* RFC2440, Chapter 5.2.3.15. Notation Data
	*/
	public class NotationData
		: SignatureSubpacket
	{
		public const int HeaderFlagLength = 4;
		public const int HeaderNameLength = 2;
		public const int HeaderValueLength = 2;

		public NotationData(
			bool	critical,
			byte[]	data)
			: base(SignatureSubpacketTag.NotationData, critical, data)
		{
		}

		public NotationData(
			bool	critical,
			bool	humanReadable,
			string	notationName,
			string	notationValue)
			: base(SignatureSubpacketTag.NotationData, critical,
				createData(humanReadable, notationName, notationValue))
		{
		}

		private static byte[] createData(
			bool	humanReadable,
			string	notationName,
			string	notationValue)
		{
			MemoryStream os = new MemoryStream();

			// (4 octets of flags, 2 octets of name length (M),
			// 2 octets of value length (N),
			// M octets of name data,
			// N octets of value data)

			// flags
			os.WriteByte(humanReadable ? (byte)0x80 : (byte)0x00);
			os.WriteByte(0x0);
			os.WriteByte(0x0);
			os.WriteByte(0x0);

			byte[] nameData, valueData = null;
			int nameLength, valueLength;

			nameData = Encoding.UTF8.GetBytes(notationName);
			nameLength = System.Math.Min(nameData.Length, 0xFF);

			valueData = Encoding.UTF8.GetBytes(notationValue);
			valueLength = System.Math.Min(valueData.Length, 0xFF);

			// name length
			os.WriteByte((byte)(nameLength >> 8));
			os.WriteByte((byte)(nameLength >> 0));

			// value length
			os.WriteByte((byte)(valueLength >> 8));
			os.WriteByte((byte)(valueLength >> 0));

			// name
			os.Write(nameData, 0, nameLength);

			// value
			os.Write(valueData, 0, valueLength);

			return os.ToArray();
		}

		public bool IsHumanReadable
		{
			get { return data[0] == (byte)0x80; }
		}

		public string GetNotationName()
		{
			int nameLength = ((data[HeaderFlagLength] << 8) + (data[HeaderFlagLength + 1] << 0));
			int namePos = HeaderFlagLength + HeaderNameLength + HeaderValueLength;

			return Encoding.UTF8.GetString(data, namePos, nameLength);
		}

		public string GetNotationValue()
		{
			int nameLength = ((data[HeaderFlagLength] << 8) + (data[HeaderFlagLength + 1] << 0));
			int valueLength = ((data[HeaderFlagLength + HeaderNameLength] << 8) + (data[HeaderFlagLength + HeaderNameLength + 1] << 0));
			int valuePos = HeaderFlagLength + HeaderNameLength + HeaderValueLength + nameLength;

			return Encoding.UTF8.GetString(data, valuePos, valueLength);
		}

		public byte[] GetNotationValueBytes()
		{
			int nameLength = ((data[HeaderFlagLength] << 8) + (data[HeaderFlagLength + 1] << 0));
			int valueLength = ((data[HeaderFlagLength + HeaderNameLength] << 8) + (data[HeaderFlagLength + HeaderNameLength + 1] << 0));
			int valuePos = HeaderFlagLength + HeaderNameLength + HeaderValueLength + nameLength;

			byte[] bytes = new byte[valueLength];
			Array.Copy(data, valuePos, bytes, 0, valueLength);
			return bytes;
		}
	}
}
