using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValorantResFix
{
    [Serializable]
    public class LoLSettings
    {
        // [Performance]
        public int ShadowQuality { get; set; }
        public int CharacterQuality { get; set; }
        public int EnvironmentQuality { get; set; }
        public int EffectsQuality { get; set; }
        public int EnableFXAA { get; set; }
        public int EnableHUDAnimations { get; set; }

        // [General]
        public int WindowMode { get; set; }
        public int WaitForVerticalSync { get; set; }
        public int ShowGodray { get; set; }
        public int HideEyeCandy { get; set; }

        // [HUD]
        public int EternalsMilestoneDisplayMode { get; set; }

        public string FilePath { get; set; }
    }
}
