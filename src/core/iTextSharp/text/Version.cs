/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2019 iText Group NV
    Authors: iText Software.

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License version 3
    as published by the Free Software Foundation with the addition of the
    following permission added to Section 15 as permitted in Section 7(a):
    FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
    ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
    OF THIRD PARTY RIGHTS
    
    This program is distributed in the hope that it will be useful, but
    WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
    or FITNESS FOR A PARTICULAR PURPOSE.
    See the GNU Affero General Public License for more details.
    You should have received a copy of the GNU Affero General Public License
    along with this program; if not, see http://www.gnu.org/licenses or write to
    the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
    Boston, MA, 02110-1301 USA, or download the license from the following URL:
    http://itextpdf.com/terms-of-use/
    
    The interactive user interfaces in modified source and object code versions
    of this program must display Appropriate Legal Notices, as required under
    Section 5 of the GNU Affero General Public License.
    
    In accordance with Section 7(b) of the GNU Affero General Public License,
    a covered work must retain the producer line in every PDF that is created
    or manipulated using iText.
    
    You can be released from the requirements of the license by purchasing
    a commercial license. Buying such a license is mandatory as soon as you
    develop commercial activities involving the iText software without
    disclosing the source code of your own applications.
    These activities include: offering paid services to customers as an ASP,
    serving PDFs on the fly in a web application, shipping iText with a closed
    source product.
    
    For more information, please contact iText Software Corp. at this
    address: sales@itextpdf.com
 */
using System;
using System.IO;
using System.Reflection;
using iTextSharp.text.log;
using Versions.Attributes;

namespace iTextSharp.text {

    /**
     * This class contains version information about iText.
     * DO NOT CHANGE THE VERSION INFORMATION WITHOUT PERMISSION OF THE COPYRIGHT HOLDERS OF ITEXT.
     * Changing the version makes it extremely difficult to debug an application.
     * Also, the nature of open source software is that you honor the copyright of the original creators of the software.
     */
    public sealed class Version {

        private static readonly object staticLock = new object();

	    // membervariables

        /** String that will indicate if the AGPL version is used. */
        public static String AGPL = " (AGPL-version)";

        /** The iText version instance. */
        private static volatile Version version = null;

	    /**
	     * This String contains the name of the product.
	     * iText is a registered trademark by iText Group NV.
	     * Please don't change this constant.
	     */
	    private const String iText = "iTextSharp\u2122";
    	
	    /**
	     * This String contains the version number of this iText release.
	     * For debugging purposes, we request you NOT to change this constant.
	     */
        private const String release = "5.5.14-SNAPSHOT";

	    /**
	     * This String contains the iText version as shown in the producer line.
	     * iText is a product developed by iText Group NV.
	     * iText Group requests that you retain the iText producer line
	     * in every PDF that is created or manipulated using iText.
	     */
	    private String iTextVersion = iText + " " + release + " \u00a92000-2019 iText Group NV";

        /**
         * The license key.
         */
        private String key = null;

	    private static Type GetLicenseKeyClass() {
		    String licenseKeyClassPartialName = "iText.License.LicenseKey, itext.licensekey";
		    String licenseKeyClassFullName = null;

		    object[] keyVersionAttrs = typeof(Version).Assembly.GetCustomAttributes(typeof(KeyVersionAttribute), false);
		    object keyVersionAttr = keyVersionAttrs.Length > 0 ? keyVersionAttrs[0] : null;
		    if (keyVersionAttr is KeyVersionAttribute) {
			    String keyVersion = ((KeyVersionAttribute) keyVersionAttr).KeyVersion;
			    String format = "{0}, Version={1}, Culture=neutral, PublicKeyToken=8354ae6d2174ddca";
			    licenseKeyClassFullName = String.Format(format, licenseKeyClassPartialName, keyVersion);
		    }

		    Type type = null;
		    if (licenseKeyClassFullName != null) {
			    String fileLoadExceptionMessage = null;
			    try {
				    type = System.Type.GetType(licenseKeyClassFullName);
			    }
			    catch (FileLoadException fileLoadException) {
				    fileLoadExceptionMessage = fileLoadException.Message;
			    }

			    if (type == null) {
				    ILogger logger = LoggerFactory.GetLogger(typeof(Version));
				    try {
					    type = System.Type.GetType(licenseKeyClassPartialName);
				    }
				    catch {
					    // ignore
				    }
				    if (type == null && fileLoadExceptionMessage != null) {
					    logger.Error(fileLoadExceptionMessage);
				    }
			    }
		    }
		    return type;
	    }

