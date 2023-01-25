using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dongle : MonoBehaviour
{
    public GameManager gameManager;
    public ParticleSystem effect;
    public int level;
    public bool isDrag;
    public bool isMerge;    // 이미 합쳐지고 있는 중인지 판단해주는 변수

    public Rigidbody2D rigid;
    CircleCollider2D circleCollider;
    Animator anim;
    SpriteRenderer spriteRenderer;

    float deadTime;     // 해당 시간(deadTime)동안 선에 걸쳐있다면 게임 오버

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // 오브젝트가 생성될 때(활성화 될 때)
    void OnEnable()
    {
        anim.SetInteger("Level",level);
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

    // 물체가 충돌 중일 때 실행 (Enter과 Exit와 다름)
    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Dongle")
        {
            Dongle other = collision.gameObject.GetComponent<Dongle>();

            // 두 개의 동글만 합쳐지도록 조건을 추가
            if (level == other.level && !isMerge && !other.isMerge && level < 7)
            {
                // 나와 상대편 위치 가져오기
                float myX = transform.position.x;
                float myY = transform.position.y;
                float otX = other.transform.position.x;
                float otY = other.transform.position.y;

                // 1. 내가 아래에 있을 때
                // 2. 동일한 높이 일 때, 내가 오른쪽에 있을 때
                if (myY < otY || (myY == otY && myX > otX))
                {
                    // 나는 레벨 업
                    LevelUp();
                    // 상대방은 숨기기
                    other.Hide(transform.position);
                }
                // 동글 합치기 로직
            }
        }
    }

    public void Hide(Vector3 targetPos)
    {
        // 합쳐지는 중이므로 isMerge = true
        isMerge = true;

        // 물리현상 무효화
        rigid.simulated = false;
        circleCollider.enabled = false;

        // 레벨업 때 뿐만 아니라 게임 오버할 때, Hide가 호출되면 Effect 효과 넣어주기
        if (targetPos == Vector3.up * 100)
            EffectPlay();

        // 상대방은 내 쪽을 향해서 없어지도록
        StartCoroutine(HideRoutine(targetPos));
    }

    IEnumerator HideRoutine(Vector3 targetPos)
    {
        // while문을 통해 마치 Update함수를 사용하듯이
        int frameCount = 0;
        while (frameCount < 20)
        {
            // targetPos의 인자값에 따라 행하는 함수를 다르게 하기
            // 1. 두 동글이 합쳐지는 것에 따른 Hide함수 실행
            if (targetPos != Vector3.up * 100)
                transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f);
            // 2. 게임 오버에 따른 Hide함수 실행
            else if (targetPos == Vector3.up * 100)
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 0.2f);  // scale에도 Lerp함수 적용 가능

            frameCount++;
            // yield return을 통해 다음 프레임으로 넘겨주기 -> 원하는 움직임대로 구현하기 위해 while문 안에 넣어줌
            yield return null;
        }

        // 2의 제곱수만큼 레벨 업
        gameManager.score += (int)Mathf.Pow(2, level);

        // 20프레임을 다 돌렸다면 합쳐지는 것을 종료하고 게임 오브젝트를 비활성화
        isMerge = false;
        gameObject.SetActive(false);
    }

    void LevelUp()
    {
        isMerge = true;

        // 레벨 업 중에 방해가 될 수 있는 물리속도 제어하기
        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;


        StartCoroutine("LevelUpRoutine");
    }

    IEnumerator LevelUpRoutine()
    {
        // 겉으로 보이는 동글 프리펩 레벨 업(애니메이션을 통해)
        yield return new WaitForSeconds(0.2f);
        anim.SetInteger("Level", level + 1);
        EffectPlay();   // 애니메이션이 실행되는 동시에 이펙트 생성

        // 실제 게임 속 동글의 레벨 업 (레벨업을 하자마자 다른 레벨과 닿았을 때, 시간 차를 두기 위해)
        yield return new WaitForSeconds(0.3f);
        level++;

        // 현재 레벨과 매니저의 최대레벨 중 더 큰 쪽이 매니저의 최대레벨로 설정됨
        // 주의 : maxLevel까지 동글이 만들어 내는 것이 아닌 maxLevel에서 한 단계 아래까지만 생성 (Random.Range)
        gameManager.maxLevel = Mathf.Max(level, gameManager.maxLevel);

        // 합쳐지기 종료
        isMerge = false;

    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Finish")
        {
            deadTime += Time.deltaTime;
            if (deadTime > 2)       // 2초 이상 동글이 걸쳐있다면 동글의 색상 변경
                spriteRenderer.color = new Color(0.9f, 0.2f, 0.2f);
            if (deadTime > 5)       // 5초 이상 동글이 걸쳐있다면 게임 오버
                gameManager.GameOver();
        }
        
    }

    // 라인에 걸쳤으나 게임 오버 전에 탈출한 경우
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Finish")
        {
            deadTime = 0;
            spriteRenderer.color = Color.white;
        }

    }

    void EffectPlay()
    {
        effect.transform.position = transform.position;
        effect.transform.localScale = transform.localScale; // 동글의 스케일을 다르게 만들어주었기 때문에 이펙트의 스케일도 바로 다르게 만들 수 있음
        effect.Play();
    }
}
