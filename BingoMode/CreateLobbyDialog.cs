using BingoMode.BingoSteamworks;
using Menu;
using Menu.Remix;
using Menu.Remix.MixedUI;
using RWCustom;
using Steamworks;
using System;
using UnityEngine;

namespace BingoMode
{
    public class CreateLobbyDialog : Dialog, CheckBox.IOwnCheckBox
    {
        float leftAnchor;
        bool opening;
        bool closing;
        float uAlpha;
        float currentAlpha;
        float lastAlpha;
        float targetAlpha;
        float num;
        FSprite pageTitle;
        SimpleButton closeButton;
        SimpleButton createButton;
        CheckBox lockout;
        //CheckBox gameMode;
        CheckBox friendsOnly;
        CheckBox banCheats;
        CheckBox[] perks;
        CheckBox[] burdens;
        FLabel[] labels;
        FSprite[] dividers;
        bool inLobby;
        BingoPage owner;
        MenuTabWrapper menuTabWrapper;
        ConfigurableBase maxPlayersConf;
        OpUpdown maxPlayers;
        UIelementWrapper maxPlayersWrapper;

        public CreateLobbyDialog(ProcessManager manager, BingoPage owner, bool inLobby = false, bool host = false) : base(manager)
        {
            this.inLobby = inLobby;
            float[] screenOffsets = Custom.GetScreenOffsets();
            leftAnchor = screenOffsets[0];
            Vector2 outOfBounds = new Vector2(10000f, 10000f);
            this.owner = owner;

            pageTitle = new FSprite(inLobby ? "lobbysettings" : "createlobby", true);
            pageTitle.SetAnchor(0.5f, 0.5f);
            pageTitle.x = 683f;
            pageTitle.y = 715f;
            pageTitle.shader = manager.rainWorld.Shaders["MenuText"];
            pages[0].Container.AddChild(pageTitle);

            num = 85f;
            float num2 = LabelTest.GetWidth(Translate("CLOSE"), false) + 10f;
            if (num2 > num)
            {
                num = num2;
            }
            closeButton = new SimpleButton(this, pages[0], Translate("CLOSE"), "CLOSE", outOfBounds, new Vector2(num, 35f));
            pages[0].subObjects.Add(closeButton);
            if (!inLobby)
            {
                createButton = new SimpleButton(this, pages[0], "CREATE", "CREATE", outOfBounds, new Vector2(num, 35f));
                pages[0].subObjects.Add(createButton);
            }

            lockout = new CheckBox(this, pages[0], this, outOfBounds, 0f, "Lockout: ", "LOCKOUT");
            lockout.label.label.alignment = FLabelAlignment.Right;
            lockout.buttonBehav.greyedOut = inLobby && !host;
            pages[0].subObjects.Add(lockout);
            //gameMode = new CheckBox(this, pages[0], this, outOfBounds, 0f, "Teams mode: ", "GAMEMODE");
            //gameMode.label.label.alignment = FLabelAlignment.Right;
            //gameMode.buttonBehav.greyedOut = inLobby && !host;
            //pages[0].subObjects.Add(gameMode);
            friendsOnly = new CheckBox(this, pages[0], this, outOfBounds, 0f, "Friends only: ", "FRIENDS");
            friendsOnly.label.label.alignment = FLabelAlignment.Right;
            friendsOnly.buttonBehav.greyedOut = inLobby && !host;
            pages[0].subObjects.Add(friendsOnly);
            banCheats = new CheckBox(this, pages[0], this, outOfBounds, 0f, "Ban cheat mods: ", "CHEATS");
            banCheats.label.label.alignment = FLabelAlignment.Right;
            banCheats.buttonBehav.greyedOut = inLobby && !host;
            pages[0].subObjects.Add(banCheats);

            menuTabWrapper = new MenuTabWrapper(this, pages[0]);
            pages[0].subObjects.Add(menuTabWrapper);
            maxPlayersConf = MenuModList.ModButton.RainWorldDummy.config.Bind<int>("_LobbyMaxPlayers", 4, new ConfigAcceptableRange<int>(1, 32));
            maxPlayers = new OpUpdown(true, maxPlayersConf, outOfBounds, 50f);
            maxPlayers.OnValueChanged += MaxPlayers_OnValueChanged;
            maxPlayersWrapper = new UIelementWrapper(menuTabWrapper, maxPlayers);
            maxPlayers.greyedOut = inLobby && !host;
            if (inLobby) maxPlayers.valueInt = SteamMatchmaking.GetLobbyMemberLimit(SteamTest.CurrentLobby);

            perks = new CheckBox[3];
            burdens = new CheckBox[3];
            string[] texts = { "Allowed - ", "Disabled - ", "Host decides - " };
            for (int i = 0; i < 3; i++)
            {
                perks[i] = new CheckBox(this, pages[0], this, outOfBounds, 0f, texts[i], "PERJ" + i.ToString());
                perks[i].label.label.alignment = FLabelAlignment.Right;
                pages[0].subObjects.Add(perks[i]);
                burdens[i] = new CheckBox(this, pages[0], this, outOfBounds, 0f, texts[i], "BURJ" + i.ToString());
                burdens[i].label.label.alignment = FLabelAlignment.Right;
                pages[0].subObjects.Add(burdens[i]);
                perks[i].buttonBehav.greyedOut = inLobby && !host;
                burdens[i].buttonBehav.greyedOut = inLobby && !host;
            }

            labels = new FLabel[3];
            for (int i = 0; i < 2; i++)
            {
                labels[i] = new FLabel(Custom.GetFont(), i == 0 ? "Perks:" : "Burdens:") { anchorX = 0.5f, anchorY = 0.5f, shader = manager.rainWorld.Shaders["MenuText"] };
                pages[0].Container.AddChild(labels[i]);
            }
            labels[2] = new FLabel(Custom.GetFont(), "Max Players: ") { anchorX = 1f, anchorY = 0.5f, color = MenuColor(MenuColors.MediumGrey).rgb };
            pages[0].Container.AddChild(labels[2]);

            dividers = new FSprite[2];
            for (int i = 0; i < 2; i++)
            {
                dividers[i] = new FSprite("pixel")
                {
                    scaleY = 2,
                    scaleX = 400,
                };
                pages[0].Container.AddChild(dividers[i]);
            }

            targetAlpha = 1f;
            opening = true;
        }

