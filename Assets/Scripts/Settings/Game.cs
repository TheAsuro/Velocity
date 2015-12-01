using UnityEngine;
using System;
using System.Collections.Generic;

namespace Settings
{
    public enum AAValue
    {
        Off = 0,
        X2 = 2,
        X4 = 4,
        X8 = 8
    }

    public enum TexSize
    {
        Full = 0,
        Half = 1,
        Quarter = 2
    }

    public enum FrameQueueLimit
    {
        Zero = 0,
        One = 1,
        Two = 2,
        Unlimited = -1
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

        public static float MouseSpeed { get { return AllSettings.GetSetting<float>(MOUSE_SPEED); } set { AllSettings.SetSetting(MOUSE_SPEED, value); } }
        public static bool InvertY { get { return AllSettings.GetSetting<bool>(INVERT_Y); } set { AllSettings.SetSetting(INVERT_Y, value); } }
        public static float Fov { get { return AllSettings.GetSetting<float>(FOV); } set { AllSettings.SetSetting(FOV, value); } }
        public static float Volume { get { return AllSettings.GetSetting<float>(VOLUME); } set { AllSettings.SetSetting(VOLUME, value); } }
        public static AnisotropicFiltering AnisotropicFiltering { get { return (AnisotropicFiltering)AllSettings.GetSetting<int>(ANISOTROPIC_FILTERING); } set { AllSettings.SetSetting(ANISOTROPIC_FILTERING, (int)value); } }
        public static AAValue AntiAliasing { get { return (AAValue)AllSettings.GetSetting<int>(ANTI_ALIASING); } set { AllSettings.SetSetting(ANTI_ALIASING, value); } }
        public static TexSize TextureSize { get { return (TexSize)AllSettings.GetSetting<int>(TEXTURE_SIZE); } set { AllSettings.SetSetting(TEXTURE_SIZE, value); } }
        public static bool VSync { get { return AllSettings.GetSetting<bool>(V_SYNC); } set { AllSettings.SetSetting(V_SYNC, value); } }
        public static bool RawMouse { get { return AllSettings.GetSetting<bool>(RAW_MOUSE); } set { AllSettings.SetSetting(RAW_MOUSE, value); } }
        public static FrameQueueLimit FrameQueueLimit { get { return (FrameQueueLimit)AllSettings.GetSetting<int>(FRAME_QUEUE_LIMIT); } set { AllSettings.SetSetting(FRAME_QUEUE_LIMIT, value); } }
        public static bool ShowHelp { get { return AllSettings.GetSetting<bool>(SHOW_HELP); } set { AllSettings.SetSetting(SHOW_HELP, value); } }

        private static Dictionary<string, SettingConverter> conversions = new Dictionary<string, SettingConverter>();

        static Game()
        {
            AllSettings.AddSetting(new FloatSetting(MOUSE_SPEED, 1f));
            AllSettings.AddSetting(new BoolSetting(INVERT_Y, false));
            AllSettings.AddSetting(new FloatSetting(FOV, 90f));
            AllSettings.AddSetting(new FloatSetting(VOLUME, 0.5f));
            AllSettings.AddSetting(new IntSetting(ANISOTROPIC_FILTERING, (int)AnisotropicFiltering.Enable));
            AllSettings.AddSetting(new IntSetting(ANTI_ALIASING, (int)AAValue.Off));
            conversions.Add(ANTI_ALIASING, new SettingConverter((iVal) => (int)iVal / 2f, (fVal) => (int)fVal * 2));
            AllSettings.AddSetting(new IntSetting(TEXTURE_SIZE, (int)TexSize.Full));
            AllSettings.AddSetting(new BoolSetting(V_SYNC, false));
            AllSettings.AddSetting(new BoolSetting(RAW_MOUSE, true));
            AllSettings.AddSetting(new IntSetting(FRAME_QUEUE_LIMIT, -1));
            conversions.Add(FRAME_QUEUE_LIMIT, new SettingConverter((iVal) => (int)iVal == -1 ? 3f : (float)iVal, (fVal) => fVal == 3f ? -1 : (int)fVal));
            AllSettings.AddSetting(new BoolSetting(SHOW_HELP, true));

            AllSettings.LoadSettings();
        }

        public static float GetSettingFloat<T>(string name)
        {
            if (conversions.ContainsKey(name))
                return conversions[name].ToFloat(AllSettings.GetSetting<T>(name));
            else
                
        }

        public static void SetSettingFloat<T>(string name, float value)
        {
            
        }

        public static void ApplyGraphicsSettings()
        {
            QualitySettings.anisotropicFiltering = (AnisotropicFiltering)AllSettings.GetSetting<int>("Aniso");
            QualitySettings.antiAliasing = AllSettings.GetSetting<int>("AA");
            QualitySettings.masterTextureLimit = AllSettings.GetSetting<int>("TextureSize");
            QualitySettings.vSyncCount = AllSettings.GetSetting<bool>("VSync") ? 1 : 0;
        }
    }

