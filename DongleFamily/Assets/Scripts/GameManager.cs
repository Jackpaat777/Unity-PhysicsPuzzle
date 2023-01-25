using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Dongle lastDongle;
    public GameObject donglePrefab;
    public GameObject effectPrefab;
    public Transform dongleGroup;
    public Transform effectGroup;

    public int score;
    public int maxLevel;
    public bool isOver;

    void Awake()
    {
        // �������� 60���� ����
        Application.targetFrameRate = 60;
    }

    void Start()
    {
        NextDongle();
    }

    Dongle GetDongle()
    {
        // ����Ʈ ����
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        // return�� Dongle�� �ϱ� ���� GetComponent�� ����
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();

        // ���� ����
        // Instantiate�Լ����� dongleGroup�� �ڽ����� �����ϵ���
        GameObject instantDongleObj = Instantiate(donglePrefab, dongleGroup);
        // return�� Dongle�� �ϱ� ���� GetComponent�� ����
        Dongle instantDongle = instantDongleObj.GetComponent<Dongle>();
        // ���� ��ũ��Ʈ�� effect�� �־��ֱ� (�ʱ�ȭ)
        instantDongle.effect = instantEffect;

        return instantDongle;
    }

    void NextDongle()
    {
        if (isOver)
            return;

        // ���� ���ۿ� ���� ���� �־��ֱ�
        Dongle newDongle = GetDongle();
        lastDongle = newDongle;
        lastDongle.gameManager = this;
        lastDongle.level = Random.Range(0, maxLevel);   // �������� �ִ밪�� ���� ũ�� ������ֱ�
        lastDongle.gameObject.SetActive(true);  // �������� ������ ���¿��� ������ ��ģ �� �� �� Ȱ��ȭ

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
    }
}
