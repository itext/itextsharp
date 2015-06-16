using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using iTextSharp.xmp.options;

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

namespace iTextSharp.xmp.impl {
    using XmpConst = XmpConst;
    using XMPError = XmpError;
    using XmpException = XmpException;


    /// <summary>
    /// A node in the internally XMP tree, which can be a schema node, a property node, an array node,
    /// an array item, a struct node or a qualifier node (without '?').
    /// 
    /// Possible improvements:
    /// 
    /// 1. The kind Node of node might be better represented by a class-hierarchy of different nodes.
    /// 2. The array type should be an enum
    /// 3. isImplicitNode should be removed completely and replaced by return values of fi.
    /// 4. hasLanguage, hasType should be automatically maintained by XMPNode
    /// 
    /// @since 21.02.2006
    /// </summary>
    public class XmpNode : IComparable, ICloneable {
        private static readonly IList EmptyList = new ArrayList();

        /// <summary>
        /// flag if the node is an alias </summary>
        private bool _alias;

        /// <summary>
        /// list of child nodes, lazy initialized </summary>
        private IList _children;

        /// <summary>
        /// flag if the node has aliases </summary>
        private bool _hasAliases;

        /// <summary>
        /// flag if the node has an "rdf:value" child node. </summary>
        private bool _hasValueChild;

        /// <summary>
        /// flag if the node is implicitly created </summary>
        private bool _implicit;

        /// <summary>
        /// name of the node, contains different information depending of the node kind </summary>
        private string _name;

        /// <summary>
        /// options describing the kind of the node </summary>
        private PropertyOptions _options;

        /// <summary>
        /// link to the parent node </summary>
        private XmpNode _parent;

        /// <summary>
        /// list of qualifier of the node, lazy initialized </summary>
        private IList _qualifier;

        /// <summary>
        /// value of the node, contains different information depending of the node kind </summary>
        private string _value;


        /// <summary>
        /// Creates an <code>XMPNode</code> with initial values.
        /// </summary>
        /// <param name="name"> the name of the node </param>
        /// <param name="value"> the value of the node </param>
        /// <param name="options"> the options of the node </param>
        public XmpNode(string name, string value, PropertyOptions options) {
            _name = name;
            _value = value;
            _options = options;
        }


        /// <summary>
        /// Constructor for the node without value.
        /// </summary>
        /// <param name="name"> the name of the node </param>
        /// <param name="options"> the options of the node </param>
        public XmpNode(string name, PropertyOptions options)
            : this(name, null, options) {
        }


        /// <returns> Returns the parent node. </returns>
        public virtual XmpNode Parent {
            get { return _parent; }
            set { _parent = value; }
        }

        /// <returns> Returns the number of children without neccessarily creating a list. </returns>
        public virtual int ChildrenLength {
            get { return _children != null ? _children.Count : 0; }
        }

        /// <returns> Returns the number of qualifier without neccessarily creating a list. </returns>
        public virtual int QualifierLength {
            get { return _qualifier != null ? _qualifier.Count : 0; }
        }

        /// <returns> Returns the name. </returns>
        public virtual string Name {
            get { return _name; }
            set { _name = value; }
        }


        /// <returns> Returns the value. </returns>
        public virtual string Value {
            get { return _value; }
            set { _value = value; }
        }


        /// <returns> Returns the options. </returns>
        public virtual PropertyOptions Options {
            get {
                _options = _options ?? new PropertyOptions();
                return _options;
            }
            set { _options = value; }
        }


        /// <returns> Returns the implicit flag </returns>
        public virtual bool Implicit {
            get { return _implicit; }
            set { _implicit = value; }
        }


        /// <returns> Returns if the node contains aliases (applies only to schema nodes) </returns>
        public virtual bool HasAliases {
            get { return _hasAliases; }
            set { _hasAliases = value; }
        }


        /// <returns> Returns if the node contains aliases (applies only to schema nodes) </returns>
        public virtual bool Alias {
            get { return _alias; }
            set { _alias = value; }
        }


