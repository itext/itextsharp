using System;
using iTextSharp.xmp;
using iTextSharp.xmp.options;

namespace iTextSharp.text.xml.xmp {

    public class XmpBasicProperties {
        /** An unordered array specifying properties that were edited outside the authoring application. Each item should contain a single namespace and XPath separated by one ASCII space (U+0020). */
        public static readonly String ADVISORY = "Advisory";
        /** The base URL for relative URLs in the document content. If this document contains Internet links, and those links are relative, they are relative to this base URL. This property provides a standard way for embedded relative URLs to be interpreted by tools. Web authoring tools should set the value based on their notion of where URLs will be interpreted. */
        public static readonly String BASEURL = "BaseURL";
        /** The date and time the resource was originally created. */
        public static readonly String CREATEDATE = "CreateDate";
        /** The name of the first known tool used to create the resource. If history is present in the metadata, this value should be equivalent to that of xmpMM:History's softwareAgent property. */
        public static readonly String CREATORTOOL = "CreatorTool";
        /** An unordered array of text strings that unambiguously identify the resource within a given context. */
        public static readonly String IDENTIFIER = "Identifier";
        /** The date and time that any metadata for this resource was last changed. */
        public static readonly String METADATADATE = "MetadataDate";
        /** The date and time the resource was last modified. */
        public static readonly String MODIFYDATE = "ModifyDate";
        /** A short informal name for the resource. */
        public static readonly String NICKNAME = "Nickname";
        /** An alternative array of thumbnail images for a file, which can differ in characteristics such as size or image encoding. */
        public static readonly String THUMBNAILS = "Thumbnails";

        /**
         * Adds the creatortool.
         *
         * @param xmpMeta
         * @param creator
         */

        public static void SetCreatorTool(IXmpMeta xmpMeta, String creator) {
            xmpMeta.SetProperty(XmpConst.NS_XMP, CREATORTOOL, creator);
        }

        /**
         * Adds the creation date.
         *
         * @param xmpMeta
         * @param date
         */

        public static void SetCreateDate(IXmpMeta xmpMeta, String date) {
            xmpMeta.SetProperty(XmpConst.NS_XMP, CREATEDATE, date);
        }

        /**
         * Adds the modification date.
         *
         * @param xmpMeta
         * @param date
         */

        public static void SetModDate(IXmpMeta xmpMeta, String date) {
            xmpMeta.SetProperty(XmpConst.NS_XMP, MODIFYDATE, date);
        }

        /**
         * Adds the meta data date.
         *
         * @param xmpMeta
         * @param date
         */

        public static void SetMetaDataDate(IXmpMeta xmpMeta, String date) {
            xmpMeta.SetProperty(XmpConst.NS_XMP, METADATADATE, date);
        }

        /** Sets the identifier.
         *
         * @param xmpMeta
         * @param id
         */

        public static void SetIdentifiers(IXmpMeta xmpMeta, String[] id) {
            XmpUtils.RemoveProperties(xmpMeta, XmpConst.NS_DC, IDENTIFIER, true, true);
            for (int i = 0; i < id.Length; i++) {
                xmpMeta.AppendArrayItem(XmpConst.NS_DC, IDENTIFIER, new PropertyOptions(PropertyOptions.ARRAY), id[i],
                                        null);
            }
        }

        /** Adds the nickname.
         *
         * @param xmpMeta
         * @param name
         */

        public static void SetNickname(IXmpMeta xmpMeta, String name) {
            xmpMeta.SetProperty(XmpConst.NS_XMP, NICKNAME, name);
        }
    }
}
