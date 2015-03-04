using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using Mono.Cecil;
using UnityEngine;
using ScrollsModLoader.Interfaces;
using System.Net;


namespace Battlelog.Mod
{


    public struct teleinfo
    {
        public string player;
        public string unit;
        public string position;

        public teleinfo(string p, string u, string pos)
        {
            this.player = p;
            this.unit = u;
            this.position = pos;
        }
    }

    public class BigLogEntry
    {
        public string type = "none";
        public string targetPlayer;
        public string targetname;
        public string targetDmg;
        public string dmgtype;
        public string targetHeal;
        public string sacrifice;
        public string playedCard;
        public string positiontext;
        public string abilityname;

        public bool killedidol = false;

        public bool ignoresummon = false;
        public bool ignoreselectedtiles = false;
        public string turnstartmessage;

        public string sourcePlayer;
        public string sourcename;

        public List<teleinfo> teleportinfo;


        public static string getLogfromBigLog(BigLogEntry ble, Settings settns)
        {
            string retval = "";
            string targetname = "";
            string sourcetargetname = "";
            if(ble.targetname != null)
            {
                targetname = ble.targetname;
                if (settns.showNames)
                {
                    targetname = ble.targetname.Replace("00CC01", "5959FF");
                    targetname = targetname.Replace("FF4718", "5959FF");
                }
                
            }

            if (ble.sourcename != null)
            {
                sourcetargetname = ble.sourcename;
                if (settns.showNames)
                {
                    sourcetargetname = ble.sourcename.Replace("00CC01", "5959FF");
                    sourcetargetname = sourcetargetname.Replace("FF4718", "5959FF");
                }

            }

            if (ble.type == "sac")
            {
                retval = ble.targetPlayer + " sacrifices for " + ble.sacrifice;
            }

            if (ble.type == "playcard")
            {
                retval = ble.targetPlayer + " played " + targetname;
            }

            if (ble.type == "turn")
            {
                retval = ble.turnstartmessage;
            }


            if (ble.type == "summon")
            {
                retval = ble.targetPlayer + " summoned a " + targetname + ((settns.showCoordinates) ? ble.positiontext : "");

                if (ble.ignoresummon && !settns.showsummon) retval = "";
            }

            if (ble.type == "idoldmg")
            {
                retval = ble.targetPlayer + "'s idol" + ((settns.showCoordinates) ? ble.positiontext : "") + " got " + ble.targetDmg + " dmg";

                if (ble.killedidol)
                {
                    retval += " and was killed";
                }
            }

            if (ble.type == "idolheal")
            {
                retval = ble.targetPlayer + "'s idol" + ((settns.showCoordinates) ? ble.positiontext : "") + " is healed by " + ble.targetHeal;

                if (ble.targetHeal == "0") retval = "";
            }


            if (ble.type == "dmgunit")
            {
                retval = ((settns.showNames) ? ble.targetPlayer + "'s " : "") + targetname + ((settns.showCoordinates) ? ble.positiontext : "") + " got " + ble.targetDmg + " " + ble.dmgtype + " dmg from " + ((settns.showNames) ? ble.sourcePlayer + "'s " : "") + sourcetargetname;


            }


            if (ble.type == "healunit")
            {
                retval = ((settns.showNames) ? ble.targetPlayer + "'s " : "") + targetname + ((settns.showCoordinates) ? ble.positiontext : "") + " is healed by " + ble.targetHeal + " from " + ((settns.showNames) ? ble.sourcePlayer + "'s " : "") + sourcetargetname;

                if (ble.targetHeal == "0") retval = "";
            }

            if (ble.type == "enchantunit")
            {
                retval = ((settns.showNames) ? ble.targetPlayer + "'s " : "") + targetname + ((settns.showCoordinates) ? ble.positiontext : "") + " was enchanted";
            }

            if (ble.type == "ability")
            {
                if (ble.abilityname != "Move" )//|| (settns.showMove))
                {
                    retval = "the Ability " + ble.abilityname + " of " + ((settns.showNames) ? ble.targetPlayer + "'s " : "") + targetname + ((settns.showCoordinates) ? ble.positiontext : "") + " was activated";
                }
            }


            if (ble.type == "ruleupdate")
            {
                retval = ((settns.showNames) ? ble.targetPlayer + "'s " : "") + targetname + "'s lingering effect lasts for " + ble.targetDmg + " rounds";

            }


            if (ble.type == "rulefade")
            {
                retval = ((settns.showNames) ? ble.targetPlayer + "'s " : "") + targetname + "'s lingering effect fades";

            }


            if (ble.type == "removeunit")
            {
                retval = ((settns.showNames) ? ble.targetPlayer + "'s " : "") + targetname + ((settns.showCoordinates) ? ble.positiontext : "") + " was " + ble.targetDmg;

            }


            if (ble.type == "teleport")
            {
                int unitss = 0;
                for (int i = 0; i < ble.teleportinfo.Count; i++)
                {
                    teleinfo ti = ble.teleportinfo[i];
                    string name = ti.player;
                    string unitname = ti.unit;
                    string pos = ti.position;

                    if (settns.showNames)
                    {
                        unitname = unitname.Replace("00CC01", "5959FF");
                        unitname = unitname.Replace("FF4718", "5959FF");
                    }

                    if (retval != "") retval += ", ";
                    retval += ((settns.showNames) ? name + "'s " : "") + unitname + ((settns.showCoordinates) ? pos : "");
                    unitss++;

                }
                retval += ((unitss >= 2) ? " are " : " is ") + "teleported";
            }

            if (ble.type == "selectTiles")
            {
                string target = "";
                for (int i = 0; i < ble.teleportinfo.Count; i++)
                {
                    teleinfo ti = ble.teleportinfo[i];
                    string name = ti.player;
                    string unitname = ti.unit;
                    string pos = ti.position;

                    if (settns.showNames)
                    {
                        unitname = unitname.Replace("00CC01", "5959FF");
                        unitname = unitname.Replace("FF4718", "5959FF");
                    }

                    if (target != "") target += " and ";
                    target += ((settns.showNames) ? name + "'s " : "") + unitname + ((settns.showCoordinates) ? pos : "");

                }

                retval = ((settns.showNames) ? ble.targetPlayer + "'s " : "") + targetname + " targets " + target;

                if (ble.ignoreselectedtiles) retval = "";
            }

            if (ble.type == "moveunit" && settns.showMove)
            {
                retval = ((settns.showNames) ? ble.targetPlayer + "'s " : "") + targetname + ((settns.showCoordinates) ? ble.positiontext : "") + " was moved" + ((settns.showCoordinates) ? ble.targetDmg : "");

            }

            



            return retval;
        }

