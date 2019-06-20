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

namespace Org.BouncyCastle.Bcpg.Attr
{
	/// <remarks>Basic type for a image attribute packet.</remarks>
    public class ImageAttrib
		: UserAttributeSubpacket
    {
		public enum Format : byte
		{
			Jpeg = 1
		}

		private static readonly byte[] Zeroes = new byte[12];

		private int     hdrLength;
        private int     _version;
        private int     _encoding;
        private byte[]  imageData;

        public ImageAttrib(
            byte[] data)
            : base(UserAttributeSubpacketTag.ImageAttribute, data)
        {
            hdrLength = ((data[1] & 0xff) << 8) | (data[0] & 0xff);
            _version = data[2] & 0xff;
            _encoding = data[3] & 0xff;

            imageData = new byte[data.Length - hdrLength];
            Array.Copy(data, hdrLength, imageData, 0, imageData.Length);
        }

		public ImageAttrib(
			Format	imageType,
			byte[]	imageData)
			: this(ToByteArray(imageType, imageData))
		{
		}

		private static byte[] ToByteArray(
			Format	imageType,
			byte[]	imageData)
		{
			MemoryStream bOut = new MemoryStream();
			bOut.WriteByte(0x10); bOut.WriteByte(0x00); bOut.WriteByte(0x01);
			bOut.WriteByte((byte) imageType);
			bOut.Write(Zeroes, 0, Zeroes.Length);
			bOut.Write(imageData, 0, imageData.Length);
			return bOut.ToArray();
		}

		public int Version
        {
			get { return _version; }
        }

        public int Encoding
        {
			get { return _encoding; }
        }

		public byte[] GetImageData()
        {
            return imageData;
        }
    }
}
