using Menu;
using static Menu.Menu;
using UnityEngine;
using RWCustom;
using Menu.Remix.MixedUI;
using System;
using Expedition;
using System.Collections.Generic;
using Menu.Remix;
using System.Globalization;

namespace BingoMode
{
    public class CustomizerDialog : Dialog
    {
        public float leftAnchor;
        public float rightAnchor;
        public BingoButton owner;
        public FSprite pageTitle;
        public SimpleButton closeButton;
        public FSprite[] dividers;
        public bool opening;
        public bool closing;
        public float uAlpha;
        public float currentAlpha;
        public float lastAlpha;
        public float targetAlpha;
        public FLabel description;
        public SymbolButton randomize;
        public SymbolButton settings;
        public SymbolButton types;
        public bool onSettings;
        public List<Challenge> testList;
        public TypeButton[] testLabels;
        public VerticalSlider slider;
        //public float floatScrollPos;
        //public bool sliderPulled;
        //public float sliderValue;
        //public float sliderValueCap;
        //public float ScrollPos;
        public float sliderF;
        public const int maxItems = 8;

        // I know this is very primitive but idc
        public CustomizerDialog(ProcessManager manager, BingoButton owner) : base(manager)
        {
            float[] screenOffsets = Custom.GetScreenOffsets();
            leftAnchor = screenOffsets[0];
            rightAnchor = screenOffsets[1];
            this.owner = owner;

            pageTitle = new FSprite("customizer", true);
            pageTitle.SetAnchor(0.5f, 0.5f);
            pageTitle.x = 683f;
            pageTitle.y = 715f;
            pageTitle.shader = manager.rainWorld.Shaders["MenuText"];
            pages[0].Container.AddChild(pageTitle);

            description = new FLabel(Custom.GetFont(), owner.challenge.description.WrapText(false, 380f));
            description.shader = manager.rainWorld.Shaders["MenuText"];
            pages[0].Container.AddChild(description);

            dividers = new FSprite[3];
            float num = 85f;
            float num2 = LabelTest.GetWidth(Translate("CLOSE"), false) + 10f;
            if (num2 > num)
            {
                num = num2;
            }
            closeButton = new SimpleButton(this, pages[0], Translate("CLOSE"), "CLOSE", new Vector2(683f - num / 2f, 220f), new Vector2(num, 35f));
            pages[0].subObjects.Add(closeButton);

            for (int i = 0; i < 3; i++)
            {
                dividers[i] = new FSprite("pixel")
                {
                    scaleY = 2,
                    scaleX = 400,
                };
                pages[0].Container.AddChild(dividers[i]);
            }

            randomize = new SymbolButton(this, pages[0], "Sandbox_Randomize", "RANDOMIZE_VARIABLE", new Vector2(663f - leftAnchor, 489f));
            randomize.size = new Vector2(40f, 40f);
            randomize.roundedRect.size = new Vector2(40f, 40f);
            //randomize.roundedRect.borderColor = new HSLColor(1f, 1f, 1f);
            randomize.symbolSprite.scale = 1f;
            pages[0].subObjects.Add(randomize);

            settings = new SymbolButton(this, pages[0], "settingscog", "CHALLENGE_SETTINGS", new Vector2(813f - leftAnchor, 489f));
            settings.size = new Vector2(40f, 40f);
            settings.roundedRect.size = new Vector2(40f, 40f);
            //settings.roundedRect.borderColor = new HSLColor(1f, 1f, 1f);
            settings.symbolSprite.scale = 1f;
            pages[0].subObjects.Add(settings);

            types = new SymbolButton(this, pages[0], "custommenu", "CHALLENGE_TYPES", new Vector2(513f - leftAnchor, 489f));
            types.size = new Vector2(40f, 40f);
            types.roundedRect.size = new Vector2(40f, 40f);
            //types.roundedRect.borderColor = new HSLColor(1f, 1f, 1f);
            types.symbolSprite.scale = 0.6f;
            pages[0].subObjects.Add(types);

            opening = true;
            targetAlpha = 1f;
            UpdateChallenge();
            onSettings = false;

            slider = new VerticalSlider(this, pages[0], "", new Vector2(843f - leftAnchor, 294f), new Vector2(30f, 160f), BingoEnums.CustomizerSlider, true) { floatValue = 1f };
            pages[0].subObjects.Add(slider);

            testList = [.. BingoData.GetAdequateChallengeList(ExpeditionData.slugcatPlayer)];
            testLabels = new TypeButton[testList.Count];
            for (int i = 0; i < testList.Count; i++)
            {
                testLabels[i] = new TypeButton(this, pages[0], new Vector2(380f, 20f), testList[i]);
                pages[0].subObjects.Add(testLabels[i]);
            }
        }

        public override void SliderSetValue(Slider slider, float f)
        {
            if (slider.ID == BingoEnums.CustomizerSlider)
            {
                sliderF = f;
                Plugin.logger.LogMessage(sliderF);
            }
        }

