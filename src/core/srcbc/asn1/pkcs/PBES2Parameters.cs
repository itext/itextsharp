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

namespace Org.BouncyCastle.Asn1.Pkcs
{
    public class PbeS2Parameters
        : Asn1Encodable
    {
        private readonly KeyDerivationFunc func;
        private readonly EncryptionScheme scheme;

        public static PbeS2Parameters GetInstance(object obj)
        {
            if (obj == null)
                return null;
            PbeS2Parameters existing = obj as PbeS2Parameters;
            if (existing != null)
                return existing;
            return new PbeS2Parameters(Asn1Sequence.GetInstance(obj));
        }

        public PbeS2Parameters(KeyDerivationFunc keyDevFunc, EncryptionScheme encScheme)
        {
            this.func = keyDevFunc;
            this.scheme = encScheme;
        }

        [Obsolete("Use GetInstance() instead")]
        public PbeS2Parameters(
            Asn1Sequence seq)
        {
            if (seq.Count != 2)
                throw new ArgumentException("Wrong number of elements in sequence", "seq");

            Asn1Sequence funcSeq = (Asn1Sequence)seq[0].ToAsn1Object();

            // TODO Not sure if this special case is really necessary/appropriate
            if (funcSeq[0].Equals(PkcsObjectIdentifiers.IdPbkdf2))
            {
                func = new KeyDerivationFunc(PkcsObjectIdentifiers.IdPbkdf2,
                    Pbkdf2Params.GetInstance(funcSeq[1]));
            }
            else
            {
                func = new KeyDerivationFunc(funcSeq);
            }

            scheme = EncryptionScheme.GetInstance(seq[1].ToAsn1Object());
        }

        public KeyDerivationFunc KeyDerivationFunc
        {
            get { return func; }
        }

        public EncryptionScheme EncryptionScheme
        {
            get { return scheme; }
        }

        public override Asn1Object ToAsn1Object()
        {
            return new DerSequence(func, scheme);
        }
    }
}
