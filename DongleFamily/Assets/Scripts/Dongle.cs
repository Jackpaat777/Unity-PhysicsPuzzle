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
        // Update에서는 여러 함수들이 사용되기 때문에 if문 안에 넣어서 구현
        if (isDrag)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // x축 경계 설정
            // 4.2f : 중앙에서 벽까지의 거리          transform.localScale.x / 2 : 동글의 반지름 (동글의 크기에 따라 반지름이 달라지기 때문)
            float leftBorder = -4.2f + transform.localScale.x / 2;
            float rightBorder = 4.2f - transform.localScale.x / 2;
            if (mousePos.x < leftBorder)
                mousePos.x = leftBorder;
            else if (mousePos.x > rightBorder)
                mousePos.x = rightBorder;

            // 게임 내에서 동글이 마우스의 y축은 따라가지 않음
            mousePos.y = 8;
            // z축은 Camera를 따라가지 않도록
            mousePos.z = 0;

            // 목표지점까지 부드럽게 이동 (강도는 0~1로만 지정)
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
