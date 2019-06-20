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

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509.Store;

namespace Org.BouncyCastle.X509
{
	/**
	 * Carrying class for an attribute certificate issuer.
	 */
	public class AttributeCertificateIssuer
		//: CertSelector, Selector
		: IX509Selector
	{
		internal readonly Asn1Encodable form;

		/**
		 * Set the issuer directly with the ASN.1 structure.
		 *
		 * @param issuer The issuer
		 */
		public AttributeCertificateIssuer(
			AttCertIssuer issuer)
		{
			form = issuer.Issuer;
		}

		public AttributeCertificateIssuer(
			X509Name principal)
		{
//			form = new V2Form(GeneralNames.GetInstance(new DerSequence(new GeneralName(principal))));
			form = new V2Form(new GeneralNames(new GeneralName(principal)));
		}

		private object[] GetNames()
		{
			GeneralNames name;
			if (form is V2Form)
			{
				name = ((V2Form)form).IssuerName;
			}
			else
			{
				name = (GeneralNames)form;
			}

            GeneralName[] names = name.GetNames();

            int count = 0;
            for (int i = 0; i != names.Length; i++)
            {
                if (names[i].TagNo == GeneralName.DirectoryName)
                {
                    ++count;
                }
            }

            object[] result = new object[count];

            int pos = 0;
            for (int i = 0; i != names.Length; i++)
			{
				if (names[i].TagNo == GeneralName.DirectoryName)
				{
                    result[pos++] = X509Name.GetInstance(names[i].Name);
				}
			}

            return result;
        }

		/// <summary>Return any principal objects inside the attribute certificate issuer object.</summary>
		/// <returns>An array of IPrincipal objects (usually X509Principal).</returns>
		public X509Name[] GetPrincipals()
		{
			object[] p = this.GetNames();

            int count = 0;
            for (int i = 0; i != p.Length; i++)
            {
                if (p[i] is X509Name)
                {
                    ++count;
                }
            }

            X509Name[] result = new X509Name[count];

            int pos = 0;
			for (int i = 0; i != p.Length; i++)
			{
				if (p[i] is X509Name)
				{
					result[pos++] = (X509Name)p[i];
				}
			}

            return result;
		}

		private bool MatchesDN(
			X509Name		subject,
			GeneralNames	targets)
		{
			GeneralName[] names = targets.GetNames();

			for (int i = 0; i != names.Length; i++)
			{
				GeneralName gn = names[i];

				if (gn.TagNo == GeneralName.DirectoryName)
				{
					try
					{
						if (X509Name.GetInstance(gn.Name).Equivalent(subject))
						{
							return true;
						}
					}
					catch (Exception)
					{
					}
				}
			}

			return false;
		}

		public object Clone()
		{
			return new AttributeCertificateIssuer(AttCertIssuer.GetInstance(form));
		}

		public bool Match(
//			Certificate cert)
			X509Certificate x509Cert)
		{
//			if (!(cert is X509Certificate))
//			{
//				return false;
//			}
//
//			X509Certificate x509Cert = (X509Certificate)cert;

			if (form is V2Form)
			{
				V2Form issuer = (V2Form) form;
				if (issuer.BaseCertificateID != null)
				{
					return issuer.BaseCertificateID.Serial.Value.Equals(x509Cert.SerialNumber)
						&& MatchesDN(x509Cert.IssuerDN, issuer.BaseCertificateID.Issuer);
				}

				return MatchesDN(x509Cert.SubjectDN, issuer.IssuerName);
			}

			return MatchesDN(x509Cert.SubjectDN, (GeneralNames) form);
		}

		public override bool Equals(
			object obj)
		{
			if (obj == this)
			{
				return true;
			}

			if (!(obj is AttributeCertificateIssuer))
			{
				return false;
			}

			AttributeCertificateIssuer other = (AttributeCertificateIssuer)obj;

			return this.form.Equals(other.form);
		}

		public override int GetHashCode()
		{
			return this.form.GetHashCode();
		}

		public bool Match(
			object obj)
		{
			if (!(obj is X509Certificate))
			{
				return false;
			}

			//return Match((Certificate)obj);
			return Match((X509Certificate)obj);
		}
	}
}
