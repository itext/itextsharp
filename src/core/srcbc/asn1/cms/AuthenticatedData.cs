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

namespace Org.BouncyCastle.Asn1.Cms
{
	public class AuthenticatedData
		: Asn1Encodable
	{
		private DerInteger version;
		private OriginatorInfo originatorInfo;
		private Asn1Set recipientInfos;
		private AlgorithmIdentifier macAlgorithm;
		private AlgorithmIdentifier digestAlgorithm;
		private ContentInfo encapsulatedContentInfo;
		private Asn1Set authAttrs;
		private Asn1OctetString mac;
		private Asn1Set unauthAttrs;

		public AuthenticatedData(
			OriginatorInfo		originatorInfo,
			Asn1Set				recipientInfos,
			AlgorithmIdentifier	macAlgorithm,
			AlgorithmIdentifier	digestAlgorithm,
			ContentInfo			encapsulatedContent,
			Asn1Set				authAttrs,
			Asn1OctetString		mac,
			Asn1Set				unauthAttrs)
		{
			if (digestAlgorithm != null || authAttrs != null)
			{
				if (digestAlgorithm == null || authAttrs == null)
				{
					throw new ArgumentException("digestAlgorithm and authAttrs must be set together");
				}
			}

			version = new DerInteger(CalculateVersion(originatorInfo));

			this.originatorInfo = originatorInfo;
			this.macAlgorithm = macAlgorithm;
			this.digestAlgorithm = digestAlgorithm;
			this.recipientInfos = recipientInfos;
			this.encapsulatedContentInfo = encapsulatedContent;
			this.authAttrs = authAttrs;
			this.mac = mac;
			this.unauthAttrs = unauthAttrs;
		}

		private AuthenticatedData(
			Asn1Sequence	seq)
		{
			int index = 0;

			version = (DerInteger)seq[index++];

			Asn1Encodable tmp = seq[index++];
			if (tmp is Asn1TaggedObject)
			{
				originatorInfo = OriginatorInfo.GetInstance((Asn1TaggedObject)tmp, false);
				tmp = seq[index++];
			}

			recipientInfos = Asn1Set.GetInstance(tmp);
			macAlgorithm = AlgorithmIdentifier.GetInstance(seq[index++]);

			tmp = seq[index++];
			if (tmp is Asn1TaggedObject)
			{
				digestAlgorithm = AlgorithmIdentifier.GetInstance((Asn1TaggedObject)tmp, false);
				tmp = seq[index++];
			}

			encapsulatedContentInfo = ContentInfo.GetInstance(tmp);

			tmp = seq[index++];
			if (tmp is Asn1TaggedObject)
			{
				authAttrs = Asn1Set.GetInstance((Asn1TaggedObject)tmp, false);
				tmp = seq[index++];
			}

			mac = Asn1OctetString.GetInstance(tmp);

			if (seq.Count > index)
			{
				unauthAttrs = Asn1Set.GetInstance((Asn1TaggedObject)seq[index], false);
			}
		}

		/**
		 * return an AuthenticatedData object from a tagged object.
		 *
		 * @param obj      the tagged object holding the object we want.
		 * @param isExplicit true if the object is meant to be explicitly
		 *                 tagged false otherwise.
		 * @throws ArgumentException if the object held by the
		 *                                  tagged object cannot be converted.
		 */
		public static AuthenticatedData GetInstance(
			Asn1TaggedObject	obj,
			bool				isExplicit)
		{
			return GetInstance(Asn1Sequence.GetInstance(obj, isExplicit));
		}

		/**
		 * return an AuthenticatedData object from the given object.
		 *
		 * @param obj the object we want converted.
		 * @throws ArgumentException if the object cannot be converted.
		 */
		public static AuthenticatedData GetInstance(
			object	obj)
		{
			if (obj == null || obj is AuthenticatedData)
			{
				return (AuthenticatedData)obj;
			}

			if (obj is Asn1Sequence)
			{
				return new AuthenticatedData((Asn1Sequence)obj);
			}

			throw new ArgumentException("Invalid AuthenticatedData: " + obj.GetType().Name);
		}

		public DerInteger Version
		{
			get { return version; }
		}

		public OriginatorInfo OriginatorInfo
		{
			get { return originatorInfo; }
		}

		public Asn1Set RecipientInfos
		{
			get { return recipientInfos; }
		}

		public AlgorithmIdentifier MacAlgorithm
		{
			get { return macAlgorithm; }
		}

        public AlgorithmIdentifier DigestAlgorithm
        {
            get { return digestAlgorithm; }
        }

		public ContentInfo EncapsulatedContentInfo
		{
			get { return encapsulatedContentInfo; }
		}

		public Asn1Set AuthAttrs
		{
			get { return authAttrs; }
		}

		public Asn1OctetString Mac
		{
			get { return mac; }
		}

		public Asn1Set UnauthAttrs
		{
			get { return unauthAttrs; }
		}

		/**
		 * Produce an object suitable for an Asn1OutputStream.
		 * <pre>
		 * AuthenticatedData ::= SEQUENCE {
		 *       version CMSVersion,
		 *       originatorInfo [0] IMPLICIT OriginatorInfo OPTIONAL,
		 *       recipientInfos RecipientInfos,
		 *       macAlgorithm MessageAuthenticationCodeAlgorithm,
		 *       digestAlgorithm [1] DigestAlgorithmIdentifier OPTIONAL,
		 *       encapContentInfo EncapsulatedContentInfo,
		 *       authAttrs [2] IMPLICIT AuthAttributes OPTIONAL,
		 *       mac MessageAuthenticationCode,
		 *       unauthAttrs [3] IMPLICIT UnauthAttributes OPTIONAL }
		 *
		 * AuthAttributes ::= SET SIZE (1..MAX) OF Attribute
		 *
		 * UnauthAttributes ::= SET SIZE (1..MAX) OF Attribute
		 *
		 * MessageAuthenticationCode ::= OCTET STRING
		 * </pre>
		 */
		public override Asn1Object ToAsn1Object()
		{
			Asn1EncodableVector v = new Asn1EncodableVector(version);

			if (originatorInfo != null)
			{
				v.Add(new DerTaggedObject(false, 0, originatorInfo));
			}

			v.Add(recipientInfos, macAlgorithm);

			if (digestAlgorithm != null)
			{
				v.Add(new DerTaggedObject(false, 1, digestAlgorithm));
			}

			v.Add(encapsulatedContentInfo);

			if (authAttrs != null)
			{
				v.Add(new DerTaggedObject(false, 2, authAttrs));
			}

			v.Add(mac);

			if (unauthAttrs != null)
			{
				v.Add(new DerTaggedObject(false, 3, unauthAttrs));
			}

			return new BerSequence(v);
		}

		public static int CalculateVersion(OriginatorInfo origInfo)
		{
			if (origInfo == null)
				return 0;

			int ver = 0;

			foreach (object obj in origInfo.Certificates)
			{
				if (obj is Asn1TaggedObject)
				{
					Asn1TaggedObject tag = (Asn1TaggedObject)obj;

					if (tag.TagNo == 2)
					{
						ver = 1;
					}
					else if (tag.TagNo == 3)
					{
						ver = 3;
						break;
					}
				}
			}

			foreach (object obj in origInfo.Crls)
			{
				if (obj is Asn1TaggedObject)
				{
					Asn1TaggedObject tag = (Asn1TaggedObject)obj;

					if (tag.TagNo == 1)
					{
						ver = 3;
						break;
					}
				}
			}

			return ver;
		}
	}
}
