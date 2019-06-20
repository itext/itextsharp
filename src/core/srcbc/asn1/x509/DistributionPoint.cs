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
     * The DistributionPoint object.
     * <pre>
     * DistributionPoint ::= Sequence {
     *      distributionPoint [0] DistributionPointName OPTIONAL,
     *      reasons           [1] ReasonFlags OPTIONAL,
     *      cRLIssuer         [2] GeneralNames OPTIONAL
     * }
     * </pre>
     */
    public class DistributionPoint
        : Asn1Encodable
    {
        internal readonly DistributionPointName	distributionPoint;
        internal readonly ReasonFlags			reasons;
        internal readonly GeneralNames			cRLIssuer;

		public static DistributionPoint GetInstance(
            Asn1TaggedObject	obj,
            bool				explicitly)
        {
            return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
        }

		public static DistributionPoint GetInstance(
            object obj)
        {
            if(obj == null || obj is DistributionPoint)
            {
                return (DistributionPoint) obj;
            }

			if(obj is Asn1Sequence)
            {
                return new DistributionPoint((Asn1Sequence) obj);
            }

			throw new ArgumentException("Invalid DistributionPoint: " + obj.GetType().Name);
        }

		private DistributionPoint(
            Asn1Sequence seq)
        {
            for (int i = 0; i != seq.Count; i++)
            {
				Asn1TaggedObject t = Asn1TaggedObject.GetInstance(seq[i]);

				switch (t.TagNo)
                {
                case 0:
                    distributionPoint = DistributionPointName.GetInstance(t, true);
                    break;
                case 1:
                    reasons = new ReasonFlags(DerBitString.GetInstance(t, false));
                    break;
                case 2:
                    cRLIssuer = GeneralNames.GetInstance(t, false);
                    break;
                }
            }
        }

		public DistributionPoint(
            DistributionPointName	distributionPointName,
            ReasonFlags				reasons,
            GeneralNames			crlIssuer)
        {
            this.distributionPoint = distributionPointName;
            this.reasons = reasons;
            this.cRLIssuer = crlIssuer;
        }

		public DistributionPointName DistributionPointName
        {
			get { return distributionPoint; }
        }

		public ReasonFlags Reasons
        {
			get { return reasons; }
        }

		public GeneralNames CrlIssuer
        {
			get { return cRLIssuer; }
        }

		public override Asn1Object ToAsn1Object()
        {
            Asn1EncodableVector v = new Asn1EncodableVector();

			if (distributionPoint != null)
            {
                //
                // as this is a CHOICE it must be explicitly tagged
                //
                v.Add(new DerTaggedObject(0, distributionPoint));
            }

			if (reasons != null)
            {
                v.Add(new DerTaggedObject(false, 1, reasons));
            }

			if (cRLIssuer != null)
            {
                v.Add(new DerTaggedObject(false, 2, cRLIssuer));
            }

			return new DerSequence(v);
        }

		public override string ToString()
		{
			string sep = Platform.NewLine;
			StringBuilder buf = new StringBuilder();
			buf.Append("DistributionPoint: [");
			buf.Append(sep);
			if (distributionPoint != null)
			{
				appendObject(buf, sep, "distributionPoint", distributionPoint.ToString());
			}
			if (reasons != null)
			{
				appendObject(buf, sep, "reasons", reasons.ToString());
			}
			if (cRLIssuer != null)
			{
				appendObject(buf, sep, "cRLIssuer", cRLIssuer.ToString());
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
