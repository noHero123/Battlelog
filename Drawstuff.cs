using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;

namespace Battlelog.Mod
{

    

    public class Drawstuff : MonoBehaviour
    {
        public Vector2 chatScroll = new Vector2();
        Vector2 chatScroll2 = new Vector2();
        private GUIStyle chatLogStyle;
        public Mod moddingmod;

        int ownMenu = 2;
        int enemyMenu = 0;

        Rect smallownrect;
        Rect bigownrect;
        Rect closeownrect;
        Rect genrectown;
        Rect boardrectown;
        Rect dynamicrectown;

        Rect smallopprect;
        Rect bigopprect;
        Rect closeopprect;
        Rect genrectopp;
        Rect boardrectopp;
        Rect dynamicrectopp;
        int osize = 0;
        bool ownsmall = false;
        public GUISkin cardListPopupSkin;
        public GUISkin cardListPopupGradientSkin;
        public GUISkin cardListPopupBigLabelSkin;
        public GUISkin cardListPopupLeftButtonSkin;
        private FieldInfo chatLogStyleinfo;

        Texture2D growthres = ResourceManager.LoadTexture("BattleUI/battlegui_icon_growth");
        Texture2D energyres = ResourceManager.LoadTexture("BattleUI/battlegui_icon_energy");
        Texture2D orderres = ResourceManager.LoadTexture("BattleUI/battlegui_icon_order");
        Texture2D decayres = ResourceManager.LoadTexture("BattleUI/battlegui_icon_decay");


        private static Texture2D _staticRectTexture_l;
        private static GUIStyle _staticRectStyle_l;
        private static Texture2D _staticRectTexture_r;
        private static GUIStyle _staticRectStyle_r;

        private float maxchatlen = 0f;
        private float currentCursor = 0f;
        Settings sttngs;

        List<string> copyBattlelog = new List<string>();
        public bool scrollbarWasDown=false;


        private bool optionmenu = false;
       

        public void Start()
        {
            Console.WriteLine("drawstuff: hello world staart");
        }
        public void Init()
        {
            Console.WriteLine("drawstuff: hello world init");
        }
        
        private static Drawstuff instance;

