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
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;

namespace Org.BouncyCastle.Asn1.Nist
{
    /**
    * Utility class for fetching curves using their NIST names as published in FIPS-PUB 186-3
    */
    public sealed class NistNamedCurves
    {
        private NistNamedCurves()
        {
        }

        private static readonly IDictionary objIds = Platform.CreateHashtable();
        private static readonly IDictionary names = Platform.CreateHashtable();

        private static void DefineCurve(
            string				name,
            DerObjectIdentifier	oid)
        {
            objIds.Add(name, oid);
            names.Add(oid, name);
        }

        static NistNamedCurves()
        {
            DefineCurve("B-571", SecObjectIdentifiers.SecT571r1);
            DefineCurve("B-409", SecObjectIdentifiers.SecT409r1);
            DefineCurve("B-283", SecObjectIdentifiers.SecT283r1);
            DefineCurve("B-233", SecObjectIdentifiers.SecT233r1);
            DefineCurve("B-163", SecObjectIdentifiers.SecT163r2);
            DefineCurve("K-571", SecObjectIdentifiers.SecT571k1);
            DefineCurve("K-409", SecObjectIdentifiers.SecT409k1);
            DefineCurve("K-283", SecObjectIdentifiers.SecT283k1);
            DefineCurve("K-233", SecObjectIdentifiers.SecT233k1);
            DefineCurve("K-163", SecObjectIdentifiers.SecT163k1);
            DefineCurve("P-521", SecObjectIdentifiers.SecP521r1);
            DefineCurve("P-384", SecObjectIdentifiers.SecP384r1);
            DefineCurve("P-256", SecObjectIdentifiers.SecP256r1);
            DefineCurve("P-224", SecObjectIdentifiers.SecP224r1);
            DefineCurve("P-192", SecObjectIdentifiers.SecP192r1);
        }

        public static X9ECParameters GetByName(
            string name)
        {
            DerObjectIdentifier oid = (DerObjectIdentifier) objIds[
                Platform.ToUpperInvariant(name)];

            if (oid != null)
            {
                return GetByOid(oid);
            }

            return null;
        }

        /**
        * return the X9ECParameters object for the named curve represented by
        * the passed in object identifier. Null if the curve isn't present.
        *
        * @param oid an object identifier representing a named curve, if present.
        */
        public static X9ECParameters GetByOid(
            DerObjectIdentifier oid)
        {
            return SecNamedCurves.GetByOid(oid);
        }

        /**
        * return the object identifier signified by the passed in name. Null
        * if there is no object identifier associated with name.
        *
        * @return the object identifier associated with name, if present.
        */
        public static DerObjectIdentifier GetOid(
            string name)
        {
            return (DerObjectIdentifier) objIds[
                Platform.ToUpperInvariant(name)];
        }

        /**
        * return the named curve name represented by the given object identifier.
        */
        public static string GetName(
            DerObjectIdentifier  oid)
        {
            return (string) names[oid];
        }

        /**
        * returns an enumeration containing the name strings for curves
        * contained in this structure.
        */
        public static IEnumerable Names
        {
            get { return new EnumerableProxy(objIds.Keys); }
        }
    }
}
