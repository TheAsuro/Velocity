using UnityEngine;
using System.Collections;

public class SettingsSlider : MonoBehaviour
{
	public UnityEngine.UI.Text valueDisplay;
	public SliderType sliderType;

	public enum SliderType
	{
		Volume,
		Sensitivity,
		Fov,
		Vsync,
		Lighting,
		AntiAliasing,
		AnisotropicFiltering,
		TextureSize
	}

	void Start()
	{
		UnityEngine.UI.Slider.SliderEvent myEvent = new UnityEngine.UI.Slider.SliderEvent();
		myEvent.AddListener(OnValueChanged);
		GetComponent<UnityEngine.UI.Slider>().onValueChanged = myEvent;
	}

	void OnValueChanged(float value)
	{
		float correctValue = value;

		switch(sliderType)
		{
			case SliderType.Volume:
				correctValue = floor(value, 2);
				valueDisplay.text = correctValue.ToString();
				GameInfo.info.volume = correctValue;
				break;
			case SliderType.Sensitivity:
				correctValue = floor(value, 1);
				valueDisplay.text = correctValue.ToString();
				GameInfo.info.mouseSpeed = correctValue;
				break;
			case SliderType.Fov:
				valueDisplay.text = correctValue.ToString();
				GameInfo.info.fov = correctValue;
				break;
			case SliderType.Vsync:
				valueDisplay.text = translateFloat(correctValue);
				GameInfo.info.vsyncLevel = correctValue;
				break;
			case SliderType.Lighting:
				valueDisplay.text = correctValue.ToString();
				GameInfo.info.lightingLevel = correctValue;
				break;
			case SliderType.AntiAliasing:
				correctValue = roundAA(value);
				valueDisplay.text = correctValue.ToString();
				GameInfo.info.antiAliasing = correctValue;
				break;
			case SliderType.AnisotropicFiltering:
				valueDisplay.text = translateFloat(correctValue);
				GameInfo.info.anisotropicFiltering = correctValue == 1f;
				break;
			case SliderType.TextureSize:
				valueDisplay.text = translateTextureSize(correctValue);
				GameInfo.info.textureSize = correctValue;
				break;
		}
	}

	//Returns rounded value of a float
	public static float floor(float input, int decimalsAfterPoint)
	{
		if(decimalsAfterPoint <= 0)
		{
			return Mathf.Round(input);
		}
		else
		{
			float temp = input * Mathf.Pow(10, decimalsAfterPoint);
			return Mathf.Round(temp) / Mathf.Pow(10, decimalsAfterPoint);
		}
	}

	//Return rounded value acceptable for AA
	public static float roundAA(float input)
	{
		if(input < 1f)
		{
			return 0f;
		}
		else if(input < 3f)
		{
			return 2f;
		}
		else if(input < 6f)
		{
			return 4f;
		}
		else
		{
			return 8f;
		}
	}

	//Returns On/Off for float input
	public static string translateFloat(float input)
	{
		if(input == 0f)
		{
			return "Off";
		}
		return "On";
	}

	public static string translateTextureSize(float input)
	{
		if(input == 2)
		{
			return "Full";
		}
		else if(input == 1)
		{
			return "Half";
		}
		else if(input == 0)
		{
			return "Quarter";
		}
		return "Error";
	}
}
