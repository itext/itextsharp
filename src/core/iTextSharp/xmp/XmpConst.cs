//Copyright (c) 2006, Adobe Systems Incorporated
//All rights reserved.
//
//        Redistribution and use in source and binary forms, with or without
//        modification, are permitted provided that the following conditions are met:
//        1. Redistributions of source code must retain the above copyright
//        notice, this list of conditions and the following disclaimer.
//        2. Redistributions in binary form must reproduce the above copyright
//        notice, this list of conditions and the following disclaimer in the
//        documentation and/or other materials provided with the distribution.
//        3. All advertising materials mentioning features or use of this software
//        must display the following acknowledgement:
//        This product includes software developed by the Adobe Systems Incorporated.
//        4. Neither the name of the Adobe Systems Incorporated nor the
//        names of its contributors may be used to endorse or promote products
//        derived from this software without specific prior written permission.
//
//        THIS SOFTWARE IS PROVIDED BY ADOBE SYSTEMS INCORPORATED ''AS IS'' AND ANY
//        EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//        WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//        DISCLAIMED. IN NO EVENT SHALL ADOBE SYSTEMS INCORPORATED BE LIABLE FOR ANY
//        DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//        (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//        LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//        ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//        (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//        SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
//        http://www.adobe.com/devnet/xmp/library/eula-xmp-library-java.html

namespace iTextSharp.xmp {
    /// <summary>
    /// Common constants for the XMP Toolkit. 
    /// 
    /// @since 20.01.2006
    /// </summary>
    public class XmpConst {
        // ---------------------------------------------------------------------------------------------
        // Standard namespace URI constants


        // Standard namespaces

        /// <summary>
        /// The XML namespace for XML. </summary>
        public const string NS_XML = "http://www.w3.org/XML/1998/namespace";

        /// <summary>
        /// The XML namespace for RDF. </summary>
        public const string NS_RDF = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";

        /// <summary>
        /// The XML namespace for the Dublin Core schema. </summary>
        public const string NS_DC = "http://purl.org/dc/elements/1.1/";

        /// <summary>
        /// The XML namespace for the IPTC Core schema. </summary>
        public const string NS_IPTCCORE = "http://iptc.org/std/Iptc4xmpCore/1.0/xmlns/";

        /// <summary>
        /// The XML namespace for the IPTC Extension schema. </summary>
        public const string NS_IPTCEXT = "http://iptc.org/std/Iptc4xmpExt/2008-02-29/";

        /// <summary>
        /// The XML namespace for the DICOM medical schema. </summary>
        public const string NS_DICOM = "http://ns.adobe.com/DICOM/";

        /// <summary>
        /// The XML namespace for the PLUS (Picture Licensing Universal System, http://www.useplus.org) </summary>
        public const string NS_PLUS = "http://ns.useplus.org/ldf/xmp/1.0/";

        // Adobe standard namespaces

        /// <summary>
        /// The XML namespace Adobe XMP Metadata. </summary>
        public const string NS_X = "adobe:ns:meta/";

        public const string NS_IX = "http://ns.adobe.com/iX/1.0/";

        /// <summary>
        /// The XML namespace for the XMP "basic" schema. </summary>
        public const string NS_XMP = "http://ns.adobe.com/xap/1.0/";

        /// <summary>
        /// The XML namespace for the XMP copyright schema. </summary>
        public const string NS_XMP_RIGHTS = "http://ns.adobe.com/xap/1.0/rights/";

        /// <summary>
        /// The XML namespace for the XMP digital asset management schema. </summary>
        public const string NS_XMP_MM = "http://ns.adobe.com/xap/1.0/mm/";

        /// <summary>
        /// The XML namespace for the job management schema. </summary>
        public const string NS_XMP_BJ = "http://ns.adobe.com/xap/1.0/bj/";

        /// <summary>
        /// The XML namespace for the job management schema. </summary>
        public const string NS_XMP_NOTE = "http://ns.adobe.com/xmp/note/";

        /// <summary>
        /// The XML namespace for the PDF schema. </summary>
        public const string NS_PDF = "http://ns.adobe.com/pdf/1.3/";

        /// <summary>
        /// The XML namespace for the PDF schema. </summary>
        public const string NS_PDFX = "http://ns.adobe.com/pdfx/1.3/";

        public const string NS_PDFX_ID = "http://www.npes.org/pdfx/ns/id/";
        public const string NS_PDFA_SCHEMA = "http://www.aiim.org/pdfa/ns/schema#";
        public const string NS_PDFA_PROPERTY = "http://www.aiim.org/pdfa/ns/property#";
        public const string NS_PDFA_TYPE = "http://www.aiim.org/pdfa/ns/type#";
        public const string NS_PDFA_FIELD = "http://www.aiim.org/pdfa/ns/field#";
        public const string NS_PDFA_ID = "http://www.aiim.org/pdfa/ns/id/";
        public const string NS_PDFUA_ID = "http://www.aiim.org/pdfua/ns/id/";
        public const string NS_PDFA_EXTENSION = "http://www.aiim.org/pdfa/ns/extension/";

        /// <summary>
        /// The XML namespace for the Photoshop custom schema. </summary>
        public const string NS_PHOTOSHOP = "http://ns.adobe.com/photoshop/1.0/";

        /// <summary>
        /// The XML namespace for the Photoshop Album schema. </summary>
        public const string NS_PSALBUM = "http://ns.adobe.com/album/1.0/";

        /// <summary>
        /// The XML namespace for Adobe's EXIF schema. </summary>
        public const string NS_EXIF = "http://ns.adobe.com/exif/1.0/";

        /// <summary>
        /// NS for the CIPA XMP for Exif document v1.1 </summary>
        public const string NS_EXIFX = "http://cipa.jp/exif/1.0/";