        /// <returns> the hasValueChild </returns>
        public virtual bool HasValueChild {
            get { return _hasValueChild; }
            set { _hasValueChild = value; }
        }

        /// <returns> Returns whether this node is a language qualifier.  </returns>
        private bool LanguageNode {
            get { return XmpConst.XML_LANG.Equals(_name); }
        }


        /// <returns> Returns whether this node is a type qualifier.  </returns>
        private bool TypeNode {
            get { return "rdf:type".Equals(_name); }
        }


        /// <summary>
        /// <em>Note:</em> This method should always be called when accessing 'children' to be sure
        /// that its initialized. </summary>
        /// <returns> Returns list of children that is lazy initialized. </returns>
        internal IList Children {
            get {
                _children = _children ?? new ArrayList();
                return _children;
            }
        }


        /// <returns> Returns a read-only copy of child nodes list. </returns>
        public virtual IList UnmodifiableChildren {
            get { return ArrayList.ReadOnly(new ArrayList(new ArrayList(Children))); }
        }


        /// <returns> Returns list of qualifier that is lazy initialized. </returns>
        private IList Qualifier {
            get {
                _qualifier = _qualifier ?? new ArrayList(0);
                return _qualifier;
            }
        }

        #region ICloneable Members

        /// <summary>
        /// Performs a <b>deep clone</b> of the node and the complete subtree.
        /// </summary>
        /// <seealso cref= java.lang.Object#clone() </seealso>
        public virtual object Clone() {
            PropertyOptions newOptions;
            try {
                newOptions = new PropertyOptions(Options.Options);
            }
            catch (XmpException) {
                // cannot happen
                newOptions = new PropertyOptions();
            }

            XmpNode newNode = new XmpNode(_name, _value, newOptions);
            CloneSubtree(newNode);

            return newNode;
        }

        #endregion

        #region IComparable Members

        /// <seealso cref= Comparable#compareTo(Object)  </seealso>
        virtual public int CompareTo(object xmpNode) {
            if (Options.SchemaNode) {
                return _value.CompareTo(((XmpNode) xmpNode).Value);
            }
            return _name.CompareTo(((XmpNode) xmpNode).Name);
        }

        #endregion

        /// <summary>
        /// Resets the node.
        /// </summary>
        public virtual void Clear() {
            _options = null;
            _name = null;
            _value = null;
            _children = null;
            _qualifier = null;
        }


        /// <param name="index"> an index [1..size] </param>
        /// <returns> Returns the child with the requested index. </returns>
        public virtual XmpNode GetChild(int index) {
            return (XmpNode) Children[index - 1];
        }


        /// <summary>
        /// Adds a node as child to this node. </summary>
        /// <param name="node"> an XMPNode </param>
        /// <exception cref="XmpException">  </exception>
        public virtual void AddChild(XmpNode node) {
            // check for duplicate properties
            AssertChildNotExisting(node.Name);
            node.Parent = this;
            Children.Add(node);
        }


        /// <summary>
        /// Adds a node as child to this node. </summary>
        /// <param name="index"> the index of the node <em>before</em> which the new one is inserted.
        /// <em>Note:</em> The node children are indexed from [1..size]! 
        /// An index of size + 1 appends a node. </param>
        /// <param name="node"> an XMPNode </param>
        /// <exception cref="XmpException">  </exception>
        public virtual void AddChild(int index, XmpNode node) {
            AssertChildNotExisting(node.Name);
            node.Parent = this;
            Children.Insert(index - 1, node);
        }


        /// <summary>
        /// Replaces a node with another one. </summary>
        /// <param name="index"> the index of the node that will be replaced.
        /// <em>Note:</em> The node children are indexed from [1..size]! </param>
        /// <param name="node"> the replacement XMPNode </param>
        public virtual void ReplaceChild(int index, XmpNode node) {
            node.Parent = this;
            Children[index - 1] = node;
        }


