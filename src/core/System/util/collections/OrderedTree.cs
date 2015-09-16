using System.Collections;

/*
 * $Id$
 *
 * This file is part of the iText project.
 * Copyright (c) 1998-2015 iText Group NV
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
 * you must retain the producer line in every PDF that is created or manipulated
 * using iText.
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

namespace System.util.collections {
    public class OrderedTree {
        // the number of nodes contained in the tree
        private int intCount;
        // the tree
        private OrderedTreeNode rbTree;
        // sentinelNode is convenient way of indicating a leaf node.
        private OrderedTreeNode sentinelNode;          
        // the node that was last found; used to optimize searches
        private OrderedTreeNode lastNodeFound;          

        //static OrderedTree() {
        //    // set up the sentinel node. the sentinel node is the key to a successfull
        //    // implementation and for understanding the red-black tree properties.
        //    sentinelNode = new OrderedTreeNode();
        //    sentinelNode.Left = sentinelNode.Right = sentinelNode;
        //    sentinelNode.Parent = null;
        //    sentinelNode.Color = OrderedTreeNode.BLACK;
        //}

        public OrderedTree() {
            // set up the sentinel node. the sentinel node is the key to a successfull
            // implementation and for understanding the red-black tree properties.
            sentinelNode = new OrderedTreeNode();
            sentinelNode.Left = sentinelNode.Right = sentinelNode;
            sentinelNode.Parent = null;
            sentinelNode.Color = OrderedTreeNode.BLACK;
            rbTree = sentinelNode;
            lastNodeFound = sentinelNode;
        }
        
        public object this[IComparable key] {
            get {
                return GetData(key);
            }
            set {
                if(key == null)
                    throw new ArgumentNullException("key");
                
                // traverse tree - find where node belongs
                int result = 0;
                // create new node
                OrderedTreeNode node = new OrderedTreeNode();
                OrderedTreeNode temp = rbTree; // grab the rbTree node of the tree

                while(temp != sentinelNode) {
                    // find Parent
                    node.Parent = temp;
                    result = key.CompareTo(temp.Key);
                    if(result == 0) {
                        lastNodeFound = temp;
                        temp.Data = value;
                        return;
                    }
                    if(result > 0)
                        temp = temp.Right;
                    else
                        temp = temp.Left;
                }
                
                // setup node
                node.Key = key;
                node.Data = value;
                node.Left = sentinelNode;
                node.Right = sentinelNode;

                // insert node into tree starting at parent's location
                if(node.Parent != null) {
                    result = node.Key.CompareTo(node.Parent.Key);
                    if(result > 0)
                        node.Parent.Right = node;
                    else
                        node.Parent.Left = node;
                }
                else
                    rbTree = node; // first node added

                RestoreAfterInsert(node); // restore red-black properities

                lastNodeFound = node;
                
                intCount = intCount + 1;
            }
        }
        
        ///<summary>
        /// Add
        /// args: ByVal key As IComparable, ByVal data As Object
        /// key is object that implements IComparable interface
        /// performance tip: change to use use int type (such as the hashcode)
        ///</summary>
        virtual public void Add(IComparable key, object data) {
            if(key == null)
                throw(new ArgumentNullException("key"));
            
            // traverse tree - find where node belongs
            int result = 0;
            // create new node
            OrderedTreeNode node = new OrderedTreeNode();
            OrderedTreeNode temp = rbTree; // grab the rbTree node of the tree

            while(temp != sentinelNode) {
                // find Parent
                node.Parent = temp;
                result = key.CompareTo(temp.Key);
                if(result == 0)
                    throw new ArgumentException("Key duplicated");
                if(result > 0)
                    temp = temp.Right;
                else
                    temp = temp.Left;
            }
            
            // setup node
            node.Key = key;
            node.Data = data;
            node.Left = sentinelNode;
            node.Right = sentinelNode;

            // insert node into tree starting at parent's location
            if(node.Parent != null) {
                result = node.Key.CompareTo(node.Parent.Key);
                if(result > 0)
                    node.Parent.Right = node;
                else
                    node.Parent.Left = node;
            }
            else
                rbTree = node; // first node added

            RestoreAfterInsert(node); // restore red-black properities

            lastNodeFound = node;
            
            intCount = intCount + 1;
        }
        ///<summary>
        /// RestoreAfterInsert
        /// Additions to red-black trees usually destroy the red-black 
        /// properties. Examine the tree and restore. Rotations are normally 
        /// required to restore it
        ///</summary>
        private void RestoreAfterInsert(OrderedTreeNode x) {   
            // x and y are used as variable names for brevity, in a more formal
            // implementation, you should probably change the names

            OrderedTreeNode y;

            // maintain red-black tree properties after adding x
            while(x != rbTree && x.Parent.Color == OrderedTreeNode.RED) {
                // Parent node is .Colored red; 
                if(x.Parent == x.Parent.Parent.Left) { // determine traversal path         
                                                        // is it on the Left or Right subtree?
                    y = x.Parent.Parent.Right; // get uncle
                    if(y!= null && y.Color == OrderedTreeNode.RED) {
                        // uncle is red; change x's Parent and uncle to black
                        x.Parent.Color = OrderedTreeNode.BLACK;
                        y.Color = OrderedTreeNode.BLACK;
                        // grandparent must be red. Why? Every red node that is not 
                        // a leaf has only black children 
                        x.Parent.Parent.Color = OrderedTreeNode.RED; 
                        x = x.Parent.Parent; // continue loop with grandparent
                    }   
                    else {
                        // uncle is black; determine if x is greater than Parent
                        if(x == x.Parent.Right) { 
                            // yes, x is greater than Parent; rotate Left
                            // make x a Left child
                            x = x.Parent;
                            RotateLeft(x);
                        }
                        // no, x is less than Parent
                        x.Parent.Color = OrderedTreeNode.BLACK; // make Parent black
                        x.Parent.Parent.Color = OrderedTreeNode.RED; // make grandparent black
                        RotateRight(x.Parent.Parent); // rotate right
                    }
                }
                else {
                    // x's Parent is on the Right subtree
                    // this code is the same as above with "Left" and "Right" swapped
                    y = x.Parent.Parent.Left;
                    if(y!= null && y.Color == OrderedTreeNode.RED) {
                        x.Parent.Color = OrderedTreeNode.BLACK;
                        y.Color = OrderedTreeNode.BLACK;
                        x.Parent.Parent.Color = OrderedTreeNode.RED;
                        x = x.Parent.Parent;
                    }
                    else {
                        if(x == x.Parent.Left) {
                            x = x.Parent;
                            RotateRight(x);
                        }
                        x.Parent.Color = OrderedTreeNode.BLACK;
                        x.Parent.Parent.Color = OrderedTreeNode.RED;
                        RotateLeft(x.Parent.Parent);
                    }
                }                                                                                                                   
            }
            rbTree.Color = OrderedTreeNode.BLACK; // rbTree should always be black
        }
        
        ///<summary>
        /// RotateLeft
        /// Rebalance the tree by rotating the nodes to the left
        ///</summary>
        virtual public void RotateLeft(OrderedTreeNode x) {
            // pushing node x down and to the Left to balance the tree. x's Right child (y)
            // replaces x (since y > x), and y's Left child becomes x's Right child 
            // (since it's < y but > x).
            
            OrderedTreeNode y = x.Right; // get x's Right node, this becomes y

            // set x's Right link
            x.Right = y.Left; // y's Left child's becomes x's Right child

            // modify parents
            if(y.Left != sentinelNode) 
                y.Left.Parent = x; // sets y's Left Parent to x

            if(y != sentinelNode)
                y.Parent = x.Parent; // set y's Parent to x's Parent

            if(x.Parent != null) {      
                // determine which side of it's Parent x was on
                if(x == x.Parent.Left)          
                    x.Parent.Left = y; // set Left Parent to y
                else
                    x.Parent.Right = y; // set Right Parent to y
            } 
            else 
                rbTree = y; // at rbTree, set it to y

            // link x and y 
            y.Left = x; // put x on y's Left 
            if(x != sentinelNode) // set y as x's Parent
                x.Parent = y;       
        }
        ///<summary>
        /// RotateRight
        /// Rebalance the tree by rotating the nodes to the right
        ///</summary>
        virtual public void RotateRight(OrderedTreeNode x) {
            // pushing node x down and to the Right to balance the tree. x's Left child (y)
            // replaces x (since x < y), and y's Right child becomes x's Left child 
            // (since it's < x but > y).
            
            OrderedTreeNode y = x.Left; // get x's Left node, this becomes y

            // set x's Right link
            x.Left = y.Right; // y's Right child becomes x's Left child

            // modify parents
            if(y.Right != sentinelNode) 
                y.Right.Parent = x; // sets y's Right Parent to x

            if(y != sentinelNode)
                y.Parent = x.Parent; // set y's Parent to x's Parent

            if(x.Parent != null) { // null=rbTree, could also have used rbTree
                // determine which side of it's Parent x was on
                if(x == x.Parent.Right)         
                    x.Parent.Right = y; // set Right Parent to y
                else
                    x.Parent.Left = y; // set Left Parent to y
            } 
            else 
                rbTree = y; // at rbTree, set it to y

            // link x and y 
            y.Right = x; // put x on y's Right
            if(x != sentinelNode) // set y as x's Parent
                x.Parent = y;       
        }
        
        virtual public bool ContainsKey(IComparable key) {
            OrderedTreeNode treeNode = rbTree; // begin at root
            int result = 0;
            // traverse tree until node is found
            while(treeNode != sentinelNode) {
                result = key.CompareTo(treeNode.Key);
                if(result == 0) {
                    lastNodeFound = treeNode;
                    return true;
                }
                if(result < 0)
                    treeNode = treeNode.Left;
                else
                    treeNode = treeNode.Right;
            }
            return false;
        }

        ///<summary>
        /// GetData
        /// Gets the data object associated with the specified key
        ///<summary>
        virtual public object GetData(IComparable key) {
            if(key == null)
                throw new ArgumentNullException("key");
            int result;
            
            OrderedTreeNode treeNode = rbTree; // begin at root
            
            // traverse tree until node is found
            while(treeNode != sentinelNode) {
                result = key.CompareTo(treeNode.Key);
                if(result == 0) {
                    lastNodeFound = treeNode;
                    return treeNode.Data;
                }
                if(result < 0)
                    treeNode = treeNode.Left;
                else
                    treeNode = treeNode.Right;
            }
            return null;
        }
        ///<summary>
        /// GetMinKey
        /// Returns the minimum key value
        ///<summary>
        virtual public IComparable GetMinKey() {
            OrderedTreeNode treeNode = rbTree;
            
            if(treeNode == null || treeNode == sentinelNode)
                throw(new InvalidOperationException("Tree is empty"));
            
            // traverse to the extreme left to find the smallest key
            while(treeNode.Left != sentinelNode)
                treeNode = treeNode.Left;
            
            lastNodeFound = treeNode;
            
            return treeNode.Key;
            
        }
        ///<summary>
        /// GetMaxKey
        /// Returns the maximum key value
        ///<summary>
        virtual public IComparable GetMaxKey() {
            OrderedTreeNode treeNode = rbTree;
            
            if(treeNode == null || treeNode == sentinelNode)
                throw(new InvalidOperationException("Tree is empty"));

            // traverse to the extreme right to find the largest key
            while(treeNode.Right != sentinelNode)
                treeNode = treeNode.Right;

            lastNodeFound = treeNode;

            return treeNode.Key;
            
        }
        ///<summary>
        /// GetMinValue
        /// Returns the object having the minimum key value
        ///<summary>
        virtual public object GetMinValue() {
            return GetData(GetMinKey());
        }
        ///<summary>
        /// GetMaxValue
        /// Returns the object having the maximum key
        ///<summary>
        virtual public object GetMaxValue() {
            return GetData(GetMaxKey());
        }
        ///<summary>
        /// GetEnumerator
        /// return an enumerator that returns the tree nodes in order
        ///<summary>
        virtual public OrderedTreeEnumerator GetEnumerator() {
            // elements is simply a generic name to refer to the 
            // data objects the nodes contain
            return Elements(true);      
        }
        ///<summary>
        /// Keys
        /// if(ascending is true, the keys will be returned in ascending order, else
        /// the keys will be returned in descending order.
        ///<summary>
        virtual public OrderedTreeEnumerator Keys {
            get {
                return KeyElements(true);
            }
        }
        virtual public OrderedTreeEnumerator KeyElements(bool ascending) {
            return new OrderedTreeEnumerator(rbTree, true, ascending, sentinelNode);
        }
        ///<summary>
        /// Values
        /// Provided for .NET compatibility. 
        ///<summary>
        virtual public OrderedTreeEnumerator Values {
            get {
                return Elements(true);
            }
        }
        ///<summary>
        /// Elements
        /// Returns an enumeration of the data objects.
        /// if(ascending is true, the objects will be returned in ascending order,
        /// else the objects will be returned in descending order.
        ///<summary>
        virtual public OrderedTreeEnumerator Elements() {
            return Elements(true);
        }
        virtual public OrderedTreeEnumerator Elements(bool ascending) {
            return new OrderedTreeEnumerator(rbTree, false, ascending, sentinelNode);
        }
        ///<summary>
        /// IsEmpty
        /// Is the tree empty?
        ///<summary>
        virtual public bool IsEmpty() {
            return (rbTree == null || rbTree == sentinelNode);
        }
        ///<summary>
        /// Remove
        /// removes the key and data object (delete)
        ///<summary>
        virtual public void Remove(IComparable key) {
            if(key == null)
                throw new ArgumentNullException("key");
        
            // find node
            int result;
            OrderedTreeNode node;

            // see if node to be deleted was the last one found,
            //check for null to avoid null reference exception
            result = lastNodeFound.Key == null ? -1 : key.CompareTo(lastNodeFound.Key);
            if(result == 0)
                node = lastNodeFound;
            else {
                // not found, must search       
                node = rbTree;
                while(node != sentinelNode) {
                    result = key.CompareTo(node.Key);
                    if(result == 0)
                        break;
                    if(result < 0)
                        node = node.Left;
                    else
                        node = node.Right;
                }

                if(node == sentinelNode)
                    return; // key not found
            }

            Delete(node);
            
            intCount = intCount - 1;
        }
        ///<summary>
        /// Delete
        /// Delete a node from the tree and restore red black properties
        ///<summary>
        private void Delete(OrderedTreeNode z) {
            // A node to be deleted will be: 
            // 1. a leaf with no children
            // 2. have one child
            // 3. have two children
            // If the deleted node is red, the red black properties still hold.
            // If the deleted node is black, the tree needs rebalancing

            OrderedTreeNode x = new OrderedTreeNode(); // work node to contain the replacement node
            OrderedTreeNode y; // work node 

            // find the replacement node (the successor to x) - the node one with 
            // at *most* one child. 
            if(z.Left == sentinelNode || z.Right == sentinelNode) 
                y = z; // node has sentinel as a child
            else {
                // z has two children, find replacement node which will 
                // be the leftmost node greater than z
                y = z.Right; // traverse right subtree   
                while(y.Left != sentinelNode) // to find next node in sequence
                    y = y.Left;
            }

            // at this point, y contains the replacement node. it's content will be copied 
            // to the valules in the node to be deleted

            // x (y's only child) is the node that will be linked to y's old parent. 
            if(y.Left != sentinelNode)
                x = y.Left;                 
            else
                x = y.Right;                    

            // replace x's parent with y's parent and
            // link x to proper subtree in parent
            // this removes y from the chain
            x.Parent = y.Parent;
            if(y.Parent != null)
                if(y == y.Parent.Left)
                    y.Parent.Left = x;
                else
                    y.Parent.Right = x;
            else
                rbTree = x; // make x the root node

            // copy the values from y (the replacement node) to the node being deleted.
            // note: this effectively deletes the node. 
            if(y != z) {
                z.Key = y.Key;
                z.Data = y.Data;
            }

            if(y.Color == OrderedTreeNode.BLACK)
                RestoreAfterDelete(x);

            lastNodeFound = sentinelNode;
        }

        ///<summary>
        /// RestoreAfterDelete
        /// Deletions from red-black trees may destroy the red-black 
        /// properties. Examine the tree and restore. Rotations are normally 
        /// required to restore it
        ///</summary>
        private void RestoreAfterDelete(OrderedTreeNode x) {
            // maintain Red-Black tree balance after deleting node          

            OrderedTreeNode y;

            while(x != rbTree && x.Color == OrderedTreeNode.BLACK) {
                if(x == x.Parent.Left) { // determine sub tree from parent
                    y = x.Parent.Right; // y is x's sibling 
                    if(y.Color == OrderedTreeNode.RED) { 
                        // x is black, y is red - make both black and rotate
                        y.Color = OrderedTreeNode.BLACK;
                        x.Parent.Color = OrderedTreeNode.RED;
                        RotateLeft(x.Parent);
                        y = x.Parent.Right;
                    }
                    if(y.Left.Color == OrderedTreeNode.BLACK && 
                        y.Right.Color == OrderedTreeNode.BLACK) { 
                        // children are both black
                        y.Color = OrderedTreeNode.RED; // change parent to red
                        x = x.Parent; // move up the tree
                    } 
                    else {
                        if(y.Right.Color == OrderedTreeNode.BLACK) {
                            y.Left.Color = OrderedTreeNode.BLACK;
                            y.Color = OrderedTreeNode.RED;
                            RotateRight(y);
                            y = x.Parent.Right;
                        }
                        y.Color = x.Parent.Color;
                        x.Parent.Color = OrderedTreeNode.BLACK;
                        y.Right.Color = OrderedTreeNode.BLACK;
                        RotateLeft(x.Parent);
                        x = rbTree;
                    }
                } 
                else { 
                    // right subtree - same as code above with right and left swapped
                    y = x.Parent.Left;
                    if(y.Color == OrderedTreeNode.RED) {
                        y.Color = OrderedTreeNode.BLACK;
                        x.Parent.Color = OrderedTreeNode.RED;
                        RotateRight (x.Parent);
                        y = x.Parent.Left;
                    }
                    if(y.Right.Color == OrderedTreeNode.BLACK && 
                        y.Left.Color == OrderedTreeNode.BLACK) {
                        y.Color = OrderedTreeNode.RED;
                        x = x.Parent;
                    } 
                    else {
                        if(y.Left.Color == OrderedTreeNode.BLACK) {
                            y.Right.Color = OrderedTreeNode.BLACK;
                            y.Color = OrderedTreeNode.RED;
                            RotateLeft(y);
                            y = x.Parent.Left;
                        }
                        y.Color = x.Parent.Color;
                        x.Parent.Color = OrderedTreeNode.BLACK;
                        y.Left.Color = OrderedTreeNode.BLACK;
                        RotateRight(x.Parent);
                        x = rbTree;
                    }
                }
            }
            x.Color = OrderedTreeNode.BLACK;
        }
        
        ///<summary>
        /// RemoveMin
        /// removes the node with the minimum key
        ///<summary>
        virtual public void RemoveMin() {
            if(rbTree == null || rbTree == sentinelNode)
                return;
            Remove(GetMinKey());
        }
        ///<summary>
        /// RemoveMax
        /// removes the node with the maximum key
        ///<summary>
        virtual public void RemoveMax() {
            if(rbTree == null || rbTree == sentinelNode)
                return;
            Remove(GetMaxKey());
        }
        ///<summary>
        /// Clear
        /// Empties or clears the tree
        ///<summary>
        virtual public void Clear () {
            rbTree = sentinelNode;
            intCount = 0;
        }

        virtual public int Count {
            get {
                return intCount;
            }
        }
    }

    public class OrderedTreeEnumerator : IEnumerator {
        // the treap uses the stack to order the nodes
        private Stack stack;
        // return the keys
        private bool keys;
        // return in ascending order (true) or descending (false)
        private bool ascending;
        private OrderedTreeNode tnode;
        private OrderedTreeNode sentinelNode;
        bool pre = true;
        
        // key
        private IComparable ordKey;
        // the data or value associated with the key
        private object objValue;

        ///<summary>
        ///Key
        ///</summary>
        virtual public IComparable Key {
            get {
                return ordKey;
            }
            
            set {
                ordKey = value;
            }
        }
        ///<summary>
        ///Data
        ///</summary>
        virtual public object Value {
            get {
                return objValue;
            }
            
            set {
                objValue = value;
            }
        }
        
        private OrderedTreeEnumerator() {
        }
        ///<summary>
        /// Determine order, walk the tree and push the nodes onto the stack
        ///</summary>
        public OrderedTreeEnumerator(OrderedTreeNode tnode, bool keys, bool ascending, OrderedTreeNode sentinelNode) {
            this.sentinelNode = sentinelNode;
            stack = new Stack();
            this.keys = keys;
            this.ascending = ascending;
            this.tnode = tnode;
            Reset();            
        }

        virtual public void Reset() {
            pre = true;
            stack.Clear();
            // use depth-first traversal to push nodes into stack
            // the lowest node will be at the top of the stack
            if(ascending) {
                // find the lowest node
                while(tnode != sentinelNode) {
                    stack.Push(tnode);
                    tnode = tnode.Left;
                }
            }
            else {
                // the highest node will be at top of stack
                while(tnode != sentinelNode) {
                    stack.Push(tnode);
                    tnode = tnode.Right;
                }
            }
        }

        virtual public object Current {
            get {
                if (pre)
                    throw new InvalidOperationException("Current");
                return keys ? Key : Value;
            }
        }

        ///<summary>
        /// HasMoreElements
        ///</summary>
        virtual public bool HasMoreElements() {
            return (stack.Count > 0);
        }
        ///<summary>
        /// NextElement
        ///</summary>
        virtual public object NextElement() {
            if(stack.Count == 0)

                throw new InvalidOperationException("Element not found");
            
            // the top of stack will always have the next item
            // get top of stack but don't remove it as the next nodes in sequence
            // may be pushed onto the top
            // the stack will be popped after all the nodes have been returned
            OrderedTreeNode node = (OrderedTreeNode) stack.Peek(); //next node in sequence
            
            if(ascending) {
                if(node.Right == sentinelNode) {   
                    // yes, top node is lowest node in subtree - pop node off stack 
                    OrderedTreeNode tn = (OrderedTreeNode) stack.Pop();
                    // peek at right node's parent 
                    // get rid of it if it has already been used
                    while(HasMoreElements()&& ((OrderedTreeNode) stack.Peek()).Right == tn)
                        tn = (OrderedTreeNode) stack.Pop();
                }
                else {
                    // find the next items in the sequence
                    // traverse to left; find lowest and push onto stack
                    OrderedTreeNode tn = node.Right;
                    while(tn != sentinelNode) {
                        stack.Push(tn);
                        tn = tn.Left;
                    }
                }
            }
            else { // descending, same comments as above apply
                if(node.Left == sentinelNode) {
                    // walk the tree
                    OrderedTreeNode tn = (OrderedTreeNode) stack.Pop();
                    while(HasMoreElements() && ((OrderedTreeNode)stack.Peek()).Left == tn)
                        tn = (OrderedTreeNode) stack.Pop();
                }
                else {
                    // determine next node in sequence
                    // traverse to left subtree and find greatest node - push onto stack
                    OrderedTreeNode tn = node.Left;
                    while(tn != sentinelNode) {
                        stack.Push(tn);
                        tn = tn.Right;
                    }
                }
            }
            
            // the following is for .NET compatibility (see MoveNext())
            Key = node.Key;
            Value = node.Data;
            // ******** testing only ********

            return keys ? node.Key : node.Data;         
        }
        ///<summary>
        /// MoveNext
        /// For .NET compatibility
        ///</summary>
        virtual public bool MoveNext() {
            if(HasMoreElements()) {
                NextElement();
                pre = false;
                return true;
            }
            pre = true;
            return false;
        }

        virtual public OrderedTreeEnumerator GetEnumerator() {
            return this;
        }
    }

    public class OrderedTreeNode {
        // tree node colors
        public const bool RED = false;
        public const bool BLACK = true;

        // key provided by the calling class
        private IComparable ordKey;
        // the data or value associated with the key
        private object objData;
        // color - used to balance the tree
        private bool intColor;
        // left node 
        private OrderedTreeNode rbnLeft;
        // right node 
        private OrderedTreeNode rbnRight;
        // parent node 
        private OrderedTreeNode rbnParent;
        
        ///<summary>
        ///Key
        ///</summary>
        virtual public IComparable Key {
            get {
                return ordKey;
            }
            
            set {
                ordKey = value;
            }
        }
        ///<summary>
        ///Data
        ///</summary>
        virtual public object Data {
            get {
                return objData;
            }
            
            set {
                objData = value;
            }
        }
        ///<summary>
        ///Color
        ///</summary>
        virtual public bool Color {
            get {
                return intColor;
            }
            
            set {
                intColor = value;
            }
        }
        ///<summary>
        ///Left
        ///</summary>
        virtual public OrderedTreeNode Left {
            get {
                return rbnLeft;
            }
            
            set {
                rbnLeft = value;
            }
        }
        ///<summary>
        /// Right
        ///</summary>
        virtual public OrderedTreeNode Right {
            get {
                return rbnRight;
            }
            
            set {
                rbnRight = value;
            }
        }
        virtual public OrderedTreeNode Parent {
            get {
                return rbnParent;
            }
            
            set {
                rbnParent = value;
            }
        }

        public OrderedTreeNode() {
            Color = RED;
        }
    }
}