        public static List<string> getLogsfromBigLogList(List<BigLogEntry> bleList, Settings settns)
        {
            List<string> retval = new List<string>();

            foreach (BigLogEntry ble in bleList)
            {
                string val = BigLogEntry.getLogfromBigLog(ble, settns);

                if (val != "") retval.Add(val);
                
            }

            return retval;
        }




    }

	public class Mod : BaseMod, ICommListener
	{
        Dictionary<long, TileColor> cardIdColorDatabase = new Dictionary<long, TileColor>();

        public List<BigLogEntry> bigBattleLog = new List<BigLogEntry>();
        public List<String> battlelog = new List<string>();
        private BattleMode bm = null;
        private BattleModeUI bmui = null;
        private Drawstuff ds;
        private bool loadedguiSkin = false;
        private GUISkin guiSkin;

        private bool ignoreSelectTiles = false;
        private bool ignoreOneSummone = false;

        private FieldInfo currentEffectField=typeof(BattleMode).GetField("currentEffect", BindingFlags.NonPublic | BindingFlags.Instance);

        MethodInfo hm = typeof(BattleMode).GetMethod("handleGameChatMessage", BindingFlags.NonPublic | BindingFlags.Instance);
        private MethodInfo getplayer;

        public Settings sttngs;

        public Mod()
		{
            this.loadGuiSkin();
			
            MethodInfo[] mis = typeof(BattleMode).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
            Console.WriteLine("------------------");
            foreach (MethodInfo mi in mis)
            {
                /*Console.WriteLine(mi.Name);
                foreach (ParameterInfo pi in mi.GetParameters())
                {
                    Console.WriteLine(pi.ParameterType.Name);
                } */

                if (mi.Name == "getPlayer")
                {
                    if ((mi.GetParameters()).Length == 1 && (mi.GetParameters())[0].ParameterType.Name == "TileColor")
                    {
                        this.getplayer = mi;
                        //Console.WriteLine("getplayer found with " + mi.GetParameters()[0].Name + " as param");
                    }
                }
            }

			try {
				App.Communicator.addListener(this);
			} catch {}

            string recordFolder = this.OwnFolder(); // +System.IO.Path.DirectorySeparatorChar;
            sttngs = new Settings(recordFolder);

            Console.WriteLine("loaded Recorder");
		}

