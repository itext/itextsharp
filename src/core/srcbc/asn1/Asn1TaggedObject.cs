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

using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1
{
    /**
     * ASN.1 TaggedObject - in ASN.1 notation this is any object preceded by
     * a [n] where n is some number - these are assumed to follow the construction
     * rules (as with sequences).
     */
    public abstract class Asn1TaggedObject
		: Asn1Object, Asn1TaggedObjectParser
    {
        internal int            tagNo;
//        internal bool           empty;
        internal bool           explicitly = true;
        internal Asn1Encodable  obj;

		static public Asn1TaggedObject GetInstance(
            Asn1TaggedObject	obj,
            bool				explicitly)
        {
            if (explicitly)
            {
                return (Asn1TaggedObject) obj.GetObject();
            }

            throw new ArgumentException("implicitly tagged tagged object");
        }

		static public Asn1TaggedObject GetInstance(
			object obj)
		{
			if (obj == null || obj is Asn1TaggedObject)
			{
				return (Asn1TaggedObject) obj;
			}

			throw new ArgumentException("Unknown object in GetInstance: " + obj.GetType().FullName, "obj");
		}

		/**
         * @param tagNo the tag number for this object.
         * @param obj the tagged object.
         */
        protected Asn1TaggedObject(
            int             tagNo,
            Asn1Encodable   obj)
        {
            this.explicitly = true;
            this.tagNo = tagNo;
            this.obj = obj;
        }

		/**
         * @param explicitly true if the object is explicitly tagged.
         * @param tagNo the tag number for this object.
         * @param obj the tagged object.
         */
        protected Asn1TaggedObject(
            bool            explicitly,
            int             tagNo,
            Asn1Encodable   obj)
        {
			// IAsn1Choice marker interface 'insists' on explicit tagging
            this.explicitly = explicitly || (obj is IAsn1Choice);
            this.tagNo = tagNo;
            this.obj = obj;
        }

		protected override bool Asn1Equals(
			Asn1Object asn1Object)
        {
			Asn1TaggedObject other = asn1Object as Asn1TaggedObject;

			if (other == null)
				return false;

			return this.tagNo == other.tagNo
//				&& this.empty == other.empty
				&& this.explicitly == other.explicitly   // TODO Should this be part of equality?
				&& Platform.Equals(GetObject(), other.GetObject());
		}

		protected override int Asn1GetHashCode()
		{
            int code = tagNo.GetHashCode();

			// TODO: actually this is wrong - the problem is that a re-encoded
			// object may end up with a different hashCode due to implicit
			// tagging. As implicit tagging is ambiguous if a sequence is involved
			// it seems the only correct method for both equals and hashCode is to
			// compare the encodings...
//			code ^= explicitly.GetHashCode();

			if (obj != null)
            {
                code ^= obj.GetHashCode();
            }

			return code;
        }

		public int TagNo
        {
			get { return tagNo; }
        }

		/**
         * return whether or not the object may be explicitly tagged.
         * <p>
         * Note: if the object has been read from an input stream, the only
         * time you can be sure if isExplicit is returning the true state of
         * affairs is if it returns false. An implicitly tagged object may appear
         * to be explicitly tagged, so you need to understand the context under
         * which the reading was done as well, see GetObject below.</p>
         */
        public bool IsExplicit()
        {
            return explicitly;
        }

        public bool IsEmpty()
        {
            return false; //empty;
        }

		/**
         * return whatever was following the tag.
         * <p>
         * Note: tagged objects are generally context dependent if you're
         * trying to extract a tagged object you should be going via the
         * appropriate GetInstance method.</p>
         */
        public Asn1Object GetObject()
        {
            if (obj != null)
            {
                return obj.ToAsn1Object();
            }

			return null;
        }

		/**
		* Return the object held in this tagged object as a parser assuming it has
		* the type of the passed in tag. If the object doesn't have a parser
		* associated with it, the base object is returned.
		*/
		public IAsn1Convertible GetObjectParser(
			int		tag,
			bool	isExplicit)
		{
			switch (tag)
			{
				case Asn1Tags.Set:
					return Asn1Set.GetInstance(this, isExplicit).Parser;
				case Asn1Tags.Sequence:
					return Asn1Sequence.GetInstance(this, isExplicit).Parser;
				case Asn1Tags.OctetString:
					return Asn1OctetString.GetInstance(this, isExplicit).Parser;
			}

			if (isExplicit)
			{
				return GetObject();
			}

			throw Platform.CreateNotImplementedException("implicit tagging for tag: " + tag);
		}

		public override string ToString()
		{
			return "[" + tagNo + "]" + obj;
		}
	}
}
