using System.Collections;
using UnityEngine;
using TMPro;

public class TimeDisplay : MonoBehaviour
{
    public TextMeshProUGUI timeText; // TextMeshPro UI 컴포넌트 참조

    private float startTime;
    private bool timerStarted = false;

    void Start()
    {
        StartCoroutine(StartTimerAfterDelay(3.0f)); // 3초 후 타이머 시작
    }

    IEnumerator StartTimerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // 지연 시간 대기
        startTime = Time.time; // 현재 시간 저장
        timerStarted = true; // 타이머 시작 플래그 설정
    }

    void Update()
    {
        if (timerStarted)
        {
            float elapsedTime = Time.time - startTime; // 경과 시간 계산
            timeText.text = "T-" + elapsedTime.ToString("F2") + "S"; // 타이머 업데이트
        }
    }
}