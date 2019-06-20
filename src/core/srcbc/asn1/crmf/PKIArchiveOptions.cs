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

namespace Org.BouncyCastle.Asn1.Crmf
{
    public class PkiArchiveOptions
        : Asn1Encodable, IAsn1Choice
    {
        public const int encryptedPrivKey = 0;
        public const int keyGenParameters = 1;
        public const int archiveRemGenPrivKey = 2;

        private readonly Asn1Encodable value;

        public static PkiArchiveOptions GetInstance(object obj)
        {
            if (obj is PkiArchiveOptions)
                return (PkiArchiveOptions)obj;

            if (obj is Asn1TaggedObject)
                return new PkiArchiveOptions((Asn1TaggedObject)obj);

            throw new ArgumentException("Invalid object: " + obj.GetType().Name, "obj");
        }

        private PkiArchiveOptions(Asn1TaggedObject tagged)
        {
            switch (tagged.TagNo)
            {
                case encryptedPrivKey:
                    value = EncryptedKey.GetInstance(tagged.GetObject());
                    break;
                case keyGenParameters:
                    value = Asn1OctetString.GetInstance(tagged, false);
                    break;
                case archiveRemGenPrivKey:
                    value = DerBoolean.GetInstance(tagged, false);
                    break;
                default:
                    throw new ArgumentException("unknown tag number: " + tagged.TagNo, "tagged");
            }
        }

        public PkiArchiveOptions(EncryptedKey encKey)
        {
            this.value = encKey;
        }

        public PkiArchiveOptions(Asn1OctetString keyGenParameters)
        {
            this.value = keyGenParameters;
        }

        public PkiArchiveOptions(bool archiveRemGenPrivKey)
        {
            this.value = DerBoolean.GetInstance(archiveRemGenPrivKey);
        }

        public virtual int Type
        {
            get
            {
                if (value is EncryptedKey)
                    return encryptedPrivKey;

                if (value is Asn1OctetString)
                    return keyGenParameters;

                return archiveRemGenPrivKey;
            }
        }

        public virtual Asn1Encodable Value
        {
            get { return value; }
        }

        /**
         * <pre>
         *  PkiArchiveOptions ::= CHOICE {
         *      encryptedPrivKey     [0] EncryptedKey,
         *      -- the actual value of the private key
         *      keyGenParameters     [1] KeyGenParameters,
         *      -- parameters which allow the private key to be re-generated
         *      archiveRemGenPrivKey [2] BOOLEAN }
         *      -- set to TRUE if sender wishes receiver to archive the private
         *      -- key of a key pair that the receiver generates in response to
         *      -- this request; set to FALSE if no archival is desired.
         * </pre>
         */
        public override Asn1Object ToAsn1Object()
        {
            if (value is EncryptedKey)
            {
                return new DerTaggedObject(true, encryptedPrivKey, value);  // choice
            }

            if (value is Asn1OctetString)
            {
                return new DerTaggedObject(false, keyGenParameters, value);
            }

            return new DerTaggedObject(false, archiveRemGenPrivKey, value);
        }
    }
}
