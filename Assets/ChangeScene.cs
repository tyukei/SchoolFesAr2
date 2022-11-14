using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public string[] sceneNames;
    // Start is called before the first frame update
    public void TransScene(int index)
    {
        SceneManager.LoadScene(sceneNames[index]);
    }
}
