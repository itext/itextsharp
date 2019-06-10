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
using System.IO;

namespace Org.BouncyCastle.Asn1.Cms
{
	/**
	* <pre>
	* SignedData ::= SEQUENCE {
	*     version CMSVersion,
	*     digestAlgorithms DigestAlgorithmIdentifiers,
	*     encapContentInfo EncapsulatedContentInfo,
	*     certificates [0] IMPLICIT CertificateSet OPTIONAL,
	*     crls [1] IMPLICIT CertificateRevocationLists OPTIONAL,
	*     signerInfos SignerInfos
	*   }
	* </pre>
	*/
	public class SignedDataParser
	{
		private Asn1SequenceParser	_seq;
		private DerInteger			_version;
		private object				_nextObject;
		private bool				_certsCalled;
		private bool				_crlsCalled;

		public static SignedDataParser GetInstance(
			object o)
		{
			if (o is Asn1Sequence)
				return new SignedDataParser(((Asn1Sequence)o).Parser);

			if (o is Asn1SequenceParser)
				return new SignedDataParser((Asn1SequenceParser)o);

			throw new IOException("unknown object encountered: " + o.GetType().Name);
		}

		public SignedDataParser(
			Asn1SequenceParser seq)
		{
			this._seq = seq;
			this._version = (DerInteger)seq.ReadObject();
		}

		public DerInteger Version
		{
			get { return _version; }
		}

		public Asn1SetParser GetDigestAlgorithms()
		{
			return (Asn1SetParser)_seq.ReadObject();
		}

		public ContentInfoParser GetEncapContentInfo()
		{
			return new ContentInfoParser((Asn1SequenceParser)_seq.ReadObject());
		}

		public Asn1SetParser GetCertificates()
		{
			_certsCalled = true;
			_nextObject = _seq.ReadObject();

			if (_nextObject is Asn1TaggedObjectParser && ((Asn1TaggedObjectParser)_nextObject).TagNo == 0)
			{
				Asn1SetParser certs = (Asn1SetParser)((Asn1TaggedObjectParser)_nextObject).GetObjectParser(Asn1Tags.Set, false);
				_nextObject = null;

				return certs;
			}

			return null;
		}

		public Asn1SetParser GetCrls()
		{
			if (!_certsCalled)
				throw new IOException("GetCerts() has not been called.");

			_crlsCalled = true;

			if (_nextObject == null)
			{
				_nextObject = _seq.ReadObject();
			}

			if (_nextObject is Asn1TaggedObjectParser && ((Asn1TaggedObjectParser)_nextObject).TagNo == 1)
			{
				Asn1SetParser crls = (Asn1SetParser)((Asn1TaggedObjectParser)_nextObject).GetObjectParser(Asn1Tags.Set, false);
				_nextObject = null;

				return crls;
			}

			return null;
		}

		public Asn1SetParser GetSignerInfos()
		{
			if (!_certsCalled || !_crlsCalled)
				throw new IOException("GetCerts() and/or GetCrls() has not been called.");

			if (_nextObject == null)
			{
				_nextObject = _seq.ReadObject();
			}

			return (Asn1SetParser)_nextObject;
		}
	}
}
