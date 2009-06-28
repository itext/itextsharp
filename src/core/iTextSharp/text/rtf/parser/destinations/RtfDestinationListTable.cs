using System;
using iTextSharp.text.rtf.parser;
using iTextSharp.text.rtf.list;
using iTextSharp.text.rtf.parser.ctrlwords;
/*
 * $Id: RtfDestinationListTable.cs,v 1.2 2008/05/13 11:26:00 psoares33 Exp $
 * 
 *
 * Copyright 2007 by Howard Shank (hgshank@yahoo.com)
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
 * the Initial Developer are Copyright (C) 1999-2006 by Bruno Lowagie.
 * All Rights Reserved.
 * Co-Developer of the code is Paulo Soares. Portions created by the Co-Developer
 * are Copyright (C) 2000-2006 by Paulo Soares. All Rights Reserved.
 *
 * Contributor(s): all the names of the contributors are added in the source code
 * where applicable.
 *
 * Alternatively, the contents of this file may be used under the terms of the
 * LGPL license (the ?GNU LIBRARY GENERAL PUBLIC LICENSE?), in which case the
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
 
namespace iTextSharp.text.rtf.parser.destinations {

    /**
    * <code>RtfDestinationListTable</code> handles data destined for the List Table destination
    * 
    * @author Howard Shank (hgshank@yahoo.com)
    *
    */
    public class RtfDestinationListTable : RtfDestination {
        /**
        * The RtfImportHeader to add List mappings to.
        */
        private RtfImportMgr importHeader = null;
        
        private RtfList newList = null;
        
        private int currentLevel = -1;
        private RtfListLevel currentListLevel = null;
        private int currentListMappingNumber = 0;
        private int currentSubGroupCount = 0;
            
        public RtfDestinationListTable() : base(null) {
        }
        
        public RtfDestinationListTable(RtfParser parser) : base(parser) {
            this.importHeader = parser.GetImportManager();
        }
        
        public override void SetParser(RtfParser parser) {
            this.rtfParser = parser;
            this.importHeader = parser.GetImportManager();
            this.SetToDefaults();
        }
        /* (non-Javadoc)
        * @see com.lowagie.text.rtf.parser.destinations.RtfDestination#handleOpenNewGroup()
        */
        public override bool HandleOpeningSubGroup() {
            this.currentSubGroupCount++;
            return true;
        }
        /* (non-Javadoc)
        * @see com.lowagie.text.rtf.direct.RtfDestination#closeDestination()
        */
        public override bool CloseDestination() {
            if (this.newList != null) {
                this.rtfParser.GetRtfDocument().Add(this.newList);
            }
            return true;
        }
        public override bool HandleControlWord(RtfCtrlWordData ctrlWordData) {
            bool result = true;
            bool skipCtrlWord = false;

            if (this.rtfParser.IsImport()) {
                skipCtrlWord = true;
                if (ctrlWordData.ctrlWord.Equals("listtable")) {
                    result = true;
                    this.currentListMappingNumber = 0;
                    
                } else
                    /* Picture info for icons/images for lists */
                    if (ctrlWordData.ctrlWord.Equals("listpicture"))/* DESTINATION */{
                    skipCtrlWord = true;
                    // this.rtfParser.SetTokeniserStateSkipGroup();
                    result = true;
                } else
                    /* list */
                    if (ctrlWordData.ctrlWord.Equals("list")) /* DESTINATION */{
                    skipCtrlWord = true;
                    this.newList = new RtfList(this.rtfParser.GetRtfDocument());
                    this.newList.SetListType(RtfList.LIST_TYPE_NORMAL); // set default
                    this.currentLevel = -1;
                    this.currentListMappingNumber++;
                    this.currentSubGroupCount = 0;
                    result = true;
                } else if (ctrlWordData.ctrlWord.Equals("listtemplateid")) /* // List item*/ {
                    // ignore this because it gets regenerated in every document
                    skipCtrlWord = true;
                    result = true;
                } else if (ctrlWordData.ctrlWord.Equals("listsimple")) /* // List item*/ {
                    // is value 0 or 1
                    if (ctrlWordData.hasParam && ctrlWordData.param == "1") {
                        this.newList.SetListType(RtfList.LIST_TYPE_SIMPLE);
                    } else {
                        this.newList.SetListType(RtfList.LIST_TYPE_NORMAL);
                    }
                    skipCtrlWord = true;
                    result = true;
                    // this gets set internally. Don't think it should be imported
                } else if (ctrlWordData.ctrlWord.Equals("listhybrid")) /* // List item*/ {
                    this.newList.SetListType(RtfList.LIST_TYPE_HYBRID);
                    skipCtrlWord = true;
                    result = true;
                    // this gets set internally. Don't think it should be imported
                } else if (ctrlWordData.ctrlWord.Equals("listrestarthdn")) /* // List item*/ {
                    skipCtrlWord = true;
                    result = true;
                } else if (ctrlWordData.ctrlWord.Equals("listid")) {    // List item cannot be between -1 and -5
                    // needs to be mapped for imports and is recreated
                    // we have the new id and the old id. Just add it to the mapping table here.
                    skipCtrlWord = true;
                    result = true;
                } else if (ctrlWordData.ctrlWord.Equals("listname"))/* // List item*/ {
                    this.newList.SetName(ctrlWordData.param);
                    skipCtrlWord = true;
                    result = true;
                } else if (ctrlWordData.ctrlWord.Equals("liststyleid"))/* // List item*/ {
                    skipCtrlWord = true;
                    result = true;
                } else if (ctrlWordData.ctrlWord.Equals("liststylename"))/* // List item*/ {
                    skipCtrlWord = true;
                    result = true;
                } else
                    /* listlevel */
                    if (ctrlWordData.ctrlWord.Equals("listlevel")) /* DESTINATION There are 1 or 9 listlevels per list */{
                    this.currentLevel++;
                    this.currentListLevel = this.newList.GetListLevel(this.currentLevel);
                    this.currentListLevel.SetTentative(false);
                    skipCtrlWord = true;
                    result = true;
                } else if (ctrlWordData.ctrlWord.Equals("leveljc")) { // listlevel item justify
                    // this is the old number. Only use it if the current type is not set
                    if ( this.currentListLevel.GetAlignment()== RtfListLevel.LIST_TYPE_UNKNOWN) {
                        switch (ctrlWordData.IntValue()) {
                            case 0:
                                this.currentListLevel.SetAlignment(Element.ALIGN_LEFT);
                                break;
                            case 1:
                                this.currentListLevel.SetAlignment(Element.ALIGN_CENTER);
                                break;
                            case 2:
                                this.currentListLevel.SetAlignment(Element.ALIGN_RIGHT);
                                break;
                        }
                    }
                    skipCtrlWord = true;
                    result = true;
                } else if (ctrlWordData.ctrlWord.Equals("leveljcn")) { // listlevel item
                    //justify
                    // if this exists, use it and it overrides the old setting
                    switch (ctrlWordData.IntValue()) {
                        case 0:
                            this.currentListLevel.SetAlignment(Element.ALIGN_LEFT);
                            break;
                        case 1:
                            this.currentListLevel.SetAlignment(Element.ALIGN_CENTER);
                            break;
                        case 2:
                            this.currentListLevel.SetAlignment(Element.ALIGN_RIGHT);
                            break;
                    }
                    skipCtrlWord = true;
                    result = true;
                } else if (ctrlWordData.ctrlWord.Equals("levelstartat")) {
                    this.currentListLevel.SetListStartAt(ctrlWordData.IntValue());
                    skipCtrlWord = true;
                    result = true;
                } else if (ctrlWordData.ctrlWord.Equals("lvltentative")) {
                    this.currentListLevel.SetTentative(true);
                    skipCtrlWord = true;
                    result = true;
                } else if (ctrlWordData.ctrlWord.Equals("levelold")) {
                    // old style. ignore
                    skipCtrlWord = true;
                    result = true;
                } else if (ctrlWordData.ctrlWord.Equals("levelprev")) {
                    // old style. ignore
                    skipCtrlWord = true;
                    result = true;
                } else if (ctrlWordData.ctrlWord.Equals("levelprevspace")) {
                    // old style. ignore
                    skipCtrlWord = true;
                    result = true;
                } else if (ctrlWordData.ctrlWord.Equals("levelspace")) {
                    skipCtrlWord = true;
                    result = true;
                } else if (ctrlWordData.ctrlWord.Equals("levelindent")) {
                    skipCtrlWord = true;
                    result = true;
                } else if (ctrlWordData.ctrlWord.Equals("leveltext")) {/* FIX */
                    skipCtrlWord = true;
                    result = true;
                }  else if (ctrlWordData.ctrlWord.Equals("levelfollow")) {
                    this.currentListLevel.SetLevelFollowValue(ctrlWordData.IntValue());
                    skipCtrlWord = true;
                    result = true;
                } else if (ctrlWordData.ctrlWord.Equals("levellegal")) {
                    this.currentListLevel.SetLegal(ctrlWordData.param=="1"?true:false);
                    skipCtrlWord = true;
                    result = true;
                } else if (ctrlWordData.ctrlWord.Equals("levelnorestart")) {
                    skipCtrlWord = true;
                    result = true;
                } else if (ctrlWordData.ctrlWord.Equals("chrfmt")) {/* FIX */
                    // set an attribute pair
                    skipCtrlWord = true;
                    result = true;
                } else if (ctrlWordData.ctrlWord.Equals("levelpicture")) {
                    skipCtrlWord = true;
                    result = true;
                } else if (ctrlWordData.ctrlWord.Equals("li")) {
                    // set an attribute pair
                    skipCtrlWord = true;
                    result = true;
                } else if (ctrlWordData.ctrlWord.Equals("fi")) {
                    // set an attribute pair
                    skipCtrlWord = true;
                    result = true;
                } else if (ctrlWordData.ctrlWord.Equals("jclisttab")) {
                    // set an attribute pair
                    skipCtrlWord = true;
                    result = true;
                } else if (ctrlWordData.ctrlWord.Equals("tx")) {
                    // set an attribute pair
                    skipCtrlWord = true;
                    result = true;
                } else
                    /* number */
                    if (ctrlWordData.ctrlWord.Equals("levelnfc")) /* old style */ {
                    if ( this.currentListLevel.GetListType()== RtfListLevel.LIST_TYPE_UNKNOWN) {
                        this.currentListLevel.SetListType(ctrlWordData.IntValue()+RtfListLevel.LIST_TYPE_BASE);
                    }
                    skipCtrlWord = true;
                    result = true;
                } else if (ctrlWordData.ctrlWord.Equals("levelnfcn")) /* new style takes priority over levelnfc.*/ {
                    this.currentListLevel.SetListType(ctrlWordData.IntValue()+RtfListLevel.LIST_TYPE_BASE);
                    skipCtrlWord = true;
                    result = true;
                } else
                    /* level text */
                    if (ctrlWordData.ctrlWord.Equals("leveltemplateid")) {
                    // ignore. this value is regenerated in each document.
                    skipCtrlWord = true;
                    result = true;
                } else
                    /* levelnumber */
                    if (ctrlWordData.ctrlWord.Equals("levelnumbers")) {
                    skipCtrlWord = true;
                    result = true;
                }
            }

            if (this.rtfParser.IsConvert()) {
                if (ctrlWordData.ctrlWord.Equals("shppict")) {
                    result = true;
                }
                if (ctrlWordData.ctrlWord.Equals("nonshppict")) {
                    skipCtrlWord = true;
                    this.rtfParser.SetTokeniserStateSkipGroup();
                    result = true;
                }
            }
            if (!skipCtrlWord) {
                switch (this.rtfParser.GetConversionType()) {
                    case RtfParser.TYPE_IMPORT_FULL:
                        // WriteBuffer();
                        // WriteText(ctrlWordData.ToString());
                        result = true;
                        break;
                    case RtfParser.TYPE_IMPORT_FRAGMENT:
                        // WriteBuffer();
                        // WriteText(ctrlWordData.ToString());
                        result = true;
                        break;
                    case RtfParser.TYPE_CONVERT:
                        result = true;
                        break;
                    default: // error because is should be an import or convert
                        result = false;
                        break;
                }
            }

            return result;
        }

        /* (non-Javadoc)
        * @see com.lowagie.text.rtf.direct.RtfDestination#handleGroupEnd()
        */
        public override bool HandleCloseGroup() {
            this.currentSubGroupCount--;
            if (this.newList != null && this.currentSubGroupCount == 0) {
                this.importHeader.ImportList(this.currentListMappingNumber.ToString(), this.newList.GetListNumber().ToString());
                this.rtfParser.GetRtfDocument().Add(this.newList);
            }
            return true;
        }

        /* (non-Javadoc)
        * @see com.lowagie.text.rtf.direct.RtfDestination#handleGroupStart()
        */
        public override bool HandleOpenGroup() {
            // TODO Auto-generated method stub
            return true;
        }
        /* (non-Javadoc)
        * @see com.lowagie.text.rtf.direct.RtfDestination#handleCharacter(int)
        */
        public override bool HandleCharacter(int ch) {
            // TODO Auto-generated method stub
            return true;
        }

        /* (non-Javadoc)
        * @see com.lowagie.text.rtf.parser.destinations.RtfDestination#setToDefaults()
        */
        public override void SetToDefaults() {
            // TODO Auto-generated method stub
            
        }

    }
}