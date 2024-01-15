//#define HighLogging
//#define ExtractLayerMask

using Rewired;
using System;
using UnityEngine;
using RewiredConsts;
using Voxeland5;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using System.Linq;
using System.Diagnostics.Eventing.Reader;
using VLB;
using JetBrains.Annotations;
using BepInEx.Configuration;

namespace AutoSaveConfig
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class AutoSaveConfig : BaseUnityPlugin
    {
        public const string pluginGuid = "autosaveconfig.nhickling.co.uk";
        public const string pluginName = "AutoSaveConfig";
        public const string pluginVersion = "0.0.0.5";

        public const string Config_TimeoutSection = "Config";
        public const string Config_TimeoutKey = "Timeout";

        public static ConfigEntry<int> AutoSaveTimeout;

        private static ManualLogSource modLogger;
        public void Awake()
        {
            modLogger = Logger;
            modLogger.LogInfo($"{pluginName}: Started {pluginVersion}");

            AutoSaveTimeout = ((BaseUnityPlugin)this).Config.Bind<int>(Config_TimeoutSection, Config_TimeoutKey, 300, new ConfigDescription("Timeout in seconds between autosaves", (AcceptableValueBase)(object)new AcceptableValueRange<int>(30, 600), Array.Empty<object>()));

            Harmony harmony = new Harmony(pluginGuid);
            modLogger.LogInfo($"{pluginName}: Fetching patch references");
            
            modLogger.LogInfo($"{pluginName}: Starting Patch");
            harmony.PatchAll(typeof(AutoSavePatch));
        }

    }

    class AutoSavePatch 
    {
        [HarmonyPatch(typeof(SaveState), "NeedsToAutoSave")]
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result)
        {
            
            try
            {
                bool isSaver = SettingsMenu.autoSaveEnabled && NetworkConnector.IsHost;
                if (isSaver)
                {
                    __result = SaveState.currentPlayTime - SaveState.instance.metadata.playTime > AutoSaveConfig.AutoSaveTimeout.Value;
                }
                else
                {
                    __result = false;
                }


                return false;
            }
            catch(Exception)
            {
            }

            return true;
        }


    }
}
