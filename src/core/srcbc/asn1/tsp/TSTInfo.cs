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

using Org.BouncyCastle.Asn1.X509;

namespace Org.BouncyCastle.Asn1.Tsp
{
	public class TstInfo
		: Asn1Encodable
	{
		private readonly DerInteger				version;
		private readonly DerObjectIdentifier	tsaPolicyId;
		private readonly MessageImprint			messageImprint;
		private readonly DerInteger				serialNumber;
		private readonly DerGeneralizedTime		genTime;
		private readonly Accuracy				accuracy;
		private readonly DerBoolean				ordering;
		private readonly DerInteger				nonce;
		private readonly GeneralName			tsa;
		private readonly X509Extensions			extensions;

		public static TstInfo GetInstance(
			object o)
		{
			if (o == null || o is TstInfo)
			{
				return (TstInfo) o;
			}

			if (o is Asn1Sequence)
			{
				return new TstInfo((Asn1Sequence) o);
			}

			if (o is Asn1OctetString)
			{
				try
				{
					byte[] octets = ((Asn1OctetString)o).GetOctets();
					return GetInstance(Asn1Object.FromByteArray(octets));
				}
				catch (IOException)
				{
					throw new ArgumentException(
						"Bad object format in 'TstInfo' factory.");
				}
			}

			throw new ArgumentException(
				"Unknown object in 'TstInfo' factory: " + o.GetType().FullName);
		}

		private TstInfo(
			Asn1Sequence seq)
		{
			IEnumerator e = seq.GetEnumerator();

			// version
			e.MoveNext();
			version = DerInteger.GetInstance(e.Current);

			// tsaPolicy
			e.MoveNext();
			tsaPolicyId = DerObjectIdentifier.GetInstance(e.Current);

			// messageImprint
			e.MoveNext();
			messageImprint = MessageImprint.GetInstance(e.Current);

			// serialNumber
			e.MoveNext();
			serialNumber = DerInteger.GetInstance(e.Current);

			// genTime
			e.MoveNext();
			genTime = DerGeneralizedTime.GetInstance(e.Current);

			// default for ordering
			ordering = DerBoolean.False;

			while (e.MoveNext())
			{
				Asn1Object o = (Asn1Object) e.Current;

				if (o is Asn1TaggedObject)
				{
					DerTaggedObject tagged = (DerTaggedObject) o;

					switch (tagged.TagNo)
					{
						case 0:
							tsa = GeneralName.GetInstance(tagged, true);
							break;
						case 1:
							extensions = X509Extensions.GetInstance(tagged, false);
							break;
						default:
							throw new ArgumentException("Unknown tag value " + tagged.TagNo);
					}
				}

				if (o is DerSequence)
				{
					accuracy = Accuracy.GetInstance(o);
				}

				if (o is DerBoolean)
				{
					ordering = DerBoolean.GetInstance(o);
				}

				if (o is DerInteger)
				{
					nonce = DerInteger.GetInstance(o);
				}
			}
		}

		public TstInfo(
			DerObjectIdentifier	tsaPolicyId,
			MessageImprint		messageImprint,
			DerInteger			serialNumber,
			DerGeneralizedTime	genTime,
			Accuracy			accuracy,
			DerBoolean			ordering,
			DerInteger			nonce,
			GeneralName			tsa,
			X509Extensions		extensions)
		{
			this.version = new DerInteger(1);
			this.tsaPolicyId = tsaPolicyId;
			this.messageImprint = messageImprint;
			this.serialNumber = serialNumber;
			this.genTime = genTime;
			this.accuracy = accuracy;
			this.ordering = ordering;
			this.nonce = nonce;
			this.tsa = tsa;
			this.extensions = extensions;
		}

        public DerInteger Version
        {
            get { return version; }
        }

		public MessageImprint MessageImprint
		{
			get { return messageImprint; }
		}

		public DerObjectIdentifier Policy
		{
			get { return tsaPolicyId; }
		}

		public DerInteger SerialNumber
		{
			get { return serialNumber; }
		}

		public Accuracy Accuracy
		{
			get { return accuracy; }
		}

		public DerGeneralizedTime GenTime
		{
			get { return genTime; }
		}

		public DerBoolean Ordering
		{
			get { return ordering; }
		}

		public DerInteger Nonce
		{
			get { return nonce; }
		}

		public GeneralName Tsa
		{
			get { return tsa; }
		}

		public X509Extensions Extensions
		{
			get { return extensions; }
		}

		/**
		 * <pre>
		 *
		 *     TstInfo ::= SEQUENCE  {
		 *        version                      INTEGER  { v1(1) },
		 *        policy                       TSAPolicyId,
		 *        messageImprint               MessageImprint,
		 *          -- MUST have the same value as the similar field in
		 *          -- TimeStampReq
		 *        serialNumber                 INTEGER,
		 *         -- Time-Stamping users MUST be ready to accommodate integers
		 *         -- up to 160 bits.
		 *        genTime                      GeneralizedTime,
		 *        accuracy                     Accuracy                 OPTIONAL,
		 *        ordering                     BOOLEAN             DEFAULT FALSE,
		 *        nonce                        INTEGER                  OPTIONAL,
		 *          -- MUST be present if the similar field was present
		 *          -- in TimeStampReq.  In that case it MUST have the same value.
		 *        tsa                          [0] GeneralName          OPTIONAL,
		 *        extensions                   [1] IMPLICIT Extensions   OPTIONAL  }
		 *
		 * </pre>
		 */
		public override Asn1Object ToAsn1Object()
		{
			Asn1EncodableVector v = new Asn1EncodableVector(
				version, tsaPolicyId, messageImprint, serialNumber, genTime);

			if (accuracy != null)
			{
				v.Add(accuracy);
			}

			if (ordering != null && ordering.IsTrue)
			{
				v.Add(ordering);
			}

			if (nonce != null)
			{
				v.Add(nonce);
			}

			if (tsa != null)
			{
				v.Add(new DerTaggedObject(true, 0, tsa));
			}

			if (extensions != null)
			{
				v.Add(new DerTaggedObject(false, 1, extensions));
			}

			return new DerSequence(v);
		}
	}
}
