using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    public const string VolumeProperty = "Volume"; //Exposed parameter name is mixer group name + this variable (Ex: (Mixer Group) Music + VolumeProperty;
    public const float DefaultVolume = 0.5f;

    [SerializeField] private Slider m_Slider;
    [SerializeField] private AudioMixerGroup m_AudioMixerGroup;
    //Instead of setting the slider min / max values to -80 and 20, set them to min 0.0001 and max 1.

    private void OnEnable()
    {
        string parameterName = GetExposedMixerGroupVolumeParameterName(m_AudioMixerGroup.name);
        float mixerVolume = PlayerPrefs.GetFloat(parameterName, DefaultVolume);

        m_Slider.value = mixerVolume;
        m_Slider.onValueChanged.AddListener(delegate { ChangeVolume(); });
    }

    private void Start()
    {
        ChangeVolume();
    }

    private void OnDisable()
    {
        m_Slider.onValueChanged.RemoveListener(delegate { ChangeVolume(); });
    }

    private void ChangeVolume()
    {
        SetAudioMixerVolume(m_AudioMixerGroup, m_Slider.value);
    }

    public void SetAudioMixerVolume(AudioMixerGroup group, float volume)
    {
        //Instead of setting the slider min / max values to -80 and 20, set them to min 0.0001 and max 1.
        float valueToSet = Mathf.Log10(volume) * 20f;

        string parameterName = GetExposedMixerGroupVolumeParameterName(group.name);
        group.audioMixer.SetFloat(parameterName, valueToSet);

        PlayerPrefs.SetFloat(parameterName, volume);
        //Debug.Log("Set " + group.name + " volume to: " + volume);
    }

    public static string GetExposedMixerGroupVolumeParameterName(string groupName)
    {
        return groupName + VolumeProperty;
    }
}
