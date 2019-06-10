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
	 * Target information extension for attributes certificates according to RFC
	 * 3281.
	 * 
	 * <pre>
	 *           SEQUENCE OF Targets
	 * </pre>
	 * 
	 */
	public class TargetInformation
		: Asn1Encodable
	{
		private readonly Asn1Sequence targets;

		/**
		 * Creates an instance of a TargetInformation from the given object.
		 * <p>
		 * <code>obj</code> can be a TargetInformation or a {@link Asn1Sequence}</p>
		 * 
		 * @param obj The object.
		 * @return A TargetInformation instance.
		 * @throws ArgumentException if the given object cannot be interpreted as TargetInformation.
		 */
		public static TargetInformation GetInstance(
			object obj)
		{
			if (obj is TargetInformation)
			{
				return (TargetInformation) obj;
			}

			if (obj is Asn1Sequence)
			{
				return new TargetInformation((Asn1Sequence) obj);
			}

			throw new ArgumentException("unknown object in factory: " + obj.GetType().Name, "obj");
		}

		/**
		 * Constructor from a Asn1Sequence.
		 * 
		 * @param seq The Asn1Sequence.
		 * @throws ArgumentException if the sequence does not contain
		 *             correctly encoded Targets elements.
		 */
		private TargetInformation(
			Asn1Sequence targets)
		{
			this.targets = targets;
		}

		/**
		 * Returns the targets in this target information extension.
		 * <p>
		 * The ArrayList is cloned before it is returned.</p>
		 * 
		 * @return Returns the targets.
		 */
		public virtual Targets[] GetTargetsObjects()
		{
			Targets[] result = new Targets[targets.Count];

			for (int i = 0; i < targets.Count; ++i)
			{
				result[i] = Targets.GetInstance(targets[i]);
			}

			return result;
		}

		/**
		 * Constructs a target information from a single targets element. 
		 * According to RFC 3281 only one targets element must be produced.
		 * 
		 * @param targets A Targets instance.
		 */
		public TargetInformation(
			Targets targets)
		{
			this.targets = new DerSequence(targets);
		}

		/**
		 * According to RFC 3281 only one targets element must be produced. If
		 * multiple targets are given they must be merged in
		 * into one targets element.
		 *
		 * @param targets An array with {@link Targets}.
		 */
		public TargetInformation(
			Target[] targets)
			: this(new Targets(targets))
		{
		}

		/**
		 * Produce an object suitable for an Asn1OutputStream.
		 * 
		 * Returns:
		 * 
		 * <pre>
		 *          SEQUENCE OF Targets
		 * </pre>
		 * 
		 * <p>
		 * According to RFC 3281 only one targets element must be produced. If
		 * multiple targets are given in the constructor they are merged into one
		 * targets element. If this was produced from a
		 * {@link Org.BouncyCastle.Asn1.Asn1Sequence} the encoding is kept.</p>
		 * 
		 * @return an Asn1Object
		 */
		public override Asn1Object ToAsn1Object()
		{
			return targets;
		}
	}
}
