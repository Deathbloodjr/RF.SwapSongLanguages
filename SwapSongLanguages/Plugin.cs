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
using System.Reflection;

namespace SwapSongLanguages
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, ModName, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        public const string ModName = "SwapSongLanguages";

        public static Plugin Instance;
        private Harmony _harmony;
        public new static ManualLogSource Log;

        public ConfigEntry<bool> ConfigEnabled;
        public ConfigEntry<string> ConfigSongTitleLanguageOverride;
        public ConfigEntry<string> ConfigSongSubtitleLanguageOverride;
        public ConfigEntry<string> ConfigSongDetailLanguageOverride;



        public override void Load()
        {
            Instance = this;

            Log = base.Log;

            SetupConfig(Config, Path.Combine("BepInEx", "data", ModName));
            SetupHarmony();

            var isSaveManagerLoaded = IsSaveManagerLoaded();
            if (isSaveManagerLoaded)
            {
                AddToSaveManager();
            }
        }

        private void SetupConfig(ConfigFile config, string saveFolder, bool isSaveManager = false)
        {
            string dataFolder = Path.Combine("BepInEx", "data", ModName);

            if (!isSaveManager)
            {
                ConfigEnabled = config.Bind("General",
                   "Enabled",
                   true,
                   "Enables the mod.");
            }

            ConfigSongTitleLanguageOverride = config.Bind("General",
                "SongTitleLanguageOverride",
                "JP",
                "Sets the song title to the selected language. (JP, EN, FR, IT, DE, ES, TW, CN, KO)");

            ConfigSongSubtitleLanguageOverride = config.Bind("General",
                "SongSubtitleLanguageOverride",
                "JP",
                "Sets the song subtitle to the selected language. (JP, EN, FR, IT, DE, ES, TW, CN, KO)");

            ConfigSongDetailLanguageOverride = config.Bind("General",
                "SongDetailLanguageOverride",
                "EN",
                "Sets the song detail (above the song title) to the selected language. (JP, EN, FR, IT, DE, ES, TW, CN, KO)");
        }

        private void SetupHarmony()
        {
            // Patch methods
            _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

            LoadPlugin(ConfigEnabled.Value);
        }


        public static void LoadPlugin(bool enabled)
        {
            if (enabled)
            {
                bool result = true;
                // If any PatchFile fails, result will become false
                result &= Instance.PatchFile(typeof(SwapSongLanguagesPatch));
                if (result)
                {
                    SwapSongLanguagesPatch.InitializeOverrideLanguages();
                    TaikoSingletonMonoBehaviour<CommonObjects>.Instance?.MyDataManager?.MusicData?.Reload();
                    Logger.Log($"Plugin {MyPluginInfo.PLUGIN_NAME} is loaded!");
                }
                else
                {
                    Logger.Log($"Plugin {MyPluginInfo.PLUGIN_GUID} failed to load.", LogType.Error);
                    // Unload this instance of Harmony
                    Instance._harmony.UnpatchSelf();
                }
            }
            else
            {
                UnloadPlugin();
                Logger.Log($"Plugin {MyPluginInfo.PLUGIN_NAME} is disabled.");
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
                Logger.Log("File patched: " + type.FullName, LogType.Debug);
                return true;
            }
            catch (Exception e)
            {
                Logger.Log("Failed to patch file: " + type.FullName);
                Logger.Log(e.Message);
                return false;
            }
        }

        public static void UnloadPlugin()
        {
            SwapSongLanguagesPatch.InitializeOverrideLanguages(true);
            TaikoSingletonMonoBehaviour<CommonObjects>.Instance.MyDataManager.MusicData.Reload();
            Instance._harmony.UnpatchSelf();
            Logger.Log($"Plugin {MyPluginInfo.PLUGIN_NAME} has been unpatched.");
        }

        public static void ReloadPlugin()
        {
            SwapSongLanguagesPatch.InitializeOverrideLanguages();
            TaikoSingletonMonoBehaviour<CommonObjects>.Instance.MyDataManager.MusicData.Reload();
        }

        public void AddToSaveManager()
        {
            var plugin = new PluginSaveDataInterface(MyPluginInfo.PLUGIN_GUID);
            plugin.AssignLoadFunction(LoadPlugin);
            plugin.AssignUnloadFunction(UnloadPlugin);
            plugin.AssignReloadSaveFunction(ReloadPlugin);
            plugin.AssignConfigSetupFunction(SetupConfig);
            plugin.AddToManager(ConfigEnabled.Value);
        }

        private bool IsSaveManagerLoaded()
        {
            try
            {
                Assembly loadedAssembly = Assembly.Load("com.DB.RF.SaveProfileManager");
                return loadedAssembly != null;
            }
            catch
            {
                return false;
            }
        }

        public static MonoBehaviour GetMonoBehaviour() => TaikoSingletonMonoBehaviour<CommonObjects>.Instance;
        public void StartCoroutine(IEnumerator enumerator)
        {
            GetMonoBehaviour().StartCoroutine(enumerator);
        }
    }
}
