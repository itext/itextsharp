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
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;

using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Smime
{
    /**
     * Handler class for dealing with S/MIME Capabilities
     */
    public class SmimeCapabilities
        : Asn1Encodable
    {
        /**
         * general preferences
         */
        public static readonly DerObjectIdentifier PreferSignedData = PkcsObjectIdentifiers.PreferSignedData;
        public static readonly DerObjectIdentifier CannotDecryptAny = PkcsObjectIdentifiers.CannotDecryptAny;
        public static readonly DerObjectIdentifier SmimeCapabilitesVersions = PkcsObjectIdentifiers.SmimeCapabilitiesVersions;

		/**
         * encryption algorithms preferences
         */
        public static readonly DerObjectIdentifier DesCbc = new DerObjectIdentifier("1.3.14.3.2.7");
        public static readonly DerObjectIdentifier DesEde3Cbc = PkcsObjectIdentifiers.DesEde3Cbc;
        public static readonly DerObjectIdentifier RC2Cbc = PkcsObjectIdentifiers.RC2Cbc;

		private Asn1Sequence capabilities;

		/**
         * return an Attr object from the given object.
         *
         * @param o the object we want converted.
         * @exception ArgumentException if the object cannot be converted.
         */
        public static SmimeCapabilities GetInstance(
            object obj)
        {
            if (obj == null || obj is SmimeCapabilities)
            {
                return (SmimeCapabilities) obj;
            }

			if (obj is Asn1Sequence)
            {
                return new SmimeCapabilities((Asn1Sequence) obj);
            }

			if (obj is AttributeX509)
            {
                return new SmimeCapabilities(
                    (Asn1Sequence)(((AttributeX509) obj).AttrValues[0]));
            }

			throw new ArgumentException("unknown object in factory: " + obj.GetType().Name, "obj");
        }

		public SmimeCapabilities(
            Asn1Sequence seq)
        {
            capabilities = seq;
        }

#if !SILVERLIGHT
        [Obsolete("Use 'GetCapabilitiesForOid' instead")]
        public ArrayList GetCapabilities(
            DerObjectIdentifier capability)
        {
            ArrayList list = new ArrayList();
            DoGetCapabilitiesForOid(capability, list);
			return list;
        }
#endif

        /**
         * returns an ArrayList with 0 or more objects of all the capabilities
         * matching the passed in capability Oid. If the Oid passed is null the
         * entire set is returned.
         */
        public IList GetCapabilitiesForOid(
            DerObjectIdentifier capability)
        {
            IList list = Platform.CreateArrayList();
            DoGetCapabilitiesForOid(capability, list);
			return list;
        }

        private void DoGetCapabilitiesForOid(DerObjectIdentifier capability, IList list)
        {
			if (capability == null)
            {
				foreach (object o in capabilities)
				{
                    SmimeCapability cap = SmimeCapability.GetInstance(o);

					list.Add(cap);
                }
            }
            else
            {
				foreach (object o in capabilities)
				{
                    SmimeCapability cap = SmimeCapability.GetInstance(o);

					if (capability.Equals(cap.CapabilityID))
                    {
                        list.Add(cap);
                    }
                }
            }
        }

        /**
         * Produce an object suitable for an Asn1OutputStream.
         * <pre>
         * SMIMECapabilities ::= Sequence OF SMIMECapability
         * </pre>
         */
        public override Asn1Object ToAsn1Object()
        {
            return capabilities;
        }
    }
}
