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
using System.Collections;
using System.IO;

namespace Org.BouncyCastle.Crypto.Tls
{
	public interface TlsClient
	{
		/// <summary>
		/// Called at the start of a new TLS session, before any other methods.
		/// </summary>
		/// <param name="context">
		/// A <see cref="TlsProtocolHandler"/>
		/// </param>
		void Init(TlsClientContext context);

		/// <summary>
		/// Get the list of cipher suites that this client supports.
		/// </summary>
		/// <returns>
        /// An array of <see cref="CipherSuite"/>, each specifying a supported cipher suite.
		/// </returns>
		CipherSuite[] GetCipherSuites();

        /// <summary>
        /// Get the list of compression methods that this client supports.
        /// </summary>
        /// <returns>
        /// An array of <see cref="CompressionMethod"/>, each specifying a supported compression method.
        /// </returns>
        CompressionMethod[] GetCompressionMethods();

		/// <summary>
		/// Get the (optional) table of client extensions to be included in (extended) client hello.
		/// </summary>
		/// <returns>
        /// A <see cref="IDictionary"/> (<see cref="ExtensionType"/> -> byte[]). May be null.
		/// </returns>
		/// <exception cref="IOException"></exception>
		IDictionary GetClientExtensions();

		/// <summary>
		/// Reports the session ID once it has been determined.
		/// </summary>
		/// <param name="sessionID">
		/// A <see cref="System.Byte"/>
		/// </param>
		void NotifySessionID(byte[] sessionID);

		/// <summary>
		/// Report the cipher suite that was selected by the server.
		/// </summary>
		/// <remarks>
		/// The protocol handler validates this value against the offered cipher suites
		/// <seealso cref="GetCipherSuites"/>
		/// </remarks>
		/// <param name="selectedCipherSuite">
		/// A <see cref="CipherSuite"/>
		/// </param>
		void NotifySelectedCipherSuite(CipherSuite selectedCipherSuite);

        /// <summary>
        /// Report the compression method that was selected by the server.
        /// </summary>
        /// <remarks>
        /// The protocol handler validates this value against the offered compression methods
        /// <seealso cref="GetCompressionMethods"/>
        /// </remarks>
        /// <param name="selectedCompressionMethod">
        /// A <see cref="CompressionMethod"/>
        /// </param>
        void NotifySelectedCompressionMethod(CompressionMethod selectedCompressionMethod);

		/// <summary>
		/// Report whether the server supports secure renegotiation
		/// </summary>
		/// <remarks>
		/// The protocol handler automatically processes the relevant extensions
		/// </remarks>
		/// <param name="secureRenegotiation">
		/// A <see cref="System.Boolean"/>, true if the server supports secure renegotiation
		/// </param>
		/// <exception cref="IOException"></exception>
		void NotifySecureRenegotiation(bool secureRenegotiation);

		/// <summary>
		/// Report the extensions from an extended server hello.
		/// </summary>
		/// <remarks>
		/// Will only be called if we returned a non-null result from <see cref="GetClientExtensions"/>.
		/// </remarks>
		/// <param name="serverExtensions">
        /// A <see cref="IDictionary"/>  (<see cref="ExtensionType"/> -> byte[])
		/// </param>
		void ProcessServerExtensions(IDictionary serverExtensions);

		/// <summary>
		/// Return an implementation of <see cref="TlsKeyExchange"/> to negotiate the key exchange
		/// part of the protocol.
		/// </summary>
		/// <returns>
		/// A <see cref="TlsKeyExchange"/>
		/// </returns>
		/// <exception cref="IOException"/>
		TlsKeyExchange GetKeyExchange();

		/// <summary>
		/// Return an implementation of <see cref="TlsAuthentication"/> to handle authentication
		/// part of the protocol.
		/// </summary>
		/// <exception cref="IOException"/>
		TlsAuthentication GetAuthentication();

		/// <summary>
		/// Return an implementation of <see cref="TlsCompression"/> to handle record compression.
		/// </summary>
		/// <exception cref="IOException"/>
		TlsCompression GetCompression();

		/// <summary>
		/// Return an implementation of <see cref="TlsCipher"/> to use for encryption/decryption.
		/// </summary>
		/// <returns>
		/// A <see cref="TlsCipher"/>
		/// </returns>
		/// <exception cref="IOException"/>
		TlsCipher GetCipher();
	}
}
