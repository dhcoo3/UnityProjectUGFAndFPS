using UnityEngine;
using UnityEngine.SceneManagement;

public class Launch : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //Launch场景什么都不要放,为了最快速度进入游戏流程
        SceneManager.LoadSceneAsync("Main");
    }
}
