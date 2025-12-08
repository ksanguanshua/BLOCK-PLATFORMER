using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTrigger : MonoBehaviour
{
    [SerializeField] string targetScene;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Scene nextScene = SceneManager.GetSceneByName(targetScene);
            if (nextScene != null)
            {
                GameManager.instance.ChangeScene(targetScene);
            }
            else
            {
                Debug.Log("Scene not found!");
            }
        }
    }
}
