/*
 * $Id: $
 *
 * This file is part of the iText (R) project.
 * Copyright (c) 1998-2012 1T3XT BVBA
 * Authors: VVB, Bruno Lowagie, et al.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License version 3
 * as published by the Free Software Foundation with the addition of the
 * following permission added to Section 15 as permitted in Section 7(a):
 * FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY 1T3XT,
 * 1T3XT DISCLAIMS THE WARRANTY OF NON INFRINGEMENT OF THIRD PARTY RIGHTS.
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
 * a covered work must retain the producer line in every PDF that is created
 * or manipulated using iText.
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

using System;
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.tool.xml.exceptions;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.pipeline;
using iTextSharp.tool.xml.pipeline.html;
using iTextSharp.tool.xml.svg.tags;

namespace iTextSharp.tool.xml.svg {

    public class SvgPipeline : AbstractPipeline{
    	
	    private SvgPipelineContext hpc;

	    public SvgPipeline(SvgPipelineContext hpc, IPipeline next) : base(next) {
		    this.hpc = hpc;
	    }
    	
	    /*
	     * (non-Javadoc)
	     *
	     * @see com.itextpdf.tool.xml.pipeline.Pipeline#open(com.itextpdf.tool.
	     * xml.Tag, com.itextpdf.tool.xml.pipeline.ProcessObject)
	     */
	    public override IPipeline Open(IWorkerContext context, Tag t, ProcessObject po) {
		    SvgPipelineContext hcc = (SvgPipelineContext) GetLocalContext(context);
		    try {
			    ITagProcessor tp = hcc.ResolveProcessor(t.Name, t.Name);
			    if (tp.IsStackOwner()) {
				    hcc.AddFirst(new StackKeeper(t));
			    }
			    IList<IElement> content = tp.StartElement(context, t);
			    if (content.Count > 0) {
				    if (tp.IsStackOwner()) {
					    StackKeeper peek;
					    try {
						    peek = hcc.Peek();
						    foreach (IElement elem in content) {
							    peek.Add(elem);
						    }
					    } catch (NoStackException e) {
						    throw new PipelineException(String.Format(LocaleMessages.STACK_404, t.ToString()), e);
					    }
				    } else {
					    if(!hcc.IsDefinition()){
						    foreach (IElement elem in content) {
							    po.Add((Graphic)elem);
						    }
					    }
				    }
			    }
		    } catch (NoTagProcessorException e) {
			    if (!hcc.AcceptUnknown()) {
				    throw e;
			    }
		    }
		    return GetNext();
	    }

	    /*
	     * (non-Javadoc)
	     *
	     * @see com.itextpdf.tool.xml.pipeline.Pipeline#content(com.itextpdf.tool
	     * .xml.Tag, java.lang.String, com.itextpdf.tool.xml.pipeline.ProcessObject)
	     */
	    public override IPipeline Content(IWorkerContext context, Tag t, String text, ProcessObject po)
			    {
		    SvgPipelineContext hcc = (SvgPipelineContext)GetLocalContext(context);
		    ITagProcessor tp;
		    try {
			    tp = hcc.ResolveProcessor(t.Name, t.NameSpace);
    //			String ctn = null;
    //			if (null != hcc.CharSet()) {
    //				try {
    //					ctn = new String(b, hcc.CharSet().Name());
    //				} catch (UnsupportedEncodingException e) {
    //					throw new RuntimeWorkerException(LocaleMessages.GetInstance().GetMessage(
    //							LocaleMessages.UNSUPPORTED_CHARSET), e);
    //				}
    //			} else {
    //				ctn = new String(b);
    //			}
    			
			    IList<IElement> elements = tp.Content(context, t, text);		
    			
			    if (elements.Count > 0) {
				    StackKeeper peek;
				    try {
					    peek = hcc.Peek();
					    foreach (IElement e in elements) {
						    peek.Add(e);
					    }
				    } catch (NoStackException e) {
					    if(!hcc.IsDefinition()){
						    foreach (IElement elem in elements) {
							    po.Add((IWritable)elem);
						    }
					    }
    //					po.Add(writableElement);
				    }
			    }
		    } catch (NoTagProcessorException e) {
			    if (!hcc.AcceptUnknown()) {
				    throw e;
			    }
		    }
		    return GetNext();
	    }	
    	
	    public override IPipeline Init(IWorkerContext context) {
		    try {
			    SvgPipelineContext clone = (SvgPipelineContext)hpc.Clone();
			    context.Put(GetContextKey(), clone);
		    } catch (Exception e) {
			    String message = String.Format(LocaleMessages.GetInstance().GetMessage(LocaleMessages.UNSUPPORTED_CLONING),
					    hpc.GetType().FullName);
			    throw new PipelineException(message, e);
		    }
		    return GetNext();

	    }
    	
	    /*
	     * (non-Javadoc)
	     *
	     * @see com.itextpdf.tool.xml.pipeline.Pipeline#close(com.itextpdf.tool
	     * .xml.Tag, com.itextpdf.tool.xml.pipeline.ProcessObject)
	     */
	    public override IPipeline Close(IWorkerContext context, Tag t, ProcessObject po) {
		    SvgPipelineContext hcc = (SvgPipelineContext) GetLocalContext(context);
		    ITagProcessor tp;
		    IList<IElement> elements = null;
		    try {
			    tp = hcc.ResolveProcessor(t.Name, t.NameSpace);

			    if (tp.IsStackOwner()) {
				    // remove the element from the StackKeeper Queue if end tag is found
				    StackKeeper tagStack;
				    try {
					    tagStack = hcc.Poll();
				    } catch (NoStackException e) {
					    throw new PipelineException(String.Format(
							    LocaleMessages.GetInstance().GetMessage(LocaleMessages.STACK_404), t.ToString()), e);
				    }
				    elements = tp.EndElement(context, t, tagStack.GetElements());
			    } else {
				    elements = tp.EndElement(context, t, hcc.CurrentContent());				
				    hcc.CurrentContent().Clear();				
			    }
    			
			    if (elements != null && elements.Count > 0) {
				    try {
					    StackKeeper stack = hcc.Peek();
					    foreach (IElement elem in elements) {
						    stack.Add(elem);
					    }
				    } catch (NoStackException e) {
					    //don't write definities, part of defs
					    if(!hcc.IsDefinition()){
						    foreach (IElement elem in elements) {
							    po.Add((IWritable)elem);
						    }		
					    }					
				    }
			    }
		    } catch (NoTagProcessorException e) {
			    if (!hcc.AcceptUnknown()) {
				    throw e;
			    }
		    }
		    return GetNext();
	    }
    }
}
