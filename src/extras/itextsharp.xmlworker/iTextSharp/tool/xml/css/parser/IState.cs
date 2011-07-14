using System;

namespace iTextSharp.tool.xml.css.parser {

    /**
     * @author redlab_b
     *
     */
    public interface IState {

	    /**
	     * Processes a character.
	     * @param c the character to process
	     */
	    void Process(char c);
    }
}