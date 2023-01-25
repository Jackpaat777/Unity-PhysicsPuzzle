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
    public int maxLevel;

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
}
