using BepInEx.Unity.IL2CPP.Utils;
using BepInEx.Unity.IL2CPP;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using BepInEx.Configuration;
using SwapSongLanguages.Plugins;
using UnityEngine;
using System.Collections;
using SaveProfileManager.Plugins;

namespace SwapSongLanguages
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, ModName, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        public const string ModName = "SwapSongLanguages";

        public static Plugin Instance;
        private Harmony _harmony;
        public new static ManualLogSource Log;

        public static PluginSaveDataInterface plugin;

        public ConfigEntry<bool> ConfigEnabled;
        public ConfigEntry<string> ConfigSongTitleLanguageOverride;
        public ConfigEntry<string> ConfigSongSubtitleLanguageOverride;
        public ConfigEntry<string> ConfigSongDetailLanguageOverride;



        public override void Load()
        {
            Instance = this;

            Log = base.Log;

            SetupConfig();
            SetupHarmony();

            AddToSaveManager();
        }

        private void SetupConfig()
        {
            var dataFolder = Path.Combine("BepInEx", "data", ModName);

            ConfigEnabled = Config.Bind("General",
                "Enabled",
                true,
                "Enables the mod.");

            ConfigSongTitleLanguageOverride = Config.Bind("General",
                "SongTitleLanguageOverride",
                "JP",
                "Sets the song title to the selected language. (JP, EN, FR, IT, DE, ES, TW, CN, KO)");

            ConfigSongSubtitleLanguageOverride = Config.Bind("General",
                "SongSubtitleLanguageOverride",
                "JP",
                "Sets the song subtitle to the selected language. (JP, EN, FR, IT, DE, ES, TW, CN, KO)");

            ConfigSongDetailLanguageOverride = Config.Bind("General",
                "SongDetailLanguageOverride",
                "EN",
                "Sets the song detail (above the song title) to the selected language. (JP, EN, FR, IT, DE, ES, TW, CN, KO)");
        }

        private void SetupHarmony()
        {
            // Patch methods
            _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

            LoadPlugin();
        }


        public static void LoadPlugin()
        {
            if (Instance.ConfigEnabled.Value)
            {
                bool result = true;
                // If any PatchFile fails, result will become false
                result &= Instance.PatchFile(typeof(SwapSongLanguagesPatch));
                if (result)
                {
                    SwapSongLanguagesPatch.InitializeOverrideLanguages();
                    Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_NAME} is loaded!");
                }
                else
                {
                    Log.LogError($"Plugin {MyPluginInfo.PLUGIN_GUID} failed to load.");
                    // Unload this instance of Harmony
                    Instance._harmony.UnpatchSelf();
                }
            }
            else
            {
                Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_NAME} is disabled.");
            }
        }

        private bool PatchFile(Type type)
        {
            if (_harmony == null)
            {
                _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            }
            try
            {
                _harmony.PatchAll(type);
#if DEBUG
                Log.LogInfo("File patched: " + type.FullName);
#endif
                return true;
            }
            catch (Exception e)
            {
                Log.LogInfo("Failed to patch file: " + type.FullName);
                Log.LogInfo(e.Message);
                return false;
            }
        }

        public static void UnloadPlugin()
        {
            SwapSongLanguagesPatch.InitializeOverrideLanguages(true);
            TaikoSingletonMonoBehaviour<CommonObjects>.Instance.MyDataManager.MusicData.Reload();
            Instance._harmony.UnpatchSelf();
            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_NAME} has been unpatched.");
        }

        public static void ReloadPlugin()
        {
            SwapSongLanguagesPatch.InitializeOverrideLanguages();
            TaikoSingletonMonoBehaviour<CommonObjects>.Instance.MyDataManager.MusicData.Reload();
        }

        public void AddToSaveManager()
        {
            plugin = new PluginSaveDataInterface(MyPluginInfo.PLUGIN_GUID);
            plugin.AssignLoadFunction(LoadPlugin);
            plugin.AssignUnloadFunction(UnloadPlugin);
            plugin.AssignReloadSaveFunction(ReloadPlugin);
            plugin.AddToManager();
            //Logger.Log("Plugin added to SaveDataManager");
        }

        public static MonoBehaviour GetMonoBehaviour() => TaikoSingletonMonoBehaviour<CommonObjects>.Instance;
        public void StartCoroutine(IEnumerator enumerator)
        {
            GetMonoBehaviour().StartCoroutine(enumerator);
        }
    }
}
