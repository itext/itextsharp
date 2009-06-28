using System;
using System.IO;
using System.Collections;
using iTextSharp.text;
using iTextSharp.text.rtf;
using iTextSharp.text.rtf.document;
using iTextSharp.text.rtf.style;
using iTextSharp.text.rtf.text;
using iTextSharp.text.factories;
/*
 * $Id: RtfList.cs,v 1.18 2008/05/16 19:31:01 psoares33 Exp $
 * 
 *
 * Copyright 2001, 2002, 2003, 2004, 2005 by Mark Hall
 *
 * The contents of this file are subject to the Mozilla Public License Version 1.1
 * (the "License"); you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.mozilla.org/MPL/
 *
 * Software distributed under the License is distributed on an "AS IS" basis,
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
 * for the specific language governing rights and limitations under the License.
 *
 * The Original Code is 'iText, a free JAVA-PDF library'.
 *
 * The Initial Developer of the Original Code is Bruno Lowagie. Portions created by
 * the Initial Developer are Copyright (C) 1999, 2000, 2001, 2002 by Bruno Lowagie.
 * All Rights Reserved.
 * Co-Developer of the code is Paulo Soares. Portions created by the Co-Developer
 * are Copyright (C) 2000, 2001, 2002 by Paulo Soares. All Rights Reserved.
 *
 * Contributor(s): all the names of the contributors are added in the source code
 * where applicable.
 *
 * Alternatively, the contents of this file may be used under the terms of the
 * LGPL license (the 'GNU LIBRARY GENERAL PUBLIC LICENSE'), in which case the
 * provisions of LGPL are applicable instead of those above.  If you wish to
 * allow use of your version of this file only under the terms of the LGPL
 * License and not to allow others to use your version of this file under
 * the MPL, indicate your decision by deleting the provisions above and
 * replace them with the notice and other provisions required by the LGPL.
 * If you do not delete the provisions above, a recipient may use your version
 * of this file under either the MPL or the GNU LIBRARY GENERAL PUBLIC LICENSE.
 *
 * This library is free software; you can redistribute it and/or modify it
 * under the terms of the MPL as stated above or under the terms of the GNU
 * Library General Public License as published by the Free Software Foundation;
 * either version 2 of the License, or any later version.
 *
 * This library is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 * FOR A PARTICULAR PURPOSE. See the GNU Library general Public License for more
 * details.
 *
 * If you didn't download this code from the following link, you should check if
 * you aren't using an obsolete version:
 * http://www.lowagie.com/iText/
 */

namespace iTextSharp.text.rtf.list {

    /**
    * The RtfList stores one List. It also provides the methods to write the
    * list declaration and the list data.
    *  
    * @version $Id: RtfList.cs,v 1.18 2008/05/16 19:31:01 psoares33 Exp $
    * @author Mark Hall (Mark.Hall@mail.room3b.eu)
    * @author Thomas Bickel (tmb99@inode.at)
    * @author Felix Satyaputra (f_satyaputra@yahoo.co.uk)
    */
    public class RtfList : RtfElement, IRtfExtendedElement {

        /**
        * Constant for the list number
        * @since 2.1.3
        */
        public static readonly byte[] LIST_NUMBER = DocWriter.GetISOBytes("\\ls");

        /**
        * Constant for the list
        */
        private static readonly byte[] LIST = DocWriter.GetISOBytes("\\list");
        /**
        * Constant for the list id
        * @since 2.1.3
        */
        public static readonly byte[] LIST_ID = DocWriter.GetISOBytes("\\listid");
        /**
        * Constant for the list template id
        */
        private static readonly byte[] LIST_TEMPLATE_ID = DocWriter.GetISOBytes("\\listtemplateid");
        /**
        * Constant for the simple list
        */
        private static readonly byte[] LIST_SIMPLE = DocWriter.GetISOBytes("\\listsimple");
        /**
        * Constant for the hybrid list
        */
        private static readonly byte[] LIST_HYBRID = DocWriter.GetISOBytes("\\listhybrid");
        /**
        * Constant to indicate if the list restarts at each section. Word 7 compatiblity
        */
        private static readonly byte[] LIST_RESTARTHDN = DocWriter.GetISOBytes("\\listrestarthdn");
        /**
        * Constant for the name of this list
        */
        private static readonly byte[] LIST_NAME = DocWriter.GetISOBytes("\\listname");
        /**
        * Constant for the identifier of the style of this list. Mutually exclusive with \\liststylename
        */
        private static readonly byte[] LIST_STYLEID = DocWriter.GetISOBytes("\\liststyleid");
        /**
        * Constant for the identifier of the style of this list. Mutually exclusive with \\liststyleid
        */
        private static readonly byte[] LIST_STYLENAME = DocWriter.GetISOBytes("\\liststylename");

