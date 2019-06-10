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
using System.Text;

namespace Org.BouncyCastle.Asn1.X509
{
    /**
     * class for breaking up an X500 Name into it's component tokens, ala
     * java.util.StringTokenizer. We need this class as some of the
     * lightweight Java environment don't support classes like
     * StringTokenizer.
     */
    public class X509NameTokenizer
    {
        private string			value;
        private int				index;
        private char			separator;
        private StringBuilder	buffer = new StringBuilder();

		public X509NameTokenizer(
            string oid)
            : this(oid, ',')
        {
        }

		public X509NameTokenizer(
            string	oid,
            char	separator)
        {
            this.value = oid;
            this.index = -1;
            this.separator = separator;
        }

		public bool HasMoreTokens()
        {
            return index != value.Length;
        }

		public string NextToken()
        {
            if (index == value.Length)
            {
                return null;
            }

            int end = index + 1;
            bool quoted = false;
            bool escaped = false;

			buffer.Remove(0, buffer.Length);

			while (end != value.Length)
            {
                char c = value[end];

				if (c == '"')
                {
                    if (!escaped)
                    {
                        quoted = !quoted;
                    }
                    else
                    {
                        buffer.Append(c);
						escaped = false;
                    }
                }
                else
                {
                    if (escaped || quoted)
                    {
						if (c == '#' && buffer[buffer.Length - 1] == '=')
						{
							buffer.Append('\\');
						}
						else if (c == '+' && separator != '+')
						{
							buffer.Append('\\');
						}
						buffer.Append(c);
                        escaped = false;
                    }
                    else if (c == '\\')
                    {
                        escaped = true;
                    }
                    else if (c == separator)
                    {
                        break;
                    }
                    else
                    {
                        buffer.Append(c);
                    }
                }

				end++;
            }

			index = end;

			return buffer.ToString().Trim();
        }
    }
}
