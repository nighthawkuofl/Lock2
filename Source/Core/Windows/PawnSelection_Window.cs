using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Locks2.Core
{
    public class PawnSelection_Window : Window
    {
        private Vector2 scrollPosition = Vector2.zero;
        private Rect viewRect = Rect.zero;
        private string searchString = "";

        public IEnumerable<Pawn> pawns;
        public Action<Pawn> onSelect;

        public PawnSelection_Window(IEnumerable<Pawn> pawns, Action<Pawn> onSelect)
        {
            this.pawns = pawns;
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
                foreach (Pawn pawn in pawns)
                {
                    var name = pawn.Name.ToString();
                    if (name.Contains(searchString))
                    {
                        var rect = standard.GetRect(50);
                        Widgets.DrawHighlightIfMouseover(rect);
                        Widgets.DrawTextureFitted(rect.LeftPartPixels(50), PortraitsCache.Get(pawn, new Vector2(50, 50)), 1);
                        Text.Anchor = TextAnchor.MiddleLeft;
                        Widgets.Label(new Rect(rect.position + new Vector2(60, 0), rect.size - new Vector2(60, 0)), name);
                        if (Widgets.ButtonInvisible(rect))
                        {
                            onSelect(pawn);
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
