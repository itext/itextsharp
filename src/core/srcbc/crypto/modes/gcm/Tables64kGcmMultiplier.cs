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

using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Modes.Gcm
{
    public class Tables64kGcmMultiplier
        : IGcmMultiplier
    {
        private byte[] H;
        private uint[][][] M;

        public void Init(byte[] H)
        {
            if (M == null)
            {
                M = new uint[16][][];
            }
            else if (Arrays.AreEqual(this.H, H))
            {
                return;
            }

            this.H = Arrays.Clone(H);

            M[0] = new uint[256][];
            M[0][0] = new uint[4];
            M[0][128] = GcmUtilities.AsUints(H);
            for (int j = 64; j >= 1; j >>= 1)
            {
                uint[] tmp = (uint[])M[0][j + j].Clone();
                GcmUtilities.MultiplyP(tmp);
                M[0][j] = tmp;
            }
            for (int i = 0; ; )
            {
                for (int j = 2; j < 256; j += j)
                {
                    for (int k = 1; k < j; ++k)
                    {
                        uint[] tmp = (uint[])M[i][j].Clone();
                        GcmUtilities.Xor(tmp, M[i][k]);
                        M[i][j + k] = tmp;
                    }
                }

                if (++i == 16) return;

                M[i] = new uint[256][];
                M[i][0] = new uint[4];
                for (int j = 128; j > 0; j >>= 1)
                {
                    uint[] tmp = (uint[])M[i - 1][j].Clone();
                    GcmUtilities.MultiplyP8(tmp);
                    M[i][j] = tmp;
                }
            }
        }

        public void MultiplyH(byte[] x)
        {
            uint[] z = new uint[4];
            for (int i = 0; i != 16; ++i)
            {
                //GcmUtilities.Xor(z, M[i][x[i]]);
                uint[] m = M[i][x[i]];
                z[0] ^= m[0];
                z[1] ^= m[1];
                z[2] ^= m[2];
                z[3] ^= m[3];
            }

            Pack.UInt32_To_BE(z, x, 0);
        }
    }
}
