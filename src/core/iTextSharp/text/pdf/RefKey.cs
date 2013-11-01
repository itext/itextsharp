namespace iTextSharp.text.pdf {
    public class RefKey {
        private int num;
        private int gen;

        internal RefKey(int num, int gen) {
            this.num = num;
            this.gen = gen;
        }

        public RefKey(PdfIndirectReference referemce) {
            num = referemce.Number;
            gen = referemce.Generation;
        }

        internal RefKey(PRIndirectReference reference) {
            num = reference.Number;
            gen = reference.Generation;
        }

        public override int GetHashCode() {
            return (gen << 16) + num;
        }

        public override bool Equals(object o) {
            if (!(o is RefKey))
                return false;
            RefKey other = (RefKey) o;
            return this.gen == other.gen && this.num == other.num;
        }

        public override string ToString() {
            return "" + num + ' ' + gen;
        }
    }
}
