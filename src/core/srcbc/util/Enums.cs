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

#if NETCF_1_0 || NETCF_2_0 || SILVERLIGHT
using System.Collections;
using System.Reflection;
#endif

using Org.BouncyCastle.Utilities.Date;

namespace Org.BouncyCastle.Utilities
{
    internal sealed class Enums
    {
        private Enums()
        {
        }

        internal static Enum GetEnumValue(System.Type enumType, string s)
        {
            if (!enumType.IsEnum)
                throw new ArgumentException("Not an enumeration type", "enumType");

            // We only want to parse single named constants
            if (s.Length > 0 && char.IsLetter(s[0]) && s.IndexOf(',') < 0)
            {
                s = s.Replace('-', '_');
                s = s.Replace('/', '_');

#if NETCF_1_0
                FieldInfo field = enumType.GetField(s, BindingFlags.Static | BindingFlags.Public);
                if (field != null)
                {
                    return (Enum)field.GetValue(null);
                }
#else
                return (Enum)Enum.Parse(enumType, s, false);
#endif		
            }

            throw new ArgumentException();
        }

        internal static Array GetEnumValues(System.Type enumType)
        {
            if (!enumType.IsEnum)
                throw new ArgumentException("Not an enumeration type", "enumType");

#if NETCF_1_0 || NETCF_2_0 || SILVERLIGHT
            IList result = Platform.CreateArrayList();
            FieldInfo[] fields = enumType.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (FieldInfo field in fields)
            {
                // Note: Argument to GetValue() ignored since the fields are static,
                //     but Silverlight for Windows Phone throws exception if we pass null
                result.Add(field.GetValue(enumType));
            }
            object[] arr = new object[result.Count];
            result.CopyTo(arr, 0);
            return arr;
#else
            return Enum.GetValues(enumType);
#endif
        }

        internal static Enum GetArbitraryValue(System.Type enumType)
        {
            Array values = GetEnumValues(enumType);
            int pos = (int)(DateTimeUtilities.CurrentUnixMs() & int.MaxValue) % values.Length;
            return (Enum)values.GetValue(pos);
        }
    }
}