        private void loadGuiSkin()
        {
            this.guiSkin = (GUISkin)ResourceManager.Load("_GUISkins/Lobby");
        }

		public static string GetName()
		{
			return "BattleLog";
		}

		public static int GetVersion()
		{
			return 10;
		}

		public void handleMessage(Message msg)
		{

		}

		public void onConnect(OnConnectData ocd)
		{
			return;
		}

		
		public static MethodDefinition[] GetHooks(TypeDefinitionCollection scrollsTypes, int version)
		{
			try {
				MethodDefinition[] defs = new MethodDefinition[] {
                    //scrollsTypes["BattleMode"].Methods.GetMethod("effectDone")[0],
                    scrollsTypes["BattleMode"].Methods.GetMethod("Start")[0],
                    //scrollsTypes["BattleMode"].Methods.GetMethod("OnGUI")[0], // not needed anymore (we do it cleverer!)
                    scrollsTypes["BattleMode"].Methods.GetMethod("forceRunEffect", new Type[]{typeof(EffectMessage)}),
                    
				};

				return defs;
			} catch {
				return new MethodDefinition[] {};
			}
		}

		public override bool WantsToReplace (InvocationInfo info)
		{
            return false;
		}

		public override void ReplaceMethod (InvocationInfo info, out object returnValue) 
        {
            returnValue = false;
		}

		public override void BeforeInvoke(InvocationInfo info)
		{

            if (info.target is BattleMode && info.targetMethod.Equals("forceRunEffect"))
            {
                EffectMessage currentEffect = (EffectMessage) info.arguments[0];

                string type = currentEffect.type;
                string log = "";
                this.bm = info.target as BattleMode;
                Console.WriteLine("####### current type  = " + type);
                int old = this.bigBattleLog.Count;
                try
                {
                    if (type == "CardSacrificed")
                    {
                        log = this.getCardSacrifieceMessage(currentEffect as EMCardSacrificed);
                    }

                    if (type == "CardPlayed")
                    {
                        log = this.getCardPlayedMessage(currentEffect as EMCardPlayed);
                    }

                    if (type == "TurnBegin")
                    {
                        log = this.getTurnBeginMessage(currentEffect as EMTurnBegin);
                    }

                    if (type == "SummonUnit")
                    {
                        log = this.getSummonMessage(currentEffect as EMSummonUnit);
                    }

                    if (type == "DamageIdol")
                    {
                        log = this.getDamageIdolMessage(currentEffect as EMDamageIdol);
                    }

                    if (type == "HealIdol")
                    {
                        log = this.getHealIdolMessage(currentEffect as EMHealIdol);
                    }


                    if (type == "DamageUnit")
                    {
                        log = this.getDamageUnitMessage(currentEffect as EMDamageUnit);
                    }

                    if (type == "HealUnit")
                    {
                        log = this.getHealUnitMessage(currentEffect as EMHealUnit);
                    }

                    if (type == "UnitActivateAbility")
                    {
                        log = this.getUnitActivateAbilityMessage(currentEffect as EMUnitActivateAbility);
                    }


                    if (type == "RuleUpdate")
                    {
                        log = this.getRuleUpdateMessage(currentEffect as EMRuleUpdate);
                    }

                    if (type == "RuleRemoved")
                    {
                        log = this.getRuleRemoveMessage(currentEffect as EMRuleRemoved);
                    }

                    if (type == "RemoveUnit")
                    {
                        log = this.getUnitRemoveMessage(currentEffect as EMRemoveUnit);
                    }

                    if (type == "TeleportUnits")
                    {
                        log = this.getUnitTeleportMessage(currentEffect as EMTeleportUnits);
                    }

                    if (type == "SelectedTiles")
                    {
                        log = this.getSelectedTilesMessage(currentEffect as EMSelectedTiles);
                    }

                    if (type == "MoveUnit")
                    {
                        log = this.getUnitMoveMessage(currentEffect as EMMoveUnit);
                    }

                }
                catch
                {
                    log = "<_<";
                }
                Console.WriteLine();
                int newone = this.bigBattleLog.Count;
                string newlog = "";
                if (newone != old)
                {
                    BigLogEntry ble = this.bigBattleLog[this.bigBattleLog.Count - 1];

                   newlog = BigLogEntry.getLogfromBigLog(ble, this.sttngs);
                }
                

                /*if (log != newlog)
                {
                    this.sendMessage("error");
                    this.sendMessage(log);
                }*/

                this.sendMessage(newlog);

            }

            if (info.target is BattleMode && info.targetMethod.Equals("effectDone"))
            {
                    EffectMessage currentEffect = ((EffectMessage)currentEffectField.GetValue(info.target));

            }
		}

