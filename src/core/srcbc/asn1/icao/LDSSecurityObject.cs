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
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Asn1.Icao
{
	/**
	 * The LDSSecurityObject object (V1.8).
	 * <pre>
	 * LDSSecurityObject ::= SEQUENCE {
	 *   version                LDSSecurityObjectVersion,
	 *   hashAlgorithm          DigestAlgorithmIdentifier,
	 *   dataGroupHashValues    SEQUENCE SIZE (2..ub-DataGroups) OF DataHashGroup,
	 *   ldsVersionInfo         LDSVersionInfo OPTIONAL
	 *     -- if present, version MUST be v1 }
	 *
	 * DigestAlgorithmIdentifier ::= AlgorithmIdentifier,
	 *
	 * LDSSecurityObjectVersion :: INTEGER {V0(0)}
	 * </pre>
	 */
	public class LdsSecurityObject
		: Asn1Encodable
	{
		public const int UBDataGroups = 16;

		private DerInteger version = new DerInteger(0);
		private AlgorithmIdentifier digestAlgorithmIdentifier;
		private DataGroupHash[] datagroupHash;
		private LdsVersionInfo versionInfo;

		public static LdsSecurityObject GetInstance(
			object obj)
		{
			if (obj is LdsSecurityObject)
				return (LdsSecurityObject)obj;

			if (obj != null)
				return new LdsSecurityObject(Asn1Sequence.GetInstance(obj));

			return null;
		}

		private LdsSecurityObject(
			Asn1Sequence seq)
		{
			if (seq == null || seq.Count == 0)
				throw new ArgumentException("null or empty sequence passed.");

			IEnumerator e = seq.GetEnumerator();

			// version
			e.MoveNext();
			version = DerInteger.GetInstance(e.Current);
			// digestAlgorithmIdentifier
			e.MoveNext();
			digestAlgorithmIdentifier = AlgorithmIdentifier.GetInstance(e.Current);

			e.MoveNext();
			Asn1Sequence datagroupHashSeq = Asn1Sequence.GetInstance(e.Current);

			if (version.Value.Equals(BigInteger.One))
			{
				e.MoveNext();
				versionInfo = LdsVersionInfo.GetInstance(e.Current);
			}

			CheckDatagroupHashSeqSize(datagroupHashSeq.Count);

			datagroupHash = new DataGroupHash[datagroupHashSeq.Count];
			for (int i= 0; i< datagroupHashSeq.Count; i++)
			{
				datagroupHash[i] = DataGroupHash.GetInstance(datagroupHashSeq[i]);
			}
		}

		public LdsSecurityObject(
			AlgorithmIdentifier	digestAlgorithmIdentifier,
			DataGroupHash[]		datagroupHash)
		{
			this.version = new DerInteger(0);
			this.digestAlgorithmIdentifier = digestAlgorithmIdentifier;
			this.datagroupHash = datagroupHash;

			CheckDatagroupHashSeqSize(datagroupHash.Length);
		}


		public LdsSecurityObject(
			AlgorithmIdentifier	digestAlgorithmIdentifier,
			DataGroupHash[]		datagroupHash,
			LdsVersionInfo		versionInfo)
		{
			this.version = new DerInteger(1);
			this.digestAlgorithmIdentifier = digestAlgorithmIdentifier;
			this.datagroupHash = datagroupHash;
			this.versionInfo = versionInfo;

			CheckDatagroupHashSeqSize(datagroupHash.Length);
		}

		private void CheckDatagroupHashSeqSize(int size)
		{
			if (size < 2 || size > UBDataGroups)
				throw new ArgumentException("wrong size in DataGroupHashValues : not in (2.."+ UBDataGroups +")");
		}

		public BigInteger Version
		{
			get { return version.Value; }
		}

		public AlgorithmIdentifier DigestAlgorithmIdentifier
		{
			get { return digestAlgorithmIdentifier; }
		}

		public DataGroupHash[] GetDatagroupHash()
		{
			return datagroupHash;
		}

		public LdsVersionInfo VersionInfo
		{
			get { return versionInfo; }
		}

		public override Asn1Object ToAsn1Object()
		{
			DerSequence hashSeq = new DerSequence(datagroupHash);

			Asn1EncodableVector v = new Asn1EncodableVector(version, digestAlgorithmIdentifier, hashSeq);

			if (versionInfo != null)
			{
				v.Add(versionInfo);
			}

			return new DerSequence(v);
		}
	}
}
