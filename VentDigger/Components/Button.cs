using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VentDigger
{
    class Button
    {
        private static List<Button> AllButtons = new List<Button>();

        private readonly HudManager _hudManager;
        private readonly string _embeddedResourcesImage;
        private Action _onClick;
        public KillButtonManager buttonManager;
        float MaxTimer = 15;
        public Sprite sprite;

        float timer;

        public bool CanPlace = true;
        private bool canUse = false;

        public Button(HudManager hudManager, string embeddedResourcesImage)
        {
            _hudManager = hudManager;
            _embeddedResourcesImage = embeddedResourcesImage;

            AllButtons.Add(this);
            OnStart();
        }

        public bool Enabled
        {
            set
            {
                canUse = value;
                buttonManager.gameObject.SetActive(canUse);
                buttonManager.renderer.sprite = sprite;
            }

            get => canUse;
        }

        void OnStart()
        {
            buttonManager = GameObject.Instantiate(_hudManager.KillButton, _hudManager.transform);
            // Set Sprite
            sprite = SpriteHelper.LoadSpriteFromEmbeddedResources(_embeddedResourcesImage, 600);
            buttonManager.SetCoolDown(timer, MaxTimer);
            Rest();

            buttonManager.transform.localPosition = new Vector3((buttonManager.transform.localPosition.x + 1.3f) * -1, buttonManager.transform.localPosition.y, buttonManager.transform.localPosition.z);
        }

        void OnUpdate()
        {

            if (timer > 0)
            {
                timer -= Time.deltaTime;
                buttonManager.SetCoolDown(timer, MaxTimer);
            }
            else
            {
                SetTarget();
            }

            buttonManager.transform.localPosition = new Vector3((_hudManager.UseButton.transform.localPosition.x) * -1, _hudManager.UseButton.transform.localPosition.y, _hudManager.KillButton.transform.localPosition.z) + new Vector3(0.2f, 0.2f);

        }

        bool CanUse()
        {
            return (timer < 0) && CanPlace;
        }

        void SetTarget()
        {
            buttonManager.renderer.color = new Color(1f, 1f, 1f, 1f);
            buttonManager.renderer.material.SetFloat("_Desat", 0f);
        }

        void SetMaxCoolDown(float maxTimer)
        {
            MaxTimer = maxTimer;
        }

        void Rest()
        {
            timer = MaxTimer;
        }

        public void AddListener(Action action)
        {
            _onClick += action;
            buttonManager.GetComponent<PassiveButton>().OnClick.AddListener((UnityAction)OnClick);
            void OnClick()
            {
                if (CanUse())
                {
                    _onClick();
                    Rest();
                }
            }
        }

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        class HudManagerPatch {
            static void Prefix()
            {
                try
                {
                    AllButtons.RemoveAll(item => item.buttonManager == null);
                    foreach (var button in AllButtons)
                    {
                        button.OnUpdate();
                    }
                }
                catch
                {

                }
            }  
        }
    }
}