        private void sendMessage(string s)
        {
            if (s == "") return;

            GameChatMessageMessage gcmm = new GameChatMessageMessage();
            gcmm.from = "BL";
            gcmm.text = s;
            //hm.Invoke(this.bm, new object[] { gcmm });
            this.battlelog.Add(s);
            this.ds.scrollDown();
            Console.WriteLine("#######"+s);
        }

        public override void AfterInvoke(InvocationInfo info, ref object returnValue)
        {
            /*if (info.target is BattleMode && info.targetMethod.Equals("OnGUI"))
            {
                try
                {
                    this.ds.draw(this.battlelog);
                }
                catch
                {
                    Console.WriteLine("typical unity error :D");
                    if (this.ds.scrollbarWasDown)
                    {
                        this.ds.chatScroll.y = int.MaxValue;
                    }
                }
            }*/

            if (info.target is BattleMode && info.targetMethod.Equals("Start"))
            {
                this.ds = new GameObject("drawing stuff").AddComponent<Drawstuff>();
                this.ds.moddingmod = this;
                //this.ds = Drawstuff.Instance;
                this.ds.setSettings(this.sttngs);
                this.battlelog.Clear();
                this.bigBattleLog.Clear();
            }
        }



        public string getCardSacrifieceMessage(EMCardSacrificed eMCardSacrificed)
        {
            string retval = "";
            string name = getName(eMCardSacrificed.color);
            string sacrifice = ((!eMCardSacrificed.isForCards()) ? (this.getResourceString(eMCardSacrificed.resource)) : "SCROLLS");
            retval = name + " sacrifices for " + sacrifice;

            BigLogEntry ble = new BigLogEntry();
            ble.sacrifice = sacrifice;
            ble.targetPlayer = name;
            ble.type = "sac";
            this.bigBattleLog.Add(ble);

            return retval;
        }

        public string getCardPlayedMessage(EMCardPlayed eMCardPlayed)
        {
            string retval = "";
            TileColor col = TileColor.white;
            if (eMCardPlayed.getRawText().StartsWith("{\"CardPlayed\":{\"color\":\"black")) col = TileColor.black;

            string name = getName(col);

            if (this.cardIdColorDatabase.ContainsKey(eMCardPlayed.card.id))
            {
                this.cardIdColorDatabase[eMCardPlayed.card.id] = col;
            }
            else
            {
                this.cardIdColorDatabase.Add(eMCardPlayed.card.id, col);
            }

            string playedcard = getUnitname(eMCardPlayed.card.getName(), col);

            retval = name + " played " + playedcard;

            if (eMCardPlayed.card.getCardType().kind == CardType.Kind.CREATURE || eMCardPlayed.card.getCardType().kind == CardType.Kind.STRUCTURE)
            {
                this.ignoreSelectTiles = true;
                this.ignoreOneSummone = true;
            }


            BigLogEntry ble = new BigLogEntry();
            ble.type = "playcard";
            ble.targetPlayer = name;
            ble.targetname = playedcard;
            this.bigBattleLog.Add(ble);


            return retval;
        }

