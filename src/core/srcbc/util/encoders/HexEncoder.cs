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

namespace Org.BouncyCastle.Utilities.Encoders
{
	public class HexEncoder
		: IEncoder
	{
		private static readonly byte[] encodingTable =
		{
			(byte)'0', (byte)'1', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7',
			(byte)'8', (byte)'9', (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', (byte)'f'
		};

        private static readonly byte[] decodingTable = ConstructDecodingTable(encodingTable);

        private static byte[] ConstructDecodingTable(byte[] et)
		{
            byte[] dt = new byte[128];
			for (int i = 0; i < et.Length; i++)
			{
				dt[et[i]] = (byte)i;
			}

			dt['A'] = dt['a'];
			dt['B'] = dt['b'];
			dt['C'] = dt['c'];
			dt['D'] = dt['d'];
			dt['E'] = dt['e'];
			dt['F'] = dt['f'];

            return dt;
		}

        /**
		* encode the input data producing a Hex output stream.
		*
		* @return the number of bytes produced.
		*/
		public int Encode(
			byte[]	data,
			int		off,
			int		length,
			Stream	outStream)
		{
			for (int i = off; i < (off + length); i++)
			{
				int v = data[i];

				outStream.WriteByte(encodingTable[v >> 4]);
				outStream.WriteByte(encodingTable[v & 0xf]);
			}

			return length * 2;
		}

        private static bool Ignore(char c)
        {
            return c == '\n' || c =='\r' || c == '\t' || c == ' ';
        }

        /**
		* decode the Hex encoded byte data writing it to the given output stream,
		* whitespace characters will be ignored.
		*
		* @return the number of bytes produced.
		*/
		public int Decode(
			byte[]	data,
			int		off,
			int		length,
			Stream	outStream)
		{
			byte b1, b2;
			int outLen = 0;
			int end = off + length;

			while (end > off)
			{
				if (!Ignore((char)data[end - 1]))
				{
					break;
				}

				end--;
			}

			int i = off;
			while (i < end)
			{
				while (i < end && Ignore((char)data[i]))
				{
					i++;
				}

				b1 = decodingTable[data[i++]];

				while (i < end && Ignore((char)data[i]))
				{
					i++;
				}

				b2 = decodingTable[data[i++]];

				outStream.WriteByte((byte)((b1 << 4) | b2));

				outLen++;
			}

			return outLen;
		}

		/**
		* decode the Hex encoded string data writing it to the given output stream,
		* whitespace characters will be ignored.
		*
		* @return the number of bytes produced.
		*/
		public int DecodeString(
			string	data,
			Stream	outStream)
		{
			byte    b1, b2;
			int     length = 0;

			int     end = data.Length;

			while (end > 0)
			{
				if (!Ignore(data[end - 1]))
				{
					break;
				}

				end--;
			}

			int i = 0;
			while (i < end)
			{
				while (i < end && Ignore(data[i]))
				{
					i++;
				}

				b1 = decodingTable[data[i++]];

				while (i < end && Ignore(data[i]))
				{
					i++;
				}

				b2 = decodingTable[data[i++]];

				outStream.WriteByte((byte)((b1 << 4) | b2));

				length++;
			}

			return length;
		}
	}
}
