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
using System.Text;

using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509.Store;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;

namespace Org.BouncyCastle.Pkix
{
	/// <summary>
    /// Summary description for PkixBuilderParameters.
	/// </summary>
	public class PkixBuilderParameters
		: PkixParameters
	{
		private int maxPathLength = 5;

		private ISet excludedCerts = new HashSet();

		/**
		* Returns an instance of <code>PkixBuilderParameters</code>.
		* <p>
		* This method can be used to get a copy from other
		* <code>PKIXBuilderParameters</code>, <code>PKIXParameters</code>,
		* and <code>ExtendedPKIXParameters</code> instances.
		* </p>
		*
		* @param pkixParams The PKIX parameters to create a copy of.
		* @return An <code>PkixBuilderParameters</code> instance.
		*/
		public static PkixBuilderParameters GetInstance(
			PkixParameters pkixParams)
		{
			PkixBuilderParameters parameters = new PkixBuilderParameters(
				pkixParams.GetTrustAnchors(),
				new X509CertStoreSelector(pkixParams.GetTargetCertConstraints()));
			parameters.SetParams(pkixParams);
			return parameters;
		}

		public PkixBuilderParameters(
			ISet			trustAnchors,
			IX509Selector	targetConstraints)
			: base(trustAnchors)
		{
			SetTargetCertConstraints(targetConstraints);
		}

		public virtual int MaxPathLength
		{
			get { return maxPathLength; }
			set
			{
				if (value < -1)
				{
					throw new InvalidParameterException(
						"The maximum path length parameter can not be less than -1.");
				}
				this.maxPathLength = value;
			}
		}

		/// <summary>
		/// Excluded certificates are not used for building a certification path.
		/// </summary>
		/// <returns>the excluded certificates.</returns>
		public virtual ISet GetExcludedCerts()
		{
			return new HashSet(excludedCerts);
		}

		/// <summary>
		/// Sets the excluded certificates which are not used for building a
		/// certification path. If the <code>ISet</code> is <code>null</code> an
		/// empty set is assumed.
		/// </summary>
		/// <remarks>
		/// The given set is cloned to protect it against subsequent modifications.
		/// </remarks>
		/// <param name="excludedCerts">The excluded certificates to set.</param>
		public virtual void SetExcludedCerts(
			ISet excludedCerts)
		{
			if (excludedCerts == null)
			{
				excludedCerts = new HashSet();
			}
			else
			{
				this.excludedCerts = new HashSet(excludedCerts);
			}
		}

		/**
		* Can alse handle <code>ExtendedPKIXBuilderParameters</code> and
		* <code>PKIXBuilderParameters</code>.
		* 
		* @param params Parameters to set.
		* @see org.bouncycastle.x509.ExtendedPKIXParameters#setParams(java.security.cert.PKIXParameters)
		*/
		protected override void SetParams(
			PkixParameters parameters)
		{
			base.SetParams(parameters);
			if (parameters is PkixBuilderParameters)
			{
				PkixBuilderParameters _params = (PkixBuilderParameters) parameters;
				maxPathLength = _params.maxPathLength;
				excludedCerts = new HashSet(_params.excludedCerts);
			}
		}

		/**
		* Makes a copy of this <code>PKIXParameters</code> object. Changes to the
		* copy will not affect the original and vice versa.
		*
		* @return a copy of this <code>PKIXParameters</code> object
		*/
		public override object Clone()
		{
			PkixBuilderParameters parameters = new PkixBuilderParameters(
				GetTrustAnchors(), GetTargetCertConstraints());
			parameters.SetParams(this);
			return parameters;
		}

		public override string ToString()
		{
			string nl = Platform.NewLine;
			StringBuilder s = new StringBuilder();
			s.Append("PkixBuilderParameters [" + nl);
			s.Append(base.ToString());
			s.Append("  Maximum Path Length: ");
			s.Append(MaxPathLength);
			s.Append(nl + "]" + nl);
			return s.ToString();
		}
	}
}
