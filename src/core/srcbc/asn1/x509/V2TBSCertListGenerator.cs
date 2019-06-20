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
using System.IO;

using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.X509
{
    /**
     * Generator for Version 2 TbsCertList structures.
     * <pre>
     *  TbsCertList  ::=  Sequence  {
     *       version                 Version OPTIONAL,
     *                                    -- if present, shall be v2
     *       signature               AlgorithmIdentifier,
     *       issuer                  Name,
     *       thisUpdate              Time,
     *       nextUpdate              Time OPTIONAL,
     *       revokedCertificates     Sequence OF Sequence  {
     *            userCertificate         CertificateSerialNumber,
     *            revocationDate          Time,
     *            crlEntryExtensions      Extensions OPTIONAL
     *                                          -- if present, shall be v2
     *                                 }  OPTIONAL,
     *       crlExtensions           [0]  EXPLICIT Extensions OPTIONAL
     *                                          -- if present, shall be v2
     *                                 }
     * </pre>
     *
     * <b>Note: This class may be subject to change</b>
     */
    public class V2TbsCertListGenerator
    {
        private DerInteger			version = new DerInteger(1);
        private AlgorithmIdentifier	signature;
        private X509Name			issuer;
        private Time				thisUpdate, nextUpdate;
        private X509Extensions		extensions;
        private IList			    crlEntries;

		public V2TbsCertListGenerator()
        {
        }

		public void SetSignature(
            AlgorithmIdentifier signature)
        {
            this.signature = signature;
        }

		public void SetIssuer(
            X509Name issuer)
        {
            this.issuer = issuer;
        }

		public void SetThisUpdate(
            DerUtcTime thisUpdate)
        {
            this.thisUpdate = new Time(thisUpdate);
        }

		public void SetNextUpdate(
            DerUtcTime nextUpdate)
        {
            this.nextUpdate = (nextUpdate != null)
				?	new Time(nextUpdate)
				:	null;
        }

		public void SetThisUpdate(
            Time thisUpdate)
        {
            this.thisUpdate = thisUpdate;
        }

		public void SetNextUpdate(
            Time nextUpdate)
        {
            this.nextUpdate = nextUpdate;
        }

		public void AddCrlEntry(
			Asn1Sequence crlEntry)
		{
			if (crlEntries == null)
			{
				crlEntries = Platform.CreateArrayList();
			}

			crlEntries.Add(crlEntry);
		}

		public void AddCrlEntry(DerInteger userCertificate, DerUtcTime revocationDate, int reason)
		{
			AddCrlEntry(userCertificate, new Time(revocationDate), reason);
		}

		public void AddCrlEntry(DerInteger userCertificate, Time revocationDate, int reason)
		{
			AddCrlEntry(userCertificate, revocationDate, reason, null);
		}

		public void AddCrlEntry(DerInteger userCertificate, Time revocationDate, int reason,
			DerGeneralizedTime invalidityDate)
		{
            IList extOids = Platform.CreateArrayList();
            IList extValues = Platform.CreateArrayList();

			if (reason != 0)
			{
				CrlReason crlReason = new CrlReason(reason);

				try
				{
					extOids.Add(X509Extensions.ReasonCode);
					extValues.Add(new X509Extension(false, new DerOctetString(crlReason.GetEncoded())));
				}
				catch (IOException e)
				{
					throw new ArgumentException("error encoding reason: " + e);
				}
			}

			if (invalidityDate != null)
			{
				try
				{
					extOids.Add(X509Extensions.InvalidityDate);
					extValues.Add(new X509Extension(false, new DerOctetString(invalidityDate.GetEncoded())));
				}
				catch (IOException e)
				{
					throw new ArgumentException("error encoding invalidityDate: " + e);
				}
			}

			if (extOids.Count != 0)
			{
				AddCrlEntry(userCertificate, revocationDate, new X509Extensions(extOids, extValues));
			}
			else
			{
				AddCrlEntry(userCertificate, revocationDate, null);
			}
		}

		public void AddCrlEntry(DerInteger userCertificate, Time revocationDate, X509Extensions extensions)
		{
			Asn1EncodableVector v = new Asn1EncodableVector(
				userCertificate, revocationDate);

			if (extensions != null)
			{
				v.Add(extensions);
			}

			AddCrlEntry(new DerSequence(v));
		}

		public void SetExtensions(
            X509Extensions extensions)
        {
            this.extensions = extensions;
        }

		public TbsCertificateList GenerateTbsCertList()
        {
            if ((signature == null) || (issuer == null) || (thisUpdate == null))
            {
                throw new InvalidOperationException("Not all mandatory fields set in V2 TbsCertList generator.");
            }

			Asn1EncodableVector v = new Asn1EncodableVector(
				version, signature, issuer, thisUpdate);

			if (nextUpdate != null)
            {
                v.Add(nextUpdate);
            }

			// Add CRLEntries if they exist
            if (crlEntries != null)
            {
                Asn1Sequence[] certs = new Asn1Sequence[crlEntries.Count];
                for (int i = 0; i < crlEntries.Count; ++i)
                {
                    certs[i] = (Asn1Sequence)crlEntries[i];
                }
				v.Add(new DerSequence(certs));
            }

			if (extensions != null)
            {
                v.Add(new DerTaggedObject(0, extensions));
            }

			return new TbsCertificateList(new DerSequence(v));
        }
    }
}