        private void MaxPlayers_OnValueChanged(UIconfig config, string value, string oldValue)
        {
            if (!inLobby) return;
            int players = maxPlayers.GetValueInt();
            SteamMatchmaking.SetLobbyMemberLimit(SteamTest.CurrentLobby, players);
            Plugin.logger.LogMessage("Set lobby member limit to " + players);
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
                labels[0].RemoveFromContainer();
                labels[1].RemoveFromContainer();
                labels[2].RemoveFromContainer();
                for (int i = 0; i < 2; i++)
                {
                    dividers[i].RemoveFromContainer();
                }
                menuTabWrapper.wrappers.Remove(maxPlayers);
                menuTabWrapper.subObjects.Remove(maxPlayersWrapper);
                manager.StopSideProcess(this);
                closing = false;
            }
            closeButton.buttonBehav.greyedOut = opening;
            if (!inLobby) createButton.buttonBehav.greyedOut = opening;
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

            Vector2 pagePos = Vector2.Lerp(pages[0].lastPos, pages[0].pos, timeStacker);

            pageTitle.SetPosition(new Vector2(683f - leftAnchor, 685f) + pagePos);

            float xPos = 670f;
            float yTop = 578f;
            lockout.pos = new Vector2(xPos, yTop);
            //gameMode.pos = new Vector2(xPos, yTop - 30f);
            friendsOnly.pos = new Vector2(xPos, yTop - 30f);
            banCheats.pos = new Vector2(xPos, yTop - 60f);
            maxPlayers.pos = new Vector2(xPos, yTop - 95f);

            for (int i = 0; i < 3; i++)
            {
                perks[i].pos = new Vector2(xPos - 120f + 120f * i, yTop - 150f);
                burdens[i].pos = new Vector2(xPos - 120f + 120f * i, yTop - 200f);
            }

            labels[0].SetPosition(pagePos + new Vector2(xPos, yTop - 115f));
            labels[1].SetPosition(pagePos + new Vector2(xPos, yTop - 165f)); 
            labels[2].SetPosition(pagePos + new Vector2(xPos, yTop - 77f)); 
            
