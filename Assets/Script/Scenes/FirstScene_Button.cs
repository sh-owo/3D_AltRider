using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChangeSceneButton : MonoBehaviour
{
    public void OnButtonClick()
    {
        SceneManager.LoadScene("Driving_Scene");
    }
}