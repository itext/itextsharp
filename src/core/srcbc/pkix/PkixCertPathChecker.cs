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
    public abstract class PkixCertPathChecker
    {
        protected PkixCertPathChecker()
        {
        }

        /**
         * Initializes the internal state of this <code>PKIXCertPathChecker</code>.
         * <p>
         * The <code>forward</code> flag specifies the order that certificates
         * will be passed to the {@link #check check} method (forward or reverse). A
         * <code>PKIXCertPathChecker</code> <b>must</b> support reverse checking
         * and <b>may</b> support forward checking.
		 * </p>
         * 
         * @param forward
         *            the order that certificates are presented to the
         *            <code>check</code> method. If <code>true</code>,
         *            certificates are presented from target to most-trusted CA
         *            (forward); if <code>false</code>, from most-trusted CA to
         *            target (reverse).
         * @exception CertPathValidatorException
         *                if this <code>PKIXCertPathChecker</code> is unable to
         *                check certificates in the specified order; it should never
         *                be thrown if the forward flag is false since reverse
         *                checking must be supported
         */
        public abstract void Init(bool forward);
        //throws CertPathValidatorException;

        /**
         * Indicates if forward checking is supported. Forward checking refers to
         * the ability of the <code>PKIXCertPathChecker</code> to perform its
         * checks when certificates are presented to the <code>check</code> method
         * in the forward direction (from target to most-trusted CA).
         * 
         * @return <code>true</code> if forward checking is supported,
         *         <code>false</code> otherwise
         */
        public abstract bool IsForwardCheckingSupported();

        /**
         * Returns an immutable <code>Set</code> of X.509 certificate extensions
         * that this <code>PKIXCertPathChecker</code> supports (i.e. recognizes,
         * is able to process), or <code>null</code> if no extensions are
         * supported.
         * <p>
         * Each element of the set is a <code>String</code> representing the
         * Object Identifier (OID) of the X.509 extension that is supported. The OID
         * is represented by a set of nonnegative integers separated by periods.
         * </p><p>
         * All X.509 certificate extensions that a <code>PKIXCertPathChecker</code>
         * might possibly be able to process should be included in the set.
		 * </p>
         * 
         * @return an immutable <code>Set</code> of X.509 extension OIDs (in
         *         <code>String</code> format) supported by this
         *         <code>PKIXCertPathChecker</code>, or <code>null</code> if no
         *         extensions are supported
         */
        public abstract ISet GetSupportedExtensions();

        /**
         * Performs the check(s) on the specified certificate using its internal
         * state and removes any critical extensions that it processes from the
         * specified collection of OID strings that represent the unresolved
         * critical extensions. The certificates are presented in the order
         * specified by the <code>init</code> method.
         * 
         * @param cert
         *            the <code>Certificate</code> to be checked
         * @param unresolvedCritExts
         *            a <code>Collection</code> of OID strings representing the
         *            current set of unresolved critical extensions
         * @exception CertPathValidatorException
         *                if the specified certificate does not pass the check
         */
        public abstract void Check(X509Certificate cert, ICollection unresolvedCritExts);
        //throws CertPathValidatorException;

        /**
         * Returns a clone of this object. Calls the <code>Object.clone()</code>
         * method. All subclasses which maintain state must support and override
         * this method, if necessary.
         * 
         * @return a copy of this <code>PKIXCertPathChecker</code>
         */
        public virtual object Clone()
        {
			// TODO Check this
			return base.MemberwiseClone();
        }
    }
}
