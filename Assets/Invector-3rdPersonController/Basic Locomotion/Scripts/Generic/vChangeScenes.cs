using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Invector;

#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

public class vChangeScenes : MonoBehaviour
{
    vGameController gm;

    private void Start()
    {
        gm = FindObjectOfType<vGameController>();
    }

    public void LoadThirdPersonScene()
    {
        Destroy(gm.currentPlayer);
        Destroy(gm.gameObject);

#if UNITY_5_3_OR_NEWER
        SceneManager.LoadScene("3rdPersonController-Demo");
#else
        Application.LoadLevel("3rdPersonController-Demo");
#endif
    }

    public void LoadTopDownScene()
    {
        Destroy(gm.currentPlayer);
        Destroy(gm.gameObject);
#if UNITY_5_3_OR_NEWER
        SceneManager.LoadScene("TopDownController-Demo");
#else
        Application.LoadLevel("TopDownController-Demo");
#endif
    }

    public void LoadPlatformScene()
    {
        Destroy(gm.currentPlayer);
        Destroy(gm.gameObject);
#if UNITY_5_3_OR_NEWER
        SceneManager.LoadScene("2.5DController-Demo");
#else
        Application.LoadLevel("2.5DController-Demo");
#endif
    }

    public void LoadIsometricScene()
    {
        Destroy(gm.currentPlayer);
        Destroy(gm.gameObject);
#if UNITY_5_3_OR_NEWER
        SceneManager.LoadScene("IsometricController-Demo");
#else
        Application.LoadLevel("IsometricController-Demo");
#endif
    }

    public void LoadVMansion()
    {
        Destroy(gm.currentPlayer);
        Destroy(gm.gameObject);
#if UNITY_5_3_OR_NEWER
        SceneManager.LoadScene("V-Mansion");
#else
        Application.LoadLevel("V-Mansion");
#endif
    }
}
