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

using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Pkix
{
	public abstract class PkixAttrCertChecker
	{
		/**
		 * Returns an immutable <code>Set</code> of X.509 attribute certificate
		 * extensions that this <code>PkixAttrCertChecker</code> supports or
		 * <code>null</code> if no extensions are supported.
		 * <p>
		 * Each element of the set is a <code>String</code> representing the
		 * Object Identifier (OID) of the X.509 extension that is supported.
		 * </p>
		 * <p>
		 * All X.509 attribute certificate extensions that a
		 * <code>PkixAttrCertChecker</code> might possibly be able to process
		 * should be included in the set.
		 * </p>
		 * 
		 * @return an immutable <code>Set</code> of X.509 extension OIDs (in
		 *         <code>String</code> format) supported by this
		 *         <code>PkixAttrCertChecker</code>, or <code>null</code> if no
		 *         extensions are supported
		 */
		public abstract ISet GetSupportedExtensions();

		/**
		* Performs checks on the specified attribute certificate. Every handled
		* extension is rmeoved from the <code>unresolvedCritExts</code>
		* collection.
		* 
		* @param attrCert The attribute certificate to be checked.
		* @param certPath The certificate path which belongs to the attribute
		*            certificate issuer public key certificate.
		* @param holderCertPath The certificate path which belongs to the holder
		*            certificate.
		* @param unresolvedCritExts a <code>Collection</code> of OID strings
		*            representing the current set of unresolved critical extensions
		* @throws CertPathValidatorException if the specified attribute certificate
		*             does not pass the check.
		*/
		public abstract void Check(IX509AttributeCertificate attrCert, PkixCertPath certPath,
			PkixCertPath holderCertPath, ICollection unresolvedCritExts);

		/**
		* Returns a clone of this object.
		* 
		* @return a copy of this <code>PkixAttrCertChecker</code>
		*/
		public abstract PkixAttrCertChecker Clone();
	}
}
