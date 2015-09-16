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

using System;
using System.Collections;
using iTextSharp.xmp.impl.xpath;
using iTextSharp.xmp.options;
using iTextSharp.xmp.properties;

namespace iTextSharp.xmp.impl {
    /// <summary>
    /// The <code>XMPIterator</code> implementation.
    /// Iterates the XMP Tree according to a set of options.
    /// During the iteration the XMPMeta-object must not be changed.
    /// Calls to <code>skipSubtree()</code> / <code>skipSiblings()</code> will affect the iteration.
    ///  
    /// @since   29.06.2006
    /// </summary>
    public class XmpIteratorImpl : IXmpIterator {
        private static readonly IList EmptyList = new ArrayList();

        /// <summary>
        /// the node iterator doing the work </summary>
        private readonly IEnumerator _nodeIterator;

        /// <summary>
        /// stores the iterator options </summary>
        private readonly IteratorOptions _options;

        /// <summary>
        /// the base namespace of the property path, will be changed during the iteration </summary>
        private string _baseNs;

        /// <summary>
        /// flag to indicate that skipSiblings() has been called. </summary>
        private bool _skipSiblings;

        /// <summary>
        /// flag to indicate that skipSiblings() has been called. </summary>
        protected internal bool skipSubtree;


        /// <summary>
        /// Constructor with optionsl initial values. If <code>propName</code> is provided, 
        /// <code>schemaNs</code> has also be provided. </summary>
        /// <param name="xmp"> the iterated metadata object. </param>
        /// <param name="schemaNs"> the iteration is reduced to this schema (optional) </param>
        /// <param name="propPath"> the iteration is redurce to this property within the <code>schemaNs</code> </param>
        /// <param name="options"> advanced iteration options, see <seealso cref="IteratorOptions"/> </param>
        /// <exception cref="XmpException"> If the node defined by the paramters is not existing.  </exception>
        public XmpIteratorImpl(XmpMetaImpl xmp, string schemaNs, string propPath, IteratorOptions options) {
            // make sure that options is defined at least with defaults
            _options = options ?? new IteratorOptions();

            // the start node of the iteration depending on the schema and property filter
            XmpNode startNode;
            string initialPath = null;
            bool baseSchema = !String.IsNullOrEmpty(schemaNs);
            bool baseProperty = !String.IsNullOrEmpty(propPath);

            if (!baseSchema && !baseProperty) {
                // complete tree will be iterated
                startNode = xmp.Root;
            }
            else if (baseSchema && baseProperty) {
                // Schema and property node provided
                XmpPath path = XmpPathParser.ExpandXPath(schemaNs, propPath);

                // base path is the prop path without the property leaf
                XmpPath basePath = new XmpPath();
                for (int i = 0; i < path.Size() - 1; i++) {
                    basePath.Add(path.GetSegment(i));
                }

                startNode = XmpNodeUtils.FindNode(xmp.Root, path, false, null);
                _baseNs = schemaNs;
                initialPath = basePath.ToString();
            }
            else if (baseSchema && !baseProperty) {
                // Only Schema provided
                startNode = XmpNodeUtils.FindSchemaNode(xmp.Root, schemaNs, false);
            }
            else // !baseSchema  &&  baseProperty
            {
                // No schema but property provided -> error
                throw new XmpException("Schema namespace URI is required", XmpError.BADSCHEMA);
            }


            // create iterator
            if (startNode != null) {
                _nodeIterator = (!_options.JustChildren)
                                    ? new NodeIterator(this, startNode, initialPath, 1)
                                    : new NodeIteratorChildren(this, startNode, initialPath);
            }
            else {
                // create null iterator
                _nodeIterator = EmptyList.GetEnumerator();
            }
        }

        /// <returns> Exposes the options for inner class. </returns>
        protected internal virtual IteratorOptions Options {
            get { return _options; }
        }


        /// <returns> Exposes the options for inner class. </returns>
        protected internal virtual string BaseNs {
            get { return _baseNs; }
            set { _baseNs = value; }
        }

        #region XmpIterator Members

        /// <seealso cref= XMPIterator#skipSubtree() </seealso>
        public virtual void SkipSubtree() {
            skipSubtree = true;
        }


        /// <seealso cref= XMPIterator#skipSiblings() </seealso>
        public virtual void SkipSiblings() {
            SkipSubtree();
            _skipSiblings = true;
        }

        /// <seealso cref= java.util.Iterator#hasNext() </seealso>
        public virtual bool MoveNext() {
            return _nodeIterator.MoveNext();
        }


        /// <seealso cref= java.util.Iterator#next() </seealso>
        public virtual object Current {
            get { return _nodeIterator.Current; }
        }

        public virtual void Reset() {
            _nodeIterator.Reset();
        }

        #endregion

        #region Nested type: NodeIterator

        /// <summary>
        /// The <code>XMPIterator</code> implementation.
        /// It first returns the node itself, then recursivly the children and qualifier of the node.
        /// 
        /// @since   29.06.2006
        /// </summary>
        private class NodeIterator : IEnumerator {
            /// <summary>
            /// iteration state </summary>
            private const int ITERATE_NODE = 0;

