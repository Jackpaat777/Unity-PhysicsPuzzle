using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Dongle lastDongle;
    public GameObject donglePrefab;
    public Transform dongleGroup;
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
        // Instantiate�Լ����� dongleGroup�� �ڽ����� �����ϵ���
        GameObject instant = Instantiate(donglePrefab, dongleGroup);
        // return�� Dongle�� �ϱ� ���� GetComponent�� ����
        Dongle instantDongle = instant.GetComponent<Dongle>();

        return instantDongle;
    }

    void NextDongle()
    {
        // lastDongle�� ���� �־��ֱ�
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
