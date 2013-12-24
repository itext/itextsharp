using System;

namespace iTextSharp.text
{
    public class AccessibleElementId : IComparable<AccessibleElementId> {

        private static int id_counter = 0;
        private readonly int id;

        public AccessibleElementId() {
            id = ++id_counter;
        }

        public override String ToString() {
            return id.ToString();
        }

        public override int GetHashCode() {
            return id;
        }

        public override bool Equals(Object o) {
            return (o is AccessibleElementId) && (id == ((AccessibleElementId) o).id);
        }

        virtual public int CompareTo(AccessibleElementId elementId) {
            if (id < elementId.id)
                return -1;
            else if (id > elementId.id)
                return 1;
            else
                return 0;
        }
    }
}
