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

using Org.BouncyCastle.Asn1.X509;

namespace Org.BouncyCastle.Asn1.Icao
{
	/**
	 * The CscaMasterList object. This object can be wrapped in a
	 * CMSSignedData to be published in LDAP.
	 *
	 * <pre>
	 * CscaMasterList ::= SEQUENCE {
	 *   version                CscaMasterListVersion,
	 *   certList               SET OF Certificate }
	 *   
	 * CscaMasterListVersion :: INTEGER {v0(0)}
	 * </pre>
	 */
	public class CscaMasterList 
		: Asn1Encodable 
	{
		private DerInteger version = new DerInteger(0);
		private X509CertificateStructure[] certList;

		public static CscaMasterList GetInstance(
			object obj)
		{
			if (obj is CscaMasterList)
				return (CscaMasterList)obj;

			if (obj != null)
				return new CscaMasterList(Asn1Sequence.GetInstance(obj));            

			return null;
		}

		private CscaMasterList(
			Asn1Sequence seq)
		{
			if (seq == null || seq.Count == 0)
				throw new ArgumentException("null or empty sequence passed.");

			if (seq.Count != 2)
				throw new ArgumentException("Incorrect sequence size: " + seq.Count);

			this.version = DerInteger.GetInstance(seq[0]);

			Asn1Set certSet = Asn1Set.GetInstance(seq[1]);

			this.certList = new X509CertificateStructure[certSet.Count];
			for (int i = 0; i < certList.Length; i++)
			{
				certList[i] = X509CertificateStructure.GetInstance(certSet[i]);
			}
		}

		public CscaMasterList(
			X509CertificateStructure[] certStructs)
		{
			certList = CopyCertList(certStructs);
		}

		public virtual int Version
		{
			get { return version.Value.IntValue; }
		}

		public X509CertificateStructure[] GetCertStructs()
		{
			return CopyCertList(certList);
		}

		private static X509CertificateStructure[] CopyCertList(X509CertificateStructure[] orig)
		{
			return (X509CertificateStructure[])orig.Clone();
		}

		public override Asn1Object ToAsn1Object() 
		{
			return new DerSequence(version, new DerSet(certList));
		}
	}
}
