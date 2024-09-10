using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class RunScene : MonoBehaviour
{
    public int buildIndex;

    public void RunScene_()
    {
        // Inicia la carga de la escena de manera as�ncrona
        StartCoroutine(LoadSceneAsync(buildIndex));
    }

    private IEnumerator LoadSceneAsync(int sceneIndex)
    {
        // Comienza a cargar la escena de manera as�ncrona pero sin activarla a�n
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        asyncLoad.allowSceneActivation = false;

        // Espera hasta que la escena est� completamente cargada
        while (!asyncLoad.isDone)
        {
            // Comprueba si la carga est� al 90% (casi completa pero no activada)
            if (asyncLoad.progress >= 0.9f)
            {
                // Activa la escena cuando est� cargada completamente
                asyncLoad.allowSceneActivation = true;
            }

            // Espera el siguiente frame antes de continuar el ciclo
            yield return null;
        }
    }
}
