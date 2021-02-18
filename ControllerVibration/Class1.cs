using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Modding;   
using XInputDotNetPure;
using UnityEngine;
using ModCommon.Util;
using ModCommon;
//leftMotor low frequency, rightMotor High Frequencies, 0 - 65,535
namespace ControllerVibration
{
    public class ControllerVibration : Mod
    {
        private float VibrationTime = 0f;
        private bool IsTimedRumbling = true;
        private bool WasContinuousRumbling = false;
 
        //List of collider.toString()'s. One contains a list of colliders that should not trigger vibration, while the other
        //can be used to trigger a specific type of vibration, if desired.
        private string[] NoVibrateColliders =
        {
            "white_grass", "Shield ", "white", "Break", "Roof ", 
            "Dream Dialogue", "town_grass", "left cliff collider", "Geo Small", "plat_float_", "GameObject", 
            "Blue Glob", "Tute Door", "Tute Pole",  "Chunk", "Mines Short Shelf", "terrain collider", "Stag Lift", "Lift",
            "Opened", "elev_plat", "joni_plinth", "Head Box", "Acid Box", "tollbooth_bottom"
        };


        private string[] LightVibrateColliders =
        {
            "Crossroads Sign Post", "Crossroads Pole Curvy", "Breakable Pole", "brk_Crystal", "Direction Pole",
            "Stag_Pole"
            
            
        };
        private string[] RegularVibrateColliders =
        {
            "Zombie Runner", "Crossroads Statue Horn",
            "Crossroads Statue Stone", "Climber", "Crawler", "Zombie Runner", "Geo Rock", "brk_barrel",
            "Buzzer", "brk_barrel", "Fly", "Gate Switch", "Tinger", "Soul Totem", "Bell (", "Hatcher", "Spitter",
            "brk_cart", "Health Scuttler", "Health Cocoon", "Ghost Warrior Slug", "Shot Slug Spear", "Quake Floor",
            "health plant", "Fungus Flyer", "Fungoon Baby", "Fungus Mushroom", "Bounce Shroom", "Phys Box", "Fung Crawler", 
            "Mantis", ""

        };



        public ControllerVibration() : base("Controller Vibration") 
        {
        }
        
            public override void Initialize() 
        {
            Log("Initializing ControllerVibration");
            
            //These functions correspond to events that should have Vibration
            ModHooks.Instance.HeroUpdateHook += OnHeroUpdate;            
            ModHooks.Instance.SlashHitHook += OnSlashHit;
            ModCommon.ModCommon.OnSpellHook += OnSpellHook;
            On.HeroController.DoHardLanding += DoHardLanding;
            ModHooks.Instance.TakeHealthHook += TakeDamage;
            ModHooks.Instance.BeforeAddHealthHook += BeforeAddHealthHook;
            On.HeroController.HeroDash += HeroDash;
            On.EnemyDreamnailReaction.RecieveDreamImpact += RecieveDreamImpact;
            On.HeroController.Pause += Pause;
            On.HeroController.UnPause += UnPause;
            //These lines of code shorten all of the elements of the collider lists to just 5 characters long. While this
            //is not a very good way of doing things, without actually being able to tell if a given collider should vibrate,
            //since there seems to be no base function for that, I have to manually identify each collider, classifying it as either no, light, or regular vibrate
            for (int i = 0; i < NoVibrateColliders.Length; i++)
            {
                if (NoVibrateColliders[i].Length > 5)
                {
                    NoVibrateColliders[i] = NoVibrateColliders[i].Substring(0, 5);
                    Log(i + NoVibrateColliders[i]);
                }
            }
            for(int i = 0; i < LightVibrateColliders.Length ; i++){
                if (LightVibrateColliders[i].Length > 5)
                {
                    LightVibrateColliders[i] = LightVibrateColliders[i].Substring(0, 5);
                    Log(i + LightVibrateColliders[i]);
                }
            }
            for(int i = 0; i < RegularVibrateColliders.Length; i++){
                if (RegularVibrateColliders[i].Length > 5)
                {
                    RegularVibrateColliders[i] = RegularVibrateColliders[i].Substring(0, 5);
                    Log(i + RegularVibrateColliders[i]);
                }

            }
        }



        private void RecieveDreamImpact(On.EnemyDreamnailReaction.orig_RecieveDreamImpact orig, EnemyDreamnailReaction self)
        {
            orig(self);
            Rumble(7);
        }

        private void HeroDash(On.HeroController.orig_HeroDash orig, HeroController self)
        {
            orig(self);
            Rumble(5); 
        }

