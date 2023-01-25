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
        // 프레임을 60으로 설정
        Application.targetFrameRate = 60;
    }

    void Start()
    {
        NextDongle();
    }

    Dongle GetDongle()
    {
        // 이펙트 생성
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        // return을 Dongle로 하기 위해 GetComponent를 해줌
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();

        // 동글 생성
        // Instantiate함수에서 dongleGroup의 자식으로 생성하도록
        GameObject instantDongleObj = Instantiate(donglePrefab, dongleGroup);
        // return을 Dongle로 하기 위해 GetComponent를 해줌
        Dongle instantDongle = instantDongleObj.GetComponent<Dongle>();
        // 동글 스크립트의 effect를 넣어주기 (초기화)
        instantDongle.effect = instantEffect;

        return instantDongle;
    }

    void NextDongle()
    {
        if (isOver)
            return;

        // 다음 동글에 동글 설정 넣어주기
        Dongle newDongle = GetDongle();
        lastDongle = newDongle;
        lastDongle.gameManager = this;
        lastDongle.level = Random.Range(0, maxLevel);   // 랜덤값의 최대값을 점점 크게 만들어주기
        lastDongle.gameObject.SetActive(true);  // 프리펩을 꺼놓은 상태에서 설정을 마친 뒤 그 때 활성화

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
    }
}
