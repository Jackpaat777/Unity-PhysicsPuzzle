using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dongle : MonoBehaviour
{
    public bool isDrag;
    Rigidbody2D rigid;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Update������ ���� �Լ����� ���Ǳ� ������ if�� �ȿ� �־ ����
        if (isDrag)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // x�� ��� ����
            // 4.2f : �߾ӿ��� �������� �Ÿ�          transform.localScale.x / 2 : ������ ������ (������ ũ�⿡ ���� �������� �޶����� ����)
            float leftBorder = -4.2f + transform.localScale.x / 2;
            float rightBorder = 4.2f - transform.localScale.x / 2;
            if (mousePos.x < leftBorder)
                mousePos.x = leftBorder;
            else if (mousePos.x > rightBorder)
                mousePos.x = rightBorder;

            // ���� ������ ������ ���콺�� y���� ������ ����
            mousePos.y = 8;
            // z���� Camera�� ������ �ʵ���
            mousePos.z = 0;

            // ��ǥ�������� �ε巴�� �̵� (������ 0~1�θ� ����)
            transform.position = Vector3.Lerp(transform.position, mousePos, 0.2f);
        }
        
    }

    public void Drag()
    {
        isDrag = true;
    }

    public void Drop()
    {
        isDrag = false;
        rigid.simulated = true;
    }
}
