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

using Org.BouncyCastle.Crypto.Parameters;

namespace Org.BouncyCastle.Crypto.Generators
{
    public class DesEdeKeyGenerator
		: DesKeyGenerator
    {
		public DesEdeKeyGenerator()
		{
		}

		internal DesEdeKeyGenerator(
			int defaultStrength)
			: base(defaultStrength)
		{
		}

		/**
        * initialise the key generator - if strength is set to zero
        * the key Generated will be 192 bits in size, otherwise
        * strength can be 128 or 192 (or 112 or 168 if you don't count
        * parity bits), depending on whether you wish to do 2-key or 3-key
        * triple DES.
        *
        * @param param the parameters to be used for key generation
        */
        protected override void engineInit(
			KeyGenerationParameters parameters)
        {
			this.random = parameters.Random;
			this.strength = (parameters.Strength + 7) / 8;

			if (strength == 0 || strength == (168 / 8))
            {
                strength = DesEdeParameters.DesEdeKeyLength;
            }
            else if (strength == (112 / 8))
            {
                strength = 2 * DesEdeParameters.DesKeyLength;
            }
            else if (strength != DesEdeParameters.DesEdeKeyLength
                && strength != (2 * DesEdeParameters.DesKeyLength))
            {
                throw new ArgumentException("DESede key must be "
                    + (DesEdeParameters.DesEdeKeyLength * 8) + " or "
                    + (2 * 8 * DesEdeParameters.DesKeyLength)
                    + " bits long.");
            }
        }

        protected override byte[] engineGenerateKey()
        {
            byte[] newKey;

			do
            {
                newKey = random.GenerateSeed(strength);
                DesEdeParameters.SetOddParity(newKey);
            }
            while (DesEdeParameters.IsWeakKey(newKey, 0, newKey.Length));

            return newKey;
        }
    }
}