        // character properties
        /**
        * Constant for the list level value
        * @since 2.1.3
        */
        public static readonly byte[] LIST_LEVEL_NUMBER = DocWriter.GetISOBytes("\\ilvl");
        
        
        /**
        * Constant for the old list text
        * @since 2.1.3
        */
        public static readonly byte[] LIST_TEXT = DocWriter.GetISOBytes("\\listtext");
        /**
        * Constant for the old list number end
        * @since 2.1.3
        */
        public static readonly byte[] LIST_NUMBER_END = DocWriter.GetISOBytes(".");
        
        

        /**
        * Constant for a tab character
        * @since 2.1.3
        */
        public static readonly byte[] TAB = DocWriter.GetISOBytes("\\tab");
        
        /**
        * The subitems of this RtfList
        */
        private ArrayList items;
        
        /**
        * The parent list if there is one.
        */
        private RtfList parentList = null;

        /**
        * The list id
        */
        private int listID = -1;
        
        /**
        * List type of NORMAL - no control word
        * @since 2.1.3
        */
        public const int LIST_TYPE_NORMAL = 0;               /*  Normal list type */
        
        /**
        * List type of listsimple
        * @since 2.1.3
        */
        public const int LIST_TYPE_SIMPLE = 1;               /*  Simple list type */
        
        /**
        * List type of listhybrid
        * @since 2.1.3
        */
        public const int LIST_TYPE_HYBRID = 2;               /*  Hybrid list type */
        
        /**
        * This RtfList type
        */
        private int listType = LIST_TYPE_HYBRID;
        
        /**
        * The name of the list if it exists 
        */
        private String name = null;
        
        /**
        * The list number of this RtfList
        */
        private int listNumber = -1;

        /**
        * The RtfList lists managed by this RtfListTable
        */
        private ArrayList listLevels = null;

        
        /**
        * Constructs an empty RtfList object.
        * @since 2.1.3
        */
        public RtfList() : base(null) {
            CreateDefaultLevels();
        }
        
