using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("--------------[ Core ]")]
    // �ٽ� ����
    public bool isOver;
    public int score;
    public int maxLevel;

    [Header("--------------[ Object Pooling ]")]
    // ���� ���� ����
    public GameObject donglePrefab;
    public Transform dongleGroup;
    public List<Dongle> donglePool;
    // ����Ʈ ���� �Լ�
    public GameObject effectPrefab;
    public Transform effectGroup;
    public List<ParticleSystem> effectPool;
    // ������Ʈ Ǯ�� ���� �Լ�
    [Range(1, 30)]
    public int poolSize;
    public int poolCursor;
    public Dongle lastDongle;

    [Header("--------------[ Audio ]")]
    // BGM ���� �Լ�
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
        // �������� 60���� ����
        Application.targetFrameRate = 60;

        donglePool = new List<Dongle>();
        effectPool = new List<ParticleSystem>();
        for (int i = 0; i < poolSize; i++)
        {
            MakeDongle();
        }

        // �ְ������� ���� ��� 0�� �־���
        if (!PlayerPrefs.HasKey("MaxScore"))
            PlayerPrefs.SetInt("MaxScore", 0);
        maxScoreText.text = "Max " +  PlayerPrefs.GetInt("MaxScore").ToString("D5");

        // ���� ����
        Invoke("NextDongle", 1.5f);
    }

    public void GameStart()
    {
        // ���� �÷���
        SFXPlay(Sfx.Button);

    }

    Dongle MakeDongle()
    {
        // ����Ʈ ����
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        instantEffectObj.name = "Effect " + effectPool.Count;
        // return�� Dongle�� �ϱ� ���� GetComponent�� ����
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect);

        // ���� ����
        // Instantiate�Լ����� dongleGroup�� �ڽ����� �����ϵ���
        GameObject instantDongleObj = Instantiate(donglePrefab, dongleGroup);
        instantDongleObj.name = "Dongle " + donglePool.Count;
        // return�� Dongle�� �ϱ� ���� GetComponent�� ����
        Dongle instantDongle = instantDongleObj.GetComponent<Dongle>();
        // ���� ��ũ��Ʈ�� effect�� �־��ֱ� (�ʱ�ȭ)
        instantDongle.effect = instantEffect;
        instantDongle.gameManager = this;
        donglePool.Add(instantDongle);

        return instantDongle;
    }

    Dongle GetDongle()
    {
        for (int i = 0; i < donglePool.Count; i++)
        {
            poolCursor = (poolCursor + 1) % donglePool.Count;   // ����Ǯ�� ũ�Ⱑ 10�̶�� 0~9����
            if (!donglePool[poolCursor].gameObject.activeSelf)
                return donglePool[poolCursor];
        }

        // pool�� ��ġ���� ������ �����Ǿ��� ��쿡�� MakeDongle�� ���� ���� ������ֱ�
        return MakeDongle();
    }

    void NextDongle()
    {
        if (isOver)
            return;

        // ���� ���ۿ� ���� ���� �־��ֱ�
        lastDongle = GetDongle();
        lastDongle.level = Random.Range(0, maxLevel);   // �������� �ִ밪�� ���� ũ�� ������ֱ�
        lastDongle.gameObject.SetActive(true);          // �������� ������ ���¿��� ������ ��ģ �� �� �� Ȱ��ȭ

        SFXPlay(Sfx.Next); // Next ȿ���� ���
        // �ڷ�ƾ�� �����ϴ� ���
        StartCoroutine("WaitNext");
    }

    // �ڷ�ƾ : ����Ƽ���� ������� �ѱ�� �Լ�
    // ������ ���������� �� Invoke��� ����ߴ� �Լ�
    IEnumerator WaitNext()
    {
        // ���� ������ �������� ���� ���
        while (lastDongle != null)
        {
            // yield return �� ������� ������ ���ѷ����� ���� ����
            yield return null;
        }

        // �� ���� �ڵ带 2.5�� �ڿ� �����ϵ���
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
        // ������ ������ ��� lastDongle�� null�� ����
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
        // ��� �ȿ� Ȱ��ȭ�� ���� ��������
        // FindObjectsOfType<T> : ȭ����� TŸ���� ���� ������Ʈ�� �������� �Լ�(VirticalShooting������ �ٷ�)
        // >> GameObject �ȿ� �ִ� �Լ������� GameObject�� ���� ����
        Dongle[] dongles = FindObjectsOfType<Dongle>();

        // ����� �� ��� ������ ���� ȿ�� ��ȿȭ
        // �ѹ��� �����ϱ� ������ yield�� ��� ��
        for (int i = 0; i < dongles.Length; i++)
            dongles[i].rigid.simulated = false;

        // ������ �ϳ��� �����ؼ� �����
        for (int i = 0; i < dongles.Length; i++)
        {
            // ���� �Լ��� �ٸ� ������ ��ġ�� �̵��ϴ� ���������� ����� ��ġ�� ������� �׳� ����� ���Ҹ� �ϰ�ͱ� ������ ���ڰ��� ȭ�� �ٱ��� �ִ� ��ġ���� �־���
            dongles[i].Hide(Vector3.up * 100);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(1f);
        // �ְ����� ����
        int maxScore = Mathf.Max(score, PlayerPrefs.GetInt("MaxScore"));
        PlayerPrefs.SetInt("MaxScore", maxScore);

        // ���ӿ��� UI ǥ��
        subScoreText.text = "���� : " + score.ToString();
        endGroup.SetActive(true);

        bgmPlayer.Stop();   // ���ӿ��� �� BGM ����
        SFXPlay(Sfx.Over);  // Over ȿ���� ���
    }

    // ����� ��ư�� ���� ���
    public void Reset()
    {
        SFXPlay(Sfx.Button);
        StartCoroutine("ResetCoroutine");
    }

    IEnumerator ResetCoroutine()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("InGame");
    }

    public void MainMenu()
    {
        SFXPlay(Sfx.Button);
        StartCoroutine("MainMenuCoroutine");
    }

    IEnumerator MainMenuCoroutine()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("MainMenu");
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
        // sfxCursor���� 0, 1, 2�� ��������
        sfxCursor = (sfxCursor + 1) % sfxPlayer.Length;
    }

    // Update ���� �� ����Ǵ� �����ֱ� �Լ�
    void LateUpdate()
    {
        // ���ھ� UI ������Ʈ
        scoreText.text = "Score " + score.ToString("D5");
    }
}
