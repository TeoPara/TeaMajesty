using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetButton : MonoBehaviour
{
    static bool count = false;
    void Awake()
    {
        if (count)
            Destroy(gameObject);
        else
        {
            count = true;
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(GameObject.Find("Music"));
        }
    }

    public void Clicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
