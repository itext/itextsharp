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

namespace Org.BouncyCastle.Asn1
{
	public class Asn1StreamParser
	{
		private readonly Stream _in;
		private readonly int _limit;

        private readonly byte[][] tmpBuffers;

        public Asn1StreamParser(
			Stream inStream)
			: this(inStream, Asn1InputStream.FindLimit(inStream))
		{
		}

		public Asn1StreamParser(
			Stream	inStream,
			int		limit)
		{
			if (!inStream.CanRead)
				throw new ArgumentException("Expected stream to be readable", "inStream");

			this._in = inStream;
			this._limit = limit;
            this.tmpBuffers = new byte[16][];
        }

		public Asn1StreamParser(
			byte[] encoding)
			: this(new MemoryStream(encoding, false), encoding.Length)
		{
		}

		internal IAsn1Convertible ReadIndef(int tagValue)
		{
			// Note: INDEF => CONSTRUCTED

			// TODO There are other tags that may be constructed (e.g. BIT_STRING)
			switch (tagValue)
			{
				case Asn1Tags.External:
					return new DerExternalParser(this);
				case Asn1Tags.OctetString:
					return new BerOctetStringParser(this);
				case Asn1Tags.Sequence:
					return new BerSequenceParser(this);
				case Asn1Tags.Set:
					return new BerSetParser(this);
				default:
					throw new Asn1Exception("unknown BER object encountered: 0x" + tagValue.ToString("X"));
			}
		}

		internal IAsn1Convertible ReadImplicit(bool constructed, int tag)
		{
			if (_in is IndefiniteLengthInputStream)
			{
				if (!constructed)
					throw new IOException("indefinite length primitive encoding encountered");

				return ReadIndef(tag);
			}

			if (constructed)
			{
				switch (tag)
				{
					case Asn1Tags.Set:
						return new DerSetParser(this);
					case Asn1Tags.Sequence:
						return new DerSequenceParser(this);
					case Asn1Tags.OctetString:
						return new BerOctetStringParser(this);
				}
			}
			else
			{
				switch (tag)
				{
					case Asn1Tags.Set:
						throw new Asn1Exception("sequences must use constructed encoding (see X.690 8.9.1/8.10.1)");
					case Asn1Tags.Sequence:
						throw new Asn1Exception("sets must use constructed encoding (see X.690 8.11.1/8.12.1)");
					case Asn1Tags.OctetString:
						return new DerOctetStringParser((DefiniteLengthInputStream)_in);
				}
			}

			throw new Asn1Exception("implicit tagging not implemented");
		}

		internal Asn1Object ReadTaggedObject(bool constructed, int tag)
		{
			if (!constructed)
			{
				// Note: !CONSTRUCTED => IMPLICIT
				DefiniteLengthInputStream defIn = (DefiniteLengthInputStream)_in;
				return new DerTaggedObject(false, tag, new DerOctetString(defIn.ToArray()));
			}

			Asn1EncodableVector v = ReadVector();

			if (_in is IndefiniteLengthInputStream)
			{
				return v.Count == 1
					?   new BerTaggedObject(true, tag, v[0])
					:   new BerTaggedObject(false, tag, BerSequence.FromVector(v));
			}

			return v.Count == 1
				?   new DerTaggedObject(true, tag, v[0])
				:   new DerTaggedObject(false, tag, DerSequence.FromVector(v));
		}

		public virtual IAsn1Convertible ReadObject()
		{
			int tag = _in.ReadByte();
			if (tag == -1)
				return null;

			// turn of looking for "00" while we resolve the tag
			Set00Check(false);

			//
			// calculate tag number
			//
			int tagNo = Asn1InputStream.ReadTagNumber(_in, tag);

			bool isConstructed = (tag & Asn1Tags.Constructed) != 0;

			//
			// calculate length
			//
			int length = Asn1InputStream.ReadLength(_in, _limit);

			if (length < 0) // indefinite length method
			{
				if (!isConstructed)
					throw new IOException("indefinite length primitive encoding encountered");

				IndefiniteLengthInputStream indIn = new IndefiniteLengthInputStream(_in, _limit);
				Asn1StreamParser sp = new Asn1StreamParser(indIn, _limit);

				if ((tag & Asn1Tags.Application) != 0)
				{
					return new BerApplicationSpecificParser(tagNo, sp);
				}

				if ((tag & Asn1Tags.Tagged) != 0)
				{
					return new BerTaggedObjectParser(true, tagNo, sp);
				}

				return sp.ReadIndef(tagNo);
			}
			else
			{
				DefiniteLengthInputStream defIn = new DefiniteLengthInputStream(_in, length);

				if ((tag & Asn1Tags.Application) != 0)
				{
					return new DerApplicationSpecific(isConstructed, tagNo, defIn.ToArray());
				}

				if ((tag & Asn1Tags.Tagged) != 0)
				{
					return new BerTaggedObjectParser(isConstructed, tagNo, new Asn1StreamParser(defIn));
				}

				if (isConstructed)
				{
					// TODO There are other tags that may be constructed (e.g. BitString)
					switch (tagNo)
					{
						case Asn1Tags.OctetString:
							//
							// yes, people actually do this...
							//
							return new BerOctetStringParser(new Asn1StreamParser(defIn));
						case Asn1Tags.Sequence:
							return new DerSequenceParser(new Asn1StreamParser(defIn));
						case Asn1Tags.Set:
							return new DerSetParser(new Asn1StreamParser(defIn));
						case Asn1Tags.External:
							return new DerExternalParser(new Asn1StreamParser(defIn));
						default:
							// TODO Add DerUnknownTagParser class?
							return new DerUnknownTag(true, tagNo, defIn.ToArray());
					}
				}

				// Some primitive encodings can be handled by parsers too...
				switch (tagNo)
				{
					case Asn1Tags.OctetString:
						return new DerOctetStringParser(defIn);
				}

				try
				{
					return Asn1InputStream.CreatePrimitiveDerObject(tagNo, defIn, tmpBuffers);
				}
				catch (ArgumentException e)
				{
					throw new Asn1Exception("corrupted stream detected", e);
				}
			}
		}

		private void Set00Check(
			bool enabled)
		{
			if (_in is IndefiniteLengthInputStream)
			{
				((IndefiniteLengthInputStream) _in).SetEofOn00(enabled);
			}
		}

		internal Asn1EncodableVector ReadVector()
		{
			Asn1EncodableVector v = new Asn1EncodableVector();

			IAsn1Convertible obj;
			while ((obj = ReadObject()) != null)
			{
				v.Add(obj.ToAsn1Object());
			}

			return v;
		}
	}
}
