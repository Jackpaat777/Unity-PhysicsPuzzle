using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class MainManager : MonoBehaviour
{
    // 슬라이더를 통한 볼륨조절 변수
    public AudioMixer audioMixer;
    public Slider sliderBGM;
    public Slider sliderSFX;

    // 버튼 클릭 효과음
    public AudioSource sfxPlayer;
    public AudioClip buttonClip;
    public AudioClip levelClip;

    void Awake()
    {
        // 오디오 믹서의 현재 소리값 반환
        float valueBGM, valueSFX;
        bool resultBGM = audioMixer.GetFloat("BGM", out valueBGM);
        bool resultSFX = audioMixer.GetFloat("SFX", out valueSFX);

        // slider의 위치 수정
        if (resultBGM)
            sliderBGM.value = valueBGM;
        if (resultSFX)
            sliderSFX.value = valueSFX;
    }

    // BGM, SFX 슬라이더
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

    // 메인화면 버튼
    public void GameStart()
    {
        SceneManager.LoadScene("InGame");
    }

    public void GameExit()
    {
        Application.Quit();
    }

    // 효과음 재생
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