            for (int i = 0; i < 2; i++)
            {
                dividers[i].alpha = darkSprite.alpha;
            }
            dividers[0].SetPosition(pagePos + new Vector2(xPos, yTop - 100f));
            dividers[1].SetPosition(pagePos + new Vector2(xPos, yTop - 215f));

            float xxx = inLobby ? 683f - num / 2f : 683f - num - 10f;
            closeButton.pos = new Vector2(xxx, yTop - 265f);
            if (!inLobby) createButton.pos = new Vector2(683f + 10f, yTop - 265f);
        }

        public override void Singal(MenuObject sender, string message)
        {
            base.Singal(sender, message);
            switch (message)
            {
                case "CLOSE":
                    closing = true;
                    targetAlpha = 0f;
                    if (owner.fromContinueGame) owner.rightPage.Clicked();
                    if (inLobby) SteamTest.UpdateOnlineSettings();
                    break;
                case "CREATE":
                    SteamTest.CreateLobby(maxPlayers.valueInt);
                    closing = true;
                    targetAlpha = 0f;
                    break;
            }
        }

        public bool GetChecked(CheckBox box)
        {
            if (box.IDString == null) return false;
            if (box.IDString.StartsWith("PERJ"))
            {
                int i = int.Parse(box.IDString.Substring(4), System.Globalization.NumberStyles.Any);
                switch (i)
                {
                    case 0:
                        return BingoData.globalSettings.perks == LobbySettings.AllowUnlocks.Any;
                    case 1:
                        return BingoData.globalSettings.perks == LobbySettings.AllowUnlocks.None;
                    case 2:
                        return BingoData.globalSettings.perks == LobbySettings.AllowUnlocks.Inherited;
                }
                return false;
            }
            if (box.IDString.StartsWith("BURJ"))
            {
                int i = int.Parse(box.IDString.Substring(4), System.Globalization.NumberStyles.Any);
                switch (i)
                {
                    case 0:
                        return BingoData.globalSettings.burdens == LobbySettings.AllowUnlocks.Any;
                    case 1:
                        return BingoData.globalSettings.burdens == LobbySettings.AllowUnlocks.None;
                    case 2:
                        return BingoData.globalSettings.burdens == LobbySettings.AllowUnlocks.Inherited;
                }
                return false;
            }
            switch (box.IDString)
            {
                case "LOCKOUT":
                    return BingoData.globalSettings.lockout;
                //case "GAMEMODE":
                //    return BingoData.globalSettings.gameMode;
                case "FRIENDS":
                    return BingoData.globalSettings.friendsOnly;
                case "CHEATS":
                    return BingoData.globalSettings.banMods;
            }
            return false;
        }

        public void SetChecked(CheckBox box, bool c)
        {
            if (box.IDString == null) return;
            if (box.IDString.StartsWith("PERJ"))
            {
                int i = int.Parse(box.IDString.Substring(4), System.Globalization.NumberStyles.Any);
                switch (i)
                {
                    case 0:
                        BingoData.globalSettings.perks = LobbySettings.AllowUnlocks.Any;
                        break;
                    case 1:
                        BingoData.globalSettings.perks = LobbySettings.AllowUnlocks.None;
                        break;
                    case 2:
                        BingoData.globalSettings.perks = LobbySettings.AllowUnlocks.Inherited;
                        break;
                }
            }
            if (box.IDString.StartsWith("BURJ"))
            {
                int i = int.Parse(box.IDString.Substring(4), System.Globalization.NumberStyles.Any);
                switch (i)
                {
                    case 0:
                        BingoData.globalSettings.burdens = LobbySettings.AllowUnlocks.Any;
                        break;
                    case 1:
                        BingoData.globalSettings.burdens = LobbySettings.AllowUnlocks.None;
                        break;
                    case 2:
                        BingoData.globalSettings.burdens = LobbySettings.AllowUnlocks.Inherited;
                        break;
                }
            }
            switch (box.IDString)
            {
                case "LOCKOUT":
                    BingoData.globalSettings.lockout = c;
                    break;
                //case "GAMEMODE":
                //    BingoData.globalSettings.gameMode = c;
                //    break;
                case "FRIENDS":
                    BingoData.globalSettings.friendsOnly = c;
                    break;
                case "CHEATS":
                    BingoData.globalSettings.banMods = c;
                    break;
            }
        }
    }
}
