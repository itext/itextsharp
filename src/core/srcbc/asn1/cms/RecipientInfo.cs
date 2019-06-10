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

using Org.BouncyCastle.Asn1;

namespace Org.BouncyCastle.Asn1.Cms
{
    public class RecipientInfo
        : Asn1Encodable, IAsn1Choice
    {
        internal Asn1Encodable info;

		public RecipientInfo(
            KeyTransRecipientInfo info)
        {
            this.info = info;
        }

		public RecipientInfo(
            KeyAgreeRecipientInfo info)
        {
            this.info = new DerTaggedObject(false, 1, info);
        }

		public RecipientInfo(
            KekRecipientInfo info)
        {
            this.info = new DerTaggedObject(false, 2, info);
        }

		public RecipientInfo(
            PasswordRecipientInfo info)
        {
            this.info = new DerTaggedObject(false, 3, info);
        }

		public RecipientInfo(
            OtherRecipientInfo info)
        {
            this.info = new DerTaggedObject(false, 4, info);
        }

		public RecipientInfo(
            Asn1Object   info)
        {
            this.info = info;
        }

		public static RecipientInfo GetInstance(
            object o)
        {
            if (o == null || o is RecipientInfo)
                return (RecipientInfo) o;

			if (o is Asn1Sequence)
                return new RecipientInfo((Asn1Sequence) o);

			if (o is Asn1TaggedObject)
                return new RecipientInfo((Asn1TaggedObject) o);

			throw new ArgumentException("unknown object in factory: " + o.GetType().Name);
        }

		public DerInteger Version
        {
			get
			{
				if (info is Asn1TaggedObject)
				{
					Asn1TaggedObject o = (Asn1TaggedObject) info;

					switch (o.TagNo)
					{
						case 1:
							return KeyAgreeRecipientInfo.GetInstance(o, false).Version;
						case 2:
							return GetKekInfo(o).Version;
						case 3:
							return PasswordRecipientInfo.GetInstance(o, false).Version;
						case 4:
							return new DerInteger(0);    // no syntax version for OtherRecipientInfo
						default:
							throw new InvalidOperationException("unknown tag");
					}
				}

				return KeyTransRecipientInfo.GetInstance(info).Version;
			}
        }

		public bool IsTagged
		{
			get { return info is Asn1TaggedObject; }
		}

		public Asn1Encodable Info
        {
			get
			{
				if (info is Asn1TaggedObject)
				{
					Asn1TaggedObject o = (Asn1TaggedObject) info;

					switch (o.TagNo)
					{
						case 1:
							return KeyAgreeRecipientInfo.GetInstance(o, false);
						case 2:
							return GetKekInfo(o);
						case 3:
							return PasswordRecipientInfo.GetInstance(o, false);
						case 4:
							return OtherRecipientInfo.GetInstance(o, false);
						default:
							throw new InvalidOperationException("unknown tag");
					}
				}

				return KeyTransRecipientInfo.GetInstance(info);
			}
        }

		private KekRecipientInfo GetKekInfo(
			Asn1TaggedObject o)
		{
			// For compatibility with erroneous version, we don't always pass 'false' here
			return KekRecipientInfo.GetInstance(o, o.IsExplicit());
		}

		/**
         * Produce an object suitable for an Asn1OutputStream.
         * <pre>
         * RecipientInfo ::= CHOICE {
         *     ktri KeyTransRecipientInfo,
         *     kari [1] KeyAgreeRecipientInfo,
         *     kekri [2] KekRecipientInfo,
         *     pwri [3] PasswordRecipientInfo,
         *     ori [4] OtherRecipientInfo }
         * </pre>
         */
        public override Asn1Object ToAsn1Object()
        {
            return info.ToAsn1Object();
        }
    }
}