        /// <summary>
        /// Removes a child at the requested index. </summary>
        /// <param name="itemIndex"> the index to remove [1..size]  </param>
        public virtual void RemoveChild(int itemIndex) {
            Children.RemoveAt(itemIndex - 1);
            CleanupChildren();
        }


        /// <summary>
        /// Removes a child node.
        /// If its a schema node and doesn't have any children anymore, its deleted.
        /// </summary>
        /// <param name="node"> the child node to delete. </param>
        public virtual void RemoveChild(XmpNode node) {
            Children.Remove(node);
            CleanupChildren();
        }


        /// <summary>
        /// Removes the children list if this node has no children anymore;
        /// checks if the provided node is a schema node and doesn't have any children anymore, 
        /// its deleted.
        /// </summary>
        protected internal virtual void CleanupChildren() {
            if (_children.Count == 0) {
                _children = null;
            }
        }


        /// <summary>
        /// Removes all children from the node. 
        /// </summary>
        public virtual void RemoveChildren() {
            _children = null;
        }


        /// <param name="expr"> child node name to look for </param>
        /// <returns> Returns an <code>XMPNode</code> if node has been found, <code>null</code> otherwise.  </returns>
        public virtual XmpNode FindChildByName(string expr) {
            return find(Children, expr);
        }


        /// <param name="index"> an index [1..size] </param>
        /// <returns> Returns the qualifier with the requested index. </returns>
        public virtual XmpNode GetQualifier(int index) {
            return (XmpNode) Qualifier[index - 1];
        }


        /// <summary>
        /// Appends a qualifier to the qualifier list and sets respective options. </summary>
        /// <param name="qualNode"> a qualifier node. </param>
        /// <exception cref="XmpException">  </exception>
        public virtual void AddQualifier(XmpNode qualNode) {
            AssertQualifierNotExisting(qualNode.Name);
            qualNode.Parent = this;
            qualNode.Options.Qualifier = true;
            Options.HasQualifiers = true;

            // contraints
            if (qualNode.LanguageNode) {
                // "xml:lang" is always first and the option "hasLanguage" is set
                _options.HasLanguage = true;
                Qualifier.Insert(0, qualNode);
            }
            else if (qualNode.TypeNode) {
                // "rdf:type" must be first or second after "xml:lang" and the option "hasType" is set
                _options.HasType = true;
                Qualifier.Insert(!_options.HasLanguage ? 0 : 1, qualNode);
            }
            else {
                // other qualifiers are appended
                Qualifier.Add(qualNode);
            }
        }


        /// <summary>
        /// Removes one qualifier node and fixes the options. </summary>
        /// <param name="qualNode"> qualifier to remove </param>
        public virtual void RemoveQualifier(XmpNode qualNode) {
            PropertyOptions opts = Options;
            if (qualNode.LanguageNode) {
                // if "xml:lang" is removed, remove hasLanguage-flag too
                opts.HasLanguage = false;
            }
            else if (qualNode.TypeNode) {
                // if "rdf:type" is removed, remove hasType-flag too
                opts.HasType = false;
            }

            Qualifier.Remove(qualNode);
            if (_qualifier.Count == 0) {
                opts.HasQualifiers = false;
                _qualifier = null;
            }
        }


        /// <summary>
        /// Removes all qualifiers from the node and sets the options appropriate. 
        /// </summary>
        public virtual void RemoveQualifiers() {
            PropertyOptions opts = Options;
            // clear qualifier related options
            opts.HasQualifiers = false;
            opts.HasLanguage = false;
            opts.HasType = false;
            _qualifier = null;
        }


        /// <param name="expr"> qualifier node name to look for </param>
        /// <returns> Returns a qualifier <code>XMPNode</code> if node has been found, 
        /// <code>null</code> otherwise.  </returns>
        public virtual XmpNode FindQualifierByName(string expr) {
            return find(_qualifier, expr);
        }


