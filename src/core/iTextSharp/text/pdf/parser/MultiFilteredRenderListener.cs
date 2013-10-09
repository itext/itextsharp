using System.Collections.Generic;

namespace iTextSharp.text.pdf.parser {
    internal class MultiFilteredRenderListener : IRenderListener {
        private List<IRenderListener> delegates;
        private List<RenderFilter[]> filters;

        public MultiFilteredRenderListener() {
            delegates = new List<IRenderListener>();
            filters = new List<RenderFilter[]>();
        }

        /**
         * Attaches a {@link RenderListener} for the corresponding filter set.
         * @param delegate RenderListener instance to be attached.
         * @param filterSet filter set to be attached. The delegate will be invoked if all the filters pass.
         */

        public IRenderListener AttachRenderListener(IRenderListener deleg, params RenderFilter[] filterSet) {
            delegates.Add(deleg);
            filters.Add(filterSet);

            return deleg;
        }

        public void BeginTextBlock() {
            foreach (IRenderListener deleg in delegates) {
                deleg.BeginTextBlock();
            }
        }

        public void RenderText(TextRenderInfo renderInfo) {
            for (int i = 0; i < delegates.Count; i++) {
                bool filtersPassed = true;
                foreach (RenderFilter filter in filters[i]) {
                    if (!filter.AllowText(renderInfo)) {
                        filtersPassed = false;
                        break;
                    }
                }
                if (filtersPassed)
                    delegates[i].RenderText(renderInfo);
            }
        }

        public void EndTextBlock() {
            foreach (IRenderListener deleg in delegates) {
                deleg.EndTextBlock();
            }
        }

        public void RenderImage(ImageRenderInfo renderInfo) {
            for (int i = 0; i < delegates.Count; i++) {
                bool filtersPassed = true;
                foreach (RenderFilter filter in filters[i]) {
                    if (!filter.AllowImage(renderInfo)) {
                        filtersPassed = false;
                        break;
                    }
                }
                if (filtersPassed)
                    delegates[i].RenderImage(renderInfo);
            }
        }
    }
}