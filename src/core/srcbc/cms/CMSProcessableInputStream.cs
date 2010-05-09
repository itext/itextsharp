using System;
using System.IO;

using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Cms
{
	public class CmsProcessableInputStream
		: CmsProcessable, CmsReadable
	{
		private Stream input;
		private bool used = false;

		public CmsProcessableInputStream(
			Stream input)
		{
			this.input = input;
		}

		public Stream Read()
		{
			CheckSingleUsage();

			return input;
		}

		public void Write(Stream output)
		{
			CheckSingleUsage();

			Streams.PipeAll(input, output);
			input.Close();
		}

		public object GetContent()
		{
			return Read();
		}

		private void CheckSingleUsage()
		{
			lock (this)
			{
				if (used)
					throw new InvalidOperationException("CmsProcessableInputStream can only be used once");

				used = true;
			}
		}
	}
}
