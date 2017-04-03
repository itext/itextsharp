using System;

namespace Org.BouncyCastle.Utilities.Date
{
	[Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public sealed class DateTimeObject
	{
		private readonly DateTime dt;

		public DateTimeObject(
			DateTime dt)
		{
			this.dt = dt;
		}

		public DateTime Value
		{
			get { return dt; }
		}

		public override string ToString()
		{
			return dt.ToString();
		}
	}
}
