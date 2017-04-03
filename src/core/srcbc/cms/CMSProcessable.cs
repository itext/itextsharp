using System;
using System.IO;

namespace Org.BouncyCastle.Cms
{
	[Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public interface CmsProcessable
	{
		/// <summary>
		/// Generic routine to copy out the data we want processed.
		/// </summary>
		/// <remarks>
		/// This routine may be called multiple times.
		/// </remarks>
		void Write(Stream outStream);

		[Obsolete]
		object GetContent();
	}
}
