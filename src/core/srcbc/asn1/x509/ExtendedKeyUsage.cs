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

using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.X509
{
    /**
     * The extendedKeyUsage object.
     * <pre>
     *      extendedKeyUsage ::= Sequence SIZE (1..MAX) OF KeyPurposeId
     * </pre>
     */
    public class ExtendedKeyUsage
        : Asn1Encodable
    {
        internal readonly IDictionary usageTable = Platform.CreateHashtable();
        internal readonly Asn1Sequence seq;

		public static ExtendedKeyUsage GetInstance(
            Asn1TaggedObject	obj,
            bool				explicitly)
        {
            return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
        }

		public static ExtendedKeyUsage GetInstance(
            object obj)
        {
            if (obj is ExtendedKeyUsage)
            {
                return (ExtendedKeyUsage) obj;
            }

			if (obj is Asn1Sequence)
            {
                return new ExtendedKeyUsage((Asn1Sequence) obj);
            }

			if (obj is X509Extension)
			{
				return GetInstance(X509Extension.ConvertValueToObject((X509Extension) obj));
			}

			throw new ArgumentException("Invalid ExtendedKeyUsage: " + obj.GetType().Name);
        }

		private ExtendedKeyUsage(
            Asn1Sequence seq)
        {
            this.seq = seq;

			foreach (object o in seq)
			{
				if (!(o is DerObjectIdentifier))
					throw new ArgumentException("Only DerObjectIdentifier instances allowed in ExtendedKeyUsage.");

				this.usageTable.Add(o, o);
            }
        }

		public ExtendedKeyUsage(
			params KeyPurposeID[] usages)
		{
			this.seq = new DerSequence(usages);

			foreach (KeyPurposeID usage in usages)
			{
				this.usageTable.Add(usage, usage);
			}
		}

#if !SILVERLIGHT
        [Obsolete]
        public ExtendedKeyUsage(
            ArrayList usages)
            : this((IEnumerable)usages)
        {
        }
#endif

        public ExtendedKeyUsage(
            IEnumerable usages)
        {
            Asn1EncodableVector v = new Asn1EncodableVector();

			foreach (Asn1Object o in usages)
            {
				v.Add(o);

				this.usageTable.Add(o, o);
            }

			this.seq = new DerSequence(v);
        }

		public bool HasKeyPurposeId(
            KeyPurposeID keyPurposeId)
        {
            return usageTable[keyPurposeId] != null;
        }

#if !SILVERLIGHT
        [Obsolete("Use 'GetAllUsages'")]
        public ArrayList GetUsages()
        {
            return new ArrayList(usageTable.Values);
        }
#endif

        /**
		 * Returns all extended key usages.
		 * The returned ArrayList contains DerObjectIdentifier instances.
		 * @return An ArrayList with all key purposes.
		 */
		public IList GetAllUsages()
		{
			return Platform.CreateArrayList(usageTable.Values);
		}

        public int Count
		{
			get { return usageTable.Count; }
		}

		public override Asn1Object ToAsn1Object()
        {
            return seq;
        }
    }
}
