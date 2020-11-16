using System;
using UnityEngine;
using Verse;

namespace Locks2.Core
{
    public abstract class ISelector : Window
    {
        private readonly bool integrated;
        private readonly Action closeAction;
        private readonly Rect integratedRect;

        public ISelector(bool integrated = false, Action closeAction = null)
        {
            this.integrated = integrated;
            if (this.integrated && closeAction == null)
            {
                throw new InvalidOperationException("In intergrated mod you must pass a listing_standard and an onclose action");
            }
            this.closeAction = closeAction;
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (integrated)
            {
                throw new InvalidOperationException("Called do DoWindowContents in integrated mod");
            }
            Listing_Standard currentStandard = new Listing_Standard();
            var font = Text.Font;
            var anchor = Text.Anchor;

            inRect.height -= 30;
            currentStandard.Begin(inRect);
            FillContents(currentStandard, inRect);
            currentStandard.End();
            inRect.height += 30;
            if (Widgets.ButtonText(inRect.BottomPartPixels(30), "Locks2Close".Translate()))
            {
                Close();
            }
            Text.Font = font;
            Text.Anchor = anchor;
        }

        public void DoIntegratedContents(Rect rect, Listing_Standard standard)
        {
            var font = Text.Font;
            var anchor = Text.Anchor;

            FillContents(standard, new Rect(Vector2.zero, rect.size));

            Text.Font = font;
            Text.Anchor = anchor;
        }

        public abstract void FillContents(Listing_Standard standard, Rect inRect);

        public override void Close(bool doCloseSound = true)
        {
            if (!integrated)
            {
                base.Close(doCloseSound);
            }
            else
            {
                closeAction.Invoke();
            }
        }
    }
}
