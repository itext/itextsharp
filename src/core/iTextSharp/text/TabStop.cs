using System;
using iTextSharp.text.pdf.draw;

namespace iTextSharp.text
{
	public class TabStop
	{
        public static TabStop NewInstance(float currentPosition, float tabInterval)
        {
            currentPosition = (float)Math.Round(currentPosition * 1000) / 1000;
            tabInterval = (float)Math.Round(tabInterval * 1000) / 1000;

            TabStop tabStop = new TabStop(currentPosition + tabInterval - currentPosition % tabInterval);
            return tabStop;
        }

        public enum Alignment
        {
            LEFT,
            RIGHT,
            CENTER,
            ANCHOR
        }

        protected float position;
        protected Alignment alignment = Alignment.LEFT;
        protected IDrawInterface leader;
        protected char anchorChar = '.';

        public TabStop(float position) : this(position, Alignment.LEFT)
        {
           
        }

        public TabStop(float position, IDrawInterface leader) : this(position, leader, Alignment.LEFT)
        {
        }

        public TabStop(float position, Alignment alignment)
            : this(position, null, alignment)
        {
        }

        public TabStop(float position, Alignment alignment, char anchorChar)
            : this(position, null, alignment, anchorChar)
        {
        }

        public TabStop(float position, IDrawInterface leader, Alignment alignment)
            : this(position, leader, alignment, '.')
        {
        }

        public TabStop(float position, IDrawInterface leader, Alignment alignment, char anchorChar)
        {
            this.position = position;
            this.leader = leader;
            this.alignment = alignment;
            this.anchorChar = anchorChar;
        }

        public TabStop(TabStop tabStop)
            : this(tabStop.Position, tabStop.Leader, tabStop.Align, tabStop.AnchorChar)
        {
        }

	    public float Position
	    {
	        get { return position; }
	        set { position = value; }
	    }

	    public Alignment Align
	    {
	        get { return alignment; }
	        set { alignment = value; }
	    }

	    public IDrawInterface Leader
	    {
	        get { return leader; }
	        set { leader = value; }
	    }

	    public char AnchorChar
	    {
	        get { return anchorChar; }
	        set { anchorChar = value; }
	    }

	    public float GetPosition(float tabPosition, float currentPosition, float anchorPosition)
        {
            float newPosition = position;
            float textWidth = currentPosition - tabPosition;
            switch (alignment)
            {
                case Alignment.RIGHT:
                    if (tabPosition + textWidth < position)
                    {
                        newPosition = position - textWidth;
                    }
                    else
                    {
                        newPosition = tabPosition;
                    }
                    break;
                case Alignment.CENTER:
                    if (tabPosition + textWidth / 2f < position)
                    {
                        newPosition = position - textWidth / 2f;
                    }
                    else
                    {
                        newPosition = tabPosition;
                    }
                    break;
                case Alignment.ANCHOR:
                    if (!float.IsNaN(anchorPosition))
                    {
                        if (anchorPosition < position)
                        {
                            newPosition = position - (anchorPosition - tabPosition);
                        }
                        else
                        {
                            newPosition = tabPosition;
                        }
                    }
                    else
                    {
                        if (tabPosition + textWidth < position)
                        {
                            newPosition = position - textWidth;
                        }
                        else
                        {
                            newPosition = tabPosition;
                        }
                    }
                    break;
            }
            return newPosition;
        }
	}
}
