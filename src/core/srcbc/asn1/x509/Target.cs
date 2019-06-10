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
	 * Target structure used in target information extension for attribute
	 * certificates from RFC 3281.
	 * 
	 * <pre>
	 *     Target  ::= CHOICE {
	 *       targetName          [0] GeneralName,
	 *       targetGroup         [1] GeneralName,
	 *       targetCert          [2] TargetCert
	 *     }
	 * </pre>
	 * 
	 * <p>
	 * The targetCert field is currently not supported and must not be used
	 * according to RFC 3281.</p>
	 */
	public class Target
		: Asn1Encodable, IAsn1Choice
	{
		public enum Choice
		{
			Name = 0,
			Group = 1
		};

		private readonly GeneralName targetName;
		private readonly GeneralName targetGroup;

		/**
		* Creates an instance of a Target from the given object.
		* <p>
		* <code>obj</code> can be a Target or a {@link Asn1TaggedObject}</p>
		* 
		* @param obj The object.
		* @return A Target instance.
		* @throws ArgumentException if the given object cannot be
		*             interpreted as Target.
		*/
		public static Target GetInstance(
			object obj)
		{
			if (obj is Target)
			{
				return (Target) obj;
			}

			if (obj is Asn1TaggedObject)
			{
				return new Target((Asn1TaggedObject) obj);
			}

			throw new ArgumentException("unknown object in factory: " + obj.GetType().Name, "obj");
		}

		/**
		 * Constructor from Asn1TaggedObject.
		 * 
		 * @param tagObj The tagged object.
		 * @throws ArgumentException if the encoding is wrong.
		 */
		private Target(
			Asn1TaggedObject tagObj)
		{
			switch ((Choice) tagObj.TagNo)
			{
				case Choice.Name:	// GeneralName is already a choice so explicit
					targetName = GeneralName.GetInstance(tagObj, true);
					break;
				case Choice.Group:
					targetGroup = GeneralName.GetInstance(tagObj, true);
					break;
				default:
					throw new ArgumentException("unknown tag: " + tagObj.TagNo);
			}
		}

		/**
		 * Constructor from given details.
		 * <p>
		 * Exactly one of the parameters must be not <code>null</code>.</p>
		 *
		 * @param type the choice type to apply to the name.
		 * @param name the general name.
		 * @throws ArgumentException if type is invalid.
		 */
		public Target(
			Choice		type,
			GeneralName	name)
			: this(new DerTaggedObject((int) type, name))
		{
		}

		/**
		 * @return Returns the targetGroup.
		 */
		public virtual GeneralName TargetGroup
		{
			get { return targetGroup; }
		}

		/**
		 * @return Returns the targetName.
		 */
		public virtual GeneralName TargetName
		{
			get { return targetName; }
		}

		/**
		 * Produce an object suitable for an Asn1OutputStream.
		 * 
		 * Returns:
		 * 
		 * <pre>
		 *     Target  ::= CHOICE {
		 *       targetName          [0] GeneralName,
		 *       targetGroup         [1] GeneralName,
		 *       targetCert          [2] TargetCert
		 *     }
		 * </pre>
		 * 
		 * @return an Asn1Object
		 */
		public override Asn1Object ToAsn1Object()
		{
			// GeneralName is a choice already so most be explicitly tagged
			if (targetName != null)
			{
				return new DerTaggedObject(true, 0, targetName);
			}

			return new DerTaggedObject(true, 1, targetGroup);
		}
	}
}