        /// <returns> Returns whether the node has children. </returns>
        public virtual bool HasChildren() {
            return _children != null && _children.Count > 0;
        }


        /// <returns> Returns an iterator for the children.
        /// <em>Note:</em> take care to use it.remove(), as the flag are not adjusted in that case. </returns>
        public virtual IEnumerator IterateChildren() {
            if (_children != null) {
                return Children.GetEnumerator();
            }
            return EmptyList.GetEnumerator();
        }


        /// <returns> Returns whether the node has qualifier attached. </returns>
        public virtual bool HasQualifier() {
            return _qualifier != null && _qualifier.Count > 0;
        }


        /// <returns> Returns an iterator for the qualifier.
        /// <em>Note:</em> take care to use it.remove(), as the flag are not adjusted in that case. </returns>
        public virtual IEnumerator IterateQualifier() {
            if (_qualifier != null) {
                IEnumerator it = Qualifier.GetEnumerator();
                return it;
            }
            return EmptyList.GetEnumerator();
        }


        /// <summary>
        /// Performs a <b>deep clone</b> of the complete subtree (children and
        /// qualifier )into and add it to the destination node.
        /// </summary>
        /// <param name="destination"> the node to add the cloned subtree </param>
        public virtual void CloneSubtree(XmpNode destination) {
            try {
                foreach (XmpNode node in Children) {
                    destination.AddChild((XmpNode) node.Clone());
                }
                foreach (XmpNode node in Qualifier) {
                    destination.AddQualifier((XmpNode) node.Clone());
                }
            }
            catch (XmpException) {
                // cannot happen (duplicate childs/quals do not exist in this node)
                Debug.Assert(false);
            }
        }


        /// <summary>
        /// Renders this node and the tree unter this node in a human readable form. </summary>
        /// <param name="recursive"> Flag is qualifier and child nodes shall be rendered too </param>
        /// <returns> Returns a multiline string containing the dump. </returns>
        public virtual string DumpNode(bool recursive) {
            StringBuilder result = new StringBuilder(512);
            DumpNode(result, recursive, 0, 0);
            return result.ToString();
        }


        /// <summary>
        /// Sorts the complete datamodel according to the following rules:
        /// <ul>
        /// 		<li>Nodes at one level are sorted by name, that is prefix + local name
        /// 		<li>Starting at the root node the children and qualifier are sorted recursively, 
        /// 			which the following exceptions.
        /// 		<li>Sorting will not be used for arrays.
        /// 		<li>Within qualifier "xml:lang" and/or "rdf:type" stay at the top in that order, 
        /// 			all others are sorted.  
        /// </ul>
        /// </summary>
        public virtual void Sort() {
            // sort qualifier
            if (HasQualifier()) {
                XmpNode[] quals = new XmpNode[Qualifier.Count];
                Qualifier.CopyTo(quals, 0);
                int sortFrom = 0;
                while (quals.Length > sortFrom &&
                       (XmpConst.XML_LANG.Equals(quals[sortFrom].Name) || "rdf:type".Equals(quals[sortFrom].Name))) {
                    quals[sortFrom].Sort();
                    sortFrom++;
                }
                Array.Sort(quals, sortFrom, quals.Length - sortFrom);
                for (int j = 0; j < quals.Length; j++) {
                    _qualifier[j] = quals[j];
                    quals[j].Sort();
                }
            }

            // sort children
            if (HasChildren()) {
                if (!Options.Array) {
                    ArrayList.Adapter(_children).Sort();
                }
                IEnumerator it = IterateChildren();
                while (it.MoveNext())
                    if (it.Current != null)
                        ((XmpNode) it.Current).Sort();
            }
        }


        //------------------------------------------------------------------------------ private methods


