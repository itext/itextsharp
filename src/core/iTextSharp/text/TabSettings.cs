using System.Collections.Generic;

namespace iTextSharp.text
{
    public class TabSettings
    {
        public const float DEFAULT_TAB_INTERVAL = 36;

        public static TabStop getTabStopNewInstance(float currentPosition, TabSettings tabSettings)
        {
            if (tabSettings != null)
                return tabSettings.GetTabStopNewInstance(currentPosition);
            return TabStop.NewInstance(currentPosition, DEFAULT_TAB_INTERVAL);
        }

        private List<TabStop> tabStops = new List<TabStop>();
        private float tabInterval = DEFAULT_TAB_INTERVAL;

        public TabSettings()
        {
        }

        public TabSettings(List<TabStop> tabStops)
        {
            this.tabStops = tabStops;
        }

        public TabSettings(float tabInterval)
        {
            this.tabInterval = tabInterval;
        }

        public TabSettings(List<TabStop> tabStops, float tabInterval)
        {
            this.tabStops = tabStops;
            this.tabInterval = tabInterval;
        }

        virtual public List<TabStop> TabStops
        {
            get { return tabStops; }
            set { tabStops = value; }
        }

        virtual public float TabInterval
        {
            get { return tabInterval; }
            set { tabInterval = value; }
        }

        virtual public TabStop GetTabStopNewInstance(float currentPosition)
        {
            TabStop tabStop = null;
            if (tabStops != null)
            {
                foreach (TabStop currentTabStop in tabStops)
                {
                    if (currentTabStop.Position - currentPosition > 0.001)
                    {
                        tabStop = new TabStop(currentTabStop);
                        break;
                    }
                }
            }

            if (tabStop == null)
            {
                tabStop = TabStop.NewInstance(currentPosition, tabInterval);
            }

            return tabStop;
        }
    }
}
