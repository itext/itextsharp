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
using Org.BouncyCastle.X509.Store;

namespace Org.BouncyCastle.Pkix
{
	public class PkixCrlUtilities
	{
		public virtual ISet FindCrls(X509CrlStoreSelector crlselect, PkixParameters paramsPkix, DateTime currentDate)
		{
			ISet initialSet = new HashSet();

			// get complete CRL(s)
			try
			{
				initialSet.AddAll(FindCrls(crlselect, paramsPkix.GetAdditionalStores()));
				initialSet.AddAll(FindCrls(crlselect, paramsPkix.GetStores()));
			}
			catch (Exception e)
			{
				throw new Exception("Exception obtaining complete CRLs.", e);
			}

			ISet finalSet = new HashSet();
			DateTime validityDate = currentDate;

			if (paramsPkix.Date != null)
			{
				validityDate = paramsPkix.Date.Value;
			}

			// based on RFC 5280 6.3.3
			foreach (X509Crl crl in initialSet)
			{
				if (crl.NextUpdate.Value.CompareTo(validityDate) > 0)
				{
					X509Certificate cert = crlselect.CertificateChecking;

					if (cert != null)
					{
						if (crl.ThisUpdate.CompareTo(cert.NotAfter) < 0)
						{
							finalSet.Add(crl);
						}
					}
					else
					{
						finalSet.Add(crl);
					}
				}
			}

			return finalSet;
		}

		public virtual ISet FindCrls(X509CrlStoreSelector crlselect, PkixParameters paramsPkix)
		{
			ISet completeSet = new HashSet();

			// get complete CRL(s)
			try
			{
				completeSet.AddAll(FindCrls(crlselect, paramsPkix.GetStores()));
			}
			catch (Exception e)
			{
				throw new Exception("Exception obtaining complete CRLs.", e);
			}

			return completeSet;
		}

		/// <summary>
		/// crl checking
		/// Return a Collection of all CRLs found in the X509Store's that are
		/// matching the crlSelect criteriums.
		/// </summary>
		/// <param name="crlSelect">a {@link X509CRLStoreSelector} object that will be used
		/// to select the CRLs</param>
		/// <param name="crlStores">a List containing only {@link org.bouncycastle.x509.X509Store
		/// X509Store} objects. These are used to search for CRLs</param>
		/// <returns>a Collection of all found {@link X509CRL X509CRL} objects. May be
		/// empty but never <code>null</code>.
		/// </returns>
		private ICollection FindCrls(X509CrlStoreSelector crlSelect, IList crlStores)
		{
			ISet crls = new HashSet();

			Exception lastException = null;
			bool foundValidStore = false;

			foreach (IX509Store store in crlStores)
			{
				try
				{
					crls.AddAll(store.GetMatches(crlSelect));
					foundValidStore = true;
				}
				catch (X509StoreException e)
				{
					lastException = new Exception("Exception searching in X.509 CRL store.", e);
				}
			}

	        if (!foundValidStore && lastException != null)
	            throw lastException;

			return crls;
		}
	}
}
