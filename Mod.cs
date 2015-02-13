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
	public class Mod : BaseMod, ICommListener
	{
        Dictionary<long, TileColor> cardIdColorDatabase = new Dictionary<long, TileColor>();

        List<String> battlelog = new List<string>();
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
			return 6;
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
                    scrollsTypes["BattleMode"].Methods.GetMethod("OnGUI")[0],
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
                        this.battlelog.Add("--- turn " + ((EMTurnBegin)currentEffect).turn + " ---");
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

                }
                catch
                {
                    log = "<_<";
                }
                this.sendMessage(log);

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

        private string getCardSacrifieceMessage(EMCardSacrificed eMCardSacrificed)
        {
            string retval = "";
            string name = getName(eMCardSacrificed.color);

            retval = name + " sacrifices for " + ((!eMCardSacrificed.isForCards()) ? ( this.getResourceString(eMCardSacrificed.resource)) : "SCROLLS");


            return retval;
        }

       

        private string getCardPlayedMessage(EMCardPlayed eMCardPlayed)
        {
            string retval = "";
            TileColor col = TileColor.white;
            if(eMCardPlayed.getRawText().StartsWith("{\"CardPlayed\":{\"color\":\"black")) col = TileColor.black;

            string name = getName(col);

            if (this.cardIdColorDatabase.ContainsKey(eMCardPlayed.card.id))
            {
                this.cardIdColorDatabase[eMCardPlayed.card.id] = col;
            }
            else
            {
                this.cardIdColorDatabase.Add(eMCardPlayed.card.id, col);
            }

            retval = name + " played " + getUnitname(eMCardPlayed.card.getName(),col);

            if (eMCardPlayed.card.getCardType().kind == CardType.Kind.CREATURE || eMCardPlayed.card.getCardType().kind == CardType.Kind.STRUCTURE)
            {
                this.ignoreSelectTiles = true;
                this.ignoreOneSummone = true;
            }

            return retval;
        }

        private string getTurnBeginMessage(EMTurnBegin eMTurnBegin)
        {
            string retval = "";
            string name = getName(eMTurnBegin.color);

            retval = name + "'s turn starts";


            return retval;
        }

        private string getSummonMessage(EMSummonUnit esu)
        {
            if (this.ignoreOneSummone)
            {
                this.ignoreOneSummone = false;
                return "";
            }
            
            string retval = "";
            TileColor col = TileColor.white;
            if (esu.getRawText().StartsWith("{\"SummonUnit\":{\"target\":{\"color\":\"black")) col = TileColor.black;

            string name = getName(col);

            retval = name + " summoned a " + getUnitname(esu.card.getName(),col);
            //if (!sttngs.showNames) retval = getUnitname(esu.card.getName()) + " was summoned";

            return retval; 
        }

        private string getDamageIdolMessage(EMDamageIdol edi)
        {
            string retval = "";
            string name = getName(edi.idol.color);

            retval = name + "'s idol got " + this.getDmg(edi.amount) +" dmg";
            if (edi.idol.hp <= 0 && this.bm.getIdol(edi.idol.color, edi.idol.position).getHitPoints()>=1 ) retval += " and was killed";


            return retval;
        }

        private string getHealIdolMessage(EMHealIdol ehi)
        {
            if (ehi.amount == 0) return "";
            string retval = "";
            string name = getName(ehi.idol.color);
            retval = name + "'s idol is healed by " + getHeal(ehi.amount);
            return retval;
        }

        private string getDamageUnitMessage(EMDamageUnit edi)
        {
            //if (edi.amount <= 0) return "";
            string retval = "";
            string name = getName(edi.targetTile.color);

            string uname = bm.getUnit(edi.targetTile).getName();

            //edi.attackType.ToString()
            retval = ((sttngs.showNames) ? name + "'s " : "") + getUnitname(uname,edi.targetTile.color) + " got " + this.getDmg(edi.amount) + " " + edi.damageType + " dmg from " + getUnitname(edi.sourceCard.getName(), edi.sourceCard.id);

            return retval;
        }

        private string getEnchantUnitMessage(EMEnchantUnit edi)
        {
            string retval = "";
            string name = getName(edi.target.color);

            string uname = bm.getUnit(edi.target).getName();

            retval = ((sttngs.showNames) ? name + "'s " : "") + getUnitname(uname, edi.target.color) + " was enchanted";

            return retval;
        }

        private string getHealUnitMessage(EMHealUnit ehi)
        {
            if (ehi.amount == 0) return "";
            string retval = "";
            string name = getName(ehi.target.color);
            string uname = bm.getUnit(ehi.target).getName();
            retval = ((sttngs.showNames) ? name + "'s " : "") + getUnitname(uname,ehi.target.color) + " is healed by " + getHeal(ehi.amount);
            return retval;
        }

        private string getUnitActivateAbilityMessage(EMUnitActivateAbility ehi)
        {
            this.ignoreSelectTiles = true;
            if (ehi.name == "Move")
            {
                this.ignoreSelectTiles = true;
                return "";
            }
            string retval = "";
            string name = getName(ehi.unit.color);
            string uname = bm.getUnit(ehi.unit.color, ehi.unit.row, ehi.unit.column).getName();
            retval = ((sttngs.showNames) ? name + "'s " : "") + getUnitname(uname,ehi.unit.color) + "'s Ability " + ehi.name + " was activated";
            return retval;
        }

        private string getRuleUpdateMessage(EMRuleUpdate ehi) //lingering spells
        {
            string retval = "";
            string name = getName(ehi.color);

            string uname = ehi.card.getName();
            retval = getUnitname(uname,ehi.color) + "'s lingering effect lasts for " + ehi.roundsLeft + " rounds";
            return retval;
        }

        private string getRuleRemoveMessage(EMRuleRemoved ehi) //lingering spells
        {
            string retval = "";
            string name = getName(ehi.color);

            string uname = ehi.card.getName();
            retval = getUnitname(uname,ehi.color) + "'s lingering effect fades";
            return retval;
        }

        private string getUnitRemoveMessage(EMRemoveUnit ehi)
        {
            string retval = "";
            string name = getName(ehi.tile.color);
            string uname = bm.getUnit(ehi.tile).getName();

            string type = "removed";
            if (EMRemoveUnit.RemovalType.DESTROY == ehi.removalType) type = "destroyed";

            retval = ((sttngs.showNames) ? name + "'s " : "") + getUnitname(uname, ehi.tile.color) + " was " + type;
           
            return retval;
        }

        private string getUnitTeleportMessage(EMTeleportUnits ehi)
        {
            string retval = "";
            for(int i = 0 ; i < ehi.units.Length; i++)
            {
                TeleportInfo ti = ehi.units[i];
                string name = getName(ti.from.color);
                string uname = bm.getUnit(ti.from).getName();
                if (retval != "") retval += ", ";
                retval += ((sttngs.showNames) ? name + "'s " : "") + getUnitname(uname, ti.from.color);
            }
            retval += " are teleported";
            return retval;
        }

        private string getSelectedTilesMessage(EMSelectedTiles est)
        {
            if (this.ignoreSelectTiles)
            {
                this.ignoreSelectTiles = false;
                return "";
            }
            string uname = getUnitname(est.card.getName(), est.color);

            string target = "";
            if (est.tiles.Length >= 1 && this.bm.getUnit(est.tiles[0]) != null)
            {
                target = ((sttngs.showNames) ? this.getName(est.tiles[0].color) + "'s " : "") + getUnitname(this.bm.getUnit(est.tiles[0]).getName(), est.tiles[0].color);
            }
            if (est.tiles.Length >= 2 && this.bm.getUnit(est.tiles[1]) != null)
            {
                target += " and " + ((sttngs.showNames) ? this.getName(est.tiles[1].color) + "'s " : "") + getUnitname(this.bm.getUnit(est.tiles[1]).getName(), est.tiles[1].color);
            }

            if (target == "") return ""; // todo  really?

            return uname + " targets " + target; 
        }


        private string getDmg(int dmg)
        {
            return "<color=#FF0000>" + dmg + "</color>";
        }

        private string getHeal(int heal)
        {
            return "<color=#FF1919>" + heal + "</color>" ;
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
            if (!sttngs.showNames)
            {
                if (this.bm.isLeftColor(c))
                {
                    col ="00CC00";
                }
                else
                {
                    col = "FF4719";
                }
            }

            return "<color=#" +col+ ">" + name + "</color>";
        }


        private string getUnitname(string name, long cardid)
        {
            string col = "5959FF";
            if (!sttngs.showNames)
            {
                if (this.cardIdColorDatabase.ContainsKey(cardid))
                {
                    TileColor c = this.cardIdColorDatabase[cardid];
                    if (this.bm.isLeftColor(c))
                    {
                        col = "00CC00";
                    }
                    else
                    {
                        col = "FF4719";
                    }
                }
            }
            return "<color=#" + col + ">" + name + "</color>";
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

        public override void AfterInvoke(InvocationInfo info, ref object returnValue)
        {
            if (info.target is BattleMode && info.targetMethod.Equals("OnGUI"))
            {
                this.ds.draw(this.battlelog);
            }

            if (info.target is BattleMode && info.targetMethod.Equals("Start"))
            {
                this.ds = Drawstuff.Instance;
                this.ds.setSettings(this.sttngs);
                this.battlelog.Clear();
            }
        }
    
    }
}

