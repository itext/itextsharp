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
using System.Text;

using Org.BouncyCastle.Asn1.IsisMtt;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X500;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;

namespace Org.BouncyCastle.Pkix
{
	/**
	* Implements the PKIX CertPathBuilding algorithm for BouncyCastle.
	*
	* @see CertPathBuilderSpi
	*/
	public class PkixCertPathBuilder
		//		: CertPathBuilderSpi
	{
		/**
		 * Build and validate a CertPath using the given parameter.
		 *
		 * @param params PKIXBuilderParameters object containing all information to
		 *            build the CertPath
		 */
		public virtual PkixCertPathBuilderResult Build(
			PkixBuilderParameters pkixParams)
		{
			// search target certificates

			IX509Selector certSelect = pkixParams.GetTargetCertConstraints();
			if (!(certSelect is X509CertStoreSelector))
			{
				throw new PkixCertPathBuilderException(
					"TargetConstraints must be an instance of "
					+ typeof(X509CertStoreSelector).FullName + " for "
					+ this.GetType() + " class.");
			}

			ISet targets = new HashSet();
			try
			{
				targets.AddAll(PkixCertPathValidatorUtilities.FindCertificates((X509CertStoreSelector)certSelect, pkixParams.GetStores()));
				// TODO Should this include an entry for pkixParams.GetAdditionalStores() too?
			}
			catch (Exception e)
			{
				throw new PkixCertPathBuilderException(
					"Error finding target certificate.", e);
			}

			if (targets.IsEmpty)
				throw new PkixCertPathBuilderException("No certificate found matching targetContraints.");

			PkixCertPathBuilderResult result = null;
			IList certPathList = Platform.CreateArrayList();

			// check all potential target certificates
			foreach (X509Certificate cert in targets)
			{
				result = Build(cert, pkixParams, certPathList);

				if (result != null)
					break;
			}

			if (result == null && certPathException != null)
			{
				throw new PkixCertPathBuilderException(certPathException.Message, certPathException.InnerException);
			}

			if (result == null && certPathException == null)
			{
				throw new PkixCertPathBuilderException("Unable to find certificate chain.");
			}

			return result;
		}

		private Exception certPathException;

		protected virtual PkixCertPathBuilderResult Build(
			X509Certificate			tbvCert,
			PkixBuilderParameters	pkixParams,
			IList					tbvPath)
		{
			// If tbvCert is readily present in tbvPath, it indicates having run
			// into a cycle in the PKI graph.
			if (tbvPath.Contains(tbvCert))
				return null;

			// step out, the certificate is not allowed to appear in a certification
			// chain.
			if (pkixParams.GetExcludedCerts().Contains(tbvCert))
				return null;

			// test if certificate path exceeds maximum length
			if (pkixParams.MaxPathLength != -1)
			{
				if (tbvPath.Count - 1 > pkixParams.MaxPathLength)
					return null;
			}

			tbvPath.Add(tbvCert);

//			X509CertificateParser certParser = new X509CertificateParser();
			PkixCertPathBuilderResult builderResult = null;
			PkixCertPathValidator validator = new PkixCertPathValidator();

			try
			{
				// check whether the issuer of <tbvCert> is a TrustAnchor
				if (PkixCertPathValidatorUtilities.FindTrustAnchor(tbvCert, pkixParams.GetTrustAnchors()) != null)
				{
					// exception message from possibly later tried certification
					// chains
					PkixCertPath certPath = null;
					try
					{
						certPath = new PkixCertPath(tbvPath);
					}
					catch (Exception e)
					{
						throw new Exception(
							"Certification path could not be constructed from certificate list.",
							e);
					}

					PkixCertPathValidatorResult result = null;
					try
					{
						result = (PkixCertPathValidatorResult)validator.Validate(
							certPath, pkixParams);
					}
					catch (Exception e)
					{
						throw new Exception(
							"Certification path could not be validated.", e);
					}

					return new PkixCertPathBuilderResult(certPath, result.TrustAnchor,
						result.PolicyTree, result.SubjectPublicKey);
				}
				else
				{
					// add additional X.509 stores from locations in certificate
					try
					{
						PkixCertPathValidatorUtilities.AddAdditionalStoresFromAltNames(
							tbvCert, pkixParams);
					}
					catch (CertificateParsingException e)
					{
						throw new Exception(
							"No additiontal X.509 stores can be added from certificate locations.",
							e);
					}

					// try to get the issuer certificate from one of the stores
					HashSet issuers = new HashSet();
					try
					{
						issuers.AddAll(PkixCertPathValidatorUtilities.FindIssuerCerts(tbvCert, pkixParams));
					}
					catch (Exception e)
					{
						throw new Exception(
							"Cannot find issuer certificate for certificate in certification path.",
							e);
					}

					if (issuers.IsEmpty)
						throw new Exception("No issuer certificate for certificate in certification path found.");

					foreach (X509Certificate issuer in issuers)
					{
						builderResult = Build(issuer, pkixParams, tbvPath);

						if (builderResult != null)
							break;
					}
				}
			}
			catch (Exception e)
			{
				certPathException = e;
			}

			if (builderResult == null)
			{
				tbvPath.Remove(tbvCert);
			}

			return builderResult;
		}
	}
}