            /// <summary>
            /// iteration state </summary>
            private const int ITERATE_CHILDREN = 1;

            /// <summary>
            /// iteration state </summary>
            private const int ITERATE_QUALIFIER = 2;

            private static readonly IList EmptyList = new ArrayList();


            private readonly XmpIteratorImpl _outerInstance;

            /// <summary>
            /// the recursively accumulated path </summary>
            private readonly string _path;

            /// <summary>
            /// the currently visited node </summary>
            private readonly XmpNode _visitedNode;

            /// <summary>
            /// the iterator that goes through the children and qualifier list </summary>
            private IEnumerator _childrenIterator;

            /// <summary>
            /// index of node with parent, only interesting for arrays </summary>
            private int _index;

            /// <summary>
            /// the cached <code>PropertyInfo</code> to return </summary>
            private IXmpPropertyInfo _returnProperty;

            /// <summary>
            /// the state of the iteration </summary>
            private int _state = ITERATE_NODE;

            /// <summary>
            /// the iterator for each child </summary>
            private IEnumerator _subIterator = EmptyList.GetEnumerator();

            /// <summary>
            /// Constructor for the node iterator. </summary>
            /// <param name="visitedNode"> the currently visited node </param>
            /// <param name="parentPath"> the accumulated path of the node </param>
            /// <param name="index"> the index within the parent node (only for arrays) </param>
            public NodeIterator(XmpIteratorImpl outerInstance, XmpNode visitedNode, string parentPath, int index) {
                _outerInstance = outerInstance;
                _visitedNode = visitedNode;
                _state = ITERATE_NODE;
                if (visitedNode.Options.SchemaNode) {
                    outerInstance.BaseNs = visitedNode.Name;
                }

                // for all but the root node and schema nodes
                _path = AccumulatePath(visitedNode, parentPath, index);
            }

            /// <returns> the childrenIterator </returns>
            protected internal virtual IEnumerator ChildrenIterator {
                get { return _childrenIterator; }
                set { _childrenIterator = value; }
            }


            /// <returns> Returns the returnProperty. </returns>
            protected internal virtual IXmpPropertyInfo ReturnProperty {
                get { return _returnProperty; }
                set { _returnProperty = value; }
            }

            #region IEnumerator Members

            virtual public object Current {
                get { return _returnProperty; }
            }

            /// <summary>
            /// Prepares the next node to return if not already done. 
            /// </summary>
            /// <seealso cref= Iterator#hasNext() </seealso>
            public virtual bool MoveNext() {
                // find next node
                if (_state == ITERATE_NODE) {
                    return ReportNode();
                }
                if (_state == ITERATE_CHILDREN) {
                    if (_childrenIterator == null) {
                        _childrenIterator = _visitedNode.IterateChildren();
                    }

                    bool hasNext = IterateChildren(_childrenIterator);

                    if (!hasNext && _visitedNode.HasQualifier() && !_outerInstance.Options.OmitQualifiers) {
                        _state = ITERATE_QUALIFIER;
                        _childrenIterator = null;
                        hasNext = MoveNext();
                    }
                    return hasNext;
                }
                if (_childrenIterator == null) {
                    _childrenIterator = _visitedNode.IterateQualifier();
                }

                return IterateChildren(_childrenIterator);
            }

            public virtual void Reset() {
                throw new NotSupportedException();
            }

            #endregion

            /// <summary>
            /// Sets the returnProperty as next item or recurses into <code>hasNext()</code>. </summary>
            /// <returns> Returns if there is a next item to return.  </returns>
            protected internal virtual bool ReportNode() {
                _state = ITERATE_CHILDREN;
                if (_visitedNode.Parent != null &&
                    (!_outerInstance.Options.JustLeafnodes || !_visitedNode.HasChildren())) {
                    _returnProperty = CreatePropertyInfo(_visitedNode, _outerInstance.BaseNs, _path);
                    return true;
                }
                return MoveNext();
            }


            /// <summary>
            /// Handles the iteration of the children or qualfier </summary>
            /// <param name="iterator"> an iterator </param>
            /// <returns> Returns if there are more elements available. </returns>
            private bool IterateChildren(IEnumerator iterator) {
                if (_outerInstance._skipSiblings) {
                    // setSkipSiblings(false);
                    _outerInstance._skipSiblings = false;
                    _subIterator = EmptyList.GetEnumerator();
                }

                // create sub iterator for every child,
                // if its the first child visited or the former child is finished 
                bool subIteratorMoveNext = _subIterator.MoveNext();
                if (!subIteratorMoveNext && iterator.MoveNext()) {
                    XmpNode child = (XmpNode) iterator.Current;
                    _index++;
                    _subIterator = new NodeIterator(_outerInstance, child, _path, _index);
                }
                if (subIteratorMoveNext) {
                    _returnProperty = (IXmpPropertyInfo) _subIterator.Current;
                    return true;
                }
                return false;
            }


