using BingoMode.Challenges;
using Expedition;
using Menu;
using Menu.Remix;
using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using RWCustom;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BingoMode
{
    public class CustomizerDialog : Dialog
    {
        readonly int[] maxItems = [10, 5];
        float leftAnchor;
        bool opening;
        bool closing;
        float uAlpha;
        float currentAlpha;
        float lastAlpha;
        float targetAlpha;
        float sliderF;
        float num;
        bool onSettings;
        bool lastOnSettings;
        BingoButton owner;
        FSprite pageTitle;
        SimpleButton closeButton;
        FSprite[] dividers;
        FLabel description;
        MenuLabel page;
        SymbolButton randomize;
        SymbolButton settings;
        SymbolButton types;
        List<Challenge> testList;
        TypeButton[] testLabels;
        VerticalSlider slider;
        List<ChallengeSetting> challengeSettings;
        MenuTab tab;
        MenuTabWrapper wrapper;

        public CustomizerDialog(ProcessManager manager, BingoButton owner) : base(manager)
        {
            float[] screenOffsets = Custom.GetScreenOffsets();
            leftAnchor = screenOffsets[0];
            this.owner = owner;
            Vector2 outOfBounds = new Vector2(10000, 10000);

            pageTitle = new FSprite("customizer", true);
            pageTitle.SetAnchor(0.5f, 0.5f);
            pageTitle.x = 683f;
            pageTitle.y = 715f;
            pageTitle.shader = manager.rainWorld.Shaders["MenuText"];
            pages[0].Container.AddChild(pageTitle);

            description = new FLabel(Custom.GetFont(), owner.challenge.description.WrapText(false, 380f));
            description.shader = manager.rainWorld.Shaders["MenuText"];
            pages[0].Container.AddChild(description);

            page = new MenuLabel(this, pages[0], ">                 <", default, new Vector2(50f, 6f), false);
            pages[0].subObjects.Add(page);

            num = 85f;
            float num2 = LabelTest.GetWidth(Translate("CLOSE"), false) + 10f;
            if (num2 > num)
            {
                num = num2;
            }
            closeButton = new SimpleButton(this, pages[0], Translate("CLOSE"), "CLOSE", outOfBounds, new Vector2(num, 35f));
            pages[0].subObjects.Add(closeButton);

            dividers = new FSprite[3];
            for (int i = 0; i < 3; i++)
            {
                dividers[i] = new FSprite("pixel")
                {
                    scaleY = 2,
                    scaleX = 400,
                };
                pages[0].Container.AddChild(dividers[i]);
            }

            randomize = new SymbolButton(this, pages[0], "Sandbox_Randomize", "RANDOMIZE_VARIABLE", outOfBounds)
            {
                size = new Vector2(40f, 40f)
            };
            randomize.roundedRect.size = new Vector2(40f, 40f);
            //randomize.roundedRect.borderColor = new HSLColor(1f, 1f, 1f);
            randomize.symbolSprite.scale = 1f;
            pages[0].subObjects.Add(randomize);

            settings = new SymbolButton(this, pages[0], "settingscog", "CHALLENGE_SETTINGS", outOfBounds)
            {
                size = new Vector2(40f, 40f)
            };
            settings.roundedRect.size = new Vector2(40f, 40f);
            //settings.roundedRect.borderColor = new HSLColor(1f, 1f, 1f);
            settings.symbolSprite.scale = 1f;
            pages[0].subObjects.Add(settings);

            types = new SymbolButton(this, pages[0], "custommenu", "CHALLENGE_TYPES", outOfBounds)
            {
                size = new Vector2(40f, 40f)
            };
            types.roundedRect.size = new Vector2(40f, 40f);
            //types.roundedRect.borderColor = new HSLColor(1f, 1f, 1f);
            types.symbolSprite.scale = 0.6f;
            pages[0].subObjects.Add(types);

            opening = true;
            targetAlpha = 1f;
            onSettings = true;

            slider = new VerticalSlider(this, pages[0], "", new Vector2(843f - leftAnchor, 294f), new Vector2(30f, 160f), BingoEnums.CustomizerSlider, true) { floatValue = 1f };
            foreach (var line in slider.lineSprites)
            {
                line.alpha = 0f;
            }
            slider.subtleSliderNob.outerCircle.alpha = 0f;
            pages[0].subObjects.Add(slider);

            testList = [.. BingoData.GetAdequateChallengeList(ExpeditionData.slugcatPlayer)];
            testLabels = new TypeButton[testList.Count];
            for (int i = 0; i < testList.Count; i++)
            {
                testLabels[i] = new TypeButton(this, pages[0], new Vector2(360f, 20f), testList[i]);
                pages[0].subObjects.Add(testLabels[i]);
            }
            wrapper = new MenuTabWrapper(this, pages[0]);
            pages[0].subObjects.Add(wrapper);

            tab = new MenuTab();
            pages[0].Container.AddChild(tab._container);
            tab._Activate();
            tab._Update();
            tab._GrafUpdate(0f);

            ResetSettings(owner.challenge as BingoChallenge);
            UpdateChallenge();
        }

        public void ResetSettings(BingoChallenge ch)
        {
            if (challengeSettings != null)
            {
                foreach (var s in challengeSettings)
                {
                    s.Remove();
                    pages[0].RemoveSubObject(s);
                }
            }
            challengeSettings = [];
            for (int i = 0; i < ch.Settings().Count; i++)
            {
                ChallengeSetting s = new(this, pages[0], new Vector2(683f - leftAnchor, 449f), i, ch.Settings()[i]);
                challengeSettings.Add(s);
                pages[0].subObjects.Add(s);
            }
        }

        public override void SliderSetValue(Slider slider, float f)
        {
            if (slider.ID == BingoEnums.CustomizerSlider)
            {
                sliderF = f;
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
            foreach (var line in slider.lineSprites)
            {
                line.alpha = uAlpha;
            }
            slider.subtleSliderNob.outerCircle.alpha = uAlpha;
            Vector2 pagePos = Vector2.Lerp(pages[0].lastPos, pages[0].pos, timeStacker);
            dividers[0].SetPosition(new Vector2(683f - leftAnchor, 534f) + pagePos);
            dividers[1].SetPosition(new Vector2(683f - leftAnchor, 484f) + pagePos);
            dividers[2].SetPosition(new Vector2(683f - leftAnchor, 284f) + pagePos);

            pageTitle.SetPosition(new Vector2(683f - leftAnchor, 685f) + pagePos);
            //pageTitle.alpha = Mathf.Lerp(0f, 1f, Mathf.Lerp(0f, 0.85f, darkSprite.alpha));

            description.SetPosition(new Vector2(683f - leftAnchor, 553f) + pagePos);
            description.alpha = darkSprite.alpha;

            page.pos = new Vector2((onSettings ? 813f : 513f) - 5f - leftAnchor, 508f);
            page.label.color = onSettings ? settings.roundedRect.sprites[settings.roundedRect.SideSprite(0)].color : types.roundedRect.sprites[types.roundedRect.SideSprite(0)].color;

            closeButton.pos = new Vector2(683f - num / 2f, 220f);
            randomize.pos = new Vector2(663f - leftAnchor, 489f);
            settings.pos = new Vector2(813f - leftAnchor, 489f);
            types.pos = new Vector2(513f - leftAnchor, 489f);

            if (onSettings)
            {
                Vector2 origPos = new Vector2(683f - leftAnchor, 449f) + pagePos;
                float dif = 200f / maxItems[1];
                float sliderDif = dif * (challengeSettings.Count - maxItems[1]);

                for (int i = 0; i < challengeSettings.Count; i++)
                {
                    challengeSettings[i].pos = origPos - new Vector2(0f, dif * i - sliderDif * (1f - (challengeSettings.Count < maxItems[1] ? 1f : sliderF)));
                    if (lastOnSettings != onSettings) challengeSettings[i].lastPos = challengeSettings[i].pos;
                    if (!challengeSettings[i].setAlpha) challengeSettings[i].alpha = Mathf.InverseLerp(274f - pagePos.y, 284f - pagePos.y, challengeSettings[i].pos.y) - Mathf.InverseLerp(454f - pagePos.y, 464f - pagePos.y, challengeSettings[i].pos.y);
                }

                for (int i = 0; i < testLabels.Length; i++)
                {
                    testLabels[i].pos = new Vector2(-10000, -10000);
                    testLabels[i].lastPos = testLabels[i].pos;
                    testLabels[i].maxAlpha = 0f;
                }
            }
            else
            {
                Vector2 origPos = new Vector2(483f - leftAnchor, 459f) + pagePos;
                float dif = 200f / maxItems[0];
                float sliderDif = dif * (testLabels.Length - maxItems[0] + 1);
                for (int i = 0; i < testLabels.Length; i++)
                {
                    testLabels[i].pos = origPos - new Vector2(0f, dif * i - sliderDif * (1f - sliderF));
                    testLabels[i].maxAlpha = Mathf.InverseLerp(284f - pagePos.y, 294f - pagePos.y, testLabels[i].pos.y) - Mathf.InverseLerp(464f - pagePos.y, 474f - pagePos.y, testLabels[i].pos.y);
                    if (lastOnSettings != onSettings) testLabels[i].lastPos = testLabels[i].pos;
                    testLabels[i].buttonBehav.greyedOut = testLabels[i].maxAlpha < 0.2f;
                }

                for (int i = 0; i < challengeSettings.Count; i++)
                {
                    challengeSettings[i].pos = new Vector2(-10000, -10000);
                    challengeSettings[i].lastPos = challengeSettings[i].pos;
                }
            }

            tab._GrafUpdate(timeStacker);
            lastOnSettings = onSettings;
        }

        public void UpdateChallenge()
        {
            if (owner.challenge is BingoEatChallenge c)
            {
                c.isCreature = ChallengeUtils.FoodTypes.IndexOf(c.foodType.Value) > 9;
            }
            else if (owner.challenge is BingoDontUseItemChallenge cc)
            {
                var l = ChallengeUtils.GetCorrectListForChallenge("banitem");
                cc.isFood = l.IndexOf(cc.item.Value) < (l.Length - 6);
            }
            else if (owner.challenge is BingoVistaChallenge ccc)
            {
                ccc.region = ccc.room.Value.Substring(0, 2);
                Plugin.logger.LogMessage("Vista bs: " + ccc.region + " " + ccc.room.Value);
                ccc.location = ChallengeTools.VistaLocations[ccc.region][ccc.room.Value];
            }
            owner.challenge.UpdateDescription();
            owner.UpdateText();
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
                    sliderF = 1f;
                    break;
                case "CHALLENGE_TYPES":
                    onSettings = false;
                    sliderF = 1f;
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
                for (int i = 0; i < 3; i++)
                {
                    dividers[i].RemoveFromContainer();
                }
                manager.StopSideProcess(this);
                closing = false;
                tab._Unload();
                testList.Clear();
            }
            closeButton.buttonBehav.greyedOut = opening;

            tab._Update();
        }

        public void AssignChallenge(Challenge ch = null)
        {
            owner.challenge = BingoHooks.GlobalBoard.RandomBingoChallenge(ch, true);
            BingoHooks.GlobalBoard.SetChallenge(owner.x, owner.y, owner.challenge, -1);
            UpdateChallenge();
            ResetSettings(owner.challenge as BingoChallenge);
        }

        public void FocusOn(ChallengeSetting exception)
        {
            int g = challengeSettings.IndexOf(exception) + 1;
            for (int i = g; i < Mathf.Min(challengeSettings.Count, g + 3); i++)
            {
                challengeSettings[i].setAlpha = true;
                challengeSettings[i].alpha = 0f;
                challengeSettings[i].lastAlpha = 0f;
            }
        }

        public void ResetFocus()
        {
            for (int i = 0; i < challengeSettings.Count; i++)
            {
                challengeSettings[i].setAlpha = false;
            }
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
                text = new FLabel(Custom.GetFont(), baseText);
                text.SetAnchor(new Vector2(0.5f, 0f));
                text.scale = 1f;
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
                    //float g = buttonBehav.extraSizeBump != 1f ? buttonBehav.extraSizeBump : ;
                    text.alpha = Mathf.Clamp01(maxAlpha - 0.5f * Mathf.Abs(Mathf.Sin(Mathf.Lerp(buttonBehav.lastSin, buttonBehav.sin, timeStacker) / 30f * Mathf.PI)));
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

        public class ChallengeSetting : PositionedMenuObject
        {
            public ConfigurableBase conf;
            public UIconfig field;
            public UIelementWrapper cWrapper;
            public MenuLabel label;
            public object value;
            public float alpha;
            public float lastAlpha;
            public Vector2 offSet;
            public bool setAlpha;

            public ChallengeSetting(CustomizerDialog menu, MenuObject owner, Vector2 pos, int index, object value) : base(menu, owner, pos)
            {
                label = new MenuLabel(menu, owner, "", pos - new Vector2(120f, -10f), default, false);
                label.label.anchorX = 1f;
                label.label.anchorY = 0.5f;
                owner.subObjects.Add(label);
                if (value is SettingBox<int> i)
                {
                    this.value = i;
                    conf = MenuModList.ModButton.RainWorldDummy.config.Bind<int>("_ChallengeSetting", i.Value, new ConfigAcceptableRange<int>(1, 500));
                    field = new OpUpdown(true, conf, pos, 60f);
                    field.OnValueChanged += UpdootInt;
                    label.text = i.name;
                    offSet = new Vector2(5f, -2.5f);
                }
                else if (value is SettingBox<string> s)
                {
                    this.value = s;
                    conf = MenuModList.ModButton.RainWorldDummy.config.Bind<string>("_ChallengeSetting", s.Value, (ConfigAcceptableBase)null);
                    field = new OpComboBox(conf as Configurable<string>, pos, 140f, s.listName != null ? ChallengeUtils.GetCorrectListForChallenge(s.listName) : ["Whoops errore"]);
                    field.OnValueChanged += UpdootString;
                    (field as OpComboBox).OnListOpen += FocusThing;
                    (field as OpComboBox).OnListClose += UnfocusThing;
                    label.text = s.name;
                    offSet = new Vector2(5f, 0f);
                }
                else if (value is SettingBox<bool> b)
                {
                    this.value = b;
                    conf = MenuModList.ModButton.RainWorldDummy.config.Bind<bool>("_ChallengeSetting", b.Value, (ConfigAcceptableBase)null);
                    field = new OpCheckBox(conf as Configurable<bool>, pos);
                    field.OnValueChanged += UpdootBool;
                    label.text = b.name;
                    offSet = new Vector2(5f, 0f);
                }
                else
                {
                    throw new Exception("Invalid type for ChallengeSetting!!");
                }
                label.text += ":";
                //if (MenuModList.ModButton.RainWorldDummy.config.configurables.TryGetValue("_ChallengeSetting" + index, out var gruh))
                //{
                //    c = gruh;
                //}
                //menu.tab.AddItems(new UIelement[]
                //{
                //    field
                //});
                cWrapper = new UIelementWrapper(menu.wrapper, field);
            }

            private void UnfocusThing(UIfocusable trigger)
            {
                (menu as CustomizerDialog).ResetFocus();
            }

            private void FocusThing(UIfocusable trigger)
            {
                (menu as CustomizerDialog).FocusOn(this);
            }

            public void UpdootInt(UIconfig config, string v, string oldV)
            {
                (value as SettingBox<int>).Value = (field as OpUpdown).GetValueInt();
                (menu as CustomizerDialog).UpdateChallenge();
            }

            public void UpdootString(UIconfig config, string v, string oldV)
            {
                Plugin.logger.LogMessage("new value: " + field.value);
                (value as SettingBox<string>).Value = field.value;
                (menu as CustomizerDialog).UpdateChallenge();
            }

            public void UpdootBool(UIconfig config, string v, string oldV)
            {
                (value as SettingBox<bool>).Value = (field as OpCheckBox).GetValueBool();
                (menu as CustomizerDialog).UpdateChallenge();
            }

            public override void GrafUpdate(float timeStacker)
            {
                base.GrafUpdate(timeStacker);
                Vector2 positio = Vector2.Lerp(lastPos, pos, timeStacker);
                float alpharad = Mathf.Lerp(lastAlpha, alpha, timeStacker);
                if (field != null)
                {
                    field.pos = positio + offSet;
                }
                label.pos = positio - new Vector2(5f, -12.5f);

                if (!setAlpha) label.label.alpha = alpharad;
                float treshHold = 0.25f; 
                bool yuh = alpharad < treshHold;
                //if (!field.Hidden && yuh) field.Deactivate();
                //else if (alpharad >= treshHold) field.Reactivate();
                field.greyedOut = yuh;
                field._lastGreyedOut = yuh;
                if (yuh && field is OpComboBox b) b._mouseDown = false;
                field.myContainer.alpha = alpharad;

                lastAlpha = alpharad;
            }

            public void Remove()
            {
                //MenuModList.ModButton.RainWorldDummy.config.configurables.Remove("_ChallengeSetting");
                label.RemoveSprites();
                owner.RemoveSubObject(label);
                field.Hide();
                //(menu as CustomizerDialog).tab._RemoveItem(field);
                field.Unload();
                (menu as CustomizerDialog).wrapper.wrappers.Remove(field);
                (menu as CustomizerDialog).wrapper.subObjects.Remove(cWrapper);
            }
        }
    }
}