        public override float ValueOfSlider(Slider slider)
        {
            if (slider.ID == BingoEnums.CustomizerSlider)
            {
                return sliderF;
            }
            return 0f;
        }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker); 
            if (opening || closing)
            {
                uAlpha = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastAlpha, currentAlpha, timeStacker)), 1.5f);
                darkSprite.alpha = uAlpha * 0.95f;
            }
            pages[0].pos.y = Mathf.Lerp(manager.rainWorld.options.ScreenSize.y + 100f, 0.01f, (uAlpha < 0.999f) ? uAlpha : 1f);
            for (int i = 0; i < dividers.Length; i++)
            {
                dividers[i].alpha = darkSprite.alpha;
            }
            Vector2 pagePos = Vector2.Lerp(pages[0].lastPos, pages[0].pos, timeStacker);
            dividers[0].SetPosition(new Vector2(683f - leftAnchor, 534f) + pagePos);
            dividers[1].SetPosition(new Vector2(683f - leftAnchor, 484f) + pagePos);
            dividers[2].SetPosition(new Vector2(683f - leftAnchor, 284f) + pagePos);

            pageTitle.SetPosition(new Vector2(683f - leftAnchor, 685f) + pagePos);
            //pageTitle.alpha = Mathf.Lerp(0f, 1f, Mathf.Lerp(0f, 0.85f, darkSprite.alpha));

            description.SetPosition(new Vector2(683f - leftAnchor, 553f) + pagePos);
            description.alpha = darkSprite.alpha;

            Vector2 origPos = new Vector2(483f - leftAnchor, 459f) + pagePos;
            float dif = 20f;
            float sliderDif = 20f * (testLabels.Length - maxItems - 1);
            for (int i = 0; i < testLabels.Length; i++)
            {
                testLabels[i].pos = origPos - new Vector2(0f, dif * i - sliderDif * (1f - sliderF));
                testLabels[i].maxAlpha = Mathf.InverseLerp(274f - pagePos.y, 294f - pagePos.y, testLabels[i].pos.y) - Mathf.InverseLerp(454f - pagePos.y, 474f - pagePos.y, testLabels[i].pos.y);
            }
        }

        public void UpdateChallenge()
        {
            description.text = owner.challenge.description.WrapText(false, 380f);
        }

        public override void Singal(MenuObject sender, string message)
        {
            base.Singal(sender, message);
            switch (message)
            {
                case "CLOSE":
                    closing = true;
                    targetAlpha = 0f;
                    break;
                case "RANDOMIZE_VARIABLE":
                    AssignChallenge(onSettings ? owner.challenge : null);
                    break;
                case "CHALLENGE_SETTINGS":
                    onSettings = true;
                    break;
                case "CHALLENGE_TYPES":
                    onSettings = false;
                    break;
            }
        }

        public override void Update()
        {
            base.Update();
            lastAlpha = currentAlpha;
            currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, 0.2f);
            if (opening && pages[0].pos.y <= 0.01f)
            {
                opening = false;
            }
            if (closing && Math.Abs(currentAlpha - targetAlpha) < 0.09f)
            {
                pageTitle.RemoveFromContainer();
                description.RemoveFromContainer();
                manager.StopSideProcess(this);
                closing = false; 
                for (int i = 0; i < testLabels.Length; i++)
                {
                    pages[0].RemoveSubObject(testLabels[i]);
                }
                testList.Clear();
            }
            closeButton.buttonBehav.greyedOut = opening;
        }

        public void AssignChallenge(Challenge ch = null)
        {
            owner.challenge = BingoHooks.GlobalBoard.RandomBingoChallenge(ch, true);
            BingoHooks.GlobalBoard.SetChallenge(owner.x, owner.y, owner.challenge);
            owner.UpdateText();
            UpdateChallenge();
        }

        public class TypeButton : ButtonTemplate
        {
            public Challenge ch;
            public FLabel text;
            public float maxAlpha;
            public string baseText;

            public bool IsSelected => (menu as CustomizerDialog).owner.challenge.GetType() == ch.GetType() || MouseOver;

            public TypeButton(Menu.Menu menu, MenuObject owner, Vector2 size, Challenge ch) : base(menu, owner, Vector2.zero, size)
            {
                baseText = ch.ChallengeName();
                text = new FLabel(Custom.GetDisplayFont(), baseText);
                text.SetAnchor(new Vector2(0.5f, 0f));
                text.scale = 0.75f;
                owner.Container.AddChild(text);
                this.ch = ch;
            }

            public override void Update()
            {
                base.Update();

                if (IsSelected)
                {
                    text.text = "> " + baseText + " <";
                }
                else text.text = baseText;
            }

            public override void GrafUpdate(float timeStacker)
            {
                base.GrafUpdate(timeStacker);

                text.SetPosition(Vector2.Lerp(lastPos, pos, timeStacker) + new Vector2(200f, 0f));
                if (MouseOver)
                {
                    float g = buttonBehav.extraSizeBump != 1f ? buttonBehav.extraSizeBump : 0.5f * Mathf.Abs(Mathf.Sin(Mathf.Lerp(buttonBehav.lastSin, buttonBehav.sin, timeStacker) / 30f * Mathf.PI));
                    text.alpha = Mathf.Clamp01(maxAlpha - g);
                }
                else text.alpha = maxAlpha;
            }

            public override void RemoveSprites()
            {
                base.RemoveSprites();
                text.RemoveFromContainer();
            }

            public override void Clicked()
            {
                base.Clicked();
                menu.PlaySound(SoundID.MENU_Add_Level);
                (menu as CustomizerDialog).AssignChallenge(ch);
            }
        }
    }
}
