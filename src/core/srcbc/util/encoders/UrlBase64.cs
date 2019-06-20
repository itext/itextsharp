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
using System.IO;

namespace Org.BouncyCastle.Utilities.Encoders
{
	/**
	* Convert binary data to and from UrlBase64 encoding.  This is identical to
	* Base64 encoding, except that the padding character is "." and the other 
	* non-alphanumeric characters are "-" and "_" instead of "+" and "/".
	* <p>
	* The purpose of UrlBase64 encoding is to provide a compact encoding of binary
	* data that is safe for use as an URL parameter. Base64 encoding does not
	* produce encoded values that are safe for use in URLs, since "/" can be 
	* interpreted as a path delimiter; "+" is the encoded form of a space; and
	* "=" is used to separate a name from the corresponding value in an URL 
	* parameter.
	* </p>
	*/
	public class UrlBase64
	{
		private static readonly IEncoder encoder = new UrlBase64Encoder();

		/**
		* Encode the input data producing a URL safe base 64 encoded byte array.
		*
		* @return a byte array containing the URL safe base 64 encoded data.
		*/
		public static byte[] Encode(
			byte[] data)
		{
			MemoryStream bOut = new MemoryStream();

			try
			{
				encoder.Encode(data, 0, data.Length, bOut);
			}
			catch (IOException e)
			{
				throw new Exception("exception encoding URL safe base64 string: " + e.Message, e);
			}

			return bOut.ToArray();
		}

		/**
		* Encode the byte data writing it to the given output stream.
		*
		* @return the number of bytes produced.
		*/
		public static int Encode(
			byte[]	data,
			Stream	outStr)
		{
			return encoder.Encode(data, 0, data.Length, outStr);
		}

		/**
		* Decode the URL safe base 64 encoded input data - white space will be ignored.
		*
		* @return a byte array representing the decoded data.
		*/
		public static byte[] Decode(
			byte[] data)
		{
			MemoryStream bOut = new MemoryStream();

			try
			{
				encoder.Decode(data, 0, data.Length, bOut);
			}
			catch (IOException e)
			{
				throw new Exception("exception decoding URL safe base64 string: " + e.Message, e);
			}

			return bOut.ToArray();
		}

		/**
		* decode the URL safe base 64 encoded byte data writing it to the given output stream,
		* whitespace characters will be ignored.
		*
		* @return the number of bytes produced.
		*/
		public static int Decode(
			byte[]	data,
			Stream	outStr)
		{
			return encoder.Decode(data, 0, data.Length, outStr);
		}

		/**
		* decode the URL safe base 64 encoded string data - whitespace will be ignored.
		*
		* @return a byte array representing the decoded data.
		*/
		public static byte[] Decode(
			string data)
		{
			MemoryStream bOut = new MemoryStream();

			try
			{
				encoder.DecodeString(data, bOut);
			}
			catch (IOException e)
			{
				throw new Exception("exception decoding URL safe base64 string: " + e.Message, e);
			}
	        
			return bOut.ToArray();
		}
	    
		/**
		* Decode the URL safe base 64 encoded string data writing it to the given output stream,
		* whitespace characters will be ignored.
		*
		* @return the number of bytes produced.
		*/
		public static int Decode(
			string	data,
			Stream	outStr)
		{
			return encoder.DecodeString(data, outStr);
		}
	}
}
