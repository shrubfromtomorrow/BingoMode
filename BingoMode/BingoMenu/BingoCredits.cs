using Menu;
using MoreSlugcats;
using Music;
using RWCustom;
using System.IO;
using System.Text;
using UnityEngine;

namespace BingoMode
{
    // Copy of EndCredits and related from the vanilla game
    public class BingoCredits : Menu.Menu
    {
        public float time;
        public RainEffect rainEffect;
        public float scrollSpeed;
        public bool anyButton;
        public bool lastAnyButton;
        public bool quitToMenu;
        public BingoCreditsObject currentCreditsObject;
        private int musicTimer = 400;
        public BingoCredits.Stage currentStage;
        public KarmaLadderScreen.SleepDeathScreenDataPackage passthroughPackage;
        public string desiredCreditsSong;

        public BingoCredits(ProcessManager manager) : base(manager, BingoEnums.BingoCredits)
        {
            this.pages.Add(new Page(this, null, "main", 0));
            this.rainEffect = new RainEffect(this, this.pages[0]);
            this.pages[0].subObjects.Add(this.rainEffect);
            this.currentStage = BingoCredits.Stage.InitialWait;
            this.desiredCreditsSong = manager.desiredCreditsSong;
            this.mySoundLoopID = SoundID.MENU_End_Credits_LOOP;
        }

        public void NextStage()
        {
            if (this.currentStage == BingoCredits.Stage.End)
            {
                if (!this.quitToMenu)
                {
                    this.ExitCredits();
                }
                this.quitToMenu = true;
                return;
            }
            this.DisposeCreditsObject();
            this.currentStage = new BingoCredits.Stage(ExtEnum<BingoCredits.Stage>.values.GetEntry(this.currentStage.Index + 1), false);
            this.SpawnCreditsObject(false);
        }

        public void PreviousStage()
        {
            if (this.currentStage.Index <= BingoCredits.Stage.ModCreators.Index || this.currentStage == BingoCredits.Stage.End)
            {
                return;
            }
            this.DisposeCreditsObject();
            this.currentStage = new BingoCredits.Stage(ExtEnum<BingoCredits.Stage>.values.GetEntry(this.currentStage.Index - 1), false);
            this.SpawnCreditsObject(true);
        }

        public void DisposeCreditsObject()
        {
            if (this.currentCreditsObject != null)
            {
                this.currentCreditsObject.RemoveSprites();
                this.pages[0].subObjects.Remove(this.currentCreditsObject);
                this.currentCreditsObject = null;
            }
        }

        public void SpawnCreditsObject(bool startFromBottom)
        {
            if (currentStage != BingoCredits.Stage.End && currentStage != BingoCredits.Stage.InitialWait)
            {
                this.currentCreditsObject = new BingoCreditsTextAndImage(this, this.pages[0], this.currentStage, startFromBottom);
                this.pages[0].subObjects.Add(this.currentCreditsObject);
                return;
            }
        }

        public override void Update()
        {
            base.Update();
            this.anyButton = false;
            for (int i = 0; i < this.manager.rainWorld.options.controls.Length; i++)
            {
                this.anyButton = (this.anyButton || this.manager.rainWorld.options.controls[i].GetAnyButton());
            }
            if (this.desiredCreditsSong != "" && this.musicTimer > 0 && this.manager.musicPlayer != null && (this.manager.musicPlayer.song == null || !(this.manager.musicPlayer.song is IntroRollMusic)))
            {
                this.musicTimer--;
                if (this.musicTimer == 0)
                {
                    this.manager.musicPlayer.MenuRequestsSong(this.desiredCreditsSong, 1.4f, 0f);
                }
            }
            if (this.currentCreditsObject != null)
            {
                if (this.input.y > 0)
                {
                    this.scrollSpeed = Custom.LerpAndTick(this.scrollSpeed, -4f, 0.12f, 0.09090909f);
                }
                else if (this.input.y < 0)
                {
                    this.scrollSpeed = Custom.LerpAndTick(this.scrollSpeed, 16f, 0.12f, 0.09090909f);
                }
                else
                {
                    this.scrollSpeed = Custom.LerpAndTick(this.scrollSpeed, this.anyButton ? 0f : this.currentCreditsObject.CurrentDefaultScrollSpeed, 0.12f, 0.09090909f);
                }
                if (this.currentCreditsObject.OutOfScreen)
                {
                    this.NextStage();
                }
                else if (this.currentCreditsObject.BeforeScreen && this.scrollSpeed < 0f)
                {
                    this.PreviousStage();
                }
            }
            else
            {
                this.NextStage();
            }
            if (!this.quitToMenu && RWInput.CheckPauseButton(0, false))
            {
                this.quitToMenu = true;
                this.ExitCredits();
            }
            this.lastAnyButton = this.anyButton;
            if (this.time > 14f && UnityEngine.Random.value < 0.00625f)
            {
                this.rainEffect.LightningSpike(Mathf.Pow(UnityEngine.Random.value, 2f) * 0.85f, Mathf.Lerp(20f, 120f, UnityEngine.Random.value));
            }
        }

