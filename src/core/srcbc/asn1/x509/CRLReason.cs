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
namespace Org.BouncyCastle.Asn1.X509
{
    /**
     * The CRLReason enumeration.
     * <pre>
     * CRLReason ::= Enumerated {
     *  unspecified             (0),
     *  keyCompromise           (1),
     *  cACompromise            (2),
     *  affiliationChanged      (3),
     *  superseded              (4),
     *  cessationOfOperation    (5),
     *  certificateHold         (6),
     *  removeFromCRL           (8),
     *  privilegeWithdrawn      (9),
     *  aACompromise           (10)
     * }
     * </pre>
     */
    public class CrlReason
        : DerEnumerated
    {
        public const int Unspecified = 0;
        public const int KeyCompromise = 1;
        public const int CACompromise = 2;
        public const int AffiliationChanged = 3;
        public const int Superseded = 4;
        public const int CessationOfOperation  = 5;
        public const int CertificateHold = 6;
		// 7 -> Unknown
        public const int RemoveFromCrl = 8;
        public const int PrivilegeWithdrawn = 9;
        public const int AACompromise = 10;

		private static readonly string[] ReasonString = new string[]
		{
			"Unspecified", "KeyCompromise", "CACompromise", "AffiliationChanged",
			"Superseded", "CessationOfOperation", "CertificateHold", "Unknown",
			"RemoveFromCrl", "PrivilegeWithdrawn", "AACompromise"
		};

		public CrlReason(
			int reason)
			: base(reason)
        {
        }

		public CrlReason(
			DerEnumerated reason)
			: base(reason.Value.IntValue)
        {
        }

		public override string ToString()
		{
			int reason = Value.IntValue;
			string str = (reason < 0 || reason > 10) ? "Invalid" : ReasonString[reason];
			return "CrlReason: " + str;
		}    
	}
}
