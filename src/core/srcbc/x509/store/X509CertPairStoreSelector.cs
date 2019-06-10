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

namespace Org.BouncyCastle.X509.Store
{
	/// <remarks>
	/// This class is an <code>IX509Selector</code> implementation to select
	/// certificate pairs, which are e.g. used for cross certificates. The set of
	/// criteria is given from two <code>X509CertStoreSelector</code> objects,
	/// each of which, if present, must match the respective component of a pair.
	/// </remarks>
	public class X509CertPairStoreSelector
		: IX509Selector
	{
		private static X509CertStoreSelector CloneSelector(
			X509CertStoreSelector s)
		{
			return s == null ? null : (X509CertStoreSelector) s.Clone();
		}

		private X509CertificatePair certPair;
		private X509CertStoreSelector forwardSelector;
		private X509CertStoreSelector reverseSelector;

		public X509CertPairStoreSelector()
		{
		}

		private X509CertPairStoreSelector(
			X509CertPairStoreSelector o)
		{
			this.certPair = o.CertPair;
			this.forwardSelector = o.ForwardSelector;
			this.reverseSelector = o.ReverseSelector;
		}

		/// <summary>The certificate pair which is used for testing on equality.</summary>
		public X509CertificatePair CertPair
		{
			get { return certPair; }
			set { this.certPair = value; }
		}

		/// <summary>The certificate selector for the forward part.</summary>
		public X509CertStoreSelector ForwardSelector
		{
			get { return CloneSelector(forwardSelector); }
			set { this.forwardSelector = CloneSelector(value); }
		}

		/// <summary>The certificate selector for the reverse part.</summary>
		public X509CertStoreSelector ReverseSelector
		{
			get { return CloneSelector(reverseSelector); }
			set { this.reverseSelector = CloneSelector(value); }
		}

		/// <summary>
		/// Decides if the given certificate pair should be selected. If
		/// <c>obj</c> is not a <code>X509CertificatePair</code>, this method
		/// returns <code>false</code>.
		/// </summary>
		/// <param name="obj">The <code>X509CertificatePair</code> to be tested.</param>
		/// <returns><code>true</code> if the object matches this selector.</returns>
		public bool Match(
			object obj)
		{
			if (obj == null)
				throw new ArgumentNullException("obj");

			X509CertificatePair pair = obj as X509CertificatePair;

			if (pair == null)
				return false;

			if (certPair != null && !certPair.Equals(pair))
				return false;

			if (forwardSelector != null && !forwardSelector.Match(pair.Forward))
				return false;

			if (reverseSelector != null && !reverseSelector.Match(pair.Reverse))
				return false;

			return true;
		}

		public object Clone()
		{
			return new X509CertPairStoreSelector(this);
		}
	}
}
