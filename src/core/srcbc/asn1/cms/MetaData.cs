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

namespace Org.BouncyCastle.Asn1.Cms
{
	public class MetaData
		: Asn1Encodable
	{
		private DerBoolean hashProtected;
		private DerUtf8String fileName;
		private DerIA5String  mediaType;
		private Attributes otherMetaData;

		public MetaData(
			DerBoolean		hashProtected,
			DerUtf8String	fileName,
			DerIA5String	mediaType,
			Attributes		otherMetaData)
		{
			this.hashProtected = hashProtected;
			this.fileName = fileName;
			this.mediaType = mediaType;
			this.otherMetaData = otherMetaData;
		}

		private MetaData(Asn1Sequence seq)
		{
			this.hashProtected = DerBoolean.GetInstance(seq[0]);

			int index = 1;

			if (index < seq.Count && seq[index] is DerUtf8String)
			{
				this.fileName = DerUtf8String.GetInstance(seq[index++]);
			}
			if (index < seq.Count && seq[index] is DerIA5String)
			{
				this.mediaType = DerIA5String.GetInstance(seq[index++]);
			}
			if (index < seq.Count)
			{
				this.otherMetaData = Attributes.GetInstance(seq[index++]);
			}
		}

		public static MetaData GetInstance(object obj)
		{
			if (obj is MetaData)
				return (MetaData)obj;

			if (obj != null)
				return new MetaData(Asn1Sequence.GetInstance(obj));

			return null;
		}

		/**
		 * <pre>
		 * MetaData ::= SEQUENCE {
		 *   hashProtected        BOOLEAN,
		 *   fileName             UTF8String OPTIONAL,
		 *   mediaType            IA5String OPTIONAL,
		 *   otherMetaData        Attributes OPTIONAL
		 * }
		 * </pre>
		 * @return
		 */
		public override Asn1Object ToAsn1Object()
		{
			Asn1EncodableVector v = new Asn1EncodableVector(hashProtected);
			v.AddOptional(fileName, mediaType, otherMetaData);
			return new DerSequence(v);
		}

		public virtual bool IsHashProtected
		{
			get { return hashProtected.IsTrue; }
		}

		public virtual DerUtf8String FileName
		{
			get { return fileName; }
		}

		public virtual DerIA5String MediaType
		{
			get { return mediaType; }
		}

		public virtual Attributes OtherMetaData
		{
			get { return otherMetaData; }
		}
	}
}
