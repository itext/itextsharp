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
using System.Collections;

using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Modes.Gcm
{
	public class Tables1kGcmExponentiator
		: IGcmExponentiator
	{
        // A lookup table of the power-of-two powers of 'x'
        // - lookupPowX2[i] = x^(2^i)
        private IList lookupPowX2;

        public void Init(byte[] x)
		{
            if (lookupPowX2 != null && Arrays.AreEqual(x, (byte[])lookupPowX2[0]))
            {
                return;
            }

            lookupPowX2 = Platform.CreateArrayList(8);
            lookupPowX2.Add(Arrays.Clone(x));
		}

		public void ExponentiateX(long pow, byte[] output)
		{
			byte[] y = GcmUtilities.OneAsBytes();
            int bit = 0;
            while (pow > 0)
            {
                if ((pow & 1L) != 0)
                {
                    EnsureAvailable(bit);
                    GcmUtilities.Multiply(y, (byte[])lookupPowX2[bit]);
                }
                ++bit;
                pow >>= 1;
            }

			Array.Copy(y, 0, output, 0, 16);
		}

        private void EnsureAvailable(int bit)
        {
            int count = lookupPowX2.Count;
            if (count <= bit)
            {
                byte[] tmp = (byte[])lookupPowX2[count - 1];
                do
                {
                    tmp = Arrays.Clone(tmp);
                    GcmUtilities.Multiply(tmp, tmp);
                    lookupPowX2.Add(tmp);
                }
                while (++count <= bit);
            }
        }
    }
}
