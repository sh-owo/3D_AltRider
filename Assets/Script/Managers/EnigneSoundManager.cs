using UnityEngine;

public class SpeedBasedAudio : MonoBehaviour
{
    public Rigidbody target;  // 속도를 계산할 Rigidbody
    public AudioSource lowSpeedAudioSource;  // 저속에서 재생될 오디오
    public AudioSource highSpeedAudioSource;  // 고속에서 재생될 오디오
    public float maxSpeed = 20f;  // 최대 속도

    private void Update()
    {
        // 속도 계산
        float speed = target.velocity.magnitude;

        // 속도에 따라 정규화된 값 계산 (0 ~ 1)
        float normalizedSpeed = Mathf.Clamp01(speed / maxSpeed);

        // 저속 오디오의 볼륨과 피치 조정
        lowSpeedAudioSource.volume = 1f - normalizedSpeed;  // 속도가 낮을수록 소리가 크게
        lowSpeedAudioSource.pitch = Mathf.Lerp(1f, 0.8f, normalizedSpeed);  // 속도가 높을수록 피치가 낮아짐

        // 고속 오디오의 볼륨과 피치 조정
        highSpeedAudioSource.volume = normalizedSpeed;  // 속도가 높을수록 소리가 크게
        highSpeedAudioSource.pitch = Mathf.Lerp(1f, 1.5f, normalizedSpeed);  // 속도가 높을수록 피치가 높아짐
    }
}