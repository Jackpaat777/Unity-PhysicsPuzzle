using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("--------------[ Core ]")]
    // 핵심 변수
    public bool isOver;
    public int score;
    public int maxLevel;

    [Header("--------------[ Object Pooling ]")]
    // 동글 관련 변수
    public GameObject donglePrefab;
    public Transform dongleGroup;
    public List<Dongle> donglePool;
    // 이펙트 관련 함수
    public GameObject effectPrefab;
    public Transform effectGroup;
    public List<ParticleSystem> effectPool;
    // 오브젝트 풀링 관련 함수
    [Range(1, 30)]
    public int poolSize;
    public int poolCursor;
    public Dongle lastDongle;

    [Header("--------------[ Audio ]")]
    // BGM 관련 함수
    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer;
    public AudioClip[] sfxClip;
    public enum Sfx { LevelUp, Next, Attach, Button, Over };
    int sfxCursor;

    [Header("--------------[ UI ]")]
    public GameObject startGroup;
    public GameObject endGroup;
    public TMP_Text scoreText;
    public TMP_Text maxScoreText;
    public TMP_Text subScoreText;

    [Header("--------------[ ETC ]")]
    public GameObject line;
    public GameObject bottom;



    void Awake()
    {
        // 프레임을 60으로 설정
        Application.targetFrameRate = 60;

        donglePool = new List<Dongle>();
        effectPool = new List<ParticleSystem>();
        for (int i = 0; i < poolSize; i++)
        {
            MakeDongle();
        }

        // 최고점수가 없을 경우 0을 넣어줌
        if (!PlayerPrefs.HasKey("MaxScore"))
            PlayerPrefs.SetInt("MaxScore", 0);
        maxScoreText.text = "Max " +  PlayerPrefs.GetInt("MaxScore").ToString("D5");
    }

    public void GameStart()
    {
        // 오브젝트 활성화
        line.SetActive(true);
        bottom.SetActive(true);
        scoreText.gameObject.SetActive(true);
        maxScoreText.gameObject.SetActive(true);
        startGroup.SetActive(false);

        // 사운드 플레이
        bgmPlayer.Play();
        SFXPlay(Sfx.Button);

        // 게임 시작
        Invoke("NextDongle",1.5f);
    }

    Dongle MakeDongle()
    {
        // 이펙트 생성
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        instantEffectObj.name = "Effect " + effectPool.Count;
        // return을 Dongle로 하기 위해 GetComponent를 해줌
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect);

        // 동글 생성
        // Instantiate함수에서 dongleGroup의 자식으로 생성하도록
        GameObject instantDongleObj = Instantiate(donglePrefab, dongleGroup);
        instantDongleObj.name = "Dongle " + donglePool.Count;
        // return을 Dongle로 하기 위해 GetComponent를 해줌
        Dongle instantDongle = instantDongleObj.GetComponent<Dongle>();
        // 동글 스크립트의 effect를 넣어주기 (초기화)
        instantDongle.effect = instantEffect;
        instantDongle.gameManager = this;
        donglePool.Add(instantDongle);

        return instantDongle;
    }

    Dongle GetDongle()
    {
        for (int i = 0; i < donglePool.Count; i++)
        {
            poolCursor = (poolCursor + 1) % donglePool.Count;   // 동글풀의 크기가 10이라면 0~9까지
            if (!donglePool[poolCursor].gameObject.activeSelf)
                return donglePool[poolCursor];
        }

        // pool이 넘치도록 동글이 생성되었을 경우에는 MakeDongle을 통해 새로 만들어주기
        return MakeDongle();
    }

    void NextDongle()
    {
        if (isOver)
            return;

        // 다음 동글에 동글 설정 넣어주기
        lastDongle = GetDongle();
        lastDongle.level = Random.Range(0, maxLevel);   // 랜덤값의 최대값을 점점 크게 만들어주기
        lastDongle.gameObject.SetActive(true);          // 프리펩을 꺼놓은 상태에서 설정을 마친 뒤 그 때 활성화

        SFXPlay(Sfx.Next); // Next 효과음 재생
        // 코루틴을 실행하는 방법
        StartCoroutine("WaitNext");
    }

    // 코루틴 : 유니티에게 로직제어를 넘기는 함수
    // 이전에 게임제작할 때 Invoke대신 사용했던 함수
    IEnumerator WaitNext()
    {
        // 아직 동글이 놓아지지 않은 경우
        while (lastDongle != null)
        {
            // yield return 을 사용하지 않으면 무한루프에 빠질 위험
            yield return null;
        }

        // 이 다음 코드를 2.5초 뒤에 실행하도록
        yield return new WaitForSeconds(2.5f);

        NextDongle();
    }
    public void TouchDown()
    {
        if (lastDongle == null)
            return;

        lastDongle.Drag();
    }

    public void TouchUp()
    {
        if (lastDongle == null)
            return;

        lastDongle.Drop();
        // 동글을 놓았을 경우 lastDongle을 null로 갱신
        lastDongle = null;
    }

    public void GameOver()
    {
        if (isOver)
            return;

        isOver = true;

        StartCoroutine("GameOverRoutine");
    }

    IEnumerator GameOverRoutine()
    {
        // 장면 안에 활성화된 동글 가져오기
        // FindObjectsOfType<T> : 화면상의 T타입을 가진 오브젝트를 가져오는 함수(VirticalShooting에서도 다룸)
        // >> GameObject 안에 있는 함수이지만 GameObject는 생략 가능
        Dongle[] dongles = FindObjectsOfType<Dongle>();

        // 지우기 전 모든 동글의 물리 효과 무효화
        // 한번에 실행하기 때문에 yield는 없어도 됨
        for (int i = 0; i < dongles.Length; i++)
            dongles[i].rigid.simulated = false;

        // 동글을 하나씩 접근해서 지우기
        for (int i = 0; i < dongles.Length; i++)
        {
            // 기존 함수는 다른 동글의 위치로 이동하는 형식이지만 현재는 위치와 관계없이 그냥 숨기는 역할만 하고싶기 때문에 인자값에 화면 바깥에 있는 위치값을 넣어줌
            dongles[i].Hide(Vector3.up * 100);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(1f);
        // 최고점수 갱신
        int maxScore = Mathf.Max(score, PlayerPrefs.GetInt("MaxScore"));
        PlayerPrefs.SetInt("MaxScore", maxScore);

        // 게임오버 UI 표시
        subScoreText.text = "점수 : " + score.ToString();
        endGroup.SetActive(true);

        bgmPlayer.Stop();   // 게임오버 시 BGM 멈춤
        SFXPlay(Sfx.Over);  // Over 효과음 재생
    }

    // 재시작 버튼을 누를 경우
    public void Reset()
    {
        SFXPlay(Sfx.Button);
        StartCoroutine("ResetCoroutine");
    }

    IEnumerator ResetCoroutine()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(0);
    }

    public void SFXPlay(Sfx type)
    {
        switch (type)
        {
            case Sfx.LevelUp:
                sfxPlayer[sfxCursor].clip = sfxClip[Random.Range(0, 3)];
                break;
            case Sfx.Next:
                sfxPlayer[sfxCursor].clip = sfxClip[3];
                break;
            case Sfx.Attach:
                sfxPlayer[sfxCursor].clip = sfxClip[4];
                break;
            case Sfx.Button:
                sfxPlayer[sfxCursor].clip = sfxClip[5];
                break;
            case Sfx.Over:
                sfxPlayer[sfxCursor].clip = sfxClip[6];
                break;
        }

        sfxPlayer[sfxCursor].Play();
        // sfxCursor값은 0, 1, 2만 나오도록
        sfxCursor = (sfxCursor + 1) % sfxPlayer.Length;
    }

    void Update()
    {
        // 모바일에서 Cancel은 뒤로가기 버튼
        if (Input.GetButtonDown("Cancel"))
            Application.Quit();
    }

    // Update 종료 후 실행되는 생명주기 함수
    void LateUpdate()
    {
        // 스코어 UI 업데이트
        scoreText.text = "Score " + score.ToString("D5");
    }
}
