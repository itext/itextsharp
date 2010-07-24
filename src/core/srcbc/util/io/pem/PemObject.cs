using System;
using System.Collections;

using Org.BouncyCastle.Utilities.Collections;

namespace Org.BouncyCastle.Utilities.IO.Pem
{
	public class PemObject
		: PemObjectGenerator
	{
		private string		type;
		private IDictionary	headers;
		private byte[]		content;

		public PemObject(string type, byte[] content)
			: this(type, new Hashtable(), content)
		{
		}

		public PemObject(String type, IDictionary headers, byte[] content)
		{
			this.type = type;
			this.headers = new UnmodifiableDictionaryProxy(headers);
			this.content = content;
		}

		public string Type
		{
			get { return type; }
		}

		public IDictionary Headers
		{
			get { return headers; }
		}

		public byte[] Content
		{
			get { return content; }
		}

		public PemObject Generate()
		{
			return this;
		}
	}
}