            /// <param name="currNode"> the node that will be added to the path. </param>
            /// <param name="parentPath"> the path up to this node. </param>
            /// <param name="currentIndex"> the current array index if an arrey is traversed </param>
            /// <returns> Returns the updated path. </returns>
            protected internal virtual string AccumulatePath(XmpNode currNode, string parentPath, int currentIndex) {
                string separator;
                string segmentName;
                if (currNode.Parent == null || currNode.Options.SchemaNode) {
                    return null;
                }
                if (currNode.Parent.Options.Array) {
                    separator = "";
                    segmentName = "[" + Convert.ToString(currentIndex) + "]";
                }
                else {
                    separator = "/";
                    segmentName = currNode.Name;
                }


                if (String.IsNullOrEmpty(parentPath)) {
                    return segmentName;
                }
                if (_outerInstance.Options.JustLeafname) {
                    return !segmentName.StartsWith("?") ? segmentName : segmentName.Substring(1); // qualifier
                }
                return parentPath + separator + segmentName;
            }


            /// <summary>
            /// Creates a property info object from an <code>XMPNode</code>. </summary>
            /// <param name="node"> an <code>XMPNode</code> </param>
            /// <param name="baseNs"> the base namespace to report </param>
            /// <param name="path"> the full property path </param>
            /// <returns> Returns a <code>XMPProperty</code>-object that serves representation of the node. </returns>
            protected internal virtual IXmpPropertyInfo CreatePropertyInfo(XmpNode node, string baseNs, string path) {
                string value = node.Options.SchemaNode ? null : node.Value;

                return new XmpPropertyInfoImpl(node, baseNs, path, value);
            }

            #region Nested type: XmpPropertyInfoImpl

            private class XmpPropertyInfoImpl : IXmpPropertyInfo {
                private readonly string _baseNs;
                private readonly XmpNode _node;
                private readonly string _path;
                private readonly string _value;

                public XmpPropertyInfoImpl(XmpNode node, string baseNs, string path, string value) {
                    _node = node;
                    _baseNs = baseNs;
                    _path = path;
                    _value = value;
                }

                #region IXmpPropertyInfo Members

                virtual public string Namespace {
                    get {
                        if (!_node.Options.SchemaNode) {
                            // determine namespace of leaf node
                            QName qname = new QName(_node.Name);
                            return XmpMetaFactory.SchemaRegistry.GetNamespaceUri(qname.Prefix);
                        }
                        return _baseNs;
                    }
                }

                virtual public string Path {
                    get { return _path; }
                }

                virtual public string Value {
                    get { return _value; }
                }

                virtual public PropertyOptions Options {
                    get { return _node.Options; }
                }

                virtual public string Language {
                    get {
                        // the language is not reported
                        return null;
                    }
                }

                #endregion
            }

            #endregion
        }

        #endregion

        #region Nested type: NodeIteratorChildren

        /// <summary>
        /// This iterator is derived from the default <code>NodeIterator</code>,
        /// and is only used for the option <seealso cref="IteratorOptions.JUST_CHILDREN"/>.
        /// 
        /// @since 02.10.2006
        /// </summary>
        private class NodeIteratorChildren : NodeIterator {
            private readonly IEnumerator _childrenIterator;
            private readonly XmpIteratorImpl _outerInstance;

            private readonly string _parentPath;
            private int _index;


            /// <summary>
            /// Constructor </summary>
            /// <param name="parentNode"> the node which children shall be iterated. </param>
            /// <param name="parentPath"> the full path of the former node without the leaf node. </param>
            public NodeIteratorChildren(XmpIteratorImpl outerInstance, XmpNode parentNode, string parentPath)
                : base(outerInstance, parentNode, parentPath, 0) {
                _outerInstance = outerInstance;
                if (parentNode.Options.SchemaNode) {
                    outerInstance.BaseNs = parentNode.Name;
                }
                _parentPath = AccumulatePath(parentNode, parentPath, 1);

                _childrenIterator = parentNode.IterateChildren();
            }


            /// <summary>
            /// Prepares the next node to return if not already done. 
            /// </summary>
            /// <seealso cref= Iterator#hasNext() </seealso>
            public override bool MoveNext() {
                if (_outerInstance._skipSiblings) {
                    return false;
                }
                if (_childrenIterator.MoveNext()) {
                    XmpNode child = (XmpNode) _childrenIterator.Current;
                    if (child != null) {
                        _index++;
                        string path = null;
                        if (child.Options.SchemaNode) {
                            _outerInstance.BaseNs = child.Name;
                        }
                        else if (child.Parent != null) {
                            // for all but the root node and schema nodes
                            path = AccumulatePath(child, _parentPath, _index);
                        }

                        // report next property, skip not-leaf nodes in case options is set
                        if (!_outerInstance.Options.JustLeafnodes || !child.HasChildren()) {
                            ReturnProperty = CreatePropertyInfo(child, _outerInstance.BaseNs, path);
                            return true;
                        }
                    }
                    return MoveNext();
                }
                return false;
            }
        }

        #endregion
    }
}
