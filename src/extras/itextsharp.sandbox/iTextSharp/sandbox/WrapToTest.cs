using System;

namespace iTextSharp.sandbox
{
    [AttributeUsage(AttributeTargets.Class)]
    public class WrapToTestAttribute : Attribute
    {
        private readonly bool _compareRenders;

        public WrapToTestAttribute() { _compareRenders = false; }
        public WrapToTestAttribute(bool compareRenders) { _compareRenders = compareRenders; }

        public bool CompareRenders
        {
            get { return _compareRenders; }
        }
    }
}
