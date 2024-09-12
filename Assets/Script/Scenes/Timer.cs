using UnityEngine;
using TMPro;

public class TimeDisplay : MonoBehaviour
{
    public TextMeshProUGUI timeText; // TextMeshPro UI 컴포넌트 참조

    void Update()
    {
        float time = Time.time;
        timeText.text = "T-" + time.ToString("F2") + "S";
    }
}