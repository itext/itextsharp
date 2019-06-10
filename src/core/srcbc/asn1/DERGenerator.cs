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
using System.IO;

using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Asn1
{
    public abstract class DerGenerator
        : Asn1Generator
    {
        private bool _tagged = false;
        private bool _isExplicit;
        private int _tagNo;

		protected DerGenerator(
            Stream outStream)
            : base(outStream)
		{
        }

        protected DerGenerator(
            Stream outStream,
            int tagNo,
            bool isExplicit)
            : base(outStream)
        {
            _tagged = true;
            _isExplicit = isExplicit;
            _tagNo = tagNo;
        }

        private static void WriteLength(
            Stream	outStr,
            int		length)
        {
            if (length > 127)
            {
                int size = 1;
                int val = length;

				while ((val >>= 8) != 0)
                {
                    size++;
                }

				outStr.WriteByte((byte)(size | 0x80));

				for (int i = (size - 1) * 8; i >= 0; i -= 8)
                {
                    outStr.WriteByte((byte)(length >> i));
                }
            }
            else
            {
                outStr.WriteByte((byte)length);
            }
        }

		internal static void WriteDerEncoded(
            Stream	outStream,
            int		tag,
            byte[]	bytes)
        {
            outStream.WriteByte((byte) tag);
            WriteLength(outStream, bytes.Length);
            outStream.Write(bytes, 0, bytes.Length);
        }

		internal void WriteDerEncoded(
            int		tag,
            byte[]	bytes)
        {
            if (_tagged)
            {
                int tagNum = _tagNo | Asn1Tags.Tagged;

                if (_isExplicit)
                {
                    int newTag = _tagNo | Asn1Tags.Constructed | Asn1Tags.Tagged;
					MemoryStream bOut = new MemoryStream();
                    WriteDerEncoded(bOut, tag, bytes);
                    WriteDerEncoded(Out, newTag, bOut.ToArray());
                }
                else
                {
					if ((tag & Asn1Tags.Constructed) != 0)
					{
						tagNum |= Asn1Tags.Constructed;
					}

					WriteDerEncoded(Out, tagNum, bytes);
                }
            }
            else
            {
                WriteDerEncoded(Out, tag, bytes);
            }
        }

		internal static void WriteDerEncoded(
            Stream	outStr,
            int		tag,
            Stream	inStr)
        {
			WriteDerEncoded(outStr, tag, Streams.ReadAll(inStr));
        }
    }
}
