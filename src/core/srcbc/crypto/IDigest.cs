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

namespace Org.BouncyCastle.Crypto
{
    /**
     * interface that a message digest conforms to.
     */
    public interface IDigest
    {
        /**
         * return the algorithm name
         *
         * @return the algorithm name
         */
        string AlgorithmName { get; }

		/**
         * return the size, in bytes, of the digest produced by this message digest.
         *
         * @return the size, in bytes, of the digest produced by this message digest.
         */
		int GetDigestSize();

		/**
         * return the size, in bytes, of the internal buffer used by this digest.
         *
         * @return the size, in bytes, of the internal buffer used by this digest.
         */
		int GetByteLength();

		/**
         * update the message digest with a single byte.
         *
         * @param inByte the input byte to be entered.
         */
        void Update(byte input);

        /**
         * update the message digest with a block of bytes.
         *
         * @param input the byte array containing the data.
         * @param inOff the offset into the byte array where the data starts.
         * @param len the length of the data.
         */
        void BlockUpdate(byte[] input, int inOff, int length);

        /**
         * Close the digest, producing the final digest value. The doFinal
         * call leaves the digest reset.
         *
         * @param output the array the digest is to be copied into.
         * @param outOff the offset into the out array the digest is to start at.
         */
        int DoFinal(byte[] output, int outOff);

        /**
         * reset the digest back to it's initial state.
         */
        void Reset();
    }
}
