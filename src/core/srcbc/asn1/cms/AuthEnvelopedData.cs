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

namespace Org.BouncyCastle.Asn1.Cms
{
	public class AuthEnvelopedData
		: Asn1Encodable
	{
		private DerInteger				version;
		private OriginatorInfo			originatorInfo;
		private Asn1Set					recipientInfos;
		private EncryptedContentInfo	authEncryptedContentInfo;
		private Asn1Set					authAttrs;
		private Asn1OctetString			mac;
		private Asn1Set					unauthAttrs;

		public AuthEnvelopedData(
			OriginatorInfo			originatorInfo,
			Asn1Set					recipientInfos,
			EncryptedContentInfo	authEncryptedContentInfo,
			Asn1Set					authAttrs,
			Asn1OctetString			mac,
			Asn1Set					unauthAttrs)
		{
			// "It MUST be set to 0."
			this.version = new DerInteger(0);

			this.originatorInfo = originatorInfo;

			// TODO
			// "There MUST be at least one element in the collection."
			this.recipientInfos = recipientInfos;

			this.authEncryptedContentInfo = authEncryptedContentInfo;

			// TODO
			// "The authAttrs MUST be present if the content type carried in
			// EncryptedContentInfo is not id-data."
			this.authAttrs = authAttrs;

			this.mac = mac;

			this.unauthAttrs = unauthAttrs;
	    }

		private AuthEnvelopedData(
			Asn1Sequence	seq)
		{
			int index = 0;

			// TODO
			// "It MUST be set to 0."
			Asn1Object tmp = seq[index++].ToAsn1Object();
			version = (DerInteger)tmp;

			tmp = seq[index++].ToAsn1Object();
			if (tmp is Asn1TaggedObject)
			{
				originatorInfo = OriginatorInfo.GetInstance((Asn1TaggedObject)tmp, false);
				tmp = seq[index++].ToAsn1Object();
			}

			// TODO
			// "There MUST be at least one element in the collection."
			recipientInfos = Asn1Set.GetInstance(tmp);

			tmp = seq[index++].ToAsn1Object();
			authEncryptedContentInfo = EncryptedContentInfo.GetInstance(tmp);

			tmp = seq[index++].ToAsn1Object();
			if (tmp is Asn1TaggedObject)
			{
				authAttrs = Asn1Set.GetInstance((Asn1TaggedObject)tmp, false);
				tmp = seq[index++].ToAsn1Object();
			}
			else
			{
				// TODO
				// "The authAttrs MUST be present if the content type carried in
				// EncryptedContentInfo is not id-data."
			}

			mac = Asn1OctetString.GetInstance(tmp);

			if (seq.Count > index)
			{
				tmp = seq[index++].ToAsn1Object();
				unauthAttrs = Asn1Set.GetInstance((Asn1TaggedObject)tmp, false);
			}
		}

		/**
		 * return an AuthEnvelopedData object from a tagged object.
		 *
		 * @param obj      the tagged object holding the object we want.
		 * @param isExplicit true if the object is meant to be explicitly
		 *                 tagged false otherwise.
		 * @throws ArgumentException if the object held by the
		 *                                  tagged object cannot be converted.
		 */
		public static AuthEnvelopedData GetInstance(
			Asn1TaggedObject	obj,
			bool				isExplicit)
		{
			return GetInstance(Asn1Sequence.GetInstance(obj, isExplicit));
		}

		/**
		 * return an AuthEnvelopedData object from the given object.
		 *
		 * @param obj the object we want converted.
		 * @throws ArgumentException if the object cannot be converted.
		 */
		public static AuthEnvelopedData GetInstance(
			object	obj)
		{
			if (obj == null || obj is AuthEnvelopedData)
				return (AuthEnvelopedData)obj;

			if (obj is Asn1Sequence)
				return new AuthEnvelopedData((Asn1Sequence)obj);

			throw new ArgumentException("Invalid AuthEnvelopedData: " + obj.GetType().Name);
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

		public EncryptedContentInfo AuthEncryptedContentInfo
		{
			get { return authEncryptedContentInfo; }
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
		 * AuthEnvelopedData ::= SEQUENCE {
		 *   version CMSVersion,
		 *   originatorInfo [0] IMPLICIT OriginatorInfo OPTIONAL,
		 *   recipientInfos RecipientInfos,
		 *   authEncryptedContentInfo EncryptedContentInfo,
		 *   authAttrs [1] IMPLICIT AuthAttributes OPTIONAL,
		 *   mac MessageAuthenticationCode,
		 *   unauthAttrs [2] IMPLICIT UnauthAttributes OPTIONAL }
		 * </pre>
		 */
	    public override Asn1Object ToAsn1Object()
		{
			Asn1EncodableVector v = new Asn1EncodableVector(version);

			if (originatorInfo != null)
			{
				v.Add(new DerTaggedObject(false, 0, originatorInfo));
			}

			v.Add(recipientInfos, authEncryptedContentInfo);

			// "authAttrs optionally contains the authenticated attributes."
			if (authAttrs != null)
			{
				// "AuthAttributes MUST be DER encoded, even if the rest of the
				// AuthEnvelopedData structure is BER encoded."
				v.Add(new DerTaggedObject(false, 1, authAttrs));
			}

			v.Add(mac);

			// "unauthAttrs optionally contains the unauthenticated attributes."
			if (unauthAttrs != null)
			{
				v.Add(new DerTaggedObject(false, 2, unauthAttrs));
			}

			return new BerSequence(v);
		}
	}
}
