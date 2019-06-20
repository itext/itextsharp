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

using Org.BouncyCastle.Asn1;

namespace Org.BouncyCastle.Asn1.Icao
{
    /**
    * The DataGroupHash object.
    * <pre>
    * DataGroupHash  ::=  SEQUENCE {
    *      dataGroupNumber         DataGroupNumber,
    *      dataGroupHashValue     OCTET STRING }
    *
    * DataGroupNumber ::= INTEGER {
    *         dataGroup1    (1),
    *         dataGroup1    (2),
    *         dataGroup1    (3),
    *         dataGroup1    (4),
    *         dataGroup1    (5),
    *         dataGroup1    (6),
    *         dataGroup1    (7),
    *         dataGroup1    (8),
    *         dataGroup1    (9),
    *         dataGroup1    (10),
    *         dataGroup1    (11),
    *         dataGroup1    (12),
    *         dataGroup1    (13),
    *         dataGroup1    (14),
    *         dataGroup1    (15),
    *         dataGroup1    (16) }
    *
    * </pre>
    */
    public class DataGroupHash
        : Asn1Encodable
    {
        private readonly DerInteger dataGroupNumber;
        private readonly Asn1OctetString dataGroupHashValue;

		public static DataGroupHash GetInstance(
            object obj)
        {
            if (obj is DataGroupHash)
                return (DataGroupHash)obj;

			if (obj != null)
				return new DataGroupHash(Asn1Sequence.GetInstance(obj));

			return null;
		}

		private DataGroupHash(
			Asn1Sequence seq)
        {
			if (seq.Count != 2)
				throw new ArgumentException("Wrong number of elements in sequence", "seq");

			this.dataGroupNumber = DerInteger.GetInstance(seq[0]);
            this.dataGroupHashValue = Asn1OctetString.GetInstance(seq[1]);
        }

		public DataGroupHash(
            int				dataGroupNumber,
            Asn1OctetString	dataGroupHashValue)
        {
            this.dataGroupNumber = new DerInteger(dataGroupNumber);
            this.dataGroupHashValue = dataGroupHashValue;
        }

		public int DataGroupNumber
		{
			get { return dataGroupNumber.Value.IntValue; }
		}

		public Asn1OctetString DataGroupHashValue
		{
			get { return dataGroupHashValue; }
		}

		public override Asn1Object ToAsn1Object()
        {
			return new DerSequence(dataGroupNumber, dataGroupHashValue);
        }
    }
}