        /**
	     * Gets an instance of the iText version that is currently used.
	     * Note that iText Group requests that you retain the iText producer line
	     * in every PDF that is created or manipulated using iText.
	     */
        public static Version GetInstance() {
            lock (staticLock) {
                if (version != null) {
                    return version;
                }
            }
            Version localVersion = new Version();
            try {
                Type type = GetLicenseKeyClass();
                Type[] cArg = new Type[] { typeof(String) };
                MethodInfo m = type.GetMethod("GetLicenseeInfoForVersion", cArg);
                String coreVersion = release;
                Object[] args = new Object[] { coreVersion };
                String[] info = (String[])m.Invoke(Activator.CreateInstance(type), args);
                if (info[3] != null && info[3].Trim().Length > 0) {
                    localVersion.key = info[3];
                } else {
                    localVersion.key = "Trial version ";
                    if (info[5] == null) {
                        localVersion.key += "unauthorised";
                    } else {
                        localVersion.key += info[5];
                    }
                }
                if (info[4] != null && info[4].Trim().Length > 0) {
                    localVersion.iTextVersion = info[4];
                } else if (info[2] != null && info[2].Trim().Length > 0) {
                    localVersion.iTextVersion += " (" + info[2];
                    if (!localVersion.key.ToLower().StartsWith("trial")) {
                        localVersion.iTextVersion += "; licensed version)";
                    } else {
                        localVersion.iTextVersion += "; " + localVersion.key + ")";
                    }
                } else if (info[0] != null && info[0].Trim().Length > 0) {
                    // fall back to contact name, if company name is unavailable
                    localVersion.iTextVersion += " (" + info[0];
                    if (!localVersion.key.ToLower().StartsWith("trial")) {
                        // we shouldn't have a licensed version without company name,
                        // but let's account for it anyway
                        localVersion.iTextVersion += "; licensed version)";
                    } else {
                        localVersion.iTextVersion += "; " + localVersion.key + ")";
                    }
                } else {
                    throw new Exception();
                }
            } catch (Exception) {
	            if (DependsOnTheOldLicense()) {
		            throw new Exception("iText License Library 1.0.* has been deprecated. Please, update to the latest version.");
	            }
                localVersion.iTextVersion += AGPL;
            }
            return localVersion;
        }

        private static bool DependsOnTheOldLicense() {
	        try {
		        return Type.GetType("iTextSharp.license.LicenseKey, itextsharp.LicenseKey") != null;
	        } catch (Exception e) {
		        return false;
	        }
        }
    	
	    /**
	     * Gets the product name.
	     * iText Group requests that you retain the iText producer line
	     * in every PDF that is created or manipulated using iText.
         * @return the product name
         */
        public String Product {
            get {
                return iText;
            }
        }
        
	    /**
	     * Gets the release number.
	     * iText Group requests that you retain the iText producer line
	     * in every PDF that is created or manipulated using iText.
         * @return the release number
         */
        public String Release {
            get {
                return release;
            }
        }

	    /**
	     * Returns the iText version as shown in the producer line.
	     * iText is a product developed by iText Group NV.
	     * iText Group requests that you retain the iText producer line
	     * in every PDF that is created or manipulated using iText.
         * @return iText version
         */
        public String GetVersion {
            get {
                return iTextVersion;
            }
        }

        /**
        * Returns a license key if one was provided, or null if not.
        * @return a license key.
        */
        public String Key {
            get {
                return key;
            }
        }

        /**
         * Checks if the AGPL version is used.
         * @return returns true if the AGPL version is used.
         */
        public static bool IsAGPLVersion {
            get { return GetInstance().GetVersion.IndexOf(AGPL) > 0; }
        }

        private static Version AtomicSetVersion(Version newVersion) {
            lock (staticLock) {
                version = newVersion;
                return version;
            }
        }
    }
}
