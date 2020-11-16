using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Locks2.Core
{
    public class DefSelection_Window : Window
    {
        private Vector2 scrollPosition = Vector2.zero;
        private Rect viewRect = Rect.zero;
        private string searchString = "";

        public IEnumerable<Def> defs;
        public Action<Def> onSelect;

        public DefSelection_Window(IEnumerable<Def> defs, Action<Def> onSelect)
        {
            this.defs = defs;
            this.onSelect = onSelect;
        }

        public override void DoWindowContents(Rect inRect)
        {
            var font = Text.Font;
            var anchor = Text.Anchor;
            if (Find.Selector.selected.Count == 0)
            {
                Close();
                return;
            }
            Listing_Standard standard = new Listing_Standard();
            standard.Begin(inRect);
            {
                var rect = standard.GetRect(30);
                Text.Font = GameFont.Tiny;
                var searchRect = new Rect(0, 0, rect.size.x, rect.size.y);
                searchString = Widgets.TextField(searchRect, searchString).ToLower();
            }
            {
                standard.Gap(5);
                var scrollRect = new Rect(inRect.position + new Vector2(0, 50), inRect.size - new Vector2(0, 75));
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
            standard.End();
            if (Widgets.ButtonText(inRect.BottomPartPixels(30), "Locks2Close".Translate()))
            {
                Close();
            }
            Text.Font = font;
            Text.Anchor = anchor;
        }
    }
}
