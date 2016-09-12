/*
 * $Id$
 *
 * This file is part of the iText (R) project.
 * Copyright (c) 1998-2016 iText Group NV
 * Authors: Bruno Lowagie, Paulo Soares, et al.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License version 3
 * as published by the Free Software Foundation with the addition of the
 * following permission added to Section 15 as permitted in Section 7(a):
 * FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
 * ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
 * OF THIRD PARTY RIGHTS
 *
 * This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
 * or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program; if not, see http://www.gnu.org/licenses or write to
 * the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
 * Boston, MA, 02110-1301 USA, or download the license from the following URL:
 * http://itextpdf.com/terms-of-use/
 *
 * The interactive user interfaces in modified source and object code versions
 * of this program must display Appropriate Legal Notices, as required under
 * Section 5 of the GNU Affero General Public License.
 *
 * In accordance with Section 7(b) of the GNU Affero General Public License,
 * a covered work must retain the producer line in every PDF that is created
 * or manipulated using iText.
 *
 * You can be released from the requirements of the license by purchasing
 * a commercial license. Buying such a license is mandatory as soon as you
 * develop commercial activities involving the iText software without
 * disclosing the source code of your own applications.
 * These activities include: offering paid services to customers as an ASP,
 * serving PDFs on the fly in a web application, shipping iText with a closed
 * source product.
 *
 * For more information, please contact iText Software Corp. at this
 * address: sales@itextpdf.com
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace iTextSharp.text.pdf.security
{
    class CryptoConst
    {
        public const string MS_DEF_PROV = "Microsoft Base Cryptographic Provider v1.0";
        public const string MS_ENHANCED_PROV = "Microsoft Enhanced Cryptographic Provider v1.0";
        public const string MS_STRONG_PROV = "Microsoft Strong Cryptographic Provider";
        public const string MS_DEF_RSA_SIG_PROV = "Microsoft RSA Signature Cryptographic Provider";
        public const string MS_DEF_RSA_SCHANNEL_PROV = "Microsoft RSA SChannel Cryptographic Provider";
        public const string MS_DEF_DSS_PROV = "Microsoft Base DSS Cryptographic Provider";
        public const string MS_DEF_DSS_DH_PROV = "Microsoft Base DSS and Diffie-Hellman Cryptographic Provider";
        public const string MS_ENH_DSS_DH_PROV = "Microsoft Enhanced DSS and Diffie-Hellman Cryptographic Provider";
        public const string MS_DEF_DH_SCHANNEL_PROV = "Microsoft DH SChannel Cryptographic Provider";
        public const string MS_SCARD_PROV = "Microsoft Base Smart Card Crypto Provider";
        public const string MS_ENH_RSA_AES_PROV = "Microsoft Enhanced RSA and AES Cryptographic Provider";

        public const int PROV_RSA_FULL = 1;
        public const int PROV_RSA_SIG = 2;
        public const int PROV_DSS = 3;
        public const int PROV_FORTEZZA = 4;
        public const int PROV_MS_EXCHANGE = 5;
        public const int PROV_SSL = 6;
        public const int PROV_RSA_SCHANNEL = 12;
        public const int PROV_DSS_DH = 13;
        public const int PROV_EC_ECDSA_SIG = 14;
        public const int PROV_EC_ECNRA_SIG = 15;
        public const int PROV_EC_ECDSA_FULL = 16;
        public const int PROV_EC_ECNRA_FULL = 17;
        public const int PROV_DH_SCHANNEL = 18;
        public const int PROV_SPYRUS_LYNKS = 20;
        public const int PROV_RNG = 21;
        public const int PROV_INTEL_SEC = 22;
        public const int PROV_REPLACE_OWF = 23;
        public const int PROV_RSA_AES = 24;
    }
}
