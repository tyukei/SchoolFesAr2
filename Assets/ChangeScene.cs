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
        StartCoroutine("TransSceneCoroutine", index);
        
    }

    IEnumerator TransSceneCoroutine(int index)
    {
        yield return new WaitForSeconds(1f);
        FadeManager.Instance.LoadScene (sceneNames[index], 2.0f);
    }

}
