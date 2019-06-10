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
using System.Text;

using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.X509
{
    /**
     * The DistributionPointName object.
     * <pre>
     * DistributionPointName ::= CHOICE {
     *     fullName                 [0] GeneralNames,
     *     nameRelativeToCRLIssuer  [1] RDN
     * }
     * </pre>
     */
    public class DistributionPointName
        : Asn1Encodable, IAsn1Choice
    {
        internal readonly Asn1Encodable	name;
        internal readonly int			type;

		public const int FullName					= 0;
        public const int NameRelativeToCrlIssuer	= 1;

		public static DistributionPointName GetInstance(
            Asn1TaggedObject	obj,
            bool				explicitly)
        {
            return GetInstance(Asn1TaggedObject.GetInstance(obj, true));
        }

		public static DistributionPointName GetInstance(
            object obj)
        {
            if (obj == null || obj is DistributionPointName)
            {
                return (DistributionPointName) obj;
            }

			if (obj is Asn1TaggedObject)
            {
                return new DistributionPointName((Asn1TaggedObject) obj);
            }

			throw new ArgumentException("unknown object in factory: " + obj.GetType().Name, "obj");
		}

        public DistributionPointName(
            int				type,
            Asn1Encodable	name)
        {
            this.type = type;
            this.name = name;
        }

		public DistributionPointName(
			GeneralNames name)
			:	this(FullName, name)
		{
		}

		public int PointType
        {
			get { return type; }
        }

		public Asn1Encodable Name
        {
			get { return name; }
        }

		public DistributionPointName(
            Asn1TaggedObject obj)
        {
            this.type = obj.TagNo;

			if (type == FullName)
            {
                this.name = GeneralNames.GetInstance(obj, false);
            }
            else
            {
                this.name = Asn1Set.GetInstance(obj, false);
            }
        }

		public override Asn1Object ToAsn1Object()
        {
            return new DerTaggedObject(false, type, name);
        }

		public override string ToString()
		{
			string sep = Platform.NewLine;
			StringBuilder buf = new StringBuilder();
			buf.Append("DistributionPointName: [");
			buf.Append(sep);
			if (type == FullName)
			{
				appendObject(buf, sep, "fullName", name.ToString());
			}
			else
			{
				appendObject(buf, sep, "nameRelativeToCRLIssuer", name.ToString());
			}
			buf.Append("]");
			buf.Append(sep);
			return buf.ToString();
		}

		private void appendObject(
			StringBuilder	buf,
			string			sep,
			string			name,
			string			val)
		{
			string indent = "    ";

			buf.Append(indent);
			buf.Append(name);
			buf.Append(":");
			buf.Append(sep);
			buf.Append(indent);
			buf.Append(indent);
			buf.Append(val);
			buf.Append(sep);
		}
	}
}
