using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SettingsSlider : MonoBehaviour
{
    public Slider slider;
    public Text display;
    public SettingType type;
    public int roundDigits = -1;
    public string[] valueNames;
    public float[] valueMeaning;
    public string prefix = "";
    public string postfix = "";

    public enum SettingType
    {
        Volume,
        Fov,
        AntiAliasing,
        AnisotropicFiltering,
        TextureSize,
        VSync,
        Lighting,
        MouseSpeed,
        YInvert,
        DemoPerspective,
        RawMouse
    }

    void Awake()
    {
        slider.onValueChanged.AddListener(OnValueChanged);
        MainMenu.SettingsOpened += OnSettingsOpened;
        UpdateValues();
    }

    private void OnValueChanged(float value)
    {
        string roundStr = value.ToString();
        float savedValue = value;

        if(roundDigits >= 0)
        {
            roundStr = System.Math.Round(value, roundDigits).ToString();
        }

        if(valueNames.Length > 0)
        {
            display.text = prefix + valueNames[(int)value] + postfix;
        }
        else
        {
            display.text = prefix + roundStr + postfix;
        }

        if(valueMeaning.Length > 0)
        {
            savedValue = valueMeaning[(int)value];
        }

        switch(type)
        {
            case SettingType.Volume:
                GameInfo.info.volume = savedValue; break;
            case SettingType.Fov:
                GameInfo.info.fov = savedValue; break;
            case SettingType.AntiAliasing:
                GameInfo.info.antiAliasing = savedValue; break;
            case SettingType.AnisotropicFiltering:
                GameInfo.info.anisotropicFiltering = savedValue; break;
            case SettingType.Lighting:
                GameInfo.info.lightingLevel = savedValue; break;
            case SettingType.TextureSize:
                GameInfo.info.textureSize = savedValue; break;
            case SettingType.VSync:
                GameInfo.info.vsyncLevel = savedValue; break;
            case SettingType.MouseSpeed:
                GameInfo.info.mouseSpeed = savedValue; break;
            case SettingType.YInvert:
                GameInfo.info.invertYInput = savedValue; break;
            case SettingType.DemoPerspective:
                GameInfo.info.demoPerspective = savedValue; break;
            case SettingType.RawMouse:
                GameInfo.info.rawMouse = savedValue; break;
            default:
                print("fok"); break;
        }
    }

    public void OnSettingsOpened(object sender, System.EventArgs e)
    {
        UpdateValues();
    }

    private void UpdateValues()
    {
        float newValue = -1f;

        switch(type)
        {
            case SettingType.Volume:
                newValue = GameInfo.info.volume; break;
            case SettingType.Fov:
                newValue = GameInfo.info.fov; break;
            case SettingType.AntiAliasing:
                newValue = GameInfo.info.antiAliasing; break;
            case SettingType.AnisotropicFiltering:
                newValue = GameInfo.info.anisotropicFiltering; break;
            case SettingType.Lighting:
                newValue = GameInfo.info.lightingLevel; break;
            case SettingType.VSync:
                newValue = GameInfo.info.vsyncLevel; break;
            case SettingType.MouseSpeed:
                newValue = GameInfo.info.mouseSpeed; break;
            case SettingType.YInvert:
                newValue = GameInfo.info.invertYInput; break;
            case SettingType.TextureSize:
                newValue = GameInfo.info.textureSize; break;
            case SettingType.DemoPerspective:
                newValue = GameInfo.info.demoPerspective; break;
            case SettingType.RawMouse:
                newValue = GameInfo.info.rawMouse; break;
        }

        newValue = ConvertToSlider(newValue);

        slider.value = newValue;
        slider.onValueChanged.Invoke(newValue);
    }

    private float ConvertToSlider(float savedValue)
    {
        //If the slider value is the same as it's meaning, return it directly
        if (valueMeaning.Length == 0)
            return savedValue;

        //Translate the internal value to the slider position
        for(int i = 0; i < valueMeaning.Length; i++)
        {
            if(valueMeaning[i] == savedValue)
            {
                return (float)i;
            }
        }

        return -1;
    }
}
