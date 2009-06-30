using System;
using System.IO;
using System.Collections;
using iTextSharp.text;
using iTextSharp.text.rtf;
using iTextSharp.text.rtf.document;
using iTextSharp.text.rtf.style;
using iTextSharp.text.rtf.text;
/*
 * $Id: RtfListLevel.java 3580 2008-08-06 15:52:00Z howard_s $
 *
 * Copyright 2008 by Howard Shank (hgshank@yahoo.com)
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
namespace iTextSharp.text.rtf.list {

    /**
    * The RtfListLevel is a listlevel object in a list.
    * 
    * @version $Id: RtfListLevel.java 3580 2008-08-06 15:52:00Z howard_s $
    * @author Howard Shank (hgshank@yahoo.com)
    * @since 2.1.3
    */
    public class RtfListLevel : RtfElement, IRtfExtendedElement {
        /**
        * Constant for list level
        */
        private static readonly byte[] LIST_LEVEL = DocWriter.GetISOBytes("\\listlevel");
        /**
        * Constant for list level
        */
        private static readonly byte[] LIST_LEVEL_TEMPLATE_ID = DocWriter.GetISOBytes("\\leveltemplateid");
        /**
        * Constant for list level style old
        */
        private static readonly byte[] LIST_LEVEL_TYPE = DocWriter.GetISOBytes("\\levelnfc");
        /**
        * Constant for list level style new
        */
        private static readonly byte[] LIST_LEVEL_TYPE_NEW = DocWriter.GetISOBytes("\\levelnfcn");
        /**
        * Constant for list level alignment old
        */
        private static readonly byte[] LIST_LEVEL_ALIGNMENT = DocWriter.GetISOBytes("\\leveljc");
        /**
        * Constant for list level alignment new
        */
        private static readonly byte[] LIST_LEVEL_ALIGNMENT_NEW = DocWriter.GetISOBytes("\\leveljcn");
        /**
        * Constant for list level start at
        */
        private static readonly byte[] LIST_LEVEL_START_AT = DocWriter.GetISOBytes("\\levelstartat");
        /**
        * Constant for list level text
        */
        private static readonly byte[] LIST_LEVEL_TEXT = DocWriter.GetISOBytes("\\leveltext");
        /**
        * Constant for the beginning of the list level numbered style
        */
        private static readonly byte[] LIST_LEVEL_STYLE_NUMBERED_BEGIN = DocWriter.GetISOBytes("\\\'02\\\'");
        /**
        * Constant for the end of the list level numbered style
        */
        private static readonly byte[] LIST_LEVEL_STYLE_NUMBERED_END = DocWriter.GetISOBytes(".;");
        /**
        * Constant for the beginning of the list level bulleted style
        */
        private static readonly byte[] LIST_LEVEL_STYLE_BULLETED_BEGIN = DocWriter.GetISOBytes("\\\'01");
        /**
        * Constant for the end of the list level bulleted style
        */
        private static readonly byte[] LIST_LEVEL_STYLE_BULLETED_END = DocWriter.GetISOBytes(";");
        /**
        * Constant for the beginning of the list level numbers
        */
        private static readonly byte[] LIST_LEVEL_NUMBERS_BEGIN = DocWriter.GetISOBytes("\\levelnumbers");
        /**
        * Constant which specifies which character follows the level text
        */
        private static readonly byte[] LIST_LEVEL_FOLOW = DocWriter.GetISOBytes("\\levelfollow");
        /**
        * Constant which specifies the levelspace controlword
        */
        private static readonly byte[] LIST_LEVEL_SPACE = DocWriter.GetISOBytes("\\levelspace");
        /**
        * Constant which specifies the levelindent control word
        */
        private static readonly byte[] LIST_LEVEL_INDENT = DocWriter.GetISOBytes("\\levelindent");
        /**
        * Constant which specifies (1) if list numbers from previous levels should be converted
        * to Arabic numbers; (0) if they should be left with the format specified by their
        * own level's definition.
        */
        private static readonly byte[] LIST_LEVEL_LEGAL = DocWriter.GetISOBytes("\\levellegal");
        /**
        * Constant which specifies 
        * (1) if this level does/does not restart its count each time a super ordinate level is incremented
        * (0) if this level does not restart its count each time a super ordinate level is incremented.
        */
        private static readonly byte[] LIST_LEVEL_NO_RESTART = DocWriter.GetISOBytes("\\levelnorestart");
        /**
        * Constant for the list level numbers
        */
        private static readonly byte[] LIST_LEVEL_NUMBERS_NUMBERED = DocWriter.GetISOBytes("\\\'01");
        /**
        * Constant for the end of the list level numbers
        */
        private static readonly byte[] LIST_LEVEL_NUMBERS_END = DocWriter.GetISOBytes(";");
        
        /**
        * Constant for the first indentation
        */
        private static readonly byte[] LIST_LEVEL_FIRST_INDENT = DocWriter.GetISOBytes("\\fi");
        /**
        * Constant for the symbol indentation
        */
        private static readonly byte[] LIST_LEVEL_SYMBOL_INDENT = DocWriter.GetISOBytes("\\tx");
        
        /**
        * Constant for the lvltentative control word
        */
        private static readonly byte[] LIST_LEVEL_TENTATIVE = DocWriter.GetISOBytes("\\lvltentative");
        /**
        * Constant for the levelpictureN control word
        */
        private static readonly byte[] LIST_LEVEL_PICTURE = DocWriter.GetISOBytes("\\levelpicture");
        

        public const int LIST_TYPE_NUMBERED = 1;
        public const int LIST_TYPE_UPPER_LETTERS = 2;
        public const int LIST_TYPE_LOWER_LETTERS = 3;
        public const int LIST_TYPE_UPPER_ROMAN = 4;
        public const int LIST_TYPE_LOWER_ROMAN = 5;

        public const int LIST_TYPE_UNKNOWN = -1;                     /* unknown type */
        public const int LIST_TYPE_BASE = 1000;                      /* BASE value to subtract to get RTF Value if above base*/
        public const int LIST_TYPE_ARABIC = 1000;                    /* 0 Arabic (1, 2, 3) */
        public const int LIST_TYPE_UPPERCASE_ROMAN_NUMERAL = 1001;   /* 1 Uppercase Roman numeral (I, II, III) */
        public const int LIST_TYPE_LOWERCASE_ROMAN_NUMERAL = 1002;   /* 2 Lowercase Roman numeral (i, ii, iii)*/
        public const int LIST_TYPE_UPPERCASE_LETTER = 1003;          /* 3 Uppercase letter (A, B, C)*/
        public const int LIST_TYPE_LOWERCASE_LETTER = 1004;          /* 4 Lowercase letter (a, b, c)*/
        public const int LIST_TYPE_ORDINAL_NUMBER = 1005;            /* 5 Ordinal number (1st, 2nd, 3rd)*/
        public const int LIST_TYPE_CARDINAL_TEXT_NUMBER = 1006;      /* 6 Cardinal text number (One, Two Three)*/
        public const int LIST_TYPE_ORDINAL_TEXT_NUMBER = 1007;       /* 7 Ordinal text number (First, Second, Third)*/
        public const int LIST_TYPE_ARABIC_LEADING_ZERO = 1022;       /* 22   Arabic with leading zero (01, 02, 03, ..., 10, 11)*/
        public const int LIST_TYPE_BULLET = 1023;                    /* 23   Bullet (no number at all)*/
        public const int LIST_TYPE_NO_NUMBER = 1255;             /*  255 No number */
    /*
     
    10  Kanji numbering without the digit character (*dbnum1)
    11  Kanji numbering with the digit character (*dbnum2)
    12  46 phonetic katakana characters in "aiueo" order (*aiueo)
    13  46 phonetic katakana characters in "iroha" order (*iroha)
    14  Double-byte character
    15  Single-byte character
    16  Kanji numbering 3 (*dbnum3)
    17  Kanji numbering 4 (*dbnum4)
    18  Circle numbering (*circlenum)
    19  Double-byte Arabic numbering    
    20  46 phonetic double-byte katakana characters (*aiueo*dbchar)
        21  46 phonetic double-byte katakana characters (*iroha*dbchar)
        22  Arabic with leading zero (01, 02, 03, ..., 10, 11)
        24  Korean numbering 2 (*ganada)
        25  Korean numbering 1 (*chosung)
        26  Chinese numbering 1 (*gb1)
        27  Chinese numbering 2 (*gb2)
        28  Chinese numbering 3 (*gb3)
        29  Chinese numbering 4 (*gb4)
        30  Chinese Zodiac numbering 1 (* zodiac1)
        31  Chinese Zodiac numbering 2 (* zodiac2) 
        32  Chinese Zodiac numbering 3 (* zodiac3)
        33  Taiwanese double-byte numbering 1
        34  Taiwanese double-byte numbering 2
        35  Taiwanese double-byte numbering 3
        36  Taiwanese double-byte numbering 4
        37  Chinese double-byte numbering 1
        38  Chinese double-byte numbering 2
        39  Chinese double-byte numbering 3
        40  Chinese double-byte numbering 4
        41  Korean double-byte numbering 1
        42  Korean double-byte numbering 2
        43  Korean double-byte numbering 3
        44  Korean double-byte numbering 4
        45  Hebrew non-standard decimal 
        46  Arabic Alif Ba Tah
        47  Hebrew Biblical standard
        48  Arabic Abjad style
        255 No number
    */
        /**
        * Whether this RtfList is numbered
        */
        private int listType = LIST_TYPE_UNKNOWN;

        /**
        * The text to use as the bullet character
        */
        private String bulletCharacter = "\u00b7"; 
        /**
        * @since 2.1.4
        */
        private Chunk bulletChunk = null;
        /**
        * The number to start counting at
        */
        private int listStartAt = 1;
        /**
        * The level of this RtfListLevel
        */
        private int listLevel = 0;
        /**
        * The first indentation of this RtfList
        */
        private int firstIndent = 0;
        /**
        * The left indentation of this RtfList
        */
        private int leftIndent = 0;
        /**
        * The right indentation of this RtfList
        */
        private int rightIndent = 0;
        /**
        * The symbol indentation of this RtfList
        */
        private int symbolIndent = 0;
        /**
        * Flag to indicate if the tentative control word should be emitted.
        */
        private bool isTentative = true;
        /**
        * Flag to indicate if the levellegal control word should be emitted.
        * true  if any list numbers from previous levels should be converted to Arabic numbers; 
        * false if they should be left with the format specified by their own level definition.
        */
        private bool isLegal = false;
        
        /**
        * Does the list restart numbering each time a super ordinate level is incremented
        */
        private int listNoRestart = 0;
        public const int LIST_LEVEL_FOLLOW_TAB = 0; 
        public const int LIST_LEVEL_FOLLOW_SPACE = 1; 
        public const int LIST_LEVEL_FOLLOW_NOTHING = 2; 
        private int levelFollowValue = LIST_LEVEL_FOLLOW_TAB;

        /**
        * The alignment of this RtfList
        */
        private int alignment = Element.ALIGN_LEFT;
        /**
        * Which picture bullet from the \listpicture destination should be applied
        */
        private int levelPicture = -1;
        
        private int levelTextNumber = 0;
        /**
        * The RtfFont for numbered lists
        */
        private RtfFont fontNumber;
        /**
        * The RtfFont for bulleted lists
        */
        private RtfFont fontBullet;
        
        private int templateID = -1;
        
        private RtfListLevel listLevelParent = null;
        
        /** 
        * Parent list object
        */
        private RtfList parent = null;
        
        public RtfListLevel(RtfDocument doc) : base(doc)
        {
            templateID = document.GetRandomInt();
            SetFontNumber( new RtfFont(document, new Font(Font.TIMES_ROMAN, 10, Font.NORMAL, new Color(0, 0, 0))));
            SetBulletFont(new Font(Font.SYMBOL, 10, Font.NORMAL, new Color(0, 0, 0)));
        }
        
        public RtfListLevel(RtfDocument doc, RtfList parent) : base(doc)
        {
            this.parent = parent;
            templateID = document.GetRandomInt();
            SetFontNumber( new RtfFont(document, new Font(Font.TIMES_ROMAN, 10, Font.NORMAL, new Color(0, 0, 0))));
            SetBulletFont(new Font(Font.SYMBOL, 10, Font.NORMAL, new Color(0, 0, 0)));
        }
        
        public RtfListLevel(RtfListLevel ll) : base(ll.document)
        {
            templateID = document.GetRandomInt();
            this.alignment = ll.alignment;
            this.bulletCharacter = ll.bulletCharacter;
            this.firstIndent = ll.firstIndent;
            this.fontBullet = ll.fontBullet;
            this.fontNumber = ll.fontNumber;
            this.inHeader = ll.inHeader;
            this.inTable = ll.inTable;
            this.leftIndent = ll.leftIndent;
            this.listLevel = ll.listLevel;
            this.listNoRestart = ll.listNoRestart;
            this.listStartAt = ll.listStartAt;
            this.listType = ll.listType;
            this.parent = ll.parent;
            this.rightIndent = ll.rightIndent;
            this.symbolIndent = ll.symbolIndent;
        }

        /**
        * @return the listNoRestart
        */
        public int GetListNoRestart() {
            return listNoRestart;
        }

        /**
        * @param listNoRestart the listNoRestart to set
        */
        public void SetListNoRestart(int listNoRestart) {
            this.listNoRestart = listNoRestart;
        }

        /**
        * @return the alignment
        */
        public int GetAlignment() {
            return alignment;
        }

        /**
        * @param alignment the alignment to set
        */
        public void SetAlignment(int alignment) {
            this.alignment = alignment;
        }

        public void WriteDefinition(Stream result) {
            byte[] t;
            result.Write(OPEN_GROUP, 0, OPEN_GROUP.Length);
            result.Write(LIST_LEVEL, 0, LIST_LEVEL.Length);
            result.Write(LIST_LEVEL_TYPE, 0, LIST_LEVEL_TYPE.Length);
            switch (this.listType) {
                case LIST_TYPE_BULLET        : result.Write(t = IntToByteArray(23), 0, t.Length); break;
                case LIST_TYPE_NUMBERED      : result.Write(t = IntToByteArray(0), 0, t.Length); break;
                case LIST_TYPE_UPPER_LETTERS : result.Write(t = IntToByteArray(3), 0, t.Length); break;
                case LIST_TYPE_LOWER_LETTERS : result.Write(t = IntToByteArray(4), 0, t.Length); break;
                case LIST_TYPE_UPPER_ROMAN   : result.Write(t = IntToByteArray(1), 0, t.Length); break;
                case LIST_TYPE_LOWER_ROMAN   : result.Write(t = IntToByteArray(2), 0, t.Length); break;
                /* New types */
                case LIST_TYPE_ARABIC        : result.Write(t = IntToByteArray(0), 0, t.Length); break;
                case LIST_TYPE_UPPERCASE_ROMAN_NUMERAL       : result.Write(t = IntToByteArray(1), 0, t.Length); break;
                case LIST_TYPE_LOWERCASE_ROMAN_NUMERAL       : result.Write(t = IntToByteArray(2), 0, t.Length); break;
                case LIST_TYPE_UPPERCASE_LETTER      : result.Write(t = IntToByteArray(3), 0, t.Length); break;
                case LIST_TYPE_ORDINAL_NUMBER        : result.Write(t = IntToByteArray(4), 0, t.Length); break;
                case LIST_TYPE_CARDINAL_TEXT_NUMBER      : result.Write(t = IntToByteArray(5), 0, t.Length); break;
                case LIST_TYPE_ORDINAL_TEXT_NUMBER       : result.Write(t = IntToByteArray(6), 0, t.Length); break;
                case LIST_TYPE_LOWERCASE_LETTER      : result.Write(t = IntToByteArray(7), 0, t.Length); break;
                case LIST_TYPE_ARABIC_LEADING_ZERO       : result.Write(t = IntToByteArray(22), 0, t.Length); break;
                case LIST_TYPE_NO_NUMBER     : result.Write(t = IntToByteArray(255), 0, t.Length); break;
                default:    // catch all for other unsupported types
                    if (this.listType >= RtfListLevel.LIST_TYPE_BASE) {
                        result.Write(t = IntToByteArray(this.listType - RtfListLevel.LIST_TYPE_BASE), 0, t.Length);
                    }
                break;
            }
            
            result.Write(LIST_LEVEL_TYPE_NEW, 0, LIST_LEVEL_TYPE_NEW.Length);
            switch (this.listType) {
                case LIST_TYPE_BULLET        : result.Write(t = IntToByteArray(23), 0, t.Length); break;
                case LIST_TYPE_NUMBERED      : result.Write(t = IntToByteArray(0), 0, t.Length); break;
                case LIST_TYPE_UPPER_LETTERS : result.Write(t = IntToByteArray(3), 0, t.Length); break;
                case LIST_TYPE_LOWER_LETTERS : result.Write(t = IntToByteArray(4), 0, t.Length); break;
                case LIST_TYPE_UPPER_ROMAN   : result.Write(t = IntToByteArray(1), 0, t.Length); break;
                case LIST_TYPE_LOWER_ROMAN   : result.Write(t = IntToByteArray(2), 0, t.Length); break;
                /* New types */
                case LIST_TYPE_ARABIC        : result.Write(t = IntToByteArray(0), 0, t.Length); break;
                case LIST_TYPE_UPPERCASE_ROMAN_NUMERAL       : result.Write(t = IntToByteArray(1), 0, t.Length); break;
                case LIST_TYPE_LOWERCASE_ROMAN_NUMERAL       : result.Write(t = IntToByteArray(2), 0, t.Length); break;
                case LIST_TYPE_UPPERCASE_LETTER      : result.Write(t = IntToByteArray(3), 0, t.Length); break;
                case LIST_TYPE_ORDINAL_NUMBER        : result.Write(t = IntToByteArray(4), 0, t.Length); break;
                case LIST_TYPE_CARDINAL_TEXT_NUMBER      : result.Write(t = IntToByteArray(5), 0, t.Length); break;
                case LIST_TYPE_ORDINAL_TEXT_NUMBER       : result.Write(t = IntToByteArray(6), 0, t.Length); break;
                case LIST_TYPE_LOWERCASE_LETTER      : result.Write(t = IntToByteArray(7), 0, t.Length); break;
                case LIST_TYPE_ARABIC_LEADING_ZERO       : result.Write(t = IntToByteArray(22), 0, t.Length); break;
                case LIST_TYPE_NO_NUMBER     : result.Write(t = IntToByteArray(255), 0, t.Length); break;
                default:    // catch all for other unsupported types
                    if (this.listType >= RtfListLevel.LIST_TYPE_BASE) {
                        result.Write(t = IntToByteArray(this.listType - RtfListLevel.LIST_TYPE_BASE), 0, t.Length);
                    }
                break;
            }
            result.Write(LIST_LEVEL_ALIGNMENT, 0, LIST_LEVEL_ALIGNMENT.Length);
            result.Write(t = IntToByteArray(0), 0, t.Length);
            result.Write(LIST_LEVEL_ALIGNMENT_NEW, 0, LIST_LEVEL_ALIGNMENT_NEW.Length);
            result.Write(t = IntToByteArray(0), 0, t.Length);
            result.Write(LIST_LEVEL_FOLOW, 0, LIST_LEVEL_FOLOW.Length);
            result.Write(t = IntToByteArray(levelFollowValue), 0, t.Length);
            result.Write(LIST_LEVEL_START_AT, 0, LIST_LEVEL_START_AT.Length);
            result.Write(t = IntToByteArray(this.listStartAt), 0, t.Length);
            if (this.isTentative) {
                result.Write(LIST_LEVEL_TENTATIVE, 0, LIST_LEVEL_TENTATIVE.Length);
            }
            if (this.isLegal) {
                result.Write(LIST_LEVEL_LEGAL, 0, LIST_LEVEL_LEGAL.Length);
            }
            result.Write(LIST_LEVEL_SPACE, 0, LIST_LEVEL_SPACE.Length);
            result.Write(t = IntToByteArray(0), 0, t.Length);
            result.Write(LIST_LEVEL_INDENT, 0, LIST_LEVEL_INDENT.Length);
            result.Write(t = IntToByteArray(0), 0, t.Length);
            if (levelPicture != -1) {
                result.Write(LIST_LEVEL_PICTURE, 0, LIST_LEVEL_PICTURE.Length);
                result.Write(t = IntToByteArray(levelPicture), 0, t.Length);
            }
            
            result.Write(OPEN_GROUP, 0, OPEN_GROUP.Length); // { leveltext
            result.Write(LIST_LEVEL_TEXT, 0, LIST_LEVEL_TEXT.Length);
            result.Write(LIST_LEVEL_TEMPLATE_ID, 0, LIST_LEVEL_TEMPLATE_ID.Length);
            result.Write(t = IntToByteArray(this.templateID), 0, t.Length);
            /* NEVER seperate the LEVELTEXT elements with a return in between 
            * them or it will not fuction correctly!
            */
            // TODO Needs to be rewritten to support 1-9 levels, not just simple single level
            if (this.listType != LIST_TYPE_BULLET) {
                result.Write(LIST_LEVEL_STYLE_NUMBERED_BEGIN, 0, LIST_LEVEL_STYLE_NUMBERED_BEGIN.Length);
                if (this.levelTextNumber < 10) {
                    result.Write(t = IntToByteArray(0), 0, t.Length);
                }
                result.Write(t = IntToByteArray(this.levelTextNumber), 0, t.Length);
                result.Write(LIST_LEVEL_STYLE_NUMBERED_END, 0, LIST_LEVEL_STYLE_NUMBERED_END.Length);
            } else {
                result.Write(LIST_LEVEL_STYLE_BULLETED_BEGIN, 0, LIST_LEVEL_STYLE_BULLETED_BEGIN.Length);
                this.document.FilterSpecialChar(result, this.bulletCharacter, false, false);
                result.Write(LIST_LEVEL_STYLE_BULLETED_END, 0, LIST_LEVEL_STYLE_BULLETED_END.Length);
            }
            result.Write(CLOSE_GROUP, 0, CLOSE_GROUP.Length);  // } leveltext
            
            result.Write(OPEN_GROUP, 0, OPEN_GROUP.Length);  // { levelnumbers
            result.Write(LIST_LEVEL_NUMBERS_BEGIN, 0, LIST_LEVEL_NUMBERS_BEGIN.Length);
            if (this.listType != LIST_TYPE_BULLET) {
                result.Write(LIST_LEVEL_NUMBERS_NUMBERED, 0, LIST_LEVEL_NUMBERS_NUMBERED.Length);
            }
            result.Write(LIST_LEVEL_NUMBERS_END, 0, LIST_LEVEL_NUMBERS_END.Length);
            result.Write(CLOSE_GROUP, 0, CLOSE_GROUP.Length);// { levelnumbers
            
            // write properties now
            result.Write(RtfFontList.FONT_NUMBER, 0, RtfFontList.FONT_NUMBER.Length);
            if (this.listType != LIST_TYPE_BULLET) {
                result.Write(t = IntToByteArray(fontNumber.GetFontNumber()), 0, t.Length);
            } else {
                result.Write(t = IntToByteArray(fontBullet.GetFontNumber()), 0, t.Length);
            }
            result.Write(t = DocWriter.GetISOBytes("\\cf"), 0, t.Length);
    //        document.GetDocumentHeader().GetColorNumber(new RtfColor(this.document,this.GetFontNumber().GetColor()));
            result.Write(t = IntToByteArray(document.GetDocumentHeader().GetColorNumber(new RtfColor(this.document,this.GetFontNumber().Color))), 0, t.Length);
                
            WriteIndentation(result);
            result.Write(CLOSE_GROUP, 0, CLOSE_GROUP.Length);
            this.document.OutputDebugLinebreak(result);
            
        }
        /**
        * unused
        */    
        public override void WriteContent(Stream result)
        {
        }     
        
        /**
        * Writes only the list number and list level number.
        * 
        * @param result The <code>Stream</code> to write to
        * @throws IOException On i/o errors.
        */
        protected void WriteListNumbers(Stream result) {
            byte[] t;
            if (listLevel > 0) {
                result.Write(RtfList.LIST_LEVEL_NUMBER, 0, RtfList.LIST_LEVEL_NUMBER.Length);
                result.Write(t = IntToByteArray(listLevel), 0, t.Length);
            }
        }
        
        
        /**
        * Write the indentation values for this <code>RtfList</code>.
        * 
        * @param result The <code>Stream</code> to write to.
        * @throws IOException On i/o errors.
        */
        public void WriteIndentation(Stream result) {
            byte[] t;
            result.Write(LIST_LEVEL_FIRST_INDENT, 0, LIST_LEVEL_FIRST_INDENT.Length);
            result.Write(t = IntToByteArray(firstIndent), 0, t.Length);
            result.Write(RtfParagraphStyle.INDENT_LEFT, 0, RtfParagraphStyle.INDENT_LEFT.Length);
            result.Write(t = IntToByteArray(leftIndent), 0, t.Length);
            result.Write(RtfParagraphStyle.INDENT_RIGHT, 0, RtfParagraphStyle.INDENT_RIGHT.Length);
            result.Write(t = IntToByteArray(rightIndent), 0, t.Length);
            result.Write(LIST_LEVEL_SYMBOL_INDENT, 0, LIST_LEVEL_SYMBOL_INDENT.Length);
            result.Write(t = IntToByteArray(this.leftIndent), 0, t.Length);

        }
        /**
        * Writes the initialization part of the RtfList
        * 
        * @param result The <code>Stream</code> to write to
        * @throws IOException On i/o errors.
        */
        public void WriteListBeginning(Stream result) {
            byte[] t;
            result.Write(RtfParagraph.PARAGRAPH_DEFAULTS, 0, RtfParagraph.PARAGRAPH_DEFAULTS.Length);
            if (this.inTable) {
                result.Write(RtfParagraph.IN_TABLE, 0, RtfParagraph.IN_TABLE.Length);
            }
            switch (this.alignment) {
                case Element.ALIGN_LEFT:
                    result.Write(RtfParagraphStyle.ALIGN_LEFT, 0, RtfParagraphStyle.ALIGN_LEFT.Length);
                    break;
                case Element.ALIGN_RIGHT:
                    result.Write(RtfParagraphStyle.ALIGN_RIGHT, 0, RtfParagraphStyle.ALIGN_RIGHT.Length);
                    break;
                case Element.ALIGN_CENTER:
                    result.Write(RtfParagraphStyle.ALIGN_CENTER, 0, RtfParagraphStyle.ALIGN_CENTER.Length);
                    break;
                case Element.ALIGN_JUSTIFIED:
                case Element.ALIGN_JUSTIFIED_ALL:
                    result.Write(RtfParagraphStyle.ALIGN_JUSTIFY, 0, RtfParagraphStyle.ALIGN_JUSTIFY.Length);
                    break;
            }
            WriteIndentation(result);
            result.Write(RtfFont.FONT_SIZE, 0, RtfFont.FONT_SIZE.Length);
            result.Write(t = IntToByteArray(fontNumber.GetFontSize() * 2), 0, t.Length);
            if (this.symbolIndent > 0) {
                result.Write(LIST_LEVEL_SYMBOL_INDENT, 0, LIST_LEVEL_SYMBOL_INDENT.Length);
                result.Write(t = IntToByteArray(this.leftIndent), 0, t.Length);
            }
        }
        /**
        * Correct the indentation of this level
        */
        protected internal void CorrectIndentation() {

            if (this.listLevelParent != null) {
                this.leftIndent = this.leftIndent + this.listLevelParent.GetLeftIndent() + this.listLevelParent.GetFirstIndent();
            }
        }
        /**
        * Gets the list level of this RtfList
        * 
        * @return Returns the list level.
        */
        public int GetListLevel() {
            return listLevel;
        }
        
        
        /**
        * Sets the list level of this RtfList. 
        * 
        * @param listLevel The list level to set.
        */
        public void SetListLevel(int listLevel) {
            this.listLevel = listLevel;
        }
        
        
        public String GetBulletCharacter() {
            return this.bulletCharacter;
        }
        /**
        * @return the listStartAt
        */
        public int GetListStartAt() {
            return listStartAt;
        }
        /**
        * @param listStartAt the listStartAt to set
        */
        public void SetListStartAt(int listStartAt) {
            this.listStartAt = listStartAt;
        }

        /**
        * @return the firstIndent
        */
        public int GetFirstIndent() {
            return firstIndent;
        }
        /**
        * @param firstIndent the firstIndent to set
        */
        public void SetFirstIndent(int firstIndent) {
            this.firstIndent = firstIndent;
        }
        /**
        * @return the leftIndent
        */
        public int GetLeftIndent() {
            return leftIndent;
        }
        /**
        * @param leftIndent the leftIndent to set
        */
        public void SetLeftIndent(int leftIndent) {
            this.leftIndent = leftIndent;
        }
        /**
        * @return the rightIndent
        */
        public int GetRightIndent() {
            return rightIndent;
        }
        /**
        * @param rightIndent the rightIndent to set
        */
        public void SetRightIndent(int rightIndent) {
            this.rightIndent = rightIndent;
        }
        /**
        * @return the symbolIndent
        */
        public int GetSymbolIndent() {
            return symbolIndent;
        }
        /**
        * @param symbolIndent the symbolIndent to set
        */
        public void SetSymbolIndent(int symbolIndent) {
            this.symbolIndent = symbolIndent;
        }
        /**
        * @return the parent
        */
        public RtfList GetParent() {
            return parent;
        }
        /**
        * @param parent the parent to set
        */
        public void SetParent(RtfList parent) {
            this.parent = parent;
        }
        /**
        * @param bulletCharacter the bulletCharacter to set
        */
        public void SetBulletCharacter(String bulletCharacter) {
            this.bulletCharacter = bulletCharacter;
        }
        /**
        * 
        * @param bulletCharacter
        * @since 2.1.4
        */
        public void SetBulletChunk(Chunk bulletCharacter) {
            this.bulletChunk = bulletCharacter;
        }
        /**
        * @return the listType
        */
        public int GetListType() {
            return listType;
        }
        /**
        * @param listType the listType to set
        */
        public void SetListType(int listType) {
            this.listType = listType;
        }
        /**
        * set the bullet font
        * @param f
        */
        public void SetBulletFont(Font f) {
            this.fontBullet = new RtfFont(document, f);
        }

        /**
        * @return the fontNumber
        */
        public RtfFont GetFontNumber() {
            return fontNumber;
        }

        /**
        * @param fontNumber the fontNumber to set
        */
        public void SetFontNumber(RtfFont fontNumber) {
            this.fontNumber = fontNumber;
        }

        /**
        * @return the fontBullet
        */
        public RtfFont GetFontBullet() {
            return fontBullet;
        }

        /**
        * @param fontBullet the fontBullet to set
        */
        public void SetFontBullet(RtfFont fontBullet) {
            this.fontBullet = fontBullet;
        }

        /**
        * @return the isTentative
        */
        public bool IsTentative() {
            return isTentative;
        }

        /**
        * @param isTentative the isTentative to set
        */
        public void SetTentative(bool isTentative) {
            this.isTentative = isTentative;
        }

        /**
        * @return the isLegal
        */
        public bool IsLegal() {
            return isLegal;
        }

        /**
        * @param isLegal the isLegal to set
        */
        public void SetLegal(bool isLegal) {
            this.isLegal = isLegal;
        }

        /**
        * @return the levelFollowValue
        */
        public int GetLevelFollowValue() {
            return levelFollowValue;
        }

        /**
        * @param levelFollowValue the levelFollowValue to set
        */
        public void SetLevelFollowValue(int levelFollowValue) {
            this.levelFollowValue = levelFollowValue;
        }

        /**
        * @return the levelTextNumber
        */
        public int GetLevelTextNumber() {
            return levelTextNumber;
        }

        /**
        * @param levelTextNumber the levelTextNumber to set
        */
        public void SetLevelTextNumber(int levelTextNumber) {
            this.levelTextNumber = levelTextNumber;
        }

        /**
        * @return the listLevelParent
        */
        public RtfListLevel GetListLevelParent() {
            return listLevelParent;
        }

        /**
        * @param listLevelParent the listLevelParent to set
        */
        public void SetListLevelParent(RtfListLevel listLevelParent) {
            this.listLevelParent = listLevelParent;
        }
    }
}