        public static Drawstuff Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Drawstuff();
                }
                return instance;
            }
        }

        public void OnGUI()
        {
            this.draw(this.moddingmod.battlelog);
        }

        private Drawstuff()
        {
            chatLogStyleinfo = typeof(ChatUI).GetField("chatLogStyle", BindingFlags.Instance | BindingFlags.NonPublic);
            this.setskins((GUISkin)ResourceManager.Load("_GUISkins/CardListPopup"), (GUISkin)ResourceManager.Load("_GUISkins/CardListPopupGradient"), (GUISkin)ResourceManager.Load("_GUISkins/CardListPopupBigLabel"), (GUISkin)ResourceManager.Load("_GUISkins/CardListPopupLeftButton"));
            setRects();
            
        }

        public void setSettings(Settings s)
        {
            this.sttngs = s;

            this.osize = cardListPopupBigLabelSkin.label.fontSize;

            if (this.sttngs.size != 0)
            {
                cardListPopupSkin.label.fontSize = this.sttngs.size;
                cardListPopupBigLabelSkin.label.fontSize = this.sttngs.size;
            }

            this.ownsmall = !this.sttngs.openonStart;
        }

        public void scrollDown()
        {
            if (this.currentCursor >= this.maxchatlen*0.99)
            {
                this.chatScroll.y = int.MaxValue;
            }
        }

        public void setChatlog()
        {
            GUISkin chatSkin = (GUISkin)ResourceManager.Load("_GUISkins/ChatUI");
            this.chatLogStyle = new GUIStyle(chatSkin.label);
            this.chatLogStyle.fontSize = 10 + Screen.height / 72;
            this.chatLogStyle.alignment = TextAnchor.UpperLeft;
            this.chatLogStyle.wordWrap = true;
        }

        public void draw( List<String> battlelog )
        {
            
            if (Event.current.type == EventType.Layout)//works only theoretical ;D
            {
                setRects();
                this.copyBattlelog = new List<string>(battlelog);
            }

            //Console.WriteLine("##" + Event.current.type + " " + this.copyBattlelog.Count );
            //Console.WriteLine("drawlol " + this.smallownrect.x + " " + this.smallownrect.y + " " + this.smallownrect.xMax +  " " + this.smallownrect.yMax);
            GUI.color = Color.white;
            GUI.skin = cardListPopupSkin;
            if (this.ownsmall)
            {
                GUI.Box(this.smallownrect, string.Empty);
                if (GUI.Button(this.smallownrect, "BttlLg"))
                {
                    this.ownsmall = false;
                }
            }
            else
            {
                //try
                //{
                    GUI.Box(this.bigownrect, string.Empty);
                    GUI.Box(this.closeownrect, string.Empty);
                    if (GUI.Button(this.closeownrect, "Minimize"))
                    {
                        this.ownsmall = true;
                    }
                    int oldmenu = this.ownMenu;

                    if (GUI.Button(this.genrectown, "S"))
                    {
                        if (this.optionmenu)
                        {
                            this.sttngs.saveSettings();
                            this.moddingmod.battlelog.Clear();
                            this.moddingmod.battlelog = BigLogEntry.getLogsfromBigLogList(this.moddingmod.bigBattleLog, this.sttngs);
                        }
                        else
                        {
                            this.sttngs.loadSettings();
                        }
                        this.optionmenu = !this.optionmenu;
                    }
       

                    if (this.optionmenu)
                    {
                        GUILayout.BeginArea(this.bigownrect);
                        GUI.skin = this.cardListPopupSkin;
                        this.chatScroll2 = GUILayout.BeginScrollView(this.chatScroll2, new GUILayoutOption[] { GUILayout.Width(this.bigownrect.width), GUILayout.Height(this.bigownrect.height) });

                        

                        this.drawOptions();


                        GUILayout.EndScrollView();
                        GUILayout.EndArea();
                        //end layout
                        
                    }
                    else
                    {
                        GUILayout.BeginArea(this.bigownrect);
                        GUI.skin = this.cardListPopupSkin;
                        this.chatScroll = GUILayout.BeginScrollView(this.chatScroll, new GUILayoutOption[] { GUILayout.Width(this.bigownrect.width), GUILayout.Height(this.bigownrect.height) });


                        GUI.skin = cardListPopupBigLabelSkin;

                        GUI.skin.label.wordWrap = true;
                        //GUI.skin.label.wordWrap = true;

                        //calculate chat-length
                        try
                        {

                            drawdata(this.copyBattlelog);

                        }
                        catch 
                        {
                            Console.WriteLine("typical unity error :D");
                            if (this.scrollbarWasDown)
                            {
                                this.chatScroll.y = int.MaxValue;
                            }
                        }

                            GUILayout.EndScrollView();
                            GUILayout.EndArea();
                    }
                    

                   

                /*}
                catch (Exception exception)
                {
                    string fullStackTrace = exception.StackTrace + Environment.StackTrace;
                    Console.WriteLine("##bttllogerror: " + fullStackTrace);
                }*/
            }
            GUI.skin = cardListPopupSkin;

            
        }

        public void drawdata(List<string> battlelog)
        {
            //GUI.skin = cardListPopupBigLabelSkin;
            bool didsomething =false;
            //for (int i = 0; i < battlelog.Count; i++)
            foreach (string text in battlelog)
            {
                GUILayout.BeginHorizontal(new GUILayoutOption[0]);

                //string text = battlelog[i];


                GUILayout.Label(text);

                didsomething = true;
                GUILayout.EndHorizontal();

                /*string t = battlelog[i];
                bool energyicon = false;
                if (t.Contains(" for ENERGY"))
                {
                    energyicon = true;
                    text = text.Replace(" ENERGY resource", "");

                }


                if (energyicon)
                {
                    GUILayout.Label(new GUIContent(energyres, text));

                    //or 
                    GUILayout.Label(energyres, new GUILayoutOption[] { GUILayout.Width(GUI.skin.label.fontSize + 4), GUILayout.Height(GUI.skin.label.fontSize + 4) });
                    GUILayout.Label(" ");
                }
                else
                {
                    GUILayout.Label(text);
                }*/


            }
            if (didsomething && Event.current.type == EventType.Repaint)
            {

                float lastone = GUILayoutUtility.GetLastRect().yMax;

                if (lastone >= 1) this.maxchatlen = lastone;

                this.currentCursor = this.chatScroll.y + this.bigownrect.height;

                if (this.currentCursor >= this.maxchatlen * 0.99)
                {
                    this.scrollbarWasDown = true;
                }
                else
                {
                    this.scrollbarWasDown = false;
                } 
            }
            

            
        }

        public void drawOptions()
        {
            int diffx = 15;

            GUILayoutOption[] glo = new GUILayoutOption[] { GUILayout.Width(this.bigownrect.width - 2 * diffx) };
            for (int i = 0; i < 10; i++)
            {
                GUILayout.BeginHorizontal(new GUILayoutOption[0]);

                if (i == 0)
                {
                    string text = "show Player";
                    GUI.color = Color.white;
                    if (!this.sttngs.showNames)
                    {
                        text = "dont " + text;
                        GUI.color = new Color(1f, 1f, 1f, 0.5f);
                    }

                    if (GUILayout.Button(text, glo))
                    {
                        this.sttngs.showNames = !this.sttngs.showNames;
                    }
                    GUI.color = Color.white;
                }

                if (i == 2)
                {
                    string text = "show coordinates";
                    GUI.color = Color.white;
                    if (!this.sttngs.showCoordinates)
                    {
                        text = "dont " + text;
                        GUI.color = new Color(1f, 1f, 1f, 0.5f);
                    }
                    if (GUILayout.Button(text, glo))
                    {
                        this.sttngs.showCoordinates = !this.sttngs.showCoordinates;
                    }
                    GUI.color = Color.white;
                }

                if (i == 3)
                {
                    GUI.skin = cardListPopupBigLabelSkin;
                    string text = "Textsize";
                    GUILayout.Label(text);
                    GUI.color = Color.white;
                    GUI.skin = cardListPopupSkin; 
                }

                if (i == 4)
                {

                    if (GUILayout.Button("+"))
                    {
                        this.ownMenu = 1;
                        cardListPopupSkin.label.fontSize = (int)(cardListPopupSkin.label.fontSize + 1);
                        cardListPopupBigLabelSkin.label.fontSize = (int)(cardListPopupBigLabelSkin.label.fontSize + 1);
                        this.sttngs.size = cardListPopupBigLabelSkin.label.fontSize;
                        
                    }
                    if (GUILayout.Button("-"))
                    {
                        this.ownMenu = 0;
                        cardListPopupSkin.label.fontSize = (int)(cardListPopupSkin.label.fontSize - 1);
                        cardListPopupBigLabelSkin.label.fontSize = (int)(cardListPopupBigLabelSkin.label.fontSize - 1);
                        this.sttngs.size = cardListPopupBigLabelSkin.label.fontSize;
                    }
                    if (GUILayout.Button("Reset") && this.osize != 0)
                    {
                        this.ownMenu = 2;
                        cardListPopupSkin.label.fontSize = (int)(this.osize);
                        cardListPopupBigLabelSkin.label.fontSize = (int)(this.osize);
                        this.sttngs.size = cardListPopupBigLabelSkin.label.fontSize;
                    }

                    if (this.ownMenu == 2 || this.osize == 0)
                    {
                        
                    }

                    GUI.color = Color.white;
                }

                if (i == 5)
                {
                    string text = "";
                    GUILayout.Label(text);
                    GUI.color = Color.white;
                }

                if (i == 6)
                {
                    string text = "show Movement";
                    GUI.color = Color.white;
                    if (!this.sttngs.showMove)
                    {
                        text = "dont " + text;
                        GUI.color = new Color(1f, 1f, 1f, 0.5f);
                    }
                    if (GUILayout.Button(text, glo))
                    {
                        this.sttngs.showMove = !this.sttngs.showMove;
                    }
                    GUI.color = Color.white;
                }
                if (i == 7)
                {
                    string text = "show all summons";
                    GUI.color = Color.white;
                    if (!this.sttngs.showsummon)
                    {
                        text = "dont " + text;
                        GUI.color = new Color(1f, 1f, 1f, 0.5f);
                    }
                    if (GUILayout.Button(text, glo))
                    {
                        this.sttngs.showsummon = !this.sttngs.showsummon;
                    }
                    GUI.color = Color.white;
                }

                if (i == 8)
                {
                    string text = "open on battlestart";
                    GUI.color = Color.white;
                    if (!this.sttngs.openonStart)
                    {
                        text = "dont " + text;
                        GUI.color = new Color(1f, 1f, 1f, 0.5f);
                    }

                    if (GUILayout.Button(text, glo))
                    {
                        this.sttngs.openonStart = !this.sttngs.openonStart;
                    }
                    GUI.color = Color.white;
                }

                
                GUILayout.EndHorizontal();
            }
        }

        private void setRects()
        {

            float smallheight = (float)(((float)Screen.height) / 10f);
            float smallwidth = (float)(((float)Screen.width) / 10f);
            int smallstarty = Screen.height / 15;
            int smallstartx = 0;//0

            smallownrect = new Rect(smallstartx, smallstarty, 0.6f * smallwidth, 0.5f * smallheight);
            bigownrect = new Rect(smallstartx, smallstarty, 1.5f * smallwidth, 5 * smallheight);

            if (this.optionmenu)
            {
                bigownrect.width *= 2;
            }

            closeownrect = new Rect(smallstartx, bigownrect.yMax, smallwidth, 0.5f * smallheight);
            this.genrectown = new Rect(bigownrect.xMax, bigownrect.yMin, 0.5f * smallheight, 0.5f * smallheight);
            this.boardrectown = new Rect(bigownrect.xMax, bigownrect.yMin + 0.5f * smallheight, 0.5f * smallheight, 0.5f * smallheight);
            this.dynamicrectown = new Rect(bigownrect.xMax, bigownrect.yMin + smallheight, 0.5f * smallheight, 0.5f * smallheight);

            int oppsmallstarty = (int)(((float)Screen.height) / 15f);
            int oppsmallstartx = Screen.width;

            smallopprect = new Rect(oppsmallstartx - 0.6f * smallwidth, oppsmallstarty, 0.6f * smallwidth, 0.5f * smallheight);
            bigopprect = new Rect(oppsmallstartx - 1.5f * smallwidth, oppsmallstarty, 1.5f * smallwidth, 5 * smallheight);
            closeopprect = new Rect(oppsmallstartx - smallwidth, bigownrect.yMax, smallwidth, 0.5f * smallheight);
            genrectopp = new Rect(bigopprect.xMin - 0.5f * smallheight, bigopprect.yMin, 0.5f * smallheight, 0.5f * smallheight);
            boardrectopp = new Rect(bigopprect.xMin - 0.5f * smallheight, bigopprect.yMin + 0.5f * smallheight, 0.5f * smallheight, 0.5f * smallheight);
            dynamicrectopp = new Rect(bigopprect.xMin - 0.5f * smallheight, bigopprect.yMin + smallheight, 0.5f * smallheight, 0.5f * smallheight);
        }

        public void setskins(GUISkin cllps, GUISkin clpgs, GUISkin clpbls, GUISkin clplbs)
        {
            this.cardListPopupSkin = cllps;
            this.cardListPopupGradientSkin = clpgs;
            this.cardListPopupBigLabelSkin = clpbls;
            this.cardListPopupLeftButtonSkin = clplbs;

        }


    }

}