        public override void RawUpdate(float dt)
        {
            base.RawUpdate(dt);
            this.time += dt;
            this.rainEffect.rainFade = Custom.SCurve(Mathf.InverseLerp(0f, 6f, this.time), 0.8f) * 0.5f;
        }

        public override void CommunicateWithUpcomingProcess(MainLoopProcess nextProcess)
        {
            base.CommunicateWithUpcomingProcess(nextProcess);
            if (this.passthroughPackage != null && nextProcess is KarmaLadderScreen)
            {
                (nextProcess as KarmaLadderScreen).GetDataFromGame(this.passthroughPackage);
            }
        }

        public void ExitCredits()
        {
            this.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MainMenu);
        }

        public class BingoCreditsTextAndImage : BingoCreditsObject
        {
            public override bool OutOfScreen
            {
                get
                {
                    for (int i = 0; i < this.subObjects.Count; i++)
                    {
                        if (this.subObjects[i] is RectangularMenuObject && this.LowestPoint(this.subObjects[i] as RectangularMenuObject) < 800f)
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }

            public override bool BeforeScreen
            {
                get
                {
                    return this.scroll <= -100f;
                }
            }

            private float LowestPoint(RectangularMenuObject testObj)
            {
                if (testObj is MenuIllustration && (testObj as MenuIllustration).anchorCenter)
                {
                    return testObj.DrawY(1f) - testObj.size.y / 2f;
                }
                return testObj.DrawY(1f);
            }

            public override float CurrentDefaultScrollSpeed
            {
                get
                {
                    return Custom.LerpMap(Mathf.Abs(this.slowDownPos - this.scroll), this.slowDownZone, this.slowDownZone + 100f, this.slowDownScrollSpeed, this.defaulScrollSpeed, 0.6f);
                }
            }

            public BingoCreditsTextAndImage(Menu.Menu menu, MenuObject owner, BingoCredits.Stage stage, bool startFromBottom) : base(menu, owner, stage, startFromBottom)
            {
                string text = "";
                this.defaulScrollSpeed = (ModManager.MMF ? 2.5f : 3f);
                this.slowDownScrollSpeed = (ModManager.MMF ? 1.5f : 2f);
                if (stage == BingoCredits.Stage.BingoLogo)
                {
                    string folder = $"illustrations{Path.DirectorySeparatorChar}intro_roll";
                    this.subObjects.Add(new MenuIllustration(menu, this, folder, "bingomaintitle", new Vector2(683f, 384f), true, true));
                    (this.subObjects[0] as MenuIllustration).alpha = 0f;
                    this.scroll = 0f;
                    this.lastScroll = 0f;
                    this.slowDownPos = 0f;
                    this.defaulScrollSpeed = 4f;
                    this.slowDownScrollSpeed = 4f;
                }
                else if (stage == BingoCredits.Stage.ModCreators)
                {
                    text = "01 - BINGO DEVELOPERS";
                }
                else if (stage == BingoCredits.Stage.BetaTesters)
                {
                    text = "02 - BINGO BETA TESTERS";
                }
                else if (stage == BingoCredits.Stage.BingoEvent)
                {
                    text = "03 - BINGO EVENT";
                }
                else if (stage == BingoCredits.Stage.SpecialThanks)
                {
                    text = "04 - BINGO SPECIAL THANKS";
                }
                else if (stage == BingoCredits.Stage.NacuThanks)
                {
                    text = "05 - BINGO NACU THANKS";
                    this.defaulScrollSpeed = 1f;
                    this.slowDownScrollSpeed = 0.5f;
                }
                else if (stage == BingoCredits.Stage.IcedThanks)
                {
                    text = "06 - BINGO ICED THANKS";
                    this.defaulScrollSpeed = 1f;
                    this.slowDownScrollSpeed = 0.5f;
                }
                else if (stage == BingoCredits.Stage.ShrubThanks)
                {
                    text = "07 - BINGO SHRUB THANKS";
                    this.defaulScrollSpeed = 1f;
                    this.slowDownScrollSpeed = 0.5f;
                }
                float num = 0f;
                if (text != null)
                {
                    string path = AssetManager.ResolveFilePath(string.Concat(new string[]
                    {
                    "Text",
                    Path.DirectorySeparatorChar.ToString(),
                    "Credits",
                    Path.DirectorySeparatorChar.ToString(),
                    text,
                    ".txt"
                    }));
                    string[] array;
                    if (File.Exists(path))
                    {
                        array = File.ReadAllLines(path, Encoding.UTF8);
                    }
                    else
                    {
                        array = new string[0];
                    }
                    for (int i = 0; i < array.Length; i++)
                    {
                        array[i] = array[i].Replace("<LINE>", "\r\n");
                        this.subObjects.Add(new MenuLabel(menu, this, array[i], new Vector2(433f, (float)(-(float)i) * 40f), new Vector2(500f, 30f), false, null));
                        (this.subObjects[this.subObjects.Count - 1] as MenuLabel).label.alignment = FLabelAlignment.Center;
                        (this.subObjects[this.subObjects.Count - 1] as MenuLabel).label.x = -1000f;
                    }
                    num = (float)array.Length * 40f + 800f;
                }
                this.scroll = (startFromBottom ? (num - 50f) : -100f);
                this.lastScroll = this.scroll;
                this.pos.y = this.scroll;
                this.lastPos.y = this.pos.y;
            }

            public override void Update()
            {
                base.Update();
                this.pos.y = this.scroll;
                if (this.stage == BingoCredits.Stage.BingoLogo)
                {
                    if (this.age < 80)
                    {
                        this.pos.y = 0f;
                        this.scroll = 0f;
                        (this.menu as BingoCredits).scrollSpeed = 0f;
                    }
                    (this.subObjects[0] as MenuIllustration).alpha = Custom.SCurve(Mathf.InverseLerp(0f, 60f, (float)this.age), 0.65f);
                }
            }

            public float slowDownPos = 500f;
        }

        public abstract class BingoCreditsObject : PositionedMenuObject
        {
            public virtual float CurrentDefaultScrollSpeed
            {
                get
                {
                    return 4f;
                }
            }

            public virtual bool OutOfScreen
            {
                get
                {
                    return false;
                }
            }

            public virtual bool BeforeScreen
            {
                get
                {
                    return this.scroll <= -1000f;
                }
            }

            public BingoCreditsObject(Menu.Menu menu, MenuObject owner, BingoCredits.Stage stage, bool startFromBottom) : base(menu, owner, default(Vector2))
            {
                this.stage = stage;
            }

            public override void Update()
            {
                base.Update();
                this.lastScroll = this.scroll;
                this.scroll += (this.menu as BingoCredits).scrollSpeed;
                this.scroll = Mathf.Max(this.scroll, -1000f);
                this.age++;
            }

            public float scroll;

            public float lastScroll;

            public BingoCredits.Stage stage;

            public float slowDownZone = 30f;

            public float defaulScrollSpeed = 4f;

            public float slowDownScrollSpeed = 1f;

            public int age;
        }

        public class Stage : ExtEnum<BingoCredits.Stage>
        {
            public Stage(string value, bool register = false) : base(value, register)
            {
            }

            public static readonly BingoCredits.Stage InitialWait = new BingoCredits.Stage("InitialWait", true);
            public static readonly BingoCredits.Stage BingoLogo = new BingoCredits.Stage("BingoLogo", true);
            public static readonly BingoCredits.Stage ModCreators = new BingoCredits.Stage("ModCreators", true);
            public static readonly BingoCredits.Stage BetaTesters = new BingoCredits.Stage("BetaTesters", true);
            public static readonly BingoCredits.Stage BingoEvent = new BingoCredits.Stage("BingoEvent", true);
            public static readonly BingoCredits.Stage SpecialThanks = new BingoCredits.Stage("SpecialThanks", true);
            public static readonly BingoCredits.Stage NacuThanks = new BingoCredits.Stage("NacuThanks", true);
            public static readonly BingoCredits.Stage IcedThanks = new BingoCredits.Stage("IcedThanks", true);
            public static readonly BingoCredits.Stage ShrubThanks = new BingoCredits.Stage("ShrubThanks", true);
            public static readonly BingoCredits.Stage End = new BingoCredits.Stage("End", true);
        }
    }
}
