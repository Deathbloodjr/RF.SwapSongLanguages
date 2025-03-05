using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MusicDataInterface;
using static DataConst;
using SaveProfileManager.Plugins;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace SwapSongLanguages.Plugins
{
    internal class SwapSongLanguagesPatch
    {
        public static LanguageType CurrentLanguage = LanguageType.English;

        private static bool overrideSongTitle = true;
        public static LanguageType SongTitleOverride = LanguageType.Japanese;

        private static bool overrideSongSubtitle = true;
        public static LanguageType SongSubtitleOverride = LanguageType.Japanese;

        private static bool overrideSongDetail = true;
        public static LanguageType SongDetailOverride = LanguageType.English;

        [HarmonyPatch(typeof(MusicInfoAccesser))]
        [HarmonyPatch(nameof(MusicInfoAccesser.SetMusicInfo))]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPostfix]
        public static void MusicInfoAccesser_SetMusicInfo_Postfix(MusicInfoAccesser __instance, MusicDataInterface.MusicInfo musicinfo, int katsuMask)
        {
            //Logger.Log("");
            //Logger.Log("MusicInfoAccesser_SetMusicInfo_Postfix");

            if (overrideSongTitle)
            {
                __instance.SongNames[(int)CurrentLanguage] = GetSongTitleFromLanguage(musicinfo, SongTitleOverride);
            }
            if (overrideSongSubtitle)
            {
                __instance.SongSubs[(int)CurrentLanguage] = GetSongSubtitleFromLanguage(musicinfo, SongSubtitleOverride);
            }
            if (overrideSongDetail)
            {
                __instance.SongNames[(int)LanguageType.Japanese] = GetSongTitleFromLanguage(musicinfo, SongDetailOverride);
            }
            //__instance.SongNames[0] = musicinfo.SongNameEN;
            //__instance.SongNames[1] = musicinfo.SongNameJP;
        }

        [HarmonyPatch(typeof(WordDataInterface))]
        [HarmonyPatch(nameof(WordDataInterface.ChangeLanguage))]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPostfix]
        public static void WordDataInterface_ChangeLanguage_Postfix(WordDataInterface __instance, string language)
        {
            //Logger.Log("");
            //Logger.Log("WordDataInterface_ChangeLanguage_Postfix");

            //Logger.Log("language: " + language);

            CurrentLanguage = SelectLanguage.GetLanguageType(language);

            //Logger.Log("CurrentLanguage: " + CurrentLanguage.ToString());

            try
            {
                TaikoSingletonMonoBehaviour<CommonObjects>.Instance.MyDataManager.MusicData.Reload();
            }
            catch
            {

            }
        }

        public static void InitializeOverrideLanguages(bool disabled = false)
        {
            if (disabled)
            {
                overrideSongTitle = false;
                overrideSongSubtitle = false;
                overrideSongDetail = false;
                return;
            }
         
            var songTitle = StringToLanguageType(Plugin.Instance.ConfigSongTitleLanguageOverride.Value);
            if (songTitle != LanguageType.Num)
            {
                overrideSongTitle = true;
                SongTitleOverride = songTitle;
            }

            var songSubtitle = StringToLanguageType(Plugin.Instance.ConfigSongSubtitleLanguageOverride.Value);
            if (songSubtitle != LanguageType.Num)
            {
                overrideSongSubtitle = true;
                SongSubtitleOverride = songSubtitle;
            }

            var songDetail = StringToLanguageType(Plugin.Instance.ConfigSongDetailLanguageOverride.Value);
            if (songDetail != LanguageType.Num)
            {
                overrideSongDetail = true;
                SongDetailOverride = songDetail;
            }
            
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="language">JP, EN, FR, IT, DE, ES, TW, CN, KO.</param>
        /// <returns></returns>
        private static LanguageType StringToLanguageType(string language)
        {
            switch (language)
            {
                case "JP": return LanguageType.Japanese;
                case "EN": return LanguageType.English;
                case "FR": return LanguageType.French;
                case "IT": return LanguageType.Italian;
                case "DE": return LanguageType.German;
                case "ES": return LanguageType.Spanish;
                case "TW": return LanguageType.ChineseT;
                case "CN": return LanguageType.ChineseS;
                case "KO": return LanguageType.Korean;
                default: Logger.Log("Couldn't parse input language: " + language, LogType.Warning); return LanguageType.Num;
            }
        }

        private static string GetSongTitleFromLanguage(MusicInfo musicInfo, LanguageType language)
        {
            switch (language)
            {
                case LanguageType.Japanese: return musicInfo.SongNameJP;
                case LanguageType.English: return musicInfo.SongNameEN;
                case LanguageType.French: return musicInfo.SongNameFR;
                case LanguageType.Italian: return musicInfo.SongNameIT;
                case LanguageType.German: return musicInfo.SongNameDE;
                case LanguageType.Spanish: return musicInfo.SongNameES;
                case LanguageType.ChineseT: return musicInfo.SongNameTW;
                case LanguageType.ChineseS: return musicInfo.SongNameCN;
                case LanguageType.Korean: return musicInfo.SongNameKO;

                case LanguageType.Num:
                default: return musicInfo.SongNameJP;
            }
        }

        private static string GetSongSubtitleFromLanguage(MusicInfo musicInfo, LanguageType language)
        {
            switch (language)
            {
                case LanguageType.Japanese: return musicInfo.SongSubJP;
                case LanguageType.English: return musicInfo.SongSubEN;
                case LanguageType.French: return musicInfo.SongSubFR;
                case LanguageType.Italian: return musicInfo.SongSubIT;
                case LanguageType.German: return musicInfo.SongSubDE;
                case LanguageType.Spanish: return musicInfo.SongSubES;
                case LanguageType.ChineseT: return musicInfo.SongSubTW;
                case LanguageType.ChineseS: return musicInfo.SongSubCN;
                case LanguageType.Korean: return musicInfo.SongSubKO;

                case LanguageType.Num:
                default: return musicInfo.SongSubJP;
            }
        }
    }
}
