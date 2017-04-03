using System;
using System.IO;

namespace Org.BouncyCastle.Utilities.IO.Pem
{
	[Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public interface PemObjectParser
	{
		/// <param name="obj">
		/// A <see cref="PemObject"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Object"/>
		/// </returns>
		/// <exception cref="IOException"></exception>
		object ParseObject(PemObject obj);
	}
}