        private void OnHeroUpdate()
        {
            //Every frame, a check is made to see if a certain type of vibration is supposed to be occuring.
            //Should Wall slide vibration be different from super dashing vibration?
            if (HeroController.instance.cState.superDashing || HeroController.instance.cState.wallSliding)
            {
                if (!WasContinuousRumbling)
                {
                    ResetTimedVibration();
                    GamePad.SetVibration(PlayerIndex.One, .5f, .8f);
                    WasContinuousRumbling = true;
                }
            } else if (this.IsTimedRumbling)
            {
                if (this.VibrationTime <= 0)
                {
                    ResetTimedVibration();
                } else
                {
                    VibrationTime -= Time.deltaTime;
                }

            } else if (WasContinuousRumbling) //WasContinuousRumbling is always set to true when the triggering event is occuring, so the first frame after getEvent is false, rumbling stops. 
            {
                GamePad.SetVibration(PlayerIndex.One, 0f, 0f);
                WasContinuousRumbling= false;
            }
        }
        
        //This function is called when slashing hits something
        //See's what Slash has collided with, and what type of vibration should occour
        private void OnSlashHit(Collider2D otherCollider, GameObject gameObject)
        {
            var ShortOtherCollider = otherCollider.ToString().Substring(0, 5);
            if ((IsInArray(NoVibrateColliders, ShortOtherCollider)) || otherCollider.ToString().Contains("Dialo") ) //If otherColider is in list of no vibrations, then do nothing
            {
                //Log("No Vibrate Collider: " + otherCollider);
            }
            else
            {
                if (!IsInArray(LightVibrateColliders, ShortOtherCollider) && !IsInArray(RegularVibrateColliders, ShortOtherCollider)) 
                {
                    Log("Unseen collider: " + otherCollider);
                    Rumble(7);
                }else{
                    
                    switch (IsInArray(RegularVibrateColliders, ShortOtherCollider))
                    
                    {    
                        case true: 
                            //Log("Regular Collider: " + otherCollider);
                            Rumble(7);
                            return;
                        case false:
                            //Log("Light Collider: " + otherCollider);
                            Rumble(8);
                            return;
                    }
                }
            }
        }
        

        private void DoHardLanding(On.HeroController.orig_DoHardLanding orig, HeroController self)
        {
            orig(self);
            Rumble(2);
        }
        //Since dame can be > 1, should the strength/style of vibration change?
        public int TakeDamage(int damage)
        {
            Rumble(1);
             return damage;
        }

        private int BeforeAddHealthHook(int health)
        {
            Rumble(10);
            return health;
        }

        private void UnPause(On.HeroController.orig_UnPause orig, HeroController self)
        {
            orig(self);
            
        }

        private void Pause(On.HeroController.orig_Pause orig, HeroController self)
        {
            orig(self);
            ResetTimedVibration();
            
        }



        //A variety of switch statements that correspond to a certain vibration effect. 
        //Left is low frequency rumble, right is high frequency
        private void Rumble(int type)
        {
            this.IsTimedRumbling = true;
            switch (type)
            {
                case 1: //Damage Taken
                    VibrationTime = 1;
                    GamePad.SetVibration(PlayerIndex.One, 1f, 1f);
                    return;
                case 2: //Hard Landing
                    VibrationTime = .35f;
                    GamePad.SetVibration(PlayerIndex.One, 1f, 1f);
                    return;
                case 4: //Finish Healing
                    VibrationTime = .2f;
                    GamePad.SetVibration(PlayerIndex.One, .1f, .5f);
                    return;
                case 5: //Dash
                    VibrationTime = .25f;
                    GamePad.SetVibration(PlayerIndex.One, .15f, .362f);
                    return;
                case 7: //Larger rumble (in comparison to case 7), object like a wall, stone monument. Default rumble for 'unknown' colliders
                    VibrationTime = .15f;
                    GamePad.SetVibration(PlayerIndex.One, .4f, .2f);
                    return;
                case 8: //Light rumble, like breaking a sign post
                    VibrationTime = .15f;
                    GamePad.SetVibration(PlayerIndex.One, 0f, .2f);
                    return;
                case 9: //Spell Rumble
                    VibrationTime = .35f;
                    GamePad.SetVibration(PlayerIndex.One, .75f, .5f);
                    return;
                case 10:
                    VibrationTime = .33f;
                    GamePad.SetVibration(0, .6f, .4f);
                    return;
                default:
                    return;
            }
        }
        //Spell cast hook, returning true means the spell actually executes, false means it doesn't
        private bool OnSpellHook(ModCommon.ModCommon.Spell s)
        {
            Rumble(9);
            return true;
        }

        private void ResetTimedVibration() 
        {
            IsTimedRumbling = false;
            VibrationTime = 0f;
            GamePad.SetVibration(PlayerIndex.One, 0f, 0f);

        }
        //Used for comparing in collider lists.
        private bool IsInArray(string[] ColliderList, string otherCollider)
        {
            foreach (var str in ColliderList)
            {
                if (str.Contains(otherCollider))
                {
                    return true;
                }
            }

            return false;
        }
        public override string GetVersion()
        {
            return "BETA 0.3.1";
        }

    }
}