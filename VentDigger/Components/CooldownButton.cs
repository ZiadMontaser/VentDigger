using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;

namespace AdminLog.Utils
{
    public class CooldownButton
    {
        internal static List<CooldownButton> allButtons = new();
        
        public KillButtonManager Manager {get; }
        public string Name { get;}

        //State
        public float Timer { get; set; }
        public float MaxTimer { get; private set; }
        public KeyCode Key { get; set; } = KeyCode.None;

        public bool IsAvailable { get => OnClick != null; }

        private bool canUse = true;
        public Func<bool> OnClick { get; set; }

        //CoolDowns
        private float _initCoolDown = 0;
        public float InitCooldown
        {
            get => _initCoolDown;
            set => Timer = MaxTimer = value;
        }

        private float _normalCooldown;
        public float NormalCooldown
        {
            get => _normalCooldown;
            set
            {
                _normalCooldown = value;
                if(InitCooldown == 0) Reset();
            } 
        } 

        Sprite _sprite;
        public Sprite Sprite
        {
            set
            {
                _sprite = value;
                Manager.renderer.sprite = value;
            }
        }

        public virtual bool CanUse
        {
            get => canUse && Timer < 0 && PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead;
            set => canUse = value;
        }


        public CooldownButton(string name)
        {
            Manager = GameObject.Instantiate(HudManager.Instance.KillButton, HudManager.Instance.transform);
            Vector3 ButtonPos = HudManager.Instance.UseButton.transform.localPosition;
            ButtonPos.x = (ButtonPos.x + 1.3f) * -1;
            Manager.transform.localPosition = ButtonPos;
            Manager.renderer.color = Color.white;
            Manager.gameObject.SetActive(false);
            Manager.GetComponent<PassiveButton>().OnClick.AddListener((UnityAction)Click);
            Manager.killText.gameObject.SetActive(false);

            if (name != null)
            {
                Name = name;
                Manager.gameObject.name = $"{Name} Button";
            }
            allButtons.Add(this);
        }

        public CooldownButton() : this(null){}

        public virtual void Update()
        {
            var buttonPos = HudManager.Instance.UseButton.transform.localPosition;
            buttonPos.x *= -1;
            Manager.transform.localPosition = buttonPos;
            
            if(!PlayerControl.LocalPlayer) return;

            Manager.gameObject.SetActive(IsAvailable && !MeetingHud.Instance && !PlayerControl.LocalPlayer.Data.IsDead);
            if (IsAvailable && !MeetingHud.Instance)
            {
                if(_sprite) Manager.renderer.sprite = _sprite;
                Timer -= Time.deltaTime;
                Manager.SetCoolDown(Timer, MaxTimer);
                if (Timer < 0 && canUse)
                {
                    Manager.renderer.color = Palette.EnabledColor;
                    Manager.renderer.material.SetFloat("_Desat", 0f);
                }
                else
                {
                    Manager.renderer.color = Palette.DisabledClear;
                    Manager.renderer.material.SetFloat("_Desat", 1f);
                }

                if (Input.GetKeyDown(Key)) OnClick();

            }
        }
        
        void Click()
        {
            if (CanUse && OnClick != null)
            {
                if (OnClick())
                {
                    Reset();
                }
            }
        }
        
        public void Reset() => Timer = MaxTimer = NormalCooldown;
        
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        class HudManagerPatch {
            [HarmonyPostfix] static void Postfix()
            {
                allButtons.RemoveAll(item => item.Manager == null);
                foreach (var button in allButtons)
                {
                    button.Update();
                }
            }  
        }
    } 
}