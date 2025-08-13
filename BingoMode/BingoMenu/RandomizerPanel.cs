using BingoMode.BingoRandomizer;
using Menu;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BingoMode.BingoMenu
{
    internal class RandomizerPanel : PositionedMenuObject
    {
        private const float MARGIN = 10f;
        private const float SYMBOL_BUTTON_SIZE = 24f; // not actually modifiable from here (though could be with a bit of work)
        private const float DIVIDER_THICKNESS = 2f;
        private const float HEADER_HEIGHT = 2f * MARGIN + SYMBOL_BUTTON_SIZE + DIVIDER_THICKNESS;
        private const float LIST_BUTTON_HEIGHT = 20f;
        private const float LIST_SPACING = 2f;
        private const float SLIDER_WIDTH = 10f;
        private const float SLIDER_OFFSET = 15f; // not actually modifiable from here
        private const float SLIDER_DEAD_ZONE = 21f; // not actually modifiable from here

        private bool _visible = true;
        private MenuObject ownerMemory;
        private MenuLabel randomizerLabel;
        private FSprite divider;
        private List<SimpleButton> randomizers = [];

        public Vector2 size;
        public float sliderF;
        public bool Visible
        {
            get => _visible;
            set
            {
                if (value == _visible)
                    return;

                if (value)
                {
                    owner = ownerMemory;
                    PopulateRandomizerList();
                }
                else
                {
                    ClearRandomizerList();
                    owner = null;
                }
                _visible = value;
            }
        }


        public RandomizerPanel(Menu.Menu menu, MenuObject owner, Vector2 pos, Vector2 size) : base(menu, owner, pos)
        {
            ownerMemory = owner;
            this.size = size;

            RoundedRect background = new(menu, this, Vector2.zero, size, true);
            subObjects.Add(background);

            SymbolButton unloadButton = new(menu, this, "Menu_Symbol_Clear_All", "UNLOAD", new Vector2(MARGIN, size.y - SYMBOL_BUTTON_SIZE - MARGIN));
            subObjects.Add(unloadButton);

            randomizerLabel = new(menu, this, "unloaded", unloadButton.pos + new Vector2(SYMBOL_BUTTON_SIZE + MARGIN, 3f), new Vector2(0f, 18f), false);
            randomizerLabel.label.alignment = FLabelAlignment.Left;
            subObjects.Add(randomizerLabel);

            divider = new("pixel")
            {
                scaleX = size.x,
                scaleY = DIVIDER_THICKNESS,
                anchorX = 0f,
                anchorY = 0f,
                x = pos.x,
                y = pos.y + size.y - HEADER_HEIGHT
            };
            Container.AddChild(divider);

            VerticalSlider slider = new(
                    menu,
                    this,
                    "",
                    new Vector2(size.x - MARGIN - SLIDER_OFFSET - SLIDER_WIDTH / 2f, MARGIN),
                    new Vector2(SLIDER_WIDTH, size.y - HEADER_HEIGHT - 2 * MARGIN - SLIDER_DEAD_ZONE),
                    BingoEnums.RandomizerSlider, true);
            subObjects.Add(slider);
            sliderF = 1f;
        }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);

            DrawProfileList(timeStacker);

            divider.x = pos.x;
            divider.y = pos.y + size.y - HEADER_HEIGHT;
        }

        public override void Singal(MenuObject sender, string message)
        {
            if (message.StartsWith("LOAD-"))
            {
                string profileName = message.Split('-')[1];
                try
                {
                    BingoRandomizationProfile.LoadFromFile(profileName);
                    randomizerLabel.text = profileName;
                    Singal(sender, "RANDOMIZE");
                }
                catch (Exception e)
                {
                    Plugin.logger.LogError($"Error loading profile {profileName} : {e.Message}");
                }
                return;
            }

            if (message == "UNLOAD")
            {
                BingoRandomizationProfile.Unload();
                randomizerLabel.text = "unloaded";
                Singal(sender, "RANDOMIZE");
                return;
            }

            // TODO : add "refresh profiles" button sending this signal
            if (message == "REFRESH")
            {
                ClearRandomizerList();
                PopulateRandomizerList();
                return;
            }

            // TODO : add "open folder" button sending this signal
            if (message == "OPEN_FOLDER")
            {
                BingoRandomizationProfile.OpenSaveFolder();
                return;
            }

            base.Singal(sender, message);
        }

        private void PopulateRandomizerList()
        {
            Vector2 position = new(MARGIN, size.y - HEADER_HEIGHT - MARGIN - LIST_BUTTON_HEIGHT);
            Vector2 buttonSize = new(size.x - 3 * MARGIN - SLIDER_WIDTH, LIST_BUTTON_HEIGHT);
            foreach (string profile in BingoRandomizationProfile.GetAvailableProfiles())
            {
                SimpleButton button = new(menu, this, profile, $"LOAD-{profile}", position, buttonSize);
                randomizers.Add(button);
                subObjects.Add(button);
                position.y -= LIST_SPACING + LIST_BUTTON_HEIGHT;
            }
        }

        private void ClearRandomizerList()
        {
            foreach (SimpleButton button in randomizers)
            {
                button.RemoveSprites();
                RemoveSubObject(button);
            }
            randomizers.Clear();
        }

        private void DrawProfileList(float timeStacker)
        {
            float top = size.y - HEADER_HEIGHT - MARGIN - LIST_BUTTON_HEIGHT;
            float bottom = MARGIN;
            float list_height = (randomizers.Count - 1) * (LIST_BUTTON_HEIGHT + LIST_SPACING);
            float y = top;
            if (list_height > top - bottom)
                y = Mathf.Lerp(list_height + bottom, top, sliderF);
            foreach (SimpleButton button in randomizers)
            {
                float overshoot = (y > bottom + (top - bottom) / 2f) ?
                        (y - top) / MARGIN :
                        (bottom - y) / MARGIN;
                for (int i = 0; i < 9; i++)
                    button.roundedRect.sprites[i].alpha = Mathf.Lerp(0.07f, 0f, overshoot);
                for (int i = 9; i < 17; i++)
                    button.roundedRect.sprites[i].alpha = Mathf.Lerp(1f, 0f, overshoot);
                button.menuLabel.label.alpha = Mathf.Lerp(1f, 0f, overshoot);
                button.buttonBehav.greyedOut = overshoot >= 1f;
                button.pos.y = y;
                y -= LIST_BUTTON_HEIGHT + LIST_SPACING;
            }
        }
    }
}
