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

namespace Org.BouncyCastle.Asn1.Tsp
{
	public class Accuracy
		: Asn1Encodable
	{
		private readonly DerInteger seconds;
		private readonly DerInteger millis;
		private readonly DerInteger micros;

		// constants
		protected const int MinMillis = 1;
		protected const int MaxMillis = 999;
		protected const int MinMicros = 1;
		protected const int MaxMicros = 999;

		public Accuracy(
			DerInteger seconds,
			DerInteger millis,
			DerInteger micros)
		{
			//Verifications
			if (millis != null
				&& (millis.Value.IntValue < MinMillis
					|| millis.Value.IntValue > MaxMillis))
			{
				throw new ArgumentException(
					"Invalid millis field : not in (1..999)");
			}

			if (micros != null
				&& (micros.Value.IntValue < MinMicros
					|| micros.Value.IntValue > MaxMicros))
			{
				throw new ArgumentException(
					"Invalid micros field : not in (1..999)");
			}

			this.seconds = seconds;
			this.millis = millis;
			this.micros = micros;
		}

		private Accuracy(
			Asn1Sequence seq)
		{
			for (int i = 0; i < seq.Count; ++i)
			{
				// seconds
				if (seq[i] is DerInteger)
				{
					seconds = (DerInteger) seq[i];
				}
				else if (seq[i] is DerTaggedObject)
				{
					DerTaggedObject extra = (DerTaggedObject) seq[i];

					switch (extra.TagNo)
					{
						case 0:
							millis = DerInteger.GetInstance(extra, false);
							if (millis.Value.IntValue < MinMillis
								|| millis.Value.IntValue > MaxMillis)
							{
								throw new ArgumentException(
									"Invalid millis field : not in (1..999).");
							}
							break;
						case 1:
							micros = DerInteger.GetInstance(extra, false);
							if (micros.Value.IntValue < MinMicros
								|| micros.Value.IntValue > MaxMicros)
							{
								throw new ArgumentException(
									"Invalid micros field : not in (1..999).");
							}
							break;
						default:
							throw new ArgumentException("Invalig tag number");
					}
				}
			}
		}

		public static Accuracy GetInstance(
			object o)
		{
			if (o == null || o is Accuracy)
			{
				return (Accuracy) o;
			}

			if (o is Asn1Sequence)
			{
				return new Accuracy((Asn1Sequence) o);
			}

			throw new ArgumentException(
				"Unknown object in 'Accuracy' factory: " + o.GetType().FullName);
		}

		public DerInteger Seconds
		{
			get { return seconds; }
		}

		public DerInteger Millis
		{
			get { return millis; }
		}

		public DerInteger Micros
		{
			get { return micros; }
		}

		/**
		 * <pre>
		 * Accuracy ::= SEQUENCE {
		 *             seconds        INTEGER              OPTIONAL,
		 *             millis     [0] INTEGER  (1..999)    OPTIONAL,
		 *             micros     [1] INTEGER  (1..999)    OPTIONAL
		 *             }
		 * </pre>
		 */
		public override Asn1Object ToAsn1Object()
		{
			Asn1EncodableVector v = new Asn1EncodableVector();

			if (seconds != null)
			{
				v.Add(seconds);
			}

			if (millis != null)
			{
				v.Add(new DerTaggedObject(false, 0, millis));
			}

			if (micros != null)
			{
				v.Add(new DerTaggedObject(false, 1, micros));
			}

			return new DerSequence(v);
		}
	}
}
