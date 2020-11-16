using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Locks2.Core
{
    public class Selector_PawnSelection : ISelector
    {
        private Vector2 scrollPosition = Vector2.zero;
        private Rect viewRect = Rect.zero;
        private string searchString = "";

        public IEnumerable<Pawn> pawns;
        public Action<Pawn> onSelect;

        public Selector_PawnSelection(IEnumerable<Pawn> pawns, Action<Pawn> onSelect, bool integrated = false, Action closeAction = null) : base(integrated, closeAction)
        {
            this.pawns = pawns;
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
        }
    }
}
