using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dongle : MonoBehaviour
{
    public GameManager gameManager;
    public ParticleSystem effect;
    public int level;
    public bool isDrag;
    public bool isMerge;    // �̹� �������� �ִ� ������ �Ǵ����ִ� ����

    public Rigidbody2D rigid;
    CircleCollider2D circleCollider;
    Animator anim;
    SpriteRenderer spriteRenderer;

    float deadTime;     // �ش� �ð�(deadTime)���� ���� �����ִٸ� ���� ����

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // ������Ʈ�� ������ ��(Ȱ��ȭ �� ��)
    void OnEnable()
    {
        anim.SetInteger("Level",level);
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

    // ��ü�� �浹 ���� �� ���� (Enter�� Exit�� �ٸ�)
    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Dongle")
        {
            Dongle other = collision.gameObject.GetComponent<Dongle>();

            // �� ���� ���۸� ���������� ������ �߰�
            if (level == other.level && !isMerge && !other.isMerge && level < 7)
            {
                // ���� ����� ��ġ ��������
                float myX = transform.position.x;
                float myY = transform.position.y;
                float otX = other.transform.position.x;
                float otY = other.transform.position.y;

                // 1. ���� �Ʒ��� ���� ��
                // 2. ������ ���� �� ��, ���� �����ʿ� ���� ��
                if (myY < otY || (myY == otY && myX > otX))
                {
                    // ���� ���� ��
                    LevelUp();
                    // ������ �����
                    other.Hide(transform.position);
                }
                // ���� ��ġ�� ����
            }
        }
    }

    public void Hide(Vector3 targetPos)
    {
        // �������� ���̹Ƿ� isMerge = true
        isMerge = true;

        // �������� ��ȿȭ
        rigid.simulated = false;
        circleCollider.enabled = false;

        // ������ �� �Ӹ� �ƴ϶� ���� ������ ��, Hide�� ȣ��Ǹ� Effect ȿ�� �־��ֱ�
        if (targetPos == Vector3.up * 100)
            EffectPlay();

        // ������ �� ���� ���ؼ� ����������
        StartCoroutine(HideRoutine(targetPos));
    }

    IEnumerator HideRoutine(Vector3 targetPos)
    {
        // while���� ���� ��ġ Update�Լ��� ����ϵ���
        int frameCount = 0;
        while (frameCount < 20)
        {
            // targetPos�� ���ڰ��� ���� ���ϴ� �Լ��� �ٸ��� �ϱ�
            // 1. �� ������ �������� �Ϳ� ���� Hide�Լ� ����
            if (targetPos != Vector3.up * 100)
                transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f);
            // 2. ���� ������ ���� Hide�Լ� ����
            else if (targetPos == Vector3.up * 100)
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 0.2f);  // scale���� Lerp�Լ� ���� ����

            frameCount++;
            // yield return�� ���� ���� ���������� �Ѱ��ֱ� -> ���ϴ� �����Ӵ�� �����ϱ� ���� while�� �ȿ� �־���
            yield return null;
        }

        // 2�� ��������ŭ ���� ��
        gameManager.score += (int)Mathf.Pow(2, level);

        // 20�������� �� ���ȴٸ� �������� ���� �����ϰ� ���� ������Ʈ�� ��Ȱ��ȭ
        isMerge = false;
        gameObject.SetActive(false);
    }

    void LevelUp()
    {
        isMerge = true;

        // ���� �� �߿� ���ذ� �� �� �ִ� �����ӵ� �����ϱ�
        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;


        StartCoroutine("LevelUpRoutine");
    }

    IEnumerator LevelUpRoutine()
    {
        // ������ ���̴� ���� ������ ���� ��(�ִϸ��̼��� ����)
        yield return new WaitForSeconds(0.2f);
        anim.SetInteger("Level", level + 1);
        EffectPlay();   // �ִϸ��̼��� ����Ǵ� ���ÿ� ����Ʈ ����

        // ���� ���� �� ������ ���� �� (�������� ���ڸ��� �ٸ� ������ ����� ��, �ð� ���� �α� ����)
        yield return new WaitForSeconds(0.3f);
        level++;

        // ���� ������ �Ŵ����� �ִ뷹�� �� �� ū ���� �Ŵ����� �ִ뷹���� ������
        // ���� : maxLevel���� ������ ����� ���� ���� �ƴ� maxLevel���� �� �ܰ� �Ʒ������� ���� (Random.Range)
        gameManager.maxLevel = Mathf.Max(level, gameManager.maxLevel);

        // �������� ����
        isMerge = false;

    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Finish")
        {
            deadTime += Time.deltaTime;
            if (deadTime > 2)       // 2�� �̻� ������ �����ִٸ� ������ ���� ����
                spriteRenderer.color = new Color(0.9f, 0.2f, 0.2f);
            if (deadTime > 5)       // 5�� �̻� ������ �����ִٸ� ���� ����
                gameManager.GameOver();
        }
        
    }

    // ���ο� �������� ���� ���� ���� Ż���� ���
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
        effect.transform.localScale = transform.localScale; // ������ �������� �ٸ��� ������־��� ������ ����Ʈ�� �����ϵ� �ٷ� �ٸ��� ���� �� ����
        effect.Play();
    }
}