        /// <summary>
        /// Dumps this node and its qualifier and children recursively.
        /// <em>Note:</em> It creats empty options on every node.
        /// </summary>
        /// <param name="result"> the buffer to append the dump. </param>
        /// <param name="recursive"> Flag is qualifier and child nodes shall be rendered too </param>
        /// <param name="indent"> the current indent level. </param>
        /// <param name="index"> the index within the parent node (important for arrays)  </param>
        private void DumpNode(StringBuilder result, bool recursive, int indent, int index) {
            // write indent
            for (int i = 0; i < indent; i++) {
                result.Append('\t');
            }

            // render Node
            if (_parent != null) {
                if (Options.Qualifier) {
                    result.Append('?');
                    result.Append(_name);
                }
                else if (Parent.Options.Array) {
                    result.Append('[');
                    result.Append(index);
                    result.Append(']');
                }
                else {
                    result.Append(_name);
                }
            }
            else {
                // applies only to the root node
                result.Append("ROOT NODE");
                if (!String.IsNullOrEmpty(_name)) {
                    // the "about" attribute
                    result.Append(" (");
                    result.Append(_name);
                    result.Append(')');
                }
            }

            if (!String.IsNullOrEmpty(_value)) {
                result.Append(" = \"");
                result.Append(_value);
                result.Append('"');
            }

            // render options if at least one is set
            if (Options.ContainsOneOf(0xffffffff)) {
                result.Append("\t(");
                result.Append(Options.ToString());
                result.Append(" : ");
                result.Append(Options.OptionsString);
                result.Append(')');
            }

            result.Append('\n');

            // render qualifier
            if (recursive && HasQualifier()) {
                XmpNode[] quals = new XmpNode[Qualifier.Count];
                Qualifier.CopyTo(quals, 0);
                int i = 0;
                while (quals.Length > i && (XmpConst.XML_LANG.Equals(quals[i].Name) || "rdf:type".Equals(quals[i].Name))) {
                    i++;
                }
                Array.Sort(quals, i, quals.Length - i);
                for (i = 0; i < quals.Length; i++) {
                    XmpNode qualifier = quals[i];
                    qualifier.DumpNode(result, recursive, indent + 2, i + 1);
                }
            }

            // render children
            if (recursive && HasChildren()) {
                XmpNode[] children = new XmpNode[Children.Count];
                Children.CopyTo(children, 0);
                if (!Options.Array) {
                    Array.Sort(children);
                }
                for (int i = 0; i < children.Length; i++) {
                    XmpNode child = children[i];
                    child.DumpNode(result, recursive, indent + 1, i + 1);
                }
            }
        }


        /// <summary>
        /// Internal find. </summary>
        /// <param name="list"> the list to search in </param>
        /// <param name="expr"> the search expression </param>
        /// <returns> Returns the found node or <code>nulls</code>. </returns>
        private XmpNode find(IList list, string expr) {
            if (list != null) {
                foreach (XmpNode child in list) {
                    if (child.Name.Equals(expr))
                        return child;
                }
            }
            return null;
        }


        /// <summary>
        /// Checks that a node name is not existing on the same level, except for array items. </summary>
        /// <param name="childName"> the node name to check </param>
        /// <exception cref="XmpException"> Thrown if a node with the same name is existing. </exception>
        private void AssertChildNotExisting(string childName) {
            if (!XmpConst.ARRAY_ITEM_NAME.Equals(childName) && FindChildByName(childName) != null) {
                throw new XmpException("Duplicate property or field node '" + childName + "'", XmpError.BADXMP);
            }
        }


        /// <summary>
        /// Checks that a qualifier name is not existing on the same level. </summary>
        /// <param name="qualifierName"> the new qualifier name </param>
        /// <exception cref="XmpException"> Thrown if a node with the same name is existing. </exception>
        private void AssertQualifierNotExisting(string qualifierName) {
            if (!XmpConst.ARRAY_ITEM_NAME.Equals(qualifierName) && FindQualifierByName(qualifierName) != null) {
                throw new XmpException("Duplicate '" + qualifierName + "' qualifier", XmpError.BADXMP);
            }
        }
    }
}
