using FlameBlame.Firefighter.Config;
using UnityEngine;

namespace FlameBlame.UI.Menu
{
    public class MenuSettings
    {
        private const string SwapKey = "FlameBlame.SwapInterval";
        private const string SfxKey = "FlameBlame.SfxVolume";
        private const string MusicKey = "FlameBlame.MusicVolume";
        private const float DefaultVolumeFraction = 0.8f;

        public SwapInterval Swap { get; set; }
        public float SfxVolume { get; set; }
        public float MusicVolume { get; set; }

        public static MenuSettings Load(SwapInterval defaultSwap) => new MenuSettings
        {
            Swap = (SwapInterval)PlayerPrefs.GetInt(SwapKey, (int)defaultSwap),
            SfxVolume = PlayerPrefs.GetFloat(SfxKey, DefaultVolumeFraction),
            MusicVolume = PlayerPrefs.GetFloat(MusicKey, DefaultVolumeFraction)
        };

        public void Save()
        {
            PlayerPrefs.SetInt(SwapKey, (int)Swap);
            PlayerPrefs.SetFloat(SfxKey, SfxVolume);
            PlayerPrefs.SetFloat(MusicKey, MusicVolume);
            PlayerPrefs.Save();
        }
    }
}
