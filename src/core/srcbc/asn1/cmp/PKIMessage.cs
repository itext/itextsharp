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

namespace Org.BouncyCastle.Asn1.Cmp
{
    public class PkiMessage
        : Asn1Encodable
    {
        private readonly PkiHeader header;
        private readonly PkiBody body;
        private readonly DerBitString protection;
        private readonly Asn1Sequence extraCerts;

        private PkiMessage(Asn1Sequence seq)
        {
            header = PkiHeader.GetInstance(seq[0]);
            body = PkiBody.GetInstance(seq[1]);

            for (int pos = 2; pos < seq.Count; ++pos)
            {
                Asn1TaggedObject tObj = (Asn1TaggedObject)seq[pos].ToAsn1Object();

                if (tObj.TagNo == 0)
                {
                    protection = DerBitString.GetInstance(tObj, true);
                }
                else
                {
                    extraCerts = Asn1Sequence.GetInstance(tObj, true);
                }
            }
        }

        public static PkiMessage GetInstance(object obj)
        {
            if (obj is PkiMessage)
                return (PkiMessage)obj;

            if (obj != null)
                return new PkiMessage(Asn1Sequence.GetInstance(obj));

            return null;
        }

        /**
         * Creates a new PkiMessage.
         *
         * @param header message header
         * @param body message body
         * @param protection message protection (may be null)
         * @param extraCerts extra certificates (may be null)
         */
        public PkiMessage(
            PkiHeader header,
            PkiBody body,
            DerBitString protection,
            CmpCertificate[] extraCerts)
        {
            this.header = header;
            this.body = body;
            this.protection = protection;
            if (extraCerts != null)
            {
                this.extraCerts = new DerSequence(extraCerts);
            }
        }

        public PkiMessage(
            PkiHeader header,
            PkiBody body,
            DerBitString protection)
            : this(header, body, protection, null)
        {
        }

        public PkiMessage(
            PkiHeader header,
            PkiBody body)
            : this(header, body, null, null)
        {
        }

        public virtual PkiHeader Header
        {
            get { return header; }
        }

        public virtual PkiBody Body
        {
            get { return body; }
        }

        public virtual DerBitString Protection
        {
            get { return protection; }
        }

        public virtual CmpCertificate[] GetExtraCerts()
        {
            if (extraCerts == null)
                return null;

            CmpCertificate[] results = new CmpCertificate[extraCerts.Count];
            for (int i = 0; i < results.Length; ++i)
            {
                results[i] = CmpCertificate.GetInstance(extraCerts[i]);
            }
            return results;
        }

        /**
         * <pre>
         * PkiMessage ::= SEQUENCE {
         *                  header           PKIHeader,
         *                  body             PKIBody,
         *                  protection   [0] PKIProtection OPTIONAL,
         *                  extraCerts   [1] SEQUENCE SIZE (1..MAX) OF CMPCertificate
         *                                                                     OPTIONAL
         * }
         * </pre>
         * @return a basic ASN.1 object representation.
         */
        public override Asn1Object ToAsn1Object()
        {
            Asn1EncodableVector v = new Asn1EncodableVector(header, body);

            AddOptional(v, 0, protection);
            AddOptional(v, 1, extraCerts);

            return new DerSequence(v);
        }

        private static void AddOptional(Asn1EncodableVector v, int tagNo, Asn1Encodable obj)
        {
            if (obj != null)
            {
                v.Add(new DerTaggedObject(true, tagNo, obj));
            }
        }
    }
}
