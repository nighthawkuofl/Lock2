using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Locks2.Core
{
    public class Selector_DefSelection : ISelector
    {
        private Vector2 scrollPosition = Vector2.zero;
        private Rect viewRect = Rect.zero;
        private string searchString = "";

        public IEnumerable<Def> defs;
        public Action<Def> onSelect;

        public Selector_DefSelection(IEnumerable<Def> defs, Action<Def> onSelect, bool integrated = false, Action closeAction = null) : base(integrated, closeAction)
        {
            this.defs = defs;
            this.onSelect = onSelect;
        }

        public override void FillContents(Listing_Standard standard, Rect inRect)
        {
            if (Find.Selector.selected.Count == 0)
            {
                Close();
                return;
            }
            {
                var rect = standard.GetRect(30);
                Text.Font = GameFont.Tiny;
                var searchRect = new Rect(0, 0, rect.width, 20);
                if (Widgets.ButtonImage(searchRect.LeftPartPixels(20), TexButton.CloseXSmall))
                {
                    Close();
                }
                searchString = Widgets.TextField(new Rect(searchRect.position + new Vector2(25, 0), searchRect.size - new Vector2(55, 0)), searchString).ToLower();
            }
            {
                standard.Gap(5);
                var scrollRect = new Rect(inRect.position + new Vector2(0, 50), inRect.size - new Vector2(0, 50));
                var section = standard.BeginSection_NewTemp(scrollRect.height);
                standard.EndSection(section);
                standard.BeginScrollView(new Rect(scrollRect.position + new Vector2(5, 0), scrollRect.size - new Vector2(10, 10)), ref scrollPosition, ref viewRect);
                Text.Font = GameFont.Tiny;
                foreach (Def def in defs)
                {
                    if (def.label.ToLower().Contains(searchString))
                    {
                        var rect = standard.GetRect(50);
                        Widgets.DefLabelWithIcon(rect, def);
                        if (Widgets.ButtonInvisible(rect))
                        {
                            onSelect(def);
                            Close();
                        }
                    }
                }
                standard.EndScrollView(ref viewRect);
            }
        }
    }
}
