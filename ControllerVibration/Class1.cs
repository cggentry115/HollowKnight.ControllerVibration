using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker.Actions;
using Modding;
using XInputDotNetPure;
using UnityEngine;

//leftMotor low frequency, rightMotor High Frequencies, 0 - 65,535
namespace ControllerVibration
{
    public class ControllerVibration : Mod
    {

        
        private float VibrationTime = .5f; //How long it will vibrate for. Interacts with time.deltaTime so a value of '1' will result in a 1-second vibration.
        private bool isRumbling = true;

        //List of collider.toString()'s. One contains a list of colliders that should not trigger vibration, while the other
        //can be used to trigger a specific type of vibration, if desired.
        private string[] NoVibrateColliders =
        {
            "white_grass", "Shield ", "white", "Break", "Roof ", 
            "Dream Dialogue", "town_grass", "left cliff collider", "Geo Small", "plat_float_", "GameObject", 
            "Blue Glob", "Tute Door", "Tute Pole",  "Chunk", "Mines Short Shelf", "terrain collider", "Stag Lift", "Lift",
            "Opened", "elev_plat"
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
            "Buzzer", "brk_barrel", "Fly", "Gate Switch", "Tinger", "Soul Totem", "Bell", "Hatcher", "Spitter",
            
        };
        
        
        public ControllerVibration() : base("Controller Vibration") 
        {
        }
            public override void Initialize() 
        {
            Log("Initializing ControllerVibration");
            
            //These functions correspond to events that should have Vibration, so when they are called, 
            //my modded vibration function is called as well
            ModHooks.Instance.HeroUpdateHook += OnHeroUpdate;            
            ModHooks.Instance.SlashHitHook += OnSlashHit;
            On.HeroController.DoHardLanding += DoHardLanding;
            ModHooks.Instance.TakeHealthHook += TakeDamage;
            On.DamageEnemies.DoDamage += DoDamage;
            ModHooks.Instance.BeforeAddHealthHook += BeforeAddHealthHook;


            
            //These lines of code shorten all of the elements of the collider lists to just 5 characters long. While this
            //is not a very good way of doing things, without actually being able to tell if a given collider should vibrate,
            //since there seems to be no base function for that, I have to manually identify each collider, classifying it as either no, light, or regular vibrate
            //Setting it to 5 makes it to where something like "Geo Rock 52 5" will still produce vibration, since it will be just seen as "Geo R"
            for(int i = 0; i < NoVibrateColliders.Length; i++)
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
        private void OnHeroUpdate() //Called every time HeroController is called as well
        {
            
            if (this.isRumbling)
            {
                if (this.VibrationTime <= 0)
                {
                    GamePad.SetVibration(PlayerIndex.One, 0f, 0f);
                    this.isRumbling = false;
                    VibrationTime = 0f;
                    return;
                }

                VibrationTime -= Time.deltaTime;
            } 

        }
        
        //This function is called when a gameobject, such as Slash, hits another collider, such an enemy
        //See's what Slash has collided with, and what type of vibration should occour
        private void OnSlashHit(Collider2D otherCollider, GameObject gameObject)
        {

            var ShortOtherCollider = otherCollider.ToString().Substring(0, 5);


            if ((IsInArray(NoVibrateColliders, ShortOtherCollider)) || otherCollider.ToString().Contains("Dialo") ) //If otherColider is in list of no vibrations, then do nothing
            {
                Log("No Vibrate Collider: " + otherCollider);
            }
            else
            {
                if (!IsInArray(LightVibrateColliders, ShortOtherCollider) && !IsInArray(RegularVibrateColliders, ShortOtherCollider)) 
                {
                    Log("Unseen collider: " + otherCollider);
                    Vibrate(7);
                    
                    
                }else{
                    
                    switch (IsInArray(RegularVibrateColliders, ShortOtherCollider))
                    
                    {    
                        case true: 
                            Log("Regular Collider: " + otherCollider);
                            Vibrate(7);
                            return;
                        case false:
                            Log("Light Collider: " + otherCollider);
                            Vibrate(8);
                            return;
                    }
                }
            }
        }
        
        private void DoHardLanding(On.HeroController.orig_DoHardLanding orig, HeroController self)
        {
            orig(self);
            Vibrate(2);
        }
        //Since dame can be > 1, should the strength/style of vibration change?
        public int TakeDamage(int damage)
        {
            Vibrate(1);
             return damage;
        }
        private void DoDamage(On.DamageEnemies.orig_DoDamage orig, DamageEnemies self, GameObject target)
        {
            orig(self, target);
            Vibrate(3);
        }

        private int BeforeAddHealthHook(int health)
        {
            Vibrate(4);
            return health;
        }
        
        
        
        
        //A variety of switch statements that correspond to a certain vibration effect. 
        //Left is low frequency rumble, right is high frequency
        public void Vibrate(int type)
        {
            this.isRumbling = true;
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
                case 3: //Hitting an Enemy
                    VibrationTime = 1f;
                    GamePad.SetVibration(PlayerIndex.One, 1f, 1f);
                    return;
                case 4: //Finish Healing
                    VibrationTime = .2f;
                    GamePad.SetVibration(PlayerIndex.One, .1f, .5f);
                    return;
                case 5: //Soul Blast
                    VibrationTime = 1;
                    GamePad.SetVibration(PlayerIndex.One, 1f, 1f);
                    return;
                case 6: //Wall Slide
                    VibrationTime = 1;
                    GamePad.SetVibration(PlayerIndex.One, 1f, 1f);
                    return;
                case 7: //Larger rumble (in comparison to case 7), object like a wall, stone monument. Default rumble for 'unknown' colliders
                    VibrationTime = .15f;
                    GamePad.SetVibration(PlayerIndex.One, .2f, 0f);
                    return;
                case 8: //Light rumble, like breaking a sign post
                    VibrationTime = .15f;
                    GamePad.SetVibration(PlayerIndex.One, 0f, .2f);
                    return;
                case 9: //Spell Rumble
                    VibrationTime = 1;
                    GamePad.SetVibration(PlayerIndex.One, 1f, 1f);
                    return;
                case 10: //Bounce Rumble
                    VibrationTime = 1;
                    GamePad.SetVibration(PlayerIndex.One, 1f, 1f);
                    return;
                default:
                    return;
            }
        }

        //Returns bool if a given string is within the given array. Used for comparing in collider lists.
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
        
    }
}