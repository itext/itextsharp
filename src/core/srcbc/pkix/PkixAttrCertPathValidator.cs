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

using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;

namespace Org.BouncyCastle.Pkix
{
	/**
	* CertPathValidatorSpi implementation for X.509 Attribute Certificates la RFC 3281.
	* 
	* @see org.bouncycastle.x509.ExtendedPkixParameters
	*/
	public class PkixAttrCertPathValidator
	//    extends CertPathValidatorSpi
	{
		/**
		* Validates an attribute certificate with the given certificate path.
		* 
		* <p>
		* <code>params</code> must be an instance of
		* <code>ExtendedPkixParameters</code>.
		* </p><p>
		* The target constraints in the <code>params</code> must be an
		* <code>X509AttrCertStoreSelector</code> with at least the attribute
		* certificate criterion set. Obey that also target informations may be
		* necessary to correctly validate this attribute certificate.
		* </p><p>
		* The attribute certificate issuer must be added to the trusted attribute
		* issuers with {@link ExtendedPkixParameters#setTrustedACIssuers(Set)}.
		* </p>
		* @param certPath The certificate path which belongs to the attribute
		*            certificate issuer public key certificate.
		* @param params The PKIX parameters.
		* @return A <code>PKIXCertPathValidatorResult</code> of the result of
		*         validating the <code>certPath</code>.
		* @throws InvalidAlgorithmParameterException if <code>params</code> is
		*             inappropriate for this validator.
		* @throws CertPathValidatorException if the verification fails.
		*/
		public virtual PkixCertPathValidatorResult Validate(
			PkixCertPath	certPath,
			PkixParameters	pkixParams)
		{
			IX509Selector certSelect = pkixParams.GetTargetConstraints();
			if (!(certSelect is X509AttrCertStoreSelector))
			{
				throw new ArgumentException(
					"TargetConstraints must be an instance of " + typeof(X509AttrCertStoreSelector).FullName,
					"pkixParams");
			}
			IX509AttributeCertificate attrCert = ((X509AttrCertStoreSelector) certSelect).AttributeCert;

			PkixCertPath holderCertPath = Rfc3281CertPathUtilities.ProcessAttrCert1(attrCert, pkixParams);
			PkixCertPathValidatorResult result = Rfc3281CertPathUtilities.ProcessAttrCert2(certPath, pkixParams);
			X509Certificate issuerCert = (X509Certificate)certPath.Certificates[0];
			Rfc3281CertPathUtilities.ProcessAttrCert3(issuerCert, pkixParams);
			Rfc3281CertPathUtilities.ProcessAttrCert4(issuerCert, pkixParams);
			Rfc3281CertPathUtilities.ProcessAttrCert5(attrCert, pkixParams);
			// 6 already done in X509AttrCertStoreSelector
			Rfc3281CertPathUtilities.ProcessAttrCert7(attrCert, certPath, holderCertPath, pkixParams);
			Rfc3281CertPathUtilities.AdditionalChecks(attrCert, pkixParams);
			DateTime date;
			try
			{
				date = PkixCertPathValidatorUtilities.GetValidCertDateFromValidityModel(pkixParams, null, -1);
			}
			catch (Exception e)
			{
				throw new PkixCertPathValidatorException(
					"Could not get validity date from attribute certificate.", e);
			}
			Rfc3281CertPathUtilities.CheckCrls(attrCert, pkixParams, issuerCert, date, certPath.Certificates);
			return result;
		}
	}
}