    public static class AllSettings
    {
        private static Dictionary<string, Setting> settings = new Dictionary<string, Setting>();

        public static void AddSetting(Setting setting)
        {
            settings.Add(setting.Name, setting);
        }

        public static T GetSetting<T>(string name)
        {
            return ((Setting<T>)settings[name]).Value;
        }

        public static void SetSetting<T>(string name, T value)
        {
            ((Setting<T>)settings[name]).Value = value;
        }

        public static void LoadSettings()
        {
            foreach (var pair in settings)
            {
                pair.Value.Load();
            }
        }

        public static void SaveSettings()
        {
            foreach (var pair in settings)
            {
                pair.Value.Save();
            }
        }

        public static void DeleteSettings()
        {
            foreach (var pair in settings)
            {
                pair.Value.Delete();
            }
        }
    }

    public abstract class Setting
    {
        public string Name { get; protected set; }

        public abstract void Load();
        public abstract void Save();
        public abstract void Delete();
        public abstract float GetSliderValue();
        public abstract void SetFromSliderValue(float value);
    }

    abstract class Setting<T> : Setting
    {
        public T Value
        {
            get { return val; }
            set { val = value; }
        }

        protected abstract void Save(T value);

        protected T defaultValue;
        protected Func<T, float> convertToSlider;
        protected Func<float, T> convertFromSlider;

        public Setting(string name, T defaultValue)
        {
            Name = name;
            Value = defaultValue;
            this.defaultValue = defaultValue;
        }

        public override void Save()
        {
            Save(Value);
        }

        public override void Delete()
        {
            PlayerPrefs.DeleteKey(Name);
        }

        private T val;
    }

    class BoolSetting : Setting<bool>
    {
        public BoolSetting(string name, bool defaultValue) : base(name, defaultValue) { }

        public override void Load()
        {
            if (PlayerPrefs.HasKey(Name))
                Value = PlayerPrefs.GetInt(Name) == 1;
            else
                Value = defaultValue;
        }

        protected override void Save(bool value)
        {
            PlayerPrefs.SetInt(Name, Value ? 1 : 0);
        }

        public override float GetSliderValue()
        {
            if (convertToSlider == null)
                return Value ? 1f : 0f;
            else
                return convertToSlider(Value);
        }

        public override void SetFromSliderValue(float value)
        {
            if (convertFromSlider == null)
                Value = value == 1f;
            else
                Value = convertFromSlider(value);
        }
    }

    class IntSetting : Setting<int>
    {
        public IntSetting(string name, int defaultValue) : base(name, defaultValue) { }

        public override void Load()
        {
            if (PlayerPrefs.HasKey(Name))
                Value = PlayerPrefs.GetInt(Name);
            else
                Value = defaultValue;
        }

        protected override void Save(int value)
        {
            PlayerPrefs.SetInt(Name, Value);
        }

        public override float GetSliderValue()
        {
            if (convertToSlider == null)
                return (float)Value;
            else
                return convertToSlider(Value);
        }

        public override void SetFromSliderValue(float value)
        {
            if (convertFromSlider == null)
                Value = Mathf.FloorToInt(value);
            else
                Value = convertFromSlider(value);
        }
    }

    class FloatSetting : Setting<float>
    {
        public FloatSetting(string name, float defaultValue) : base(name, defaultValue) { }

        public override void Load()
        {
            if (PlayerPrefs.HasKey(Name))
                Value = PlayerPrefs.GetFloat(Name);
            else
                Value = defaultValue;
        }

        protected override void Save(float value)
        {
            PlayerPrefs.SetFloat(Name, Value);
        }

        public override float GetSliderValue()
        {
            if (convertToSlider == null)
                return Value;
            else
                return convertToSlider(Value);
        }

        public override void SetFromSliderValue(float value)
        {
            if (convertFromSlider == null)
                Value = value;
            else
                Value = convertFromSlider(value);
        }
    }

    class StringSetting : Setting<string>
    {
        public StringSetting(string name, string defaultValue) : base(name, defaultValue) { }

        public override void Load()
        {
            if (PlayerPrefs.HasKey(Name))
                Value = PlayerPrefs.GetString(Name);
            else
                Value = defaultValue;
        }

        protected override void Save(string value)
        {
            PlayerPrefs.SetString(Name, Value);
        }

        public override float GetSliderValue()
        {
            if (convertToSlider == null)
                throw new InvalidOperationException();
            else
                return convertToSlider(Value);
        }

        public override void SetFromSliderValue(float value)
        {
            if (convertFromSlider == null)
                throw new InvalidOperationException();
            else
                Value = convertFromSlider(value);
        }
    }

    class SettingConverter
    {
        protected Func<object, float> toFloat;
        protected Func<float, object> fromFloat;
        
        public SettingConverter(Func<object, float> toFloat, Func<float, object> fromFloat)
        {
            this.toFloat = toFloat;
            this.fromFloat = fromFloat;
        }

        public float ToFloat(object value)
        {
            return toFloat(value);
        }

        public object FromFloat(float value)
        {
            return fromFloat(value);
        }
    }
}