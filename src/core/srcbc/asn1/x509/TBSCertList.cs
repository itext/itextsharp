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

using Org.BouncyCastle.Utilities.Collections;

namespace Org.BouncyCastle.Asn1.X509
{
	public class CrlEntry
		: Asn1Encodable
	{
		internal Asn1Sequence	seq;
		internal DerInteger		userCertificate;
		internal Time			revocationDate;
		internal X509Extensions	crlEntryExtensions;

		public CrlEntry(
			Asn1Sequence seq)
		{
			if (seq.Count < 2 || seq.Count > 3)
			{
				throw new ArgumentException("Bad sequence size: " + seq.Count);
			}

			this.seq = seq;

			userCertificate = DerInteger.GetInstance(seq[0]);
			revocationDate = Time.GetInstance(seq[1]);
		}

		public DerInteger UserCertificate
		{
			get { return userCertificate; }
		}

		public Time RevocationDate
		{
			get { return revocationDate; }
		}

		public X509Extensions Extensions
		{
			get
			{
				if (crlEntryExtensions == null && seq.Count == 3)
				{
					crlEntryExtensions = X509Extensions.GetInstance(seq[2]);
				}

				return crlEntryExtensions;
			}
		}

		public override Asn1Object ToAsn1Object()
		{
			return seq;
		}
	}

	/**
     * PKIX RFC-2459 - TbsCertList object.
     * <pre>
     * TbsCertList  ::=  Sequence  {
     *      version                 Version OPTIONAL,
     *                                   -- if present, shall be v2
     *      signature               AlgorithmIdentifier,
     *      issuer                  Name,
     *      thisUpdate              Time,
     *      nextUpdate              Time OPTIONAL,
     *      revokedCertificates     Sequence OF Sequence  {
     *           userCertificate         CertificateSerialNumber,
     *           revocationDate          Time,
     *           crlEntryExtensions      Extensions OPTIONAL
     *                                         -- if present, shall be v2
     *                                }  OPTIONAL,
     *      crlExtensions           [0]  EXPLICIT Extensions OPTIONAL
     *                                         -- if present, shall be v2
     *                                }
     * </pre>
     */
    public class TbsCertificateList
        : Asn1Encodable
    {
		private class RevokedCertificatesEnumeration
			: IEnumerable
		{
			private readonly IEnumerable en;

			internal RevokedCertificatesEnumeration(
				IEnumerable en)
			{
				this.en = en;
			}

			public IEnumerator GetEnumerator()
			{
				return new RevokedCertificatesEnumerator(en.GetEnumerator());
			}

			private class RevokedCertificatesEnumerator
				: IEnumerator
			{
				private readonly IEnumerator e;

				internal RevokedCertificatesEnumerator(
					IEnumerator e)
				{
					this.e = e;
				}

				public bool MoveNext()
				{
					return e.MoveNext();
				}

				public void Reset()
				{
					e.Reset();
				}

				public object Current
				{
					get { return new CrlEntry(Asn1Sequence.GetInstance(e.Current)); }
				}
			}
		}

		internal Asn1Sequence			seq;
		internal DerInteger				version;
        internal AlgorithmIdentifier	signature;
        internal X509Name				issuer;
        internal Time					thisUpdate;
        internal Time					nextUpdate;
		internal Asn1Sequence			revokedCertificates;
		internal X509Extensions			crlExtensions;

		public static TbsCertificateList GetInstance(
            Asn1TaggedObject	obj,
            bool				explicitly)
        {
            return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
        }

		public static TbsCertificateList GetInstance(
            object obj)
        {
            TbsCertificateList list = obj as TbsCertificateList;

			if (obj == null || list != null)
            {
                return list;
            }

			if (obj is Asn1Sequence)
            {
                return new TbsCertificateList((Asn1Sequence) obj);
            }

			throw new ArgumentException("unknown object in factory: " + obj.GetType().Name, "obj");
        }

		internal TbsCertificateList(
            Asn1Sequence seq)
        {
			if (seq.Count < 3 || seq.Count > 7)
			{
				throw new ArgumentException("Bad sequence size: " + seq.Count);
			}

			int seqPos = 0;

			this.seq = seq;

			if (seq[seqPos] is DerInteger)
            {
				version = DerInteger.GetInstance(seq[seqPos++]);
			}
            else
            {
                version = new DerInteger(0);
            }

			signature = AlgorithmIdentifier.GetInstance(seq[seqPos++]);
            issuer = X509Name.GetInstance(seq[seqPos++]);
            thisUpdate = Time.GetInstance(seq[seqPos++]);

			if (seqPos < seq.Count
                && (seq[seqPos] is DerUtcTime
                   || seq[seqPos] is DerGeneralizedTime
                   || seq[seqPos] is Time))
            {
                nextUpdate = Time.GetInstance(seq[seqPos++]);
            }

			if (seqPos < seq.Count
                && !(seq[seqPos] is DerTaggedObject))
            {
				revokedCertificates = Asn1Sequence.GetInstance(seq[seqPos++]);
			}

			if (seqPos < seq.Count
                && seq[seqPos] is DerTaggedObject)
            {
				crlExtensions = X509Extensions.GetInstance(seq[seqPos]);
			}
        }

		public int Version
        {
            get { return version.Value.IntValue + 1; }
        }

		public DerInteger VersionNumber
        {
            get { return version; }
        }

		public AlgorithmIdentifier Signature
        {
            get { return signature; }
        }

		public X509Name Issuer
        {
            get { return issuer; }
        }

		public Time ThisUpdate
        {
            get { return thisUpdate; }
        }

		public Time NextUpdate
        {
            get { return nextUpdate; }
        }

		public CrlEntry[] GetRevokedCertificates()
        {
			if (revokedCertificates == null)
			{
				return new CrlEntry[0];
			}

			CrlEntry[] entries = new CrlEntry[revokedCertificates.Count];

			for (int i = 0; i < entries.Length; i++)
			{
				entries[i] = new CrlEntry(Asn1Sequence.GetInstance(revokedCertificates[i]));
			}

			return entries;
		}

		public IEnumerable GetRevokedCertificateEnumeration()
		{
			if (revokedCertificates == null)
			{
				return EmptyEnumerable.Instance;
			}

			return new RevokedCertificatesEnumeration(revokedCertificates);
		}

		public X509Extensions Extensions
        {
            get { return crlExtensions; }
        }

		public override Asn1Object ToAsn1Object()
        {
            return seq;
        }
    }
}
