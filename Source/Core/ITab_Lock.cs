using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using static Locks2.Core.LockConfig;

namespace Locks2.Core
{
    public class ITab_Lock : ITab
    {
        private const int debugInfoSize = 100;

        public override bool IsVisible => (SelThing as Building).GetConfig() != null && SelThing.Map.IsPlayerHome && (SelThing.Faction?.IsPlayer ?? false);
        public override bool StillValid => base.StillValid;

        private LockConfig config;
        private Building door;
        private Rect inRect;
        private Rect viewRect = Rect.zero;
        private Vector2 scrollPosition = Vector2.zero;
        private HashSet<IConfigRule> removalSet = new HashSet<IConfigRule>();

        private static LockConfig currentClip;
        private static readonly Vector2 offset = new Vector2(0, 25);

        public static Rect tabRect;

        public float PawnSectionHeight
        {
            get
            {
                var height = 0f;
                foreach (var rule in config.rules)
                {
                    height += rule.Height;
                }
                return height + 100 + 54 * config.rules.Count + 30;
            }
        }



        public IEnumerable<Pawn> Pawns
        {
            get
            {
                var pawns = door.Map.mapPawns.FreeColonists;
                pawns.AddRange(door.Map.mapPawns.PrisonersOfColony.AsEnumerable());
                return new HashSet<Pawn>(pawns).AsEnumerable();
            }
        }

        public ITab_Lock()
        {
            size = new Vector2(Finder.settings.tabSizeX, Finder.settings.tabSizeY);
            inRect = new Rect(offset, size - offset);
            inRect = inRect.ContractedBy(5);
            labelKey = "lockTab";
            tutorTag = "locks2Tab";
        }

        public override void OnOpen()
        {
            base.OnOpen();
            this.door = SelThing as Building;
            this.config = door.GetConfig();
        }

        public override void CloseTab()
        {
            base.CloseTab();
            var map = SelThing.Map;
            map.reachability.ClearCache();
            config.Dirty();
        }

        public override void UpdateSize()
        {
            base.UpdateSize();
            size = new Vector2(Finder.settings.tabSizeX, Finder.settings.tabSizeY);
            inRect = new Rect(offset, size - offset);
            inRect = inRect.ContractedBy(5);
        }

        public override void FillTab()
        {
            this.UpdateSize();
            if (SelThing is Building temp && (temp.GetConfig() != config || !Find.Selector.selected.Contains(door)))
            {
                CloseTab();
                foreach (Window window in Find.WindowStack.windows)
                {
                    var type = window.GetType();
                    if (false
                        || type == typeof(DefSelection_Window)
                        || type == typeof(PawnSelection_Window)
                        || type == typeof(RuleSelection_Window))
                    {
                        window.Close(doCloseSound: true);
                    }
                }
                return;
            }
            else
            {
                this.config.Dirty();
                Listing_Standard standard = new Listing_Standard();
                standard.Begin(inRect);

                viewRect.size = inRect.size;
                viewRect.height = PawnSectionHeight;
                viewRect.width *= 0.90f;

                standard.BeginScrollView(inRect.AtZero(), ref scrollPosition, ref viewRect);

                var font = Text.Font;
                Text.Font = GameFont.Medium;

                var rect = standard.GetRect(30);
                Widgets.Label(rect, "Locks2DoorSettings".Translate());
                if (Widgets.ButtonImageFitted(rect.RightPartPixels(18), TexButton.Plus))
                {
                    Find.WindowStack.Add(new RuleSelection_Window((rule) => config.rules.Add(rule)));
                    Find.CurrentMap.reachability.ClearCache();
                }
                if (Widgets.ButtonImageFitted(rect.RightPartPixels(36).LeftPartPixels(18), TexButton.Copy))
                {
                    Finder.clip = config;
                    currentClip = config;
                }
                if (currentClip != config && currentClip != null && Widgets.ButtonImageFitted(rect.RightPartPixels(54).LeftPartPixels(18), TexButton.Paste))
                {
                    config.rules.Clear();
                    foreach (var rule in currentClip.rules)
                    {
                        config.rules.Add(rule.Duplicate());
                    }
                }

                Text.Font = GameFont.Tiny;
                FillRules(standard);

                Text.Font = font;
                standard.EndScrollView(ref viewRect);
                standard.End();
            }
        }

        private void FillRules(Listing_Standard standard)
        {
            var counter = 0;
            var moveUp = false;
            var moveDown = false;
            removalSet.Clear();
            if (Finder.debug)
            {
                standard.Gap(5);
                var section = standard.BeginSection_NewTemp(debugInfoSize);
                Text.Font = GameFont.Tiny;
                FillDebuggingInfo(section);
                standard.EndSection(section);
            }
            foreach (var rule in config.rules)
            {
                if (counter != 0) FillSeperator(standard);
                FillRow(rule, standard, (listing) =>
                {
                    var rect = listing.GetRect(rule.Height);
                    var leftPart = rect.LeftPartPixels(18);
                    if (counter != 0 && Widgets.ButtonImageFitted(leftPart.TopPartPixels(18), TexButton.ReorderUp))
                    {
                        moveUp = true;
                    }
                    if (counter != config.rules.Count - 1 && Widgets.ButtonImageFitted(leftPart.TopPartPixels(54).BottomPartPixels(18), TexButton.ReorderDown))
                    {
                        moveDown = true;
                    }
                    if (Widgets.ButtonImageFitted(leftPart.TopPartPixels(36).BottomPartPixels(18), TexButton.Minus))
                    {
                        removalSet.Add(rule);
                        Find.CurrentMap.reachability.ClearCache();
                    }
                    rule.DoContent(Pawns, rect.RightPart(0.90f));
                }, Mathf.Max(rule.Height, 54));
                if (moveUp || moveDown) break;
                counter++;
            }
            if (moveUp)
            {
                var temp = config.rules[counter];
                config.rules[counter] = config.rules[counter - 1];
                config.rules[counter - 1] = temp;
            }
            if (moveDown)
            {
                var temp = config.rules[counter];
                config.rules[counter] = config.rules[counter + 1];
                config.rules[counter + 1] = temp;
            }
            foreach (var rule in removalSet)
            {
                config.rules.Remove(rule);
            }
        }

        private void FillSeperator(Listing_Standard standard)
        {
            var font = Text.Font;
            var anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Tiny;
            standard.Label("Locks2Or".Translate(), tooltip: "Locks2OrTooltip".Translate());
            Text.Anchor = anchor;
            Text.Font = font;
        }

        private void FillRow(IConfigRule rule, Listing_Standard standard, Action<Listing_Standard> doContent, float height)
        {
            standard.Gap(gapHeight: 5f);
            var font = Text.Font;
            var anchor = Text.Anchor;
            Listing_Standard row_standard = standard.BeginSection_NewTemp(height);
            doContent.Invoke(row_standard);
            standard.EndSection(row_standard);
            Text.Font = font;
            Text.Anchor = anchor;
        }

        private void FillDebuggingInfo(Listing_Standard standard)
        {

        }
    }
}
