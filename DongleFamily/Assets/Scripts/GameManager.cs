using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
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
    // BGM ���� �Լ�
    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer;
    public AudioClip[] sfxClip;
    public enum SFX { LevelUp, Next, Attach, Button, Over };
    int sfxCursor;

    public int score;
    public int maxLevel;
    public bool isOver;

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
    }

    void Start()
    {
        bgmPlayer.Play();
        NextDongle();
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

        SFXPlay(SFX.Next); // Next ȿ���� ���
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
        SFXPlay(SFX.Over);  // 1�� �ڿ� Over ȿ���� ���
    }

    public void SFXPlay(SFX type)
    {
        switch (type)
        {
            case SFX.LevelUp:
                sfxPlayer[sfxCursor].clip = sfxClip[Random.Range(0, 3)];
                break;
            case SFX.Next:
                sfxPlayer[sfxCursor].clip = sfxClip[3];
                break;
            case SFX.Attach:
                sfxPlayer[sfxCursor].clip = sfxClip[4];
                break;
            case SFX.Button:
                sfxPlayer[sfxCursor].clip = sfxClip[5];
                break;
            case SFX.Over:
                sfxPlayer[sfxCursor].clip = sfxClip[6];
                break;
        }

        sfxPlayer[sfxCursor].Play();
        // sfxCursor���� 0, 1, 2�� ��������
        sfxCursor = (sfxCursor + 1) % sfxPlayer.Length;
    }
}
