using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnhollowerBaseLib;
using UnityEngine;

namespace LobbyOptionsAPI
{
    public class ScrollerOptionsManager
    {
        private static StringNames titleNum = (StringNames) 1337;
        private static List<ScrollerOptions> options = new List<ScrollerOptions>();

        public CustomNumberOption AddOption(byte defaultValue, string title, byte min, byte max, byte step = 1,
            string extension = "")
        {
            var obj = new CustomNumberOption(defaultValue, titleNum++, title, min, max, step);
            options.Add(obj);
            obj.format = "{0:0}" + extension;
            return obj;
        }

        public CustomNumberOption AddOption(float defaultValue, string title, float min, float max, float step = 0.25f,
            string extension = "")
        {
            var obj = new CustomNumberOption(defaultValue, titleNum++, title, min, max, step);
            options.Add(obj);
            obj.format = "{0:0.0#}" + extension;
            return obj;
        }

        public CustomToggleOption AddOption(bool defaultValue, string title)
        {
            var obj = new CustomToggleOption(defaultValue, titleNum++, title);
            options.Add(obj);
            return obj;
        }

        [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString),
            new Type[] {typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>)})]
        static class TranslationController_GetString
        {
            public static bool Prefix(StringNames HKOIECMDOKL, ref string __result)
            {
                foreach (var opt in options)
                {
                    if (opt.optionTitleName == HKOIECMDOKL)
                    {
                        __result = opt.optionTitle;
                        return false;
                    }
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
        static class GameOptionsMenu_Start
        {
            static float GetLowestConfigY(GameOptionsMenu __instance)
            {
                return __instance.GetComponentsInChildren<OptionBehaviour>()
                    .Min(option => option.transform.localPosition.y);
            }

            public static void OnValueChanged(OptionBehaviour option)
            {
                if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost) return;
                foreach (var opt in options)
                {
                    if (opt.optionTitleName == option.Title)
                    {
                        opt.Update(option);
                        break;
                    }
                }

                if (PlayerControl.GameOptions.isDefaults)
                {
                    PlayerControl.GameOptions.isDefaults = false;
                    UnityEngine.Object.FindObjectOfType<GameOptionsMenu>().Method_16(); //RefreshChildren
                }

                var local = PlayerControl.LocalPlayer;
                if (local != null)
                {
                    local.RpcSyncSettings(PlayerControl.GameOptions);
                }
            }

            static void Postfix(ref GameOptionsMenu __instance)
            {
                var lowestY = GetLowestConfigY(__instance);
                float offset = 0.0f;
                foreach (var opt in options)
                {
                    offset += 0.5f;

                    opt.Start(__instance, lowestY, offset, OnValueChanged);
                }

                __instance.GetComponentInParent<Scroller>().YBounds.max += offset - 0.9f;
            }
        }

        [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.OnEnable))]
        static class GameSettingsMenu_OnEnable
        {
            static void Prefix(ref GameSettingMenu __instance)
            {
                __instance.HideForOnline = new Il2CppReferenceArray<Transform>(0);
            }
        }

        [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.OnEnable))]
        static class NumberOption_OnEnable
        {
            static bool Prefix(ref NumberOption __instance)
            {
                foreach (var opt in options)
                {
                    if (__instance.Title == opt.optionTitleName)
                    {
                        opt.OnEnable(ref __instance, GameOptionsMenu_Start.OnValueChanged);
                        return false;
                    }
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(ToggleOption), nameof(ToggleOption.OnEnable))]
        static class ToggleOption_OnEnable
        {
            static bool Prefix(ref ToggleOption __instance)
            {
                foreach (var opt in options)
                {
                    if (__instance.Title == opt.optionTitleName)
                    {
                        opt.OnEnable(ref __instance, GameOptionsMenu_Start.OnValueChanged);
                        return false;
                    }
                }

                return true;
            }
        }
    }

    public class CustomNumberOption : ScrollerOptions
    {
        public float value;
        public float min;
        public float max;
        public float step;
        public string format = "{0:0}";

        public CustomNumberOption(float defaultValue, StringNames optionTitleName, string optionTitle, float min,
            float max, float step) : base(optionTitleName, optionTitle)
        {
            this.value = defaultValue;
            this.min = min;
            this.max = max;
            this.step = step;
        }

        public override void Start(GameOptionsMenu __instance, float lowestY, float offset,
            Action<OptionBehaviour> callback)
        {
            var countOption = UnityEngine.Object.Instantiate(
                __instance.GetComponentsInChildren<NumberOption>()[1],
                __instance.transform);
            countOption.transform.localPosition = new Vector3(countOption.transform.localPosition.x,
                lowestY - offset,
                countOption.transform.localPosition.z);
            countOption.Title = optionTitleName;
            countOption.Value = value;
            countOption.TitleText.Text = optionTitle;
            countOption.OnValueChanged = new Action<OptionBehaviour>(callback);
            countOption.gameObject.AddComponent<OptionBehaviour>();
            countOption.ValidRange.max = max;
            countOption.ValidRange.min = min;
            countOption.Increment = step;
            countOption.FormatString = format;
        }

        public override void Update(OptionBehaviour option)
        {
            value = option.GetFloat();
        }

        public override void OnEnable(ref NumberOption __instance, Action<OptionBehaviour> callback)
        {
            __instance.TitleText.Text = optionTitle;
            __instance.OnValueChanged = new Action<OptionBehaviour>(callback);
            __instance.Value = value;
            __instance.enabled = true;
        }
    }

    public class CustomToggleOption : ScrollerOptions
    {
        public bool value;

        public CustomToggleOption(bool defaultValue, StringNames optionTitleName, string optionTitle) : base(
            optionTitleName, optionTitle)
        {
            this.value = defaultValue;
        }

        public override void Start(GameOptionsMenu __instance, float lowestY, float offset,
            Action<OptionBehaviour> callback)
        {
            var toggleOption = UnityEngine.Object.Instantiate(
                __instance.GetComponentsInChildren<ToggleOption>()[1],
                __instance.transform);
            toggleOption.transform.localPosition = new Vector3(toggleOption.transform.localPosition.x,
                lowestY - offset, toggleOption.transform.localPosition.z);
            toggleOption.Title = optionTitleName;
            toggleOption.CheckMark.enabled = value;
            toggleOption.TitleText.Text = optionTitle;
            toggleOption.OnValueChanged = new Action<OptionBehaviour>(callback);
            toggleOption.gameObject.AddComponent<OptionBehaviour>();
        }

        public override void Update(OptionBehaviour option)
        {
            value = option.GetBool();
        }

        public override void OnEnable(ref ToggleOption __instance, Action<OptionBehaviour> callback)
        {
            __instance.TitleText.Text = optionTitle;
            __instance.OnValueChanged = new Action<OptionBehaviour>(callback);
            __instance.enabled = value;
        }
    }

    public abstract class ScrollerOptions
    {
        public StringNames optionTitleName;
        public string optionTitle;

        public ScrollerOptions(StringNames optionTitleName, string optionTitle)
        {
            this.optionTitleName = optionTitleName;
            this.optionTitle = optionTitle;
        }

        public abstract void Start(GameOptionsMenu __instance, float lowestY, float offset,
            Action<OptionBehaviour> callback);

        public abstract void Update(OptionBehaviour option);

        public virtual void OnEnable(ref NumberOption __instance, Action<OptionBehaviour> callback)
        {
        }

        public virtual void OnEnable(ref ToggleOption __instance, Action<OptionBehaviour> callback)
        {
        }
    }
}