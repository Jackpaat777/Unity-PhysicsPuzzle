using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainManager : MonoBehaviour
{
    public AudioSource sfxPlayer;
    public AudioClip sfxClip;

    public void GameStart()
    {
        SFXPlay();
        SceneManager.LoadScene("InGame");
    }

    public void GameSetting()
    {
        SFXPlay();
    }

    public void GameExit()
    {
        SFXPlay();
        Application.Quit();
    }

    void SFXPlay()
    {
        sfxPlayer.clip = sfxClip;
        sfxPlayer.Play();
    }
}
