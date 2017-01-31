using System;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace Settings
{
    public abstract class NameSetting<T>
    {
        public readonly T value;
        public readonly float sliderValue;
        public readonly string name;

        private static List<NameSetting<T>> myInstances = new List<NameSetting<T>>();

        public static NameSetting<T> FromSlider(float val)
        {
            return myInstances.Find(instance => instance.sliderValue == val);
        }

        protected NameSetting(T value, float sliderValue, string name)
        {
            this.value = value;
            this.sliderValue = sliderValue;
            this.name = name;

            myInstances.Add(this);
        }

        public override string ToString()
        {
            return name;
        }
    }

    public sealed class OnOff : NameSetting<bool>
    {
        public static readonly OnOff ON = new OnOff(true, 1f, "On");
        public static readonly OnOff OFF = new OnOff(false, 0f, "Off");

        private OnOff(bool value, float sliderVal, string name) : base(value, sliderVal, name) { }
    }

    public sealed class AnisoFilterSetting : NameSetting<AnisotropicFiltering>
    {
        public static readonly AnisoFilterSetting OFF = new AnisoFilterSetting(AnisotropicFiltering.Disable, 0f, "Off");
        public static readonly AnisoFilterSetting ON = new AnisoFilterSetting(AnisotropicFiltering.Enable, 1f, "On");
        public static readonly AnisoFilterSetting FORCE = new AnisoFilterSetting(AnisotropicFiltering.ForceEnable, 2f, "Force On");

        private AnisoFilterSetting(AnisotropicFiltering value, float sliderVal, string name) : base(value, sliderVal, name) { }
    }

    public sealed class AntiAliasingSetting : NameSetting<int>
    {
        public static readonly AntiAliasingSetting X8 = new AntiAliasingSetting(8, 3f, "8x MSAA");
        public static readonly AntiAliasingSetting X4 = new AntiAliasingSetting(4, 2f, "4x MSAA");
        public static readonly AntiAliasingSetting X2 = new AntiAliasingSetting(2, 1f, "2x MSAA");
        public static readonly AntiAliasingSetting OFF = new AntiAliasingSetting(0, 0f, "Off");

        private AntiAliasingSetting(int value, float sliderVal, string name) : base(value, sliderVal, name) { }
    }

    public sealed class TextureSizeSetting : NameSetting<int>
    {
        public static readonly TextureSizeSetting FULL = new TextureSizeSetting(0, 1f, "Full");
        public static readonly TextureSizeSetting HALF = new TextureSizeSetting(1, 0.5f, "Half");
        public static readonly TextureSizeSetting QUARTER = new TextureSizeSetting(2, 0f, "Quarter");

        private TextureSizeSetting(int value, float sliderVal, string name) : base(value, sliderVal, name) { }
    }

    public sealed class VSyncSetting : NameSetting<int>
    {
        public static readonly VSyncSetting OFF = new VSyncSetting(0, 0f, "Off");
        public static readonly VSyncSetting ON = new VSyncSetting(1, 1f, "On");
        public static readonly VSyncSetting DOUBLE = new VSyncSetting(2, 2f, "Two Frames");

        private VSyncSetting(int value, float sliderVal, string name) : base(value, sliderVal, name) { }
    }

    public sealed class MaxQueuedFramesSetting : NameSetting<int>
    {
        public static readonly MaxQueuedFramesSetting OFF = new MaxQueuedFramesSetting(0, 1f, "Off");
        public static readonly MaxQueuedFramesSetting ON = new MaxQueuedFramesSetting(1, 0.5f, "On");
        public static readonly MaxQueuedFramesSetting DOUBLE = new MaxQueuedFramesSetting(2, 0f, "Two Frames");

        private MaxQueuedFramesSetting(int value, float sliderVal, string name) : base(value, sliderVal, name) { }
    }

    public class GameSettings : ICloneable
    {
        private static GameSettings singletonInstance;

        public static GameSettings SingletonInstance
        {
            get
            {
                if (singletonInstance == null)
                    Reload();
                return singletonInstance;
            }
            set { singletonInstance = value; }
        }

        public static void Reload()
        {
            singletonInstance = new GameSettings();
        }

        // Since this is cloned, only use structs/primitive types/unchangable values to avoid having to implement deep cloning
        public NameSetting<AnisotropicFiltering> AnisotropicFiltering { get; set; }
        public NameSetting<int> AntiAliasing { get; set; }
        public NameSetting<int> TextureSize { get; set; }
        public NameSetting<int> VSyncSetting { get; set; }
        public NameSetting<int> MaxQueuedFrames { get; set; }
        public float Fov { get; set; }
        public float MouseSpeed { get; set; }
        public NameSetting<bool> InvertY { get; set; }
        public NameSetting<bool> RawMouse { get; set; }
        public float Volume { get; set; }

        private GameSettings()
        {
            AnisotropicFiltering = AnisoFilterSetting.ON;
            AntiAliasing = AntiAliasingSetting.OFF;
        }

        public void ApplyGraphicsSettings()
        {
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.value;
            QualitySettings.antiAliasing = AntiAliasing.value;
            QualitySettings.masterTextureLimit = TextureSize.value;
            QualitySettings.vSyncCount = VSyncSetting.value;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}