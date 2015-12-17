using System;
using System.Collections.Generic;
using UnityEngine;

namespace Settings
{
    public static class AllSettings
    {
        private static Dictionary<string, Setting> settings = new Dictionary<string, Setting>();

        static AllSettings()
        {
            Game.Initialize();
        }

        public static void AddSetting(Setting setting)
        {
            settings.Add(setting.Name, setting);
        }

        public static object GetSetting(string name)
        {
            return settings[name].GetValue();
        }

        public static void SetSetting(string name, object value)
        {
            settings[name].SetValue(value);
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

        public abstract object GetValue();
        public abstract void SetValue(object value);
        public abstract void Load();
        public abstract void Save();
        public abstract void Delete();
    }

    abstract class Setting<T> : Setting
    {
        protected abstract void Save(T value);

        protected T defaultValue;

        public Setting(string name, T defaultValue)
        {
            Name = name;
            Value = defaultValue;
            this.defaultValue = defaultValue;
        }

        public override object GetValue()
        {
            return Value;
        }

        public override void SetValue(object value)
        {
            Value = (T)value;
        }

        public T Value
        {
            get { return val; }
            set { val = (T)value; }
        }

        public override void Save()
        {
            Save((T)Value);
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
    }

    class SettingConverter
    {
        protected Func<object, float> toFloat;
        protected Func<float, object> fromFloat;
        protected Func<object, string> toString;

        public SettingConverter(Func<object, float> toFloat, Func<float, object> fromFloat, Func<object, string> toString = null)
        {
            this.toFloat = toFloat;
            this.fromFloat = fromFloat;
            this.toString = toString;
        }

        public virtual float ToFloat(object value)
        {
            return toFloat(value);
        }

        public virtual object FromFloat(float value)
        {
            return fromFloat(value);
        }

        public virtual string ToString(object value)
        {
            if (toString != null)
                return toString(value);
            else
                return value.ToString();
        }
    }

    class EnumSettingConverter<T> : SettingConverter
    {
        public EnumSettingConverter(Func<object, float> toFloat, Func<float, object> fromFloat) : base(toFloat, fromFloat, (obj) => Enum.GetName(typeof(T), obj)) { }
    }
}