        /**
        * Set the document.
        * @param doc The RtfDocument
        * @since 2.1.3
        */
        public void SetDocument(RtfDocument doc) {
            this.document = doc;
            // get the list number or create a new one adding it to the table
            this.listNumber = document.GetDocumentHeader().GetListNumber(this); 

            
        }
        /**
        * Constructs an empty RtfList object.
        * @param doc The RtfDocument this RtfList belongs to
        * @since 2.1.3
        */
        public RtfList(RtfDocument doc) : base(doc) {
            CreateDefaultLevels();
            // get the list number or create a new one adding it to the table
            this.listNumber = document.GetDocumentHeader().GetListNumber(this); 

        }

        
        /**
        * Constructs a new RtfList for the specified List.
        * 
        * @param doc The RtfDocument this RtfList belongs to
        * @param list The List this RtfList is based on
        * @since 2.1.3
        */
        public RtfList(RtfDocument doc, List list) : base(doc) {
            // setup the listlevels
            // Then, setup the list data below
            
            // setup 1 listlevel if it's a simple list
            // setup 9 if it's a regular list
            // setup 9 if it's a hybrid list (default)
            CreateDefaultLevels();
            
            this.items = new ArrayList();       // list content
            RtfListLevel ll = (RtfListLevel)this.listLevels[0];
            
            // get the list number or create a new one adding it to the table
            this.listNumber = document.GetDocumentHeader().GetListNumber(this); 
            
            if (list.SymbolIndent > 0 && list.IndentationLeft > 0) {
                ll.SetFirstIndent((int) (list.SymbolIndent * RtfElement.TWIPS_FACTOR * -1));
                ll.SetLeftIndent((int) ((list.IndentationLeft + list.SymbolIndent) * RtfElement.TWIPS_FACTOR));
            } else if (list.SymbolIndent > 0) {
                ll.SetFirstIndent((int) (list.SymbolIndent * RtfElement.TWIPS_FACTOR * -1));
                ll.SetLeftIndent((int) (list.SymbolIndent * RtfElement.TWIPS_FACTOR));
            } else if (list.IndentationLeft > 0) {
                ll.SetFirstIndent(0);
                ll.SetLeftIndent((int) (list.IndentationLeft * RtfElement.TWIPS_FACTOR));
            } else {
                ll.SetFirstIndent(0);
                ll.SetLeftIndent(0);
            }
            ll.SetRightIndent((int) (list.IndentationRight * RtfElement.TWIPS_FACTOR));
            ll.SetSymbolIndent((int) ((list.SymbolIndent + list.IndentationLeft) * RtfElement.TWIPS_FACTOR));
            ll.CorrectIndentation();
            ll.SetTentative(false);
            
            if (list is RomanList) {
                if (list.Lowercase) {
                    ll.SetListType(RtfListLevel.LIST_TYPE_LOWER_ROMAN);
                } else {
                    ll.SetListType(RtfListLevel.LIST_TYPE_UPPER_ROMAN);
                }
            } else if (list.Numbered) {
                ll.SetListType(RtfListLevel.LIST_TYPE_NUMBERED);
            } else if (list.Lettered) {
                if (list.Lowercase) {
                    ll.SetListType(RtfListLevel.LIST_TYPE_LOWER_LETTERS);
                } else {
                    ll.SetListType(RtfListLevel.LIST_TYPE_UPPER_LETTERS);
                }
            } 
            else {
    //          Paragraph p = new Paragraph();
    //          p.Add(new Chunk(list.GetPreSymbol()) );
    //          p.Add(list.GetSymbol());
    //          p.Add(new Chunk(list.GetPostSymbol()) );
    //          ll.SetBulletChunk(list.GetSymbol());
                ll.SetBulletCharacter(list.PreSymbol + list.Symbol.Content + list.PostSymbol);
                ll.SetListType(RtfListLevel.LIST_TYPE_BULLET);
            }
            
            // now setup the actual list contents.
            for (int i = 0; i < list.Items.Count; i++) {
                try {
                    IElement element = (IElement) list.Items[i];
                    
                    if (element.Type == Element.CHUNK) {
                        element = new ListItem((Chunk) element);
                    }
                    if (element is ListItem) {
                        ll.SetAlignment(((ListItem) element).Alignment);
                    }
                    IRtfBasicElement[] rtfElements = doc.GetMapper().MapElement(element);
                    for (int j = 0; j < rtfElements.Length; j++) {
                        IRtfBasicElement rtfElement = rtfElements[j];
                        if (rtfElement is RtfList) {
                            ((RtfList) rtfElement).SetParentList(this);
                        } else if (rtfElement is RtfListItem) {
                            ((RtfListItem) rtfElement).SetParent(ll);
                        }
                        ll.SetFontNumber( new RtfFont(document, new Font(Font.TIMES_ROMAN, 10, Font.NORMAL, new Color(0, 0, 0))) );
                        if (list.Symbol != null && list.Symbol.Font != null && !list.Symbol.Content.StartsWith("-") && list.Symbol.Content.Length > 0) {
                            // only set this to bullet symbol is not default
                            ll.SetBulletFont( list.Symbol.Font);
                            ll.SetBulletCharacter(list.Symbol.Content.Substring(0, 1));
                        } else
                        if (list.Symbol != null && list.Symbol.Font != null) {
                            ll.SetBulletFont(list.Symbol.Font);
                         
                        } else {
                            ll.SetBulletFont(new Font(Font.SYMBOL, 10, Font.NORMAL, new Color(0, 0, 0)));
                        } 
                        items.Add(rtfElement);
                    }

                } catch (DocumentException) {
                }
            }
        }
        