        public string getTurnBeginMessage(EMTurnBegin eMTurnBegin)
        {
            string retval = "";
            string name = getName(eMTurnBegin.color);

            retval = "--- turn " + eMTurnBegin.turn + " ---" + "\r\n" +name + "'s turn starts";

            BigLogEntry ble = new BigLogEntry();
            ble.type = "turn";
            ble.turnstartmessage = retval;
            this.bigBattleLog.Add(ble);

            return retval;
        }

        public string getSummonMessage(EMSummonUnit esu)
        {
            

            string retval = "";
            TileColor col = TileColor.white;
            if (esu.getRawText().StartsWith("{\"SummonUnit\":{\"target\":{\"color\":\"black")) col = TileColor.black;

            string name = getName(col);
            string position = " on " + this.getPosition(esu.target);
            string unitname = getUnitname(esu.card.getName(), col);

            retval = name + " summoned a " + unitname + ((this.sttngs.showCoordinates) ? position : "");
            //if (!sttngs.showNames) retval = getUnitname(esu.card.getName()) + " was summoned";


            BigLogEntry ble = new BigLogEntry();
            ble.type = "summon";
            ble.targetPlayer = name;
            ble.targetname = unitname;
            ble.positiontext = position;
            ble.ignoresummon = this.ignoreOneSummone;

            this.bigBattleLog.Add(ble);

            if (this.ignoreOneSummone)
            {
                this.ignoreOneSummone = false;
                return "";
            }

            return retval;
        }

        public string getDamageIdolMessage(EMDamageIdol edi)
        {
            string retval = "";
            string name = getName(edi.idol.color);
            string pos = " on " + (edi.idol.position + 1);
            string dmg = this.getDmg(edi.amount);

            retval = name + "'s idol" + ((this.sttngs.showCoordinates) ? pos : "") + " got " + dmg + " dmg";
            


            BigLogEntry ble = new BigLogEntry();
            ble.type = "idoldmg";
            ble.targetPlayer = name;
            ble.positiontext = pos;
            ble.targetDmg = dmg;

            if (edi.idol.hp <= 0 && this.bm.getIdol(edi.idol.color, edi.idol.position).getHitPoints() >= 1)
            {
                retval += " and was killed";
                ble.killedidol = true;

            }

            this.bigBattleLog.Add(ble);

            return retval;
        }

        public string getHealIdolMessage(EMHealIdol ehi)
        {
            
            string retval = "";
            string name = getName(ehi.idol.color);
            string pos = " on " + (ehi.idol.position + 1);
            string heal = getHeal(ehi.amount);
            retval = name + "'s idol" + ((this.sttngs.showCoordinates) ? pos : "") + " is healed by " + heal;


            BigLogEntry ble = new BigLogEntry();
            ble.type = "idolheal";
            ble.targetPlayer = name;
            ble.positiontext = pos;
            ble.targetHeal = heal;
            this.bigBattleLog.Add(ble);

            if (ehi.amount == 0) return "";
            return retval;
        }

        public string getDamageUnitMessage(EMDamageUnit edi)
        {
            //if (edi.amount <= 0) return "";
            string retval = "";
            string name = getName(edi.targetTile.color);

            string uname = bm.getUnit(edi.targetTile).getName();

            string unitname = getUnitname(uname, edi.targetTile.color);
            string pos = " on " + getPosition(edi.targetTile);
            string dmgtype = edi.damageType + "" ;
            string dmg = this.getDmg(edi.amount);

            string dmgsourceplayer = getName(edi.sourceCard.id);
            string dmgsourceunit = getUnitname(edi.sourceCard.getName(), edi.sourceCard.id);

            //edi.attackType.ToString()
            retval = ((sttngs.showNames) ? name + "'s " : "") + unitname + ((this.sttngs.showCoordinates) ? pos : "") + " got " + dmg + " " + dmgtype + " dmg from " + ((sttngs.showNames) ? dmgsourceplayer + "'s " : "") + dmgsourceunit;

            BigLogEntry ble = new BigLogEntry();
            ble.type = "dmgunit";
            ble.targetPlayer = name;
            ble.targetname = unitname;
            ble.positiontext = pos;
            ble.dmgtype = dmgtype;
            ble.targetDmg = dmg;
            ble.sourcename = dmgsourceunit;
            ble.sourcePlayer = dmgsourceplayer;
            this.bigBattleLog.Add(ble);

            return retval;
        }

