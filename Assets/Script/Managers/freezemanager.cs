using System.Collections;
using UnityEngine;
using UnityEngine.UI;  // Text 또는 TextMeshPro 사용 시
using TMPro;          // TextMeshPro 사용 시 필요

public class CountdownManager : MonoBehaviour
{

    public TextMeshProUGUI countdownText;  // TextMeshPro 사용 시
    public GameObject[] objectsToDeactivate;  // 비활성화할 오브젝트 그룹

    void Start()
    {
        // 처음 시작할 때 모든 오브젝트 비활성화
        foreach (GameObject obj in objectsToDeactivate)
        {
            obj.SetActive(false);
        }

        // 카운트다운 시작
        StartCoroutine(StartCountdown());
    }

    IEnumerator StartCountdown()
    {
        int countdown = 3;

        while (countdown > 0)
        {
            countdownText.text = countdown.ToString(); // UI에 카운트 출력
            yield return new WaitForSeconds(1);        // 1초 대기
            countdown--;
        }

        countdownText.text = "";  // 카운트 끝나면 텍스트 지우기

        // 모든 오브젝트 다시 활성화
        foreach (GameObject obj in objectsToDeactivate)
        {
            obj.SetActive(true);
        }
    }
}