        /**
        * Writes the definition part of this list level
        * @param result
        * @throws IOException
        * @since 2.1.3
        */
        public void WriteDefinition(Stream result)
        {
            byte[] t;
            result.Write(OPEN_GROUP, 0, OPEN_GROUP.Length);
            result.Write(LIST, 0, LIST.Length);
            result.Write(LIST_TEMPLATE_ID, 0, LIST_TEMPLATE_ID.Length);
            result.Write(t = IntToByteArray(document.GetRandomInt()), 0, t.Length);

            int levelsToWrite = -1;
            
            switch (this.listType) {
            case LIST_TYPE_NORMAL:
                levelsToWrite = listLevels.Count;
                break;
            case LIST_TYPE_SIMPLE:
                result.Write(LIST_SIMPLE, 0, LIST_SIMPLE.Length);
                result.Write(t = IntToByteArray(1), 0, t.Length); 
                levelsToWrite = 1;
                break;
            case LIST_TYPE_HYBRID:
                result.Write(LIST_HYBRID, 0, LIST_HYBRID.Length);
                levelsToWrite = listLevels.Count;
                break;
            default:
                break;
            }
            this.document.OutputDebugLinebreak(result);

            // TODO: Figure out hybrid because multi-level hybrid does not work.
            // Seems hybrid is mixed type all single level - Simple = single level
            // SIMPLE1/HYRBID
            // 1. Line 1
            // 2. Line 2
            // MULTI-LEVEL LISTS Are Simple0 - 9 levels (0-8) all single digit
            // 1. Line 1
            // 1.1. Line 1.1
            // 1.2. Line 1.2
            // 2. Line 2
             
            // write the listlevels here
            for (int i = 0; i<levelsToWrite; i++) {
                ((RtfListLevel)listLevels[i]).WriteDefinition(result);
                this.document.OutputDebugLinebreak(result);
            }
            
            result.Write(LIST_ID, 0, LIST_ID.Length);
            result.Write(t = IntToByteArray(this.listID), 0, t.Length);
            result.Write(CLOSE_GROUP, 0, CLOSE_GROUP.Length);
            this.document.OutputDebugLinebreak(result);
            if (items != null) {
            for (int i = 0; i < items.Count; i++) {
                RtfElement rtfElement = (RtfElement) items[i];
                if (rtfElement is RtfList) {
                    RtfList rl = (RtfList)rtfElement;
                    rl.WriteDefinition(result);
                    break;
                } else if (rtfElement is RtfListItem) {
                    RtfListItem rli = (RtfListItem) rtfElement;
                    if (rli.WriteDefinition(result)) break;
                }
            }    
            }
        }
        