        public string getHealUnitMessage(EMHealUnit ehi)
        {
            
            string retval = "";
            string name = getName(ehi.target.color);
            string uname = bm.getUnit(ehi.target).getName();
            string unitname = getUnitname(uname, ehi.target.color);
            string pos = " on " + getPosition(ehi.target);
            string heal = getHeal(ehi.amount);
            long sourcid = 0;
            int type = 1;
            if (ehi.getRawText().Contains("\"sourceCard\":{\"id\":"))
            {
                string si = ehi.getRawText().Split(new string[] { "\"sourceCard\":{\"id\":" }, StringSplitOptions.RemoveEmptyEntries)[1];
                string typetemp = ehi.getRawText().Split(new string[] { ",\"typeId\":" }, StringSplitOptions.RemoveEmptyEntries)[1];
                si = si.Split(',')[0];
                typetemp = typetemp.Split(',')[0];
                sourcid = Convert.ToInt64(si);
                type = Convert.ToInt32(typetemp);

               
            }

            string dmgsourceplayer = getName(sourcid);
            string dmgsourceunit = getUnitname(getUnitNameFromType(type), sourcid);

            retval = ((sttngs.showNames) ? name + "'s " : "") + unitname + ((this.sttngs.showCoordinates) ? pos : "") + " is healed by " + heal + " from " + ((sttngs.showNames) ? dmgsourceplayer + "'s " : "") + dmgsourceunit;

            BigLogEntry ble = new BigLogEntry();
            ble.type = "healunit";
            ble.targetPlayer = name;
            ble.targetname = unitname;
            ble.positiontext = pos;
            ble.targetHeal = heal;
            ble.sourcename = dmgsourceunit;
            ble.sourcePlayer = dmgsourceplayer;
            this.bigBattleLog.Add(ble);

            if (ehi.amount == 0) return "";
            return retval;
        }


        public string getEnchantUnitMessage(EMEnchantUnit edi)
        {
            string retval = "";
            string name = getName(edi.target.color);

            string uname = bm.getUnit(edi.target).getName();
            string unitname = getUnitname(uname, edi.target.color);
            string pos = " on " + getPosition(edi.target);

            retval = ((sttngs.showNames) ? name + "'s " : "") + unitname + ((this.sttngs.showCoordinates) ? pos : "") + " was enchanted";


            BigLogEntry ble = new BigLogEntry();
            ble.type = "enchantunit";
            ble.targetPlayer = name;
            ble.targetname = unitname;
            ble.positiontext = pos;
            this.bigBattleLog.Add(ble);

            return retval;
        }

        public string getUnitActivateAbilityMessage(EMUnitActivateAbility ehi)
        {
            this.ignoreSelectTiles = true;
            
            string retval = "";
            string name = getName(ehi.unit.color);
            string uname = bm.getUnit(ehi.unit.color, ehi.unit.row, ehi.unit.column).getName();
            string unitname = getUnitname(uname, ehi.unit.color);
            string pos = " on " + getPosition(ehi.unit);

            string ability = ehi.name;

            retval = "the Ability " + ability + " of " + ((sttngs.showNames) ? name + "'s " : "") + unitname + ((this.sttngs.showCoordinates) ? pos : "") + " was activated";
            
            BigLogEntry ble = new BigLogEntry();
            ble.type = "ability";
            ble.targetPlayer = name;
            ble.targetname = unitname;
            ble.positiontext = pos;
            ble.abilityname = ability;
            this.bigBattleLog.Add(ble);

            if (ehi.name == "Move")
            {
                this.ignoreSelectTiles = true;
                return "";
            }
            return retval;
        }

