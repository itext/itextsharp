/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2017 iText Group NV
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
using System.Collections.Generic;
using System.Collections;
namespace iTextSharp.tool.xml {

    /**
     * Represents an encountered tag.
     *
     * @author redlab_b
     *
     */
    public class Tag : IEnumerable<Tag> {

        private Tag parent;
        private String tag;
        private IDictionary<String, String> attributes;
        private IDictionary<String, String> css;
        private IList<Tag> children;
        private String ns;
        private Object lastMarginBottom;

        /**
         * Construct a tag.
         *
         * @param tag the tag name
         * @param attr the attributes in the tag
         */
        public Tag(String tag, IDictionary<String, String> attr) : this(tag, attr, new Dictionary<String, String>(0), ""){
        }

        /**
         * @param tag the tag name
         */
        public Tag(String tag) : this(tag, new Dictionary<String, String>(0), new Dictionary<String, String>(0), "") {
        }

        /**
         *
         * @param tag the tag name
         * @param attr the attributes
         * @param css a map with CSS
         * @param ns the namespace
         */
        public Tag(String tag, IDictionary<String, String> attr, IDictionary<String, String> css, String ns ) {
            this.tag = tag;
            this.attributes = attr;
            this.css = css;
            this.children = new List<Tag>(0);
            if (ns == null) {
                throw new ArgumentNullException("ns");
            }
            this.ns = ns;
        }

        /**
         *
         * @param tag the tag name
         * @param attr the attributes
         * @param ns the namespace
         */
        public Tag(String tag, IDictionary<String, String> attr, String ns) : this(tag, attr,new Dictionary<String, String>(0),ns ) {
        }

        /**
         * @param tag
         * @param ns
         */
        public Tag(String tag, String ns) : this(tag, new Dictionary<String, String>(0), new Dictionary<String, String>(0), ns) {
        }

        /**
         * Set/get the tags parent tag.
         *
         * @param parent the parent tag of this tag
         */
        virtual public Tag Parent {
            set {
                parent = value;
            }
            get {
                return parent;
            }
        }

        /**
         * The tags name.
         *
         * @return the tag name
         */
        virtual public String Name {
            get {
                return this.tag;
            }
        }

        /**
         * Set the css map. If <code>null</code> is given the css is cleared.
         *
         * @param css set css properties
         */
        virtual public IDictionary<String, String> CSS {
            set {
                if (null != css) {
                    this.css = value;
                } else {
                    this.css.Clear();
                }
            }
            get {
                return css;
            }
        }

        /**
         * @return the attributes of the tag
         */
        virtual public IDictionary<String, String> Attributes {
            get {
                return attributes;
            }
        }

        virtual public Object LastMarginBottom {
            get { return lastMarginBottom; }
            set { lastMarginBottom = value; }
        }

        /**
         * Add a child tag to this tag. The given tags parent is set to this tag.
         *
         * @param t the tag
         */
        virtual public void AddChild(Tag t) {
            t.Parent = this;
            this.children.Add(t);

        }

        /**
         * Returns all children of this tag.
         *
         * @return the children tags of this tag.
         */
        virtual public IList<Tag> Children {
            get {
                return this.children;
            }
        }

        /**
         * Returns all children of this tag with the given name.
         * @param name the name of the tags to look for
         *
         * @return the children tags of this tag with the given name.
         */
        virtual public IList<Tag> GetChildren(String name) {
            List<Tag> named = new List<Tag>();
            foreach (Tag child in this.children) {
                if(child.Name.Equals(name)) {
                    named.Add(child);
                }
            }
            return named;
        }

        /**
         * @return the ns
         */
        virtual public String NameSpace {
            get {
                return ns;
            }
        }

        /**
         * Print the tag
         */
        public override String ToString() {
            if ("".Equals(ns)) {
                return String.Format("{0}", this.tag);
            }
            return  String.Format("{0}:{1}", this.ns, this.tag);

        }

        /**
         * Compare this tag to t for namespace and name equality.
         * @param t the tag to compare with
         * @return true if the namespace and tag are the same.
         */
        virtual public bool CompareTag(Tag t) {
            if (this == t) {
                return true;
            }
            if (t == null) {
                return false;
            }
            Tag other = t;
            if (ns == null) {
                if (other.ns != null) {
                    return false;
                }
            } else if (!ns.Equals(other.ns)) {
                return false;
            }
            if (tag == null) {
                if (other.tag != null) {
                    return false;
                }
            } else if (!tag.Equals(other.tag)) {
                return false;
            }
            return true;
        }

        /**
         * @return the child iterator.
         */
        virtual public IEnumerator<Tag> GetEnumerator() {
            return children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return children.GetEnumerator();
        }

        /**
         * @param name
         * @param ns
         * @return the child
         */
        virtual public Tag GetChild(String name, String ns) {
            return GetChild(name, ns, false);
        }

        /**
         * @param name
         * @param ns
         * @param recursive true if the tree should be fully inwards inspected.
         * @return the child if found
         */
        virtual public Tag GetChild(String name, String ns, bool recursive) {
            return RecursiveGetChild(this, name, ns, recursive);
        }

        /**
         * Whether or not this DOMLike has children.
         *
         * @return true if there are children
         */
        virtual public bool HasChildren() {
            return Children.Count != 0;
        }

        /**
         * Whether or not this DOMLike has a parent.
         *
         * @return true if parent is not <code>null</code>
         */
        virtual public bool HasParent() {
            return Parent != null;
        }

        /**
         * @param name
         * @param ns
         * @return true if a child with given name and ns is found
         */
        virtual public bool HasChild(String name, String ns) {
            return HasChild(name, ns, false);
        }

        /**
         *
         * @param name
         * @param ns
         * @param recursive true if childrens children children children ... should be inspected too.
         * @return true if a child with the given name and ns is found.
         */
        virtual public bool HasChild(String name, String ns, bool recursive) {
            if (recursive) {
                return RecursiveHasChild(this, name, ns, true);
            } else {
                return RecursiveHasChild(this, name, ns, false);
            }
        }

        /**
         * @param tag
         * @param name
         * @param ns
         * @param recursive
         * @return true if the child is found in the child tree
         */
        private bool RecursiveHasChild(Tag tag, String name, String ns, bool recursive) {
            foreach (Tag t in tag) {
                if (t.tag.Equals(name) && t.ns.Equals(ns)) {
                    return true;
                } else if (recursive) {
                    if (RecursiveHasChild(t, name, ns, recursive)) {
                        return true;
                    }
                }
            }
            return false;
        }

        /**
         * @param tag
         * @param name
         * @param ns
         * @param recursive
         * @return the child tag
         */
        private Tag RecursiveGetChild(Tag tag, String name, String ns, bool recursive) {
            foreach (Tag t in tag) {
                if (t.tag.Equals(name) && t.ns.Equals(ns)) {
                    return t;
                } else if (recursive) {
                    Tag rT = null;
                    if (null != (rT = RecursiveGetChild(t, name, ns, recursive))) {
                        return rT;
                    }
                }
            }
            return null;
        }
    }
}
