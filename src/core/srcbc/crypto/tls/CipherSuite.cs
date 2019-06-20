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
	/// RFC 2246 A.5
	/// </summary>
	public enum CipherSuite : int
	{
		TLS_NULL_WITH_NULL_NULL = 0x0000,
		TLS_RSA_WITH_NULL_MD5 = 0x0001,
		TLS_RSA_WITH_NULL_SHA = 0x0002,
		TLS_RSA_EXPORT_WITH_RC4_40_MD5 = 0x0003,
		TLS_RSA_WITH_RC4_128_MD5 = 0x0004,
		TLS_RSA_WITH_RC4_128_SHA = 0x0005,
		TLS_RSA_EXPORT_WITH_RC2_CBC_40_MD5 = 0x0006,
		TLS_RSA_WITH_IDEA_CBC_SHA = 0x0007,
		TLS_RSA_EXPORT_WITH_DES40_CBC_SHA = 0x0008,
		TLS_RSA_WITH_DES_CBC_SHA = 0x0009,
		TLS_RSA_WITH_3DES_EDE_CBC_SHA = 0x000A,
		TLS_DH_DSS_EXPORT_WITH_DES40_CBC_SHA = 0x000B,
		TLS_DH_DSS_WITH_DES_CBC_SHA = 0x000C,
		TLS_DH_DSS_WITH_3DES_EDE_CBC_SHA = 0x000D,
		TLS_DH_RSA_EXPORT_WITH_DES40_CBC_SHA = 0x000E,
		TLS_DH_RSA_WITH_DES_CBC_SHA = 0x000F,
		TLS_DH_RSA_WITH_3DES_EDE_CBC_SHA = 0x0010,
		TLS_DHE_DSS_EXPORT_WITH_DES40_CBC_SHA = 0x0011,
		TLS_DHE_DSS_WITH_DES_CBC_SHA = 0x0012,
		TLS_DHE_DSS_WITH_3DES_EDE_CBC_SHA = 0x0013,
		TLS_DHE_RSA_EXPORT_WITH_DES40_CBC_SHA = 0x0014,
		TLS_DHE_RSA_WITH_DES_CBC_SHA = 0x0015,
		TLS_DHE_RSA_WITH_3DES_EDE_CBC_SHA = 0x0016,
		TLS_DH_anon_EXPORT_WITH_RC4_40_MD5 = 0x0017,
		TLS_DH_anon_WITH_RC4_128_MD5 = 0x0018,
		TLS_DH_anon_EXPORT_WITH_DES40_CBC_SHA = 0x0019,
		TLS_DH_anon_WITH_DES_CBC_SHA = 0x001A,
		TLS_DH_anon_WITH_3DES_EDE_CBC_SHA = 0x001B,

		/*
		 * RFC 3268
		 */
		TLS_RSA_WITH_AES_128_CBC_SHA = 0x002F,
		TLS_DH_DSS_WITH_AES_128_CBC_SHA = 0x0030,
		TLS_DH_RSA_WITH_AES_128_CBC_SHA = 0x0031,
		TLS_DHE_DSS_WITH_AES_128_CBC_SHA = 0x0032,
		TLS_DHE_RSA_WITH_AES_128_CBC_SHA = 0x0033,
		TLS_DH_anon_WITH_AES_128_CBC_SHA = 0x0034,
		TLS_RSA_WITH_AES_256_CBC_SHA = 0x0035,
		TLS_DH_DSS_WITH_AES_256_CBC_SHA = 0x0036,
		TLS_DH_RSA_WITH_AES_256_CBC_SHA = 0x0037,
		TLS_DHE_DSS_WITH_AES_256_CBC_SHA = 0x0038,
		TLS_DHE_RSA_WITH_AES_256_CBC_SHA = 0x0039,
		TLS_DH_anon_WITH_AES_256_CBC_SHA = 0x003A,

		/*
		 * RFC 4279
		 */
		TLS_PSK_WITH_RC4_128_SHA = 0x008A,
		TLS_PSK_WITH_3DES_EDE_CBC_SHA = 0x008B,
		TLS_PSK_WITH_AES_128_CBC_SHA = 0x008C,
		TLS_PSK_WITH_AES_256_CBC_SHA = 0x008D,
		TLS_DHE_PSK_WITH_RC4_128_SHA = 0x008E,
		TLS_DHE_PSK_WITH_3DES_EDE_CBC_SHA = 0x008F,
		TLS_DHE_PSK_WITH_AES_128_CBC_SHA = 0x0090,
		TLS_DHE_PSK_WITH_AES_256_CBC_SHA = 0x0091,
		TLS_RSA_PSK_WITH_RC4_128_SHA = 0x0092,
		TLS_RSA_PSK_WITH_3DES_EDE_CBC_SHA = 0x0093,
		TLS_RSA_PSK_WITH_AES_128_CBC_SHA = 0x0094,
		TLS_RSA_PSK_WITH_AES_256_CBC_SHA = 0x0095,

		/*
		 * RFC 4492
		 */
		TLS_ECDH_ECDSA_WITH_NULL_SHA = 0xC001,
		TLS_ECDH_ECDSA_WITH_RC4_128_SHA = 0xC002,
		TLS_ECDH_ECDSA_WITH_3DES_EDE_CBC_SHA = 0xC003,
		TLS_ECDH_ECDSA_WITH_AES_128_CBC_SHA = 0xC004,
		TLS_ECDH_ECDSA_WITH_AES_256_CBC_SHA = 0xC005,
		TLS_ECDHE_ECDSA_WITH_NULL_SHA = 0xC006,
		TLS_ECDHE_ECDSA_WITH_RC4_128_SHA = 0xC007,
		TLS_ECDHE_ECDSA_WITH_3DES_EDE_CBC_SHA = 0xC008,
		TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA = 0xC009,
		TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA = 0xC00A,
		TLS_ECDH_RSA_WITH_NULL_SHA = 0xC00B,
		TLS_ECDH_RSA_WITH_RC4_128_SHA = 0xC00C,
		TLS_ECDH_RSA_WITH_3DES_EDE_CBC_SHA = 0xC00D,
		TLS_ECDH_RSA_WITH_AES_128_CBC_SHA = 0xC00E,
		TLS_ECDH_RSA_WITH_AES_256_CBC_SHA = 0xC00F,
		TLS_ECDHE_RSA_WITH_NULL_SHA = 0xC010,
		TLS_ECDHE_RSA_WITH_RC4_128_SHA = 0xC011,
		TLS_ECDHE_RSA_WITH_3DES_EDE_CBC_SHA = 0xC012,
		TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA = 0xC013,
		TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA = 0xC014,
		TLS_ECDH_anon_WITH_NULL_SHA = 0xC015,
		TLS_ECDH_anon_WITH_RC4_128_SHA = 0xC016,
		TLS_ECDH_anon_WITH_3DES_EDE_CBC_SHA = 0xC017,
		TLS_ECDH_anon_WITH_AES_128_CBC_SHA = 0xC018,
		TLS_ECDH_anon_WITH_AES_256_CBC_SHA = 0xC019,

		/*
		 * RFC 5054
		 */
		TLS_SRP_SHA_WITH_3DES_EDE_CBC_SHA = 0xC01A,
		TLS_SRP_SHA_RSA_WITH_3DES_EDE_CBC_SHA = 0xC01B,
		TLS_SRP_SHA_DSS_WITH_3DES_EDE_CBC_SHA = 0xC01C,
		TLS_SRP_SHA_WITH_AES_128_CBC_SHA = 0xC01D,
		TLS_SRP_SHA_RSA_WITH_AES_128_CBC_SHA = 0xC01E,
		TLS_SRP_SHA_DSS_WITH_AES_128_CBC_SHA = 0xC01F,
		TLS_SRP_SHA_WITH_AES_256_CBC_SHA = 0xC020,
		TLS_SRP_SHA_RSA_WITH_AES_256_CBC_SHA = 0xC021,
		TLS_SRP_SHA_DSS_WITH_AES_256_CBC_SHA = 0xC022,

		/*
		 * RFC 5289
		 */
		TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256 = 0xC023,
		TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA384 = 0xC024,
		TLS_ECDH_ECDSA_WITH_AES_128_CBC_SHA256 = 0xC025,
		TLS_ECDH_ECDSA_WITH_AES_256_CBC_SHA384 = 0xC026,
		TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256 = 0xC027,
		TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA384 = 0xC028,
		TLS_ECDH_RSA_WITH_AES_128_CBC_SHA256 = 0xC029,
		TLS_ECDH_RSA_WITH_AES_256_CBC_SHA384 = 0xC02A,
		TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256 = 0xC02B,
		TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384 = 0xC02C,
		TLS_ECDH_ECDSA_WITH_AES_128_GCM_SHA256 = 0xC02D,
		TLS_ECDH_ECDSA_WITH_AES_256_GCM_SHA384 = 0xC02E,
		TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256 = 0xC02F,
		TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384 = 0xC030,
		TLS_ECDH_RSA_WITH_AES_128_GCM_SHA256 = 0xC031,
		TLS_ECDH_RSA_WITH_AES_256_GCM_SHA384 = 0xC032,

		/*
		 * RFC 5746
		 */
		TLS_EMPTY_RENEGOTIATION_INFO_SCSV = 0x00FF,
	}
}
