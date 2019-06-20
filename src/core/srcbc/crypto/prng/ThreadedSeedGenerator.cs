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
using System.Threading;

namespace Org.BouncyCastle.Crypto.Prng
{
	/**
	 * A thread based seed generator - one source of randomness.
	 * <p>
	 * Based on an idea from Marcus Lippert.
	 * </p>
	 */
	public class ThreadedSeedGenerator
	{
		private class SeedGenerator
		{
#if NETCF_1_0
			// No volatile keyword, but all fields implicitly volatile anyway
			private int		counter = 0;
			private bool	stop = false;
#else
			private volatile int	counter = 0;
			private volatile bool	stop = false;
#endif

			private void Run(object ignored)
			{
				while (!this.stop)
				{
					this.counter++;
				}
			}

			public byte[] GenerateSeed(
				int		numBytes,
				bool	fast)
			{
#if SILVERLIGHT
                return DoGenerateSeed(numBytes, fast);
#else
                ThreadPriority originalPriority = Thread.CurrentThread.Priority;
                try
                {
                    Thread.CurrentThread.Priority = ThreadPriority.Normal;
                    return DoGenerateSeed(numBytes, fast);
                }
                finally
                {
                    Thread.CurrentThread.Priority = originalPriority;
                }
#endif
            }

            private byte[] DoGenerateSeed(
				int		numBytes,
				bool	fast)
            {
                this.counter = 0;
				this.stop = false;

				byte[] result = new byte[numBytes];
				int last = 0;
				int end = fast ? numBytes : numBytes * 8;

				ThreadPool.QueueUserWorkItem(new WaitCallback(Run));

				for (int i = 0; i < end; i++)
				{
					while (this.counter == last)
					{
						try
						{
							Thread.Sleep(1);
						}
						catch (Exception)
						{
							// ignore
						}
					}

					last = this.counter;

					if (fast)
					{
						result[i] = (byte) last;
					}
					else
					{
						int bytepos = i / 8;
						result[bytepos] = (byte) ((result[bytepos] << 1) | (last & 1));
					}
				}

				this.stop = true;

				return result;
			}
		}

		/**
		 * Generate seed bytes. Set fast to false for best quality.
		 * <p>
		 * If fast is set to true, the code should be round about 8 times faster when
		 * generating a long sequence of random bytes. 20 bytes of random values using
		 * the fast mode take less than half a second on a Nokia e70. If fast is set to false,
		 * it takes round about 2500 ms.
		 * </p>
		 * @param numBytes the number of bytes to generate
		 * @param fast true if fast mode should be used
		 */
		public byte[] GenerateSeed(
			int		numBytes,
			bool	fast)
		{
			return new SeedGenerator().GenerateSeed(numBytes, fast);
		}
	}
}
