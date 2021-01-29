using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using HudManager = PIEFJFEOGOL;
using KillButtonManager = MLPJGKEACMM;
using PassiveButton = HHMBANDDIOA;
using PlayerControl = FFGALNAPKCD;
using ShipStatus = HLBNNHFCNAJ;
using Vent = OPPMFCFACJB;


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

        float timer;

        bool CanPlace = true;

        public Button(HudManager hudManager, string embeddedResourcesImage)
        {
            _hudManager = hudManager;
            _embeddedResourcesImage = embeddedResourcesImage;

            AllButtons.Add(this);
            OnStart();
        }

        void OnStart()
        {
            buttonManager = GameObject.Instantiate(_hudManager.KillButton, _hudManager.transform);
            //buttonManager.gameObject.SetActive(true);
            // Set Sprite
            buttonManager.renderer.sprite = SpriteHelper.LoadSpriteFromEmbeddedResources(_embeddedResourcesImage, 600);
            buttonManager.SetCoolDown(timer, MaxTimer);
            Rest();

            buttonManager.renderer.SetCooldownNormalizedUvs();
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

            List<Vent> nearVents = new List<Vent>();
            if (ShipStatus.Instance) { 
            }
            {
                if (ShipStatus.Instance.CIAHFBANKDD != null)
                {
                    for (int i = 0; i < ShipStatus.Instance.CIAHFBANKDD.Count; i++)
                    {
                        var vent = ShipStatus.Instance.CIAHFBANKDD[i];
                        if (vent)
                        {
                            if (Vector2.Distance(PlayerControl.LocalPlayer.transform.position, vent.transform.position) < 2)
                            {
                                nearVents.Add(vent);
                            }
                        }
                    }

                    if (nearVents.Count == 0)
                    {
                        CanPlace = true;
                        //if (Patches.lastVent)
                        //{
                        //    if (ShipStatus.Instance.CIAHFBANKDD.Count - 1 > Patches.lastVent.Id)
                        //    {
                        //        CanPlace = true;
                        //    }
                        //    else { CanPlace = false; }
                        //}
                    }
                    else
                    {
                        CanPlace = false;
                    }
                }
            }

            if (CanPlace)
            {
                buttonManager.renderer.color = new Color(1f, 1f, 1f, 1f);
            }
            else
            {
                buttonManager.renderer.color = new Color(1f, 1f, 1f, 0.3f);
            }
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

        void SetMAxCoolDown(float maxTimer)
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
