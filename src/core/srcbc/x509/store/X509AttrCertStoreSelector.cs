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
using System.IO;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.Utilities.Date;
using Org.BouncyCastle.X509.Extension;

namespace Org.BouncyCastle.X509.Store
{
	/**
	* This class is an <code>Selector</code> like implementation to select
	* attribute certificates from a given set of criteria.
	*
	* @see org.bouncycastle.x509.X509AttributeCertificate
	* @see org.bouncycastle.x509.X509Store
	*/
	public class X509AttrCertStoreSelector
		: IX509Selector
	{
		// TODO: name constraints???

		private IX509AttributeCertificate attributeCert;
		private DateTimeObject attributeCertificateValid;
		private AttributeCertificateHolder holder;
		private AttributeCertificateIssuer issuer;
		private BigInteger serialNumber;
		private ISet targetNames = new HashSet();
		private ISet targetGroups = new HashSet();

		public X509AttrCertStoreSelector()
		{
		}

		private X509AttrCertStoreSelector(
			X509AttrCertStoreSelector o)
		{
			this.attributeCert = o.attributeCert;
			this.attributeCertificateValid = o.attributeCertificateValid;
			this.holder = o.holder;
			this.issuer = o.issuer;
			this.serialNumber = o.serialNumber;
			this.targetGroups = new HashSet(o.targetGroups);
			this.targetNames = new HashSet(o.targetNames);
		}

		/// <summary>
		/// Decides if the given attribute certificate should be selected.
		/// </summary>
		/// <param name="obj">The attribute certificate to be checked.</param>
		/// <returns><code>true</code> if the object matches this selector.</returns>
		public bool Match(
			object obj)
		{
			if (obj == null)
				throw new ArgumentNullException("obj");

			IX509AttributeCertificate attrCert = obj as IX509AttributeCertificate;

			if (attrCert == null)
				return false;

			if (this.attributeCert != null && !this.attributeCert.Equals(attrCert))
				return false;

			if (serialNumber != null && !attrCert.SerialNumber.Equals(serialNumber))
				return false;

			if (holder != null && !attrCert.Holder.Equals(holder))
				return false;

			if (issuer != null && !attrCert.Issuer.Equals(issuer))
				return false;

			if (attributeCertificateValid != null && !attrCert.IsValid(attributeCertificateValid.Value))
				return false;

			if (targetNames.Count > 0 || targetGroups.Count > 0)
			{
				Asn1OctetString targetInfoExt = attrCert.GetExtensionValue(
					X509Extensions.TargetInformation);

				if (targetInfoExt != null)
				{
					TargetInformation targetinfo;
					try
					{
						targetinfo = TargetInformation.GetInstance(
							X509ExtensionUtilities.FromExtensionValue(targetInfoExt));
					}
					catch (Exception)
					{
						return false;
					}

					Targets[] targetss = targetinfo.GetTargetsObjects();

					if (targetNames.Count > 0)
					{
						bool found = false;

						for (int i = 0; i < targetss.Length && !found; i++)
						{
							Target[] targets = targetss[i].GetTargets();

							for (int j = 0; j < targets.Length; j++)
							{
								GeneralName targetName = targets[j].TargetName;

								if (targetName != null && targetNames.Contains(targetName))
								{
									found = true;
									break;
								}
							}
						}
						if (!found)
						{
							return false;
						}
					}

					if (targetGroups.Count > 0)
					{
						bool found = false;

						for (int i = 0; i < targetss.Length && !found; i++)
						{
							Target[] targets = targetss[i].GetTargets();

							for (int j = 0; j < targets.Length; j++)
							{
								GeneralName targetGroup = targets[j].TargetGroup;

								if (targetGroup != null && targetGroups.Contains(targetGroup))
								{
									found = true;
									break;
								}
							}
						}

						if (!found)
						{
							return false;
						}
					}
				}
			}

			return true;
		}

		public object Clone()
		{
			return new X509AttrCertStoreSelector(this);
		}

		/// <summary>The attribute certificate which must be matched.</summary>
		/// <remarks>If <c>null</c> is given, any will do.</remarks>
		public IX509AttributeCertificate AttributeCert
		{
			get { return attributeCert; }
			set { this.attributeCert = value; }
		}

		[Obsolete("Use AttributeCertificateValid instead")]
		public DateTimeObject AttribueCertificateValid
		{
			get { return attributeCertificateValid; }
			set { this.attributeCertificateValid = value; }
		}

		/// <summary>The criteria for validity</summary>
		/// <remarks>If <c>null</c> is given any will do.</remarks>
		public DateTimeObject AttributeCertificateValid
		{
			get { return attributeCertificateValid; }
			set { this.attributeCertificateValid = value; }
		}

		/// <summary>The holder.</summary>
		/// <remarks>If <c>null</c> is given any will do.</remarks>
		public AttributeCertificateHolder Holder
		{
			get { return holder; }
			set { this.holder = value; }
		}

		/// <summary>The issuer.</summary>
		/// <remarks>If <c>null</c> is given any will do.</remarks>
		public AttributeCertificateIssuer Issuer
		{
			get { return issuer; }
			set { this.issuer = value; }
		}

		/// <summary>The serial number.</summary>
		/// <remarks>If <c>null</c> is given any will do.</remarks>
		public BigInteger SerialNumber
		{
			get { return serialNumber; }
			set { this.serialNumber = value; }
		}

		/**
		* Adds a target name criterion for the attribute certificate to the target
		* information extension criteria. The <code>X509AttributeCertificate</code>
		* must contain at least one of the specified target names.
		* <p>
		* Each attribute certificate may contain a target information extension
		* limiting the servers where this attribute certificate can be used. If
		* this extension is not present, the attribute certificate is not targeted
		* and may be accepted by any server.
		* </p>
		*
		* @param name The name as a GeneralName (not <code>null</code>)
		*/
		public void AddTargetName(
			GeneralName name)
		{
			targetNames.Add(name);
		}

		/**
		* Adds a target name criterion for the attribute certificate to the target
		* information extension criteria. The <code>X509AttributeCertificate</code>
		* must contain at least one of the specified target names.
		* <p>
		* Each attribute certificate may contain a target information extension
		* limiting the servers where this attribute certificate can be used. If
		* this extension is not present, the attribute certificate is not targeted
		* and may be accepted by any server.
		* </p>
		*
		* @param name a byte array containing the name in ASN.1 DER encoded form of a GeneralName
		* @throws IOException if a parsing error occurs.
		*/
		public void AddTargetName(
			byte[] name)
		{
			AddTargetName(GeneralName.GetInstance(Asn1Object.FromByteArray(name)));
		}

		/**
		* Adds a collection with target names criteria. If <code>null</code> is
		* given any will do.
		* <p>
		* The collection consists of either GeneralName objects or byte[] arrays representing
		* DER encoded GeneralName structures.
		* </p>
		* 
		* @param names A collection of target names.
		* @throws IOException if a parsing error occurs.
		* @see #AddTargetName(byte[])
		* @see #AddTargetName(GeneralName)
		*/
		public void SetTargetNames(
			IEnumerable names)
		{
			targetNames = ExtractGeneralNames(names);
		}

		/**
		* Gets the target names. The collection consists of <code>List</code>s
		* made up of an <code>Integer</code> in the first entry and a DER encoded
		* byte array or a <code>String</code> in the second entry.
		* <p>The returned collection is immutable.</p>
		* 
		* @return The collection of target names
		* @see #setTargetNames(Collection)
		*/
		public IEnumerable GetTargetNames()
		{
			return new EnumerableProxy(targetNames);
		}

		/**
		* Adds a target group criterion for the attribute certificate to the target
		* information extension criteria. The <code>X509AttributeCertificate</code>
		* must contain at least one of the specified target groups.
		* <p>
		* Each attribute certificate may contain a target information extension
		* limiting the servers where this attribute certificate can be used. If
		* this extension is not present, the attribute certificate is not targeted
		* and may be accepted by any server.
		* </p>
		*
		* @param group The group as GeneralName form (not <code>null</code>)
		*/
		public void AddTargetGroup(
			GeneralName group)
		{
			targetGroups.Add(group);
		}

		/**
		* Adds a target group criterion for the attribute certificate to the target
		* information extension criteria. The <code>X509AttributeCertificate</code>
		* must contain at least one of the specified target groups.
		* <p>
		* Each attribute certificate may contain a target information extension
		* limiting the servers where this attribute certificate can be used. If
		* this extension is not present, the attribute certificate is not targeted
		* and may be accepted by any server.
		* </p>
		*
		* @param name a byte array containing the group in ASN.1 DER encoded form of a GeneralName
		* @throws IOException if a parsing error occurs.
		*/
		public void AddTargetGroup(
			byte[] name)
		{
			AddTargetGroup(GeneralName.GetInstance(Asn1Object.FromByteArray(name)));
		}

		/**
		* Adds a collection with target groups criteria. If <code>null</code> is
		* given any will do.
		* <p>
		* The collection consists of <code>GeneralName</code> objects or <code>byte[]</code>
		* representing DER encoded GeneralNames.
		* </p>
		*
		* @param names A collection of target groups.
		* @throws IOException if a parsing error occurs.
		* @see #AddTargetGroup(byte[])
		* @see #AddTargetGroup(GeneralName)
		*/
		public void SetTargetGroups(
			IEnumerable names)
		{
			targetGroups = ExtractGeneralNames(names);
		}

		/**
		* Gets the target groups. The collection consists of <code>List</code>s
		* made up of an <code>Integer</code> in the first entry and a DER encoded
		* byte array or a <code>String</code> in the second entry.
		* <p>The returned collection is immutable.</p>
		*
		* @return The collection of target groups.
		* @see #setTargetGroups(Collection)
		*/
		public IEnumerable GetTargetGroups()
		{
			return new EnumerableProxy(targetGroups);
		}

		private ISet ExtractGeneralNames(
			IEnumerable names)
		{
			ISet result = new HashSet();

			if (names != null)
			{
				foreach (object o in names)
				{
					if (o is GeneralName)
					{
						result.Add(o);
					}
					else
					{
						result.Add(GeneralName.GetInstance(Asn1Object.FromByteArray((byte[]) o)));
					}
				}
			}

			return result;
		}
	}
}