        private string getRuleUpdateMessage(EMRuleUpdate ehi) //lingering spells
        {
            string retval = "";
            string name = getName(ehi.color);

            string uname = ehi.card.getName();
            string unitname = getUnitname(uname, ehi.color);
            retval = ((sttngs.showNames) ? name + "'s " : "") + unitname + "'s lingering effect lasts for " + ehi.roundsLeft + " rounds";

            BigLogEntry ble = new BigLogEntry();
            ble.type = "ruleupdate";
            ble.targetPlayer = name;
            ble.targetname = unitname;
            ble.targetDmg = ehi.roundsLeft+"";
            this.bigBattleLog.Add(ble);

            return retval;
        }

        private string getRuleRemoveMessage(EMRuleRemoved ehi) //lingering spells
        {
            string retval = "";
            string name = getName(ehi.color);

            string uname = ehi.card.getName();
            string unitname = getUnitname(uname, ehi.color);
            retval = ((sttngs.showNames) ? name + "'s " : "") + unitname + "'s lingering effect fades";

            BigLogEntry ble = new BigLogEntry();
            ble.type = "rulefade";
            ble.targetPlayer = name;
            ble.targetname = unitname;
            this.bigBattleLog.Add(ble);

            return retval;
        }

        private string getUnitRemoveMessage(EMRemoveUnit ehi)
        {
            string retval = "";
            string name = getName(ehi.tile.color);
            string uname = bm.getUnit(ehi.tile).getName();
            string unitname = getUnitname(uname, ehi.tile.color);
            string pos = " on " + getPosition(ehi.tile);

            string type = "removed";
            if (EMRemoveUnit.RemovalType.DESTROY == ehi.removalType) type = "destroyed";

            retval = ((sttngs.showNames) ? name + "'s " : "") + unitname + ((this.sttngs.showCoordinates) ? pos : "") + " was " + type;


            BigLogEntry ble = new BigLogEntry();
            ble.type = "removeunit";
            ble.targetPlayer = name;
            ble.targetname = unitname;
            ble.positiontext = pos;
            ble.targetDmg = type;
            this.bigBattleLog.Add(ble);


            return retval;
        }

        private string getUnitTeleportMessage(EMTeleportUnits ehi)
        {
            string retval = "";
            BigLogEntry ble = new BigLogEntry();
            ble.type = "teleport";
            ble.teleportinfo = new List<teleinfo>();

            for (int i = 0; i < ehi.units.Length; i++)
            {
                TeleportInfo ti = ehi.units[i];
                string name = getName(ti.from.color);
                string uname = bm.getUnit(ti.from).getName();
                string unitname = getUnitname(uname, ti.from.color);
                string pos = " on " + getPosition(ti.from);

                if (retval != "") retval += ", ";
                retval += ((sttngs.showNames) ? name + "'s " : "") + unitname + ((this.sttngs.showCoordinates) ? pos : "");
            
                ble.teleportinfo.Add(new teleinfo(name, unitname, pos));

            }
            retval += " are teleported";

            this.bigBattleLog.Add(ble);

            return retval;
        }

        private string getSelectedTilesMessage(EMSelectedTiles est)
        {
            
            string name = getName(est.color);
            string uname = getUnitname(est.card.getName(), est.color);

            BigLogEntry ble = new BigLogEntry();
            ble.type = "selectTiles";
            ble.targetPlayer = name;
            ble.targetname = uname;
            

            string target = "";
            if (est.tiles.Length >= 1 && this.bm.getUnit(est.tiles[0]) != null)
            {
                string tname = this.getName(est.tiles[0].color);
                string tu = getUnitname(this.bm.getUnit(est.tiles[0]).getName(), est.tiles[0].color);
                string tp = " on " + getPosition(est.tiles[0]);
                target = ((sttngs.showNames) ? tname + "'s " : "") + tu + ((this.sttngs.showCoordinates) ? tp : "");

                ble.teleportinfo = new List<teleinfo>();
                ble.teleportinfo.Add(new teleinfo(tname, tu, tp));
            }
            if (est.tiles.Length >= 2 && this.bm.getUnit(est.tiles[1]) != null)
            {
                string tname = this.getName(est.tiles[1].color);
                string tu = getUnitname(this.bm.getUnit(est.tiles[1]).getName(), est.tiles[1].color);
                string tp = " on " + getPosition(est.tiles[1]);

                target += " and " + ((sttngs.showNames) ? tname + "'s " : "") + tu + ((this.sttngs.showCoordinates) ? tp : "");

                ble.teleportinfo.Add(new teleinfo(tname, tu, tp));
            }

            

            if (target == "") return ""; // todo  really?

            ble.ignoreselectedtiles = true;

            this.bigBattleLog.Add(ble);

            if (this.ignoreSelectTiles)
            {
                this.ignoreSelectTiles = false;
                return "";
            }

            return ((sttngs.showNames) ? name + "'s " : "") + uname + " targets " + target;
        }