        public const string NS_EXIF_AUX = "http://ns.adobe.com/exif/1.0/aux/";

        /// <summary>
        /// The XML namespace for Adobe's TIFF schema. </summary>
        public const string NS_TIFF = "http://ns.adobe.com/tiff/1.0/";

        public const string NS_PNG = "http://ns.adobe.com/png/1.0/";
        public const string NS_JPEG = "http://ns.adobe.com/jpeg/1.0/";
        public const string NS_JP2K = "http://ns.adobe.com/jp2k/1.0/";
        public const string NS_CAMERARAW = "http://ns.adobe.com/camera-raw-settings/1.0/";
        public const string NS_ADOBESTOCKPHOTO = "http://ns.adobe.com/StockPhoto/1.0/";
        public const string NS_CREATOR_ATOM = "http://ns.adobe.com/creatorAtom/1.0/";
        public const string NS_ASF = "http://ns.adobe.com/asf/1.0/";
        public const string NS_WAV = "http://ns.adobe.com/xmp/wav/1.0/";

        /// <summary>
        /// BExt Schema </summary>
        public const string NS_BWF = "http://ns.adobe.com/bwf/bext/1.0/";

        /// <summary>
        /// RIFF Info Schema </summary>
        public const string NS_RIFFINFO = "http://ns.adobe.com/riff/info/";

        public const string NS_SCRIPT = "http://ns.adobe.com/xmp/1.0/Script/";

        /// <summary>
        /// Transform XMP </summary>
        public const string NS_TXMP = "http://ns.adobe.com/TransformXMP/";

        /// <summary>
        /// Adobe Flash SWF </summary>
        public const string NS_SWF = "http://ns.adobe.com/swf/1.0/";


        // XMP namespaces that are Adobe private

        public const string NS_DM = "http://ns.adobe.com/xmp/1.0/DynamicMedia/";
        public const string NS_TRANSIENT = "http://ns.adobe.com/xmp/transient/1.0/";

        /// <summary>
        /// legacy Dublin Core NS, will be converted to NS_DC </summary>
        public const string NS_DC_DEPRECATED = "http://purl.org/dc/1.1/";


        // XML namespace constants for qualifiers and structured property fields.

        /// <summary>
        /// The XML namespace for qualifiers of the xmp:Identifier property. </summary>
        public const string TYPE_IDENTIFIERQUAL = "http://ns.adobe.com/xmp/Identifier/qual/1.0/";

        /// <summary>
        /// The XML namespace for fields of the Dimensions type. </summary>
        public const string TYPE_DIMENSIONS = "http://ns.adobe.com/xap/1.0/sType/Dimensions#";

        public const string TYPE_TEXT = "http://ns.adobe.com/xap/1.0/t/";
        public const string TYPE_PAGEDFILE = "http://ns.adobe.com/xap/1.0/t/pg/";
        public const string TYPE_GRAPHICS = "http://ns.adobe.com/xap/1.0/g/";

        /// <summary>
        /// The XML namespace for fields of a graphical image. Used for the Thumbnail type. </summary>
        public const string TYPE_IMAGE = "http://ns.adobe.com/xap/1.0/g/img/";

        public const string TYPE_FONT = "http://ns.adobe.com/xap/1.0/sType/Font#";

        /// <summary>
        /// The XML namespace for fields of the ResourceEvent type. </summary>
        public const string TYPE_RESOURCEEVENT = "http://ns.adobe.com/xap/1.0/sType/ResourceEvent#";

        /// <summary>
        /// The XML namespace for fields of the ResourceRef type. </summary>
        public const string TYPE_RESOURCEREF = "http://ns.adobe.com/xap/1.0/sType/ResourceRef#";

        /// <summary>
        /// The XML namespace for fields of the Version type. </summary>
        public const string TYPE_ST_VERSION = "http://ns.adobe.com/xap/1.0/sType/Version#";

        /// <summary>
        /// The XML namespace for fields of the JobRef type. </summary>
        public const string TYPE_ST_JOB = "http://ns.adobe.com/xap/1.0/sType/Job#";

        public const string TYPE_MANIFESTITEM = "http://ns.adobe.com/xap/1.0/sType/ManifestItem#";


        // ---------------------------------------------------------------------------------------------
        // Basic types and constants

        /// <summary>
        /// The canonical true string value for Booleans in serialized XMP. Code that converts from the
        /// string to a bool should be case insensitive, and even allow "1".
        /// </summary>
        public const string TRUESTR = "True";

        /// <summary>
        /// The canonical false string value for Booleans in serialized XMP. Code that converts from the
        /// string to a bool should be case insensitive, and even allow "0".
        /// </summary>
        public const string FALSESTR = "False";

        /// <summary>
        /// Index that has the meaning to be always the last item in an array. </summary>
        public const int ARRAY_LAST_ITEM = -1;

        /// <summary>
        /// Node name of an array item. </summary>
        public const string ARRAY_ITEM_NAME = "[]";

        /// <summary>
        /// The x-default string for localized properties </summary>
        public const string X_DEFAULT = "x-default";

        /// <summary>
        /// xml:lang qualfifier </summary>
        public const string XML_LANG = "xml:lang";

        /// <summary>
        /// rdf:type qualfifier </summary>
        public const string RDF_TYPE = "rdf:type";

        /// <summary>
        /// Processing Instruction (PI) for xmp packet </summary>
        public const string XMP_PI = "xpacket";

        /// <summary>
        /// XMP meta tag version new </summary>
        public const string TAG_XMPMETA = "xmpmeta";

        /// <summary>
        /// XMP meta tag version old </summary>
        public const string TAG_XAPMETA = "xapmeta";
    }
}