        /**
        * Writes the content of the RtfList
        * @since 2.1.3
        */    
        public override void WriteContent(Stream result)
        {
            if (!this.inTable) {
                result.Write(OPEN_GROUP, 0, OPEN_GROUP.Length);
            }
            
            int itemNr = 0;
            if (items != null) {
            for (int i = 0; i < items.Count; i++) {
                
                RtfElement thisRtfElement = (RtfElement) items[i];
            //thisRtfElement.WriteContent(result);
                if (thisRtfElement is RtfListItem) {
                    itemNr++;
                    RtfListItem rtfElement = (RtfListItem)thisRtfElement;
                    RtfListLevel listLevel =  rtfElement.GetParent();
                    if (listLevel.GetListLevel() == 0) {
                        CorrectIndentation();
                    }
                    
                    if (i == 0) {
                        listLevel.WriteListBeginning(result);
                        WriteListNumbers(result);
                    }

                    WriteListTextBlock(result, itemNr, listLevel);
                    
                    rtfElement.WriteContent(result);
                    
                    if (i < (items.Count - 1) || !this.inTable || listLevel.GetListType() > 0) { // TODO Fix no paragraph on last list item in tables
                        result.Write(RtfParagraph.PARAGRAPH, 0, RtfParagraph.PARAGRAPH.Length);
                    }
                    this.document.OutputDebugLinebreak(result);
                } else if (thisRtfElement is RtfList) {
                    ((RtfList)thisRtfElement).WriteContent(result);
    //              ((RtfList)thisRtfElement).WriteListBeginning(result);
                    WriteListNumbers(result);
                    this.document.OutputDebugLinebreak(result);
                }
            }
            }
            if (!this.inTable) {
                result.Write(CLOSE_GROUP, 0, CLOSE_GROUP.Length);
                result.Write(RtfParagraph.PARAGRAPH_DEFAULTS, 0, RtfParagraph.PARAGRAPH_DEFAULTS.Length);
            }
        }        
        /**
        * 
        * @param result
        * @param itemNr
        * @param listLevel
        * @throws IOException
        * @since 2.1.3
        */
        protected void WriteListTextBlock(Stream result, int itemNr, RtfListLevel listLevel) {
            byte[] t;
            result.Write(OPEN_GROUP, 0, OPEN_GROUP.Length);
            result.Write(RtfList.LIST_TEXT, 0, RtfList.LIST_TEXT.Length);
            result.Write(RtfParagraph.PARAGRAPH_DEFAULTS, 0, RtfParagraph.PARAGRAPH_DEFAULTS.Length);
            if (this.inTable) {
                result.Write(RtfParagraph.IN_TABLE, 0, RtfParagraph.IN_TABLE.Length);
            }
            result.Write(RtfFontList.FONT_NUMBER, 0, RtfFontList.FONT_NUMBER.Length);
            if (listLevel.GetListType() != RtfListLevel.LIST_TYPE_BULLET) {
                result.Write(t = IntToByteArray(listLevel.GetFontNumber().GetFontNumber()), 0, t.Length);
            } else {
                result.Write(t = IntToByteArray(listLevel.GetFontBullet().GetFontNumber()), 0, t.Length);
            }
            listLevel.WriteIndentation(result);
            result.Write(DELIMITER, 0, DELIMITER.Length);
            if (listLevel.GetListType() != RtfListLevel.LIST_TYPE_BULLET) {
                switch (listLevel.GetListType()) {
                    case RtfListLevel.LIST_TYPE_NUMBERED      : result.Write(t = IntToByteArray(itemNr), 0, t.Length); break;
                    case RtfListLevel.LIST_TYPE_UPPER_LETTERS : result.Write(t = DocWriter.GetISOBytes(RomanAlphabetFactory.GetUpperCaseString(itemNr)), 0, t.Length); break;
                    case RtfListLevel.LIST_TYPE_LOWER_LETTERS : result.Write(t = DocWriter.GetISOBytes(RomanAlphabetFactory.GetLowerCaseString(itemNr)), 0, t.Length); break;
                    case RtfListLevel.LIST_TYPE_UPPER_ROMAN   : result.Write(t = DocWriter.GetISOBytes(RomanNumberFactory.GetUpperCaseString(itemNr)), 0, t.Length); break;
                    case RtfListLevel.LIST_TYPE_LOWER_ROMAN   : result.Write(t = DocWriter.GetISOBytes(RomanNumberFactory.GetLowerCaseString(itemNr)), 0, t.Length); break;
                }
                result.Write(LIST_NUMBER_END, 0, LIST_NUMBER_END.Length);
            } else {
                this.document.FilterSpecialChar(result, listLevel.GetBulletCharacter(), true, false);
            }
            result.Write(TAB, 0, TAB.Length);
            result.Write(CLOSE_GROUP, 0, CLOSE_GROUP.Length);
        }

        /**
        * Writes only the list number and list level number.
        * 
        * @param result The <code>Stream</code> to write to
        * @throws IOException On i/o errors.
        * @since 2.1.3
        */
        protected void WriteListNumbers(Stream result) {
            byte[] t;
            result.Write(RtfList.LIST_NUMBER, 0, RtfList.LIST_NUMBER.Length);
            result.Write(t = IntToByteArray(listNumber), 0, t.Length);
        }
        /**
        * Create a default set of listlevels
        * @since 2.1.3
        */
        protected void CreateDefaultLevels() {
            this.listLevels = new ArrayList();  // listlevels
            for (int i=0; i<=8; i++) {
                // create a list level
                RtfListLevel ll = new RtfListLevel(this.document);
                ll.SetListType(RtfListLevel.LIST_TYPE_NUMBERED);
                ll.SetFirstIndent(0);
                ll.SetLeftIndent(0);
                ll.SetLevelTextNumber(i);
                ll.SetTentative(true);
                ll.CorrectIndentation();
                this.listLevels.Add(ll);
            }

        }
        /**
        * Gets the id of this list
        * 
        * @return Returns the list number.
        * @since 2.1.3
        */
        public int GetListNumber() {
            return listNumber;
        }
        