        private string getUnitMoveMessage(EMMoveUnit emu)
        {
            string retval = "";
            string name = getName(emu.from.color);
            string uname = bm.getUnit(emu.from).getName();
            string unitname = getUnitname(uname, emu.from.color);
            string pos = " on " + getPosition(emu.from);
            string pos2 = " to " + getPosition(emu.to);


            retval = ((sttngs.showNames) ? name + "'s " : "") + unitname + ((this.sttngs.showCoordinates) ? pos : "") + " was moved " + ((this.sttngs.showCoordinates) ? pos2 : "");


            BigLogEntry ble = new BigLogEntry();
            ble.type = "moveunit";
            ble.targetPlayer = name;
            ble.targetname = unitname;
            ble.positiontext = pos;
            ble.targetDmg = pos2;
            this.bigBattleLog.Add(ble);


            return retval;
        }





        private string getDmg(int dmg)
        {
            return "<color=#FF0000>" + dmg + "</color>";
        }

        private string getHeal(int heal)
        {
            return "<color=#FF1919>" + heal + "</color>";
        }

        private string getName(TileColor c)
        {
            string name = "";
            name = ((BMPlayer)getplayer.Invoke(this.bm, new object[] { c })).name;
            if (this.bm.isLeftColor(c))
            {
                name = "<color=#00CC00>" + name + "</color>";
            }
            else
            {
                name = "<color=#FF4719>" + name + "</color>";
            }

            return name;
        }

        private string getUnitname(string name, TileColor c)
        {
            string col = "5959FF";
            if (this.bm.isLeftColor(c))
            {
                col = "00CC01";
            }
            else
            {
                col = "FF4718";
            }

            return "<color=#" + col + ">" + name + "</color>";
        }


        private string getUnitname(string name, long cardid)
        {
            string col = "5959FF";
            if (this.cardIdColorDatabase.ContainsKey(cardid))
            {
                TileColor c = this.cardIdColorDatabase[cardid];
                if (this.bm.isLeftColor(c))
                {
                    col = "00CC01";
                }
                else
                {
                    col = "FF4718";
                }
            }
            return "<color=#" + col + ">" + name + "</color>";
        }

        private string getName(long cardid)
        {
            string name = "";
            if (this.cardIdColorDatabase.ContainsKey(cardid))
            {
                TileColor c = this.cardIdColorDatabase[cardid];

                name = ((BMPlayer)getplayer.Invoke(this.bm, new object[] { c })).name;
                if (this.bm.isLeftColor(c))
                {
                    name = "<color=#00CC00>" + name + "</color>";
                }
                else
                {
                    name = "<color=#FF4719>" + name + "</color>";
                }
            }
            else
            {
                name = "unknown";
            }
            return name;
        }

        private string getUnitNameFromType(int type)
        {
            string retval = "";
            retval = CardTypeManager.getInstance().get(type).name;
            return retval;
        }

        private string getPosition(TilePosition tp)
        {
            string pos = (tp.row + 1) + "," + (tp.column + 1);

            return pos;
        }

        private string getResourceString(ResourceType res)
        {
            string retval = "";

            if (res == ResourceType.DECAY) retval = "<color=#CC00FF>" + "DECAY" + "</color>";
            if (res == ResourceType.ORDER) retval = "<color=#3399FF>" + "ORDER" + "</color>";
            if (res == ResourceType.ENERGY) retval = "<color=#FFFF33>" + "ENERGY" + "</color>";
            if (res == ResourceType.GROWTH) retval = "<color=#66E066>" + "GROWTH" + "</color>";
            if (res == ResourceType.SPECIAL) retval = "<color=#FFFFFF>" + "WILD" + "</color>";

            return retval;
        }




    }
}

