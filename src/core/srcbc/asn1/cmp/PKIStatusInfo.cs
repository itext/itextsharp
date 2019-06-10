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

using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Asn1.Cmp
{
	public class PkiStatusInfo
		: Asn1Encodable
	{
		DerInteger      status;
		PkiFreeText     statusString;
		DerBitString    failInfo;

		public static PkiStatusInfo GetInstance(
			Asn1TaggedObject obj,
			bool isExplicit)
		{
			return GetInstance(Asn1Sequence.GetInstance(obj, isExplicit));
		}

		public static PkiStatusInfo GetInstance(
			object obj)
		{
			if (obj is PkiStatusInfo)
			{
				return (PkiStatusInfo)obj;
			}
			else if (obj is Asn1Sequence)
			{
				return new PkiStatusInfo((Asn1Sequence)obj);
			}

			throw new ArgumentException("Unknown object in factory: " + obj.GetType().Name, "obj");
		}

		public PkiStatusInfo(
			Asn1Sequence seq)
		{
			this.status = DerInteger.GetInstance(seq[0]);

			this.statusString = null;
			this.failInfo = null;

			if (seq.Count > 2)
			{
				this.statusString = PkiFreeText.GetInstance(seq[1]);
				this.failInfo = DerBitString.GetInstance(seq[2]);
			}
			else if (seq.Count > 1)
			{
				object obj = seq[1];
				if (obj is DerBitString)
				{
					this.failInfo = DerBitString.GetInstance(obj);
				}
				else
				{
					this.statusString = PkiFreeText.GetInstance(obj);
				}
			}
		}

		/**
		 * @param status
		 */
		public PkiStatusInfo(int status)
		{
			this.status = new DerInteger(status);
		}

		/**
		 * @param status
		 * @param statusString
		 */
		public PkiStatusInfo(
			int			status,
			PkiFreeText	statusString)
		{
			this.status = new DerInteger(status);
			this.statusString = statusString;
		}

		public PkiStatusInfo(
			int				status,
			PkiFreeText		statusString,
			PkiFailureInfo	failInfo)
		{
			this.status = new DerInteger(status);
			this.statusString = statusString;
			this.failInfo = failInfo;
		}

		public BigInteger Status
		{
			get
			{
				return status.Value;
			}
		}

		public PkiFreeText StatusString
		{
			get
			{
				return statusString;
			}
		}

		public DerBitString FailInfo
		{
			get
			{
				return failInfo;
			}
		}

		/**
		 * <pre>
		 * PkiStatusInfo ::= SEQUENCE {
		 *     status        PKIStatus,                (INTEGER)
		 *     statusString  PkiFreeText     OPTIONAL,
		 *     failInfo      PkiFailureInfo  OPTIONAL  (BIT STRING)
		 * }
		 *
		 * PKIStatus:
		 *   granted                (0), -- you got exactly what you asked for
		 *   grantedWithMods        (1), -- you got something like what you asked for
		 *   rejection              (2), -- you don't get it, more information elsewhere in the message
		 *   waiting                (3), -- the request body part has not yet been processed, expect to hear more later
		 *   revocationWarning      (4), -- this message contains a warning that a revocation is imminent
		 *   revocationNotification (5), -- notification that a revocation has occurred
		 *   keyUpdateWarning       (6)  -- update already done for the oldCertId specified in CertReqMsg
		 *
		 * PkiFailureInfo:
		 *   badAlg           (0), -- unrecognized or unsupported Algorithm Identifier
		 *   badMessageCheck  (1), -- integrity check failed (e.g., signature did not verify)
		 *   badRequest       (2), -- transaction not permitted or supported
		 *   badTime          (3), -- messageTime was not sufficiently close to the system time, as defined by local policy
		 *   badCertId        (4), -- no certificate could be found matching the provided criteria
		 *   badDataFormat    (5), -- the data submitted has the wrong format
		 *   wrongAuthority   (6), -- the authority indicated in the request is different from the one creating the response token
		 *   incorrectData    (7), -- the requester's data is incorrect (for notary services)
		 *   missingTimeStamp (8), -- when the timestamp is missing but should be there (by policy)
		 *   badPOP           (9)  -- the proof-of-possession failed
		 *
		 * </pre>
		 */
		public override Asn1Object ToAsn1Object()
		{
			Asn1EncodableVector v = new Asn1EncodableVector(status);

			if (statusString != null)
			{
				v.Add(statusString);
			}

			if (failInfo!= null)
			{
				v.Add(failInfo);
			}

			return new DerSequence(v);
		}
	}
}
