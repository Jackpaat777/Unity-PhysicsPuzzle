using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class MainManager : MonoBehaviour
{
    // �����̴��� ���� �������� ����
    public AudioMixer audioMixer;
    public Slider sliderBGM;
    public Slider sliderSFX;

    // ��ư Ŭ�� ȿ����
    public AudioSource sfxPlayer;
    public AudioClip buttonClip;
    public AudioClip levelClip;

    void Awake()
    {
        // ����� �ͼ��� ���� �Ҹ��� ��ȯ
        float valueBGM, valueSFX;
        bool resultBGM = audioMixer.GetFloat("BGM", out valueBGM);
        bool resultSFX = audioMixer.GetFloat("SFX", out valueSFX);

        // slider�� ��ġ ����
        if (resultBGM)
            sliderBGM.value = valueBGM;
        if (resultSFX)
            sliderSFX.value = valueSFX;
    }

    // BGM, SFX �����̴�
    public void BGMControl()
    {
        float sound = sliderBGM.value;

        if (sound == -40f) audioMixer.SetFloat("BGM", -80);
        else audioMixer.SetFloat("BGM", sound);
    }

    public void SFXControl()
    {
        float sound = sliderSFX.value;

        if (sound == -40f) audioMixer.SetFloat("SFX", -80);
        else audioMixer.SetFloat("SFX", sound);
    }

    public void ResetAudio()
    {
        sliderBGM.value = -20;
        sliderSFX.value = -20;
        audioMixer.SetFloat("BGM", -20);
        audioMixer.SetFloat("SFX", -20);
    }

    // ����ȭ�� ��ư
    public void GameStart()
    {
        SceneManager.LoadScene("InGame");
    }

    public void GameExit()
    {
        Application.Quit();
    }

    // ȿ���� ���
    public void SFXButton()
    {
        sfxPlayer.clip = buttonClip;
        sfxPlayer.Play();
    }

    public void SFXLevel()
    {
        sfxPlayer.clip = levelClip;
        sfxPlayer.Play();
    }
}
