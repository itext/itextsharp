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
namespace Org.BouncyCastle.Crypto.Tls
{
	/// <summary>
	/// RFC 2246 7.2
	/// </summary>
	public enum AlertDescription : byte
	{
		close_notify = 0,
		unexpected_message = 10,
		bad_record_mac = 20,
		decryption_failed = 21,
		record_overflow = 22,
		decompression_failure = 30,
		handshake_failure = 40,
		/* 41 is not defined, for historical reasons */
		bad_certificate = 42,
		unsupported_certificate = 43,
		certificate_revoked = 44,
		certificate_expired = 45,
		certificate_unknown = 46,
		illegal_parameter = 47,
		unknown_ca = 48,
		access_denied = 49,
		decode_error = 50,
		decrypt_error = 51,
		export_restriction = 60,
		protocol_version = 70,
		insufficient_security = 71,
		internal_error = 80,
		user_canceled = 90,
		no_renegotiation = 100,

		/*
		 *  RFC 3546
		 */
		unsupported_extension = 110,
		certificate_unobtainable = 111,
		unrecognized_name = 112,
		bad_certificate_status_response = 113,
		bad_certificate_hash_value = 114,

		/*
		 *  RFC 4279
		 */
		unknown_psk_identity = 115,
	}
}
