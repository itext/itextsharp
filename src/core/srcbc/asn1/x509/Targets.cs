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

namespace Org.BouncyCastle.Asn1.X509
{
	/**
	 * Targets structure used in target information extension for attribute
	 * certificates from RFC 3281.
	 * 
	 * <pre>
	 *            Targets ::= SEQUENCE OF Target
	 *           
	 *            Target  ::= CHOICE {
	 *              targetName          [0] GeneralName,
	 *              targetGroup         [1] GeneralName,
	 *              targetCert          [2] TargetCert
	 *            }
	 *           
	 *            TargetCert  ::= SEQUENCE {
	 *              targetCertificate    IssuerSerial,
	 *              targetName           GeneralName OPTIONAL,
	 *              certDigestInfo       ObjectDigestInfo OPTIONAL
	 *            }
	 * </pre>
	 * 
	 * @see org.bouncycastle.asn1.x509.Target
	 * @see org.bouncycastle.asn1.x509.TargetInformation
	 */
	public class Targets
		: Asn1Encodable
	{
		private readonly Asn1Sequence targets;

		/**
		 * Creates an instance of a Targets from the given object.
		 * <p>
		 * <code>obj</code> can be a Targets or a {@link Asn1Sequence}</p>
		 * 
		 * @param obj The object.
		 * @return A Targets instance.
		 * @throws ArgumentException if the given object cannot be interpreted as Target.
		 */
		public static Targets GetInstance(
			object obj)
		{
			if (obj is Targets)
			{
				return (Targets) obj;
			}

			if (obj is Asn1Sequence)
			{
				return new Targets((Asn1Sequence) obj);
			}

			throw new ArgumentException("unknown object in factory: " + obj.GetType().Name, "obj");
		}

		/**
		 * Constructor from Asn1Sequence.
		 * 
		 * @param targets The ASN.1 SEQUENCE.
		 * @throws ArgumentException if the contents of the sequence are
		 *             invalid.
		 */
		private Targets(
			Asn1Sequence targets)
		{
			this.targets = targets;
		}

		/**
		 * Constructor from given targets.
		 * <p>
		 * The ArrayList is copied.</p>
		 * 
		 * @param targets An <code>ArrayList</code> of {@link Target}s.
		 * @see Target
		 * @throws ArgumentException if the ArrayList contains not only Targets.
		 */
		public Targets(
			Target[] targets)
		{
			this.targets = new DerSequence(targets);
		}

		/**
		 * Returns the targets in an <code>ArrayList</code>.
		 * <p>
		 * The ArrayList is cloned before it is returned.</p>
		 * 
		 * @return Returns the targets.
		 */
		public virtual Target[] GetTargets()
		{
			Target[] result = new Target[targets.Count];

			for (int i = 0; i < targets.Count; ++i)
			{
				result[i] = Target.GetInstance(targets[i]);
			}

			return result;
		}

		/**
		 * Produce an object suitable for an Asn1OutputStream.
		 * 
		 * Returns:
		 * 
		 * <pre>
		 *            Targets ::= SEQUENCE OF Target
		 * </pre>
		 * 
		 * @return an Asn1Object
		 */
		public override Asn1Object ToAsn1Object()
		{
			return targets;
		}
	}
}
