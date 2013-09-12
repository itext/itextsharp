using System;

namespace iTextSharp.text.xml.xmp {

    public class XmpMMProperties {
        /** A reference to the original document from which this one is derived. It is a minimal reference; missing components can be assumed to be unchanged. For example, a new version might only need to specify the instance ID and version number of the previous version, or a rendition might only need to specify the instance ID and rendition class of the original. */
        public static readonly String DERIVEDFROM = "DerivedFrom";
        /** The common identifier for all versions and renditions of a document. */
        public static readonly String DOCUMENTID = "DocumentID";
        /** An ordered array of high-level user actions that resulted in this resource. It is intended to give human readers a general indication of the steps taken to make the changes from the previous version to this one. The list should be at an abstract level; it is not intended to be an exhaustive keystroke or other detailed history. */
        public static readonly String HISTORY = "History";
        /** A reference to the document as it was prior to becoming managed. It is set when a managed document is introduced to an asset management system that does not currently own it. It may or may not include references to different management systems. */
        public static readonly String MANAGEDFROM = "ManagedFrom";
        /** The name of the asset management system that manages this resource. */
        public static readonly String MANAGER = "Manager";
        /** A URI identifying the managed resource to the asset management system; the presence of this property is the formal indication that this resource is managed. The form and content of this URI is private to the asset management system. */
        public static readonly String MANAGETO = "ManageTo";
        /** A URI that can be used to access information about the managed resource through a web browser. It might require a custom browser plugin. */
        public static readonly String MANAGEUI = "ManageUI";
        /** Specifies a particular variant of the asset management system. The format of this property is private to the specific asset management system. */
        public static readonly String MANAGERVARIANT = "ManagerVariant";
        /** The rendition class name for this resource.*/
        public static readonly String RENDITIONCLASS = "RenditionClass";
        /**  Can be used to provide additional rendition parameters that are too complex or verbose to encode in xmpMM: RenditionClass. */
        public static readonly String RENDITIONPARAMS = "RenditionParams";
        /** The document version identifier for this resource. */
        public static readonly String VERSIONID = "VersionID";
        /** The version history associated with this resource.*/
        public static readonly String VERSIONS = "Versions";
    }
}
