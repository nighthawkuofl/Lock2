using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using static Locks2.Core.LockConfig;

namespace Locks2.Core
{
    public class Selector_RuleSelection : ISelector
    {
        private Vector2 scrollPosition = Vector2.zero;
        private Rect viewRect = Rect.zero;

        public Type[] rulesTypes;
        public Action<IConfigRule> onSelect;

        private string searchString = "";

        public Selector_RuleSelection(Action<IConfigRule> onSelect, bool integrated = false, Action closeAction = null) : base(integrated, closeAction)
        {
            this.onSelect = onSelect;
            this.rulesTypes = typeof(IConfigRule).AllSubclasses().ToArray();
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

                foreach (Type type in rulesTypes)
                {
                    var name = type.Name.Translate().ToLower().ToString();
                    if (name.Contains(searchString))
                    {
                        var rect = standard.GetRect(30);
                        Widgets.DrawHighlightIfMouseover(rect);
                        Text.Anchor = TextAnchor.MiddleLeft;
                        Widgets.Label(new Rect(rect.position + new Vector2(60, 0), rect.size - new Vector2(60, 0)), name);
                        if (Widgets.ButtonInvisible(rect))
                        {
                            onSelect(Activator.CreateInstance(type) as IConfigRule);
                            Close();
                        }
                    }
                }
                standard.EndScrollView(ref viewRect);
            }
        }
    }
}
