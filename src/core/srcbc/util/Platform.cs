using System;
using System.IO;
using System.Text;

namespace Org.BouncyCastle.Utilities
{
	internal sealed class Platform
	{
		private Platform()
		{
		}

#if NETCF_1_0 || NETCF_2_0
		private static string GetNewLine()
		{
			MemoryStream buf = new MemoryStream();
			StreamWriter w = new StreamWriter(buf, Encoding.ASCII);
			w.WriteLine();
			w.Close();
			byte[] bs = buf.ToArray();
			return Encoding.ASCII.GetString(bs, 0, bs.Length);
		}

		internal static string GetEnvironmentVariable(
			string variable)
		{
			return null;
		}
#else
		private static string GetNewLine()
		{
			return Environment.NewLine;
		}

		internal static string GetEnvironmentVariable(
			string variable)
		{
			try
			{
				return Environment.GetEnvironmentVariable(variable);
			}
			catch (System.Security.SecurityException)
			{
				// We don't have the required permission to read this environment variable,
				// which is fine, just act as if it's not set
				return null;
			}
		}
#endif

#if NETCF_1_0
		internal static Exception CreateNotImplementedException(
			string message)
		{
			return new Exception("Not implemented: " + message);
		}

		internal static bool Equals(
			object	a,
			object	b)
		{
			return a == b || (a != null && b != null && a.Equals(b));
		}
#else
		internal static Exception CreateNotImplementedException(
			string message)
		{
			return new NotImplementedException(message);
		}
#endif

		internal static readonly string NewLine = GetNewLine();
	}
}
