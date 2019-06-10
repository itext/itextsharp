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
     * The base interface for implementations of message authentication codes (MACs).
     */
    public interface IMac
    {
        /**
         * Initialise the MAC.
         *
         * @param param the key and other data required by the MAC.
         * @exception ArgumentException if the parameters argument is
         * inappropriate.
         */
        void Init(ICipherParameters parameters);

        /**
         * Return the name of the algorithm the MAC implements.
         *
         * @return the name of the algorithm the MAC implements.
         */
        string AlgorithmName { get; }

		/**
		 * Return the block size for this MAC (in bytes).
		 *
		 * @return the block size for this MAC in bytes.
		 */
		int GetMacSize();

        /**
         * add a single byte to the mac for processing.
         *
         * @param in the byte to be processed.
         * @exception InvalidOperationException if the MAC is not initialised.
         */
        void Update(byte input);

		/**
         * @param in the array containing the input.
         * @param inOff the index in the array the data begins at.
         * @param len the length of the input starting at inOff.
         * @exception InvalidOperationException if the MAC is not initialised.
         * @exception DataLengthException if there isn't enough data in in.
         */
        void BlockUpdate(byte[] input, int inOff, int len);

		/**
         * Compute the final stage of the MAC writing the output to the out
         * parameter.
         * <p>
         * doFinal leaves the MAC in the same state it was after the last init.
         * </p>
         * @param out the array the MAC is to be output to.
         * @param outOff the offset into the out buffer the output is to start at.
         * @exception DataLengthException if there isn't enough space in out.
         * @exception InvalidOperationException if the MAC is not initialised.
         */
        int DoFinal(byte[] output, int outOff);

		/**
         * Reset the MAC. At the end of resetting the MAC should be in the
         * in the same state it was after the last init (if there was one).
         */
        void Reset();
    }
}
