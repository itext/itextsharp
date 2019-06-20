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
	/**
	 * Produce an object suitable for an Asn1OutputStream.
	 * 
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
	public class AuthEnvelopedDataParser
	{
		private Asn1SequenceParser	seq;
		private DerInteger			version;
		private IAsn1Convertible	nextObject;
		private bool				originatorInfoCalled;

		public AuthEnvelopedDataParser(
			Asn1SequenceParser	seq)
		{
			this.seq = seq;

			// TODO
			// "It MUST be set to 0."
			this.version = (DerInteger)seq.ReadObject();
		}

		public DerInteger Version
		{
			get { return version; }
		}

		public OriginatorInfo GetOriginatorInfo()
		{
			originatorInfoCalled = true;

			if (nextObject == null)
			{
				nextObject = seq.ReadObject();
			}

			if (nextObject is Asn1TaggedObjectParser && ((Asn1TaggedObjectParser)nextObject).TagNo == 0)
			{
				Asn1SequenceParser originatorInfo = (Asn1SequenceParser) ((Asn1TaggedObjectParser)nextObject).GetObjectParser(Asn1Tags.Sequence, false);
				nextObject = null;
				return OriginatorInfo.GetInstance(originatorInfo.ToAsn1Object());
			}

			return null;
		}

		public Asn1SetParser GetRecipientInfos()
		{
			if (!originatorInfoCalled)
			{
				GetOriginatorInfo();
			}

			if (nextObject == null)
			{
				nextObject = seq.ReadObject();
			}

			Asn1SetParser recipientInfos = (Asn1SetParser)nextObject;
			nextObject = null;
			return recipientInfos;
		}

		public EncryptedContentInfoParser GetAuthEncryptedContentInfo() 
		{
			if (nextObject == null)
			{
				nextObject = seq.ReadObject();
			}

			if (nextObject != null)
			{
				Asn1SequenceParser o = (Asn1SequenceParser) nextObject;
				nextObject = null;
				return new EncryptedContentInfoParser(o);
			}

			return null;
		}
		
		public Asn1SetParser GetAuthAttrs()
		{
			if (nextObject == null)
			{
				nextObject = seq.ReadObject();
			}

			if (nextObject is Asn1TaggedObjectParser)
			{
				IAsn1Convertible o = nextObject;
				nextObject = null;
				return (Asn1SetParser)((Asn1TaggedObjectParser)o).GetObjectParser(Asn1Tags.Set, false);
			}

			// TODO
			// "The authAttrs MUST be present if the content type carried in
			// EncryptedContentInfo is not id-data."

			return null;
		}
		
		public Asn1OctetString GetMac()
		{
			if (nextObject == null)
			{
				nextObject = seq.ReadObject();
			}

			IAsn1Convertible o = nextObject;
			nextObject = null;

			return Asn1OctetString.GetInstance(o.ToAsn1Object());
		}
		
		public Asn1SetParser GetUnauthAttrs()
		{
			if (nextObject == null)
			{
				nextObject = seq.ReadObject();
			}

			if (nextObject != null)
			{
				IAsn1Convertible o = nextObject;
				nextObject = null;
				return (Asn1SetParser)((Asn1TaggedObjectParser)o).GetObjectParser(Asn1Tags.Set, false);
			}

			return null;
		}
	}
}
