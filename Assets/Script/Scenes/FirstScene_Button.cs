using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeSceneButton : MonoBehaviour
{
    // 씬 전환을 위한 메서드
    public void OnButtonClick(int sceneBuildIndex)
    {
        SceneManager.LoadScene(sceneBuildIndex);
    }
}