        /**
        * Sets the id of this list
        * 
        * @param listNumber The list number to set.
        * @since 2.1.3
        */
        public void SetListNumber(int listNumber) {
            this.listNumber = listNumber;
        }
        
        /**
        * Sets whether this RtfList is in a table. Sets the correct inTable setting for all
        * child elements.
        * 
        * @param inTable <code>True</code> if this RtfList is in a table, <code>false</code> otherwise
        * @since 2.1.3
        */
        public override void SetInTable(bool inTable) {
            base.SetInTable(inTable);
            for (int i = 0; i < this.items.Count; i++) {
                ((IRtfBasicElement) this.items[i]).SetInTable(inTable);
            }
            for (int i = 0; i < this.listLevels.Count; i++) {
                ((RtfListLevel) this.listLevels[i]).SetInTable(inTable);
            }
        }
        
        /**
        * Sets whether this RtfList is in a header. Sets the correct inTable setting for all
        * child elements.
        * 
        * @param inHeader <code>True</code> if this RtfList is in a header, <code>false</code> otherwise
        * @since 2.1.3
        */
        public override void SetInHeader(bool inHeader) {
            base.SetInHeader(inHeader);
            for (int i = 0; i < this.items.Count; i++) {
                ((IRtfBasicElement) this.items[i]).SetInHeader(inHeader);
            }
        }

        /**
        * Correct the indentation of this RtfList by adding left/first line indentation
        * from the parent RtfList. Also calls correctIndentation on all child RtfLists.
        * @since 2.1.3
        */
        protected internal void CorrectIndentation() {
            // TODO: Fix
    //        if (this.parentList != null) {
    //            this.leftIndent = this.leftIndent + this.parentList.GetLeftIndent() + this.parentList.GetFirstIndent();
    //        }
            for (int i = 0; i < this.items.Count; i++) {
                if (this.items[i] is RtfList) {
                    ((RtfList) this.items[i]).CorrectIndentation();
                } else if (this.items[i] is RtfListItem) {
                    ((RtfListItem) this.items[i]).CorrectIndentation();
                }
            }
        }


        /**
        * Set the list ID number
        * @param id
        * @since 2.1.3
        */
        public void SetID(int id) {
            this.listID = id;
        }
        /**
        * Get the list ID number
        * @return this list id
        * @since 2.1.3
        */
        public int GetID() {
            return this.listID;
        }

        /**
        * @return the listType
        * @see RtfList#LIST_TYPE_NORMAL
        * @see RtfList#LIST_TYPE_SIMPLE
        * @see RtfList#LIST_TYPE_HYBRID
        * @since 2.1.3
        */
        public int GetListType() {
            return listType;
        }

        /**
        * @param listType the listType to set
        * @see RtfList#LIST_TYPE_NORMAL
        * @see RtfList#LIST_TYPE_SIMPLE
        * @see RtfList#LIST_TYPE_HYBRID
        * @since 2.1.3
        */
        public void SetListType(int listType) {
            if (listType == LIST_TYPE_NORMAL || 
                    listType == LIST_TYPE_SIMPLE || 
                    listType == LIST_TYPE_HYBRID ) {
                this.listType = listType;
            }
            else {
                throw new ArgumentException("Invalid listType value.");
            }
        }

        /**
        * @return the parentList
        * @since 2.1.3
        */
        public RtfList GetParentList() {
            return parentList;
        }

        /**
        * @param parentList the parentList to set
        * @since 2.1.3
        */
        public void SetParentList(RtfList parentList) {
            this.parentList = parentList;
        }

        /**
        * @return the name
        * @since 2.1.3
        */
        public String GetName() {
            return name;
        }

        /**
        * @param name the name to set
        * @since 2.1.3
        */
        public void SetName(String name) {
            this.name = name;
        }
        /**
        * @return the list at the index
        * @since 2.1.3
        */
        public RtfListLevel GetListLevel(int index) {
            if (listLevels != null) {
            return (RtfListLevel)this.listLevels[index];
            }
            else
                return null;
        }
    }
}