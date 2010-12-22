using System;
using System.Collections;
using System.IO;

namespace Org.BouncyCastle.Crypto.Tls
{
	internal interface TlsClient
	{
		/// <summary>
		/// Called at the start of a new TLS session, before any other methods.
		/// </summary>
		/// <param name="handler">
		/// A <see cref="TlsProtocolHandler"/>
		/// </param>
		void Init(TlsProtocolHandler handler);

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
		IDictionary GenerateClientExtensions();

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
		void NotifySecureRenegotiation(bool secureRenegotiation);

		/// <summary>
		/// Report the extensions from an extended server hello.
		/// </summary>
		/// <remarks>
		/// Will only be called if we returned a non-null result from <see cref="GenerateClientExtensions"/>.
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
		TlsKeyExchange CreateKeyExchange();

		/// <summary>
		/// If the server requests a certificate, this is called to report the details of the request.
		/// </summary>
		/// <param name="certificateTypes">
        /// An array of <see cref="ClientCertificateType"/>, each specifying a supported client certificate type
		/// </param>
		/// <param name="certificateAuthorities">
		/// A <see cref="IList"/> (X509Name)
		/// </param>
		void ProcessServerCertificateRequest(ClientCertificateType[] certificateTypes, IList certificateAuthorities);

		/// <summary>
		/// Return the certificate (chain) to use for client authentication, if any.
		/// </summary>
		/// <returns>
		/// A <see cref="Certificate"/>
		/// </returns>
		Certificate GetCertificate();

		/// <summary>
		/// Generate a raw signature using the appropriate hash data and the client certificate's
		/// corresponding private key.
		/// </summary>
		/// <param name="md5andsha1">
		/// An array containing an MD5 hash, followed by a SHA-1 hash, of the handshake data.
		/// </param>
		/// <returns>
		/// The signature bytes.
		/// </returns>
		/// <exception cref="IOException"/>
		byte[] GenerateCertificateSignature(byte[] md5andsha1);

		/// <summary>
		/// Return an implementation of <see cref="TlsCipher"/> to use for encryption/decryption.
		/// </summary>
		/// <param name="securityParameters">
		/// A <see cref="SecurityParameters"/>
		/// </param>
		/// <returns>
		/// A <see cref="TlsCipher"/>
		/// </returns>
		/// <exception cref="IOException"/>
		TlsCipher CreateCipher(SecurityParameters securityParameters);
	}
}
