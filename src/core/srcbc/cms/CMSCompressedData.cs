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
using Org.BouncyCastle.Utilities.Zlib;

namespace Org.BouncyCastle.Cms
{
    /**
    * containing class for an CMS Compressed Data object
    */
    public class CmsCompressedData
    {
        internal ContentInfo contentInfo;

		public CmsCompressedData(
            byte[] compressedData)
            : this(CmsUtilities.ReadContentInfo(compressedData))
        {
        }

		public CmsCompressedData(
            Stream compressedDataStream)
            : this(CmsUtilities.ReadContentInfo(compressedDataStream))
        {
        }

		public CmsCompressedData(
            ContentInfo contentInfo)
        {
            this.contentInfo = contentInfo;
        }

		/**
		 * Return the uncompressed content.
		 *
		 * @return the uncompressed content
		 * @throws CmsException if there is an exception uncompressing the data.
		 */
		public byte[] GetContent()
        {
            CompressedData comData = CompressedData.GetInstance(contentInfo.Content);
            ContentInfo content = comData.EncapContentInfo;

			Asn1OctetString bytes = (Asn1OctetString) content.Content;
			ZInputStream zIn = new ZInputStream(bytes.GetOctetStream());

			try
			{
				return CmsUtilities.StreamToByteArray(zIn);
			}
			catch (IOException e)
			{
				throw new CmsException("exception reading compressed stream.", e);
			}
			finally
			{
				zIn.Close();
			}
        }

	    /**
	     * Return the uncompressed content, throwing an exception if the data size
	     * is greater than the passed in limit. If the content is exceeded getCause()
	     * on the CMSException will contain a StreamOverflowException
	     *
	     * @param limit maximum number of bytes to read
	     * @return the content read
	     * @throws CMSException if there is an exception uncompressing the data.
	     */
		public byte[] GetContent(int limit)
		{
			CompressedData  comData = CompressedData.GetInstance(contentInfo.Content);
			ContentInfo     content = comData.EncapContentInfo;

			Asn1OctetString bytes = (Asn1OctetString)content.Content;

			ZInputStream zIn = new ZInputStream(new MemoryStream(bytes.GetOctets(), false));

			try
			{
				return CmsUtilities.StreamToByteArray(zIn, limit);
			}
			catch (IOException e)
			{
				throw new CmsException("exception reading compressed stream.", e);
			}
		}

		/**
		 * return the ContentInfo 
		 */
		public ContentInfo ContentInfo
		{
			get { return contentInfo; }
		}

		/**
        * return the ASN.1 encoded representation of this object.
        */
        public byte[] GetEncoded()
        {
			return contentInfo.GetEncoded();
        }
    }
}
