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
	* <pre>
	* EnvelopedData ::= SEQUENCE {
	*     version CMSVersion,
	*     originatorInfo [0] IMPLICIT OriginatorInfo OPTIONAL,
	*     recipientInfos RecipientInfos,
	*     encryptedContentInfo EncryptedContentInfo,
	*     unprotectedAttrs [1] IMPLICIT UnprotectedAttributes OPTIONAL
	* }
	* </pre>
	*/
	public class EnvelopedDataParser
	{
		private Asn1SequenceParser	_seq;
		private DerInteger			_version;
		private IAsn1Convertible	_nextObject;
		private bool				_originatorInfoCalled;

		public EnvelopedDataParser(
			Asn1SequenceParser seq)
		{
			this._seq = seq;
			this._version = (DerInteger)seq.ReadObject();
		}

		public DerInteger Version
		{
			get { return _version; }
		}

		public OriginatorInfo GetOriginatorInfo() 
		{
			_originatorInfoCalled = true; 

			if (_nextObject == null)
			{
				_nextObject = _seq.ReadObject();
			}

			if (_nextObject is Asn1TaggedObjectParser && ((Asn1TaggedObjectParser)_nextObject).TagNo == 0)
			{
				Asn1SequenceParser originatorInfo = (Asn1SequenceParser)
					((Asn1TaggedObjectParser)_nextObject).GetObjectParser(Asn1Tags.Sequence, false);
				_nextObject = null;
				return OriginatorInfo.GetInstance(originatorInfo.ToAsn1Object());
			}

			return null;
		}

		public Asn1SetParser GetRecipientInfos()
		{
			if (!_originatorInfoCalled)
			{
				GetOriginatorInfo();
			}

			if (_nextObject == null)
			{
				_nextObject = _seq.ReadObject();
			}

			Asn1SetParser recipientInfos = (Asn1SetParser)_nextObject;
			_nextObject = null;
			return recipientInfos;
		}

		public EncryptedContentInfoParser GetEncryptedContentInfo()
		{
			if (_nextObject == null)
			{
				_nextObject = _seq.ReadObject();
			}

			if (_nextObject != null)
			{
				Asn1SequenceParser o = (Asn1SequenceParser) _nextObject;
				_nextObject = null;
				return new EncryptedContentInfoParser(o);
			}

			return null;
		}

		public Asn1SetParser GetUnprotectedAttrs()
		{
			if (_nextObject == null)
			{
				_nextObject = _seq.ReadObject();
			}

			if (_nextObject != null)
			{
				IAsn1Convertible o = _nextObject;
				_nextObject = null;
				return (Asn1SetParser)((Asn1TaggedObjectParser)o).GetObjectParser(Asn1Tags.Set, false);
			}
        
			return null;
		}
	}
}
