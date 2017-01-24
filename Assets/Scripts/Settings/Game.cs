using UnityEngine;
using System.Collections.Generic;

namespace Settings
{
    public enum AaValue
    {
        OFF = 0,
        X2 = 1,
        X4 = 2,
        X8 = 3
    }

    public enum TexSize
    {
        FULL = 0,
        HALF = 1,
        QUARTER = 2
    }

    public enum FrameQueueLimit
    {
        ZERO = 0,
        ONE = 1,
        TWO = 2,
        UNLIMITED = -1
    }

    public static class Game
    {
        private const string MOUSE_SPEED = "MouseSpeed";
        private const string INVERT_Y = "InvertY";
        private const string FOV = "Fov";
        private const string VOLUME = "Volume";
        private const string ANISOTROPIC_FILTERING = "Aniso";
        private const string ANTI_ALIASING = "AA";
        private const string TEXTURE_SIZE = "TextureSize";
        private const string V_SYNC = "VSync";
        private const string RAW_MOUSE = "RawMouse";
        private const string FRAME_QUEUE_LIMIT = "FrameQueueLimit";
        private const string SHOW_HELP = "ShowHelp";

        public static float MouseSpeed { get { return (float)AllSettings.GetSetting(MOUSE_SPEED); } set { AllSettings.SetSetting(MOUSE_SPEED, value); } }
        public static bool InvertY { get { return (bool)AllSettings.GetSetting(INVERT_Y); } set { AllSettings.SetSetting(INVERT_Y, value); } }
        public static float Fov { get { return (float)AllSettings.GetSetting(FOV); } set { AllSettings.SetSetting(FOV, value); } }
        public static float Volume { get { return (float)AllSettings.GetSetting(VOLUME); } set { AllSettings.SetSetting(VOLUME, value); } }
        public static AnisotropicFiltering AnisotropicFiltering { get { return (AnisotropicFiltering)AllSettings.GetSetting(ANISOTROPIC_FILTERING); } set { AllSettings.SetSetting(ANISOTROPIC_FILTERING, (int)value); } }
        public static AaValue AntiAliasing { get { return (AaValue)AllSettings.GetSetting(ANTI_ALIASING); } set { AllSettings.SetSetting(ANTI_ALIASING, value); } }
        public static TexSize TextureSize { get { return (TexSize)AllSettings.GetSetting(TEXTURE_SIZE); } set { AllSettings.SetSetting(TEXTURE_SIZE, value); } }
        public static bool VSync { get { return (bool)AllSettings.GetSetting(V_SYNC); } set { AllSettings.SetSetting(V_SYNC, value); } }
        public static bool RawMouse { get { return (bool)AllSettings.GetSetting(RAW_MOUSE); } set { AllSettings.SetSetting(RAW_MOUSE, value); } }
        public static FrameQueueLimit FrameQueueLimit { get { return (FrameQueueLimit)AllSettings.GetSetting(FRAME_QUEUE_LIMIT); } set { AllSettings.SetSetting(FRAME_QUEUE_LIMIT, value); } }
        public static bool ShowHelp { get { return (bool)AllSettings.GetSetting(SHOW_HELP); } set { AllSettings.SetSetting(SHOW_HELP, value); } }

        private static Dictionary<string, SettingConverter> conversions = new Dictionary<string, SettingConverter>();

        static Game()
        {
            SettingConverter boolConverter = new SettingConverter((bVal) => (bool)bVal ? 1f : 0f, (fVal) => fVal == 1f, (bVal) => (bool)bVal ? "On" : "Off");

            AllSettings.AddSetting(new FloatSetting(MOUSE_SPEED, 1f));
            conversions.Add(MOUSE_SPEED, new SettingConverter((fVal) => Mathf.Round((float)fVal * 10f) / 10f, (fVal) => fVal));

            AllSettings.AddSetting(new BoolSetting(INVERT_Y, false));
            conversions.Add(INVERT_Y, boolConverter);

            AllSettings.AddSetting(new FloatSetting(FOV, 90f));

            AllSettings.AddSetting(new FloatSetting(VOLUME, 0.5f));
            conversions.Add(VOLUME, new SettingConverter((fVal) => Mathf.Round((float)fVal * 100f) / 100f, (fVal) => fVal));

            AllSettings.AddSetting(new IntSetting(ANISOTROPIC_FILTERING, (int)AnisotropicFiltering.Enable));
            conversions.Add(ANISOTROPIC_FILTERING, new EnumSettingConverter<AnisotropicFiltering>((anVal) => (int)anVal, (fVal) => (AnisotropicFiltering)Mathf.RoundToInt(fVal)));

            AllSettings.AddSetting(new IntSetting(ANTI_ALIASING, (int)AaValue.OFF));
            conversions.Add(ANTI_ALIASING, new EnumSettingConverter<AaValue>((iVal) => (int)iVal, (fVal) => (AaValue)Mathf.RoundToInt(fVal)));

            AllSettings.AddSetting(new IntSetting(TEXTURE_SIZE, (int)TexSize.FULL));
            conversions.Add(TEXTURE_SIZE, new EnumSettingConverter<TexSize>((texVal) => (int)texVal, (fVal) => (TexSize)Mathf.RoundToInt(fVal)));

            AllSettings.AddSetting(new BoolSetting(V_SYNC, false));
            conversions.Add(V_SYNC, boolConverter);

            AllSettings.AddSetting(new BoolSetting(RAW_MOUSE, true));
            conversions.Add(RAW_MOUSE, boolConverter);

            AllSettings.AddSetting(new IntSetting(FRAME_QUEUE_LIMIT, -1));
            conversions.Add(FRAME_QUEUE_LIMIT, new EnumSettingConverter<FrameQueueLimit>((iVal) => (int)iVal == -1 ? 3f : (float)iVal, (fVal) => fVal == 3f ? -1 : (int)fVal));

            AllSettings.AddSetting(new BoolSetting(SHOW_HELP, true));
            conversions.Add(SHOW_HELP, boolConverter);
        }

        public static string GetSettingValueName(string name)
        {
            if (conversions.ContainsKey(name))
                return conversions[name].ToString(AllSettings.GetSetting(name));
            else
                return AllSettings.GetSetting(name).ToString();
        }

        public static float GetSettingFloat(string name)
        {
            if (conversions.ContainsKey(name))
                return conversions[name].ToFloat(AllSettings.GetSetting(name));
            else
                return (float)AllSettings.GetSetting(name);
        }

        public static void SetSettingFloat(string name, float value)
        {
            if (conversions.ContainsKey(name))
                AllSettings.SetSetting(name, conversions[name].FromFloat(value));
            else
                AllSettings.SetSetting(name, value);
        }

        public static void ApplyGraphicsSettings()
        {
            QualitySettings.anisotropicFiltering = (AnisotropicFiltering)AllSettings.GetSetting("Aniso");
            int aa = (int)AllSettings.GetSetting("AA");
            QualitySettings.antiAliasing = aa * aa;
            QualitySettings.masterTextureLimit = (int)AllSettings.GetSetting("TextureSize");
            QualitySettings.vSyncCount = (bool)AllSettings.GetSetting("VSync") ? 1 : 0;
        }
    }
}