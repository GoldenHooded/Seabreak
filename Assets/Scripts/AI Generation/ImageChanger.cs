using System.Collections;
using System.Threading.Tasks;
using ContentGeneration;
using ContentGeneration.Helpers;
using ContentGeneration.Models;
using UnityEngine;
using UnityEngine.UI;

public class ImageChanger : MonoBehaviour
{
    public RawImage rawImage; // Donde se mostrará la imagen generada
    public Toggle publishToggle; // Para hacer pública la imagen
    public Texture2D loadingTexture; // Textura que se mostrará mientras se genera la imagen
    private bool isGeneratingImage = false;
    private string playerId = "UniquePlayerID";
    private string generatedImageUrl = ""; // URL de la imagen generada

    // Método para iniciar la generación de la imagen
    public void ChangeImage()
    {
        if (isGeneratingImage)
        {
            Debug.Log("La imagen aún se está generando.");
            return;
        }

        Debug.Log("Iniciando generación de imagen...");
        StartCoroutine(GenerateImageAndApplyCoroutine());
    }

    // Corrutina que maneja el flujo general de la generación y aplicación de la imagen
    private IEnumerator GenerateImageAndApplyCoroutine()
    {
        isGeneratingImage = true;

        // Establecemos la textura de carga antes de comenzar el proceso de generación
        rawImage.texture = loadingTexture;
        rawImage.enabled = true;

        string prompt = "Pirate flag of any color";
        Debug.Log("Solicitando generación de imagen con el prompt: " + prompt);

        // Realizamos la solicitud fuera de la corrutina principal y esperamos su resultado
        Task<string> requestTask = RequestImageGeneration(prompt);
        yield return new WaitUntil(() => requestTask.IsCompleted);

        if (requestTask.IsFaulted)
        {
            Debug.LogError("Error durante la solicitud de generación: " + requestTask.Exception.InnerException.Message);
            isGeneratingImage = false;
            yield break;
        }

        string requestId = requestTask.Result;
        Debug.Log("ID de la solicitud: " + requestId);

        // Ahora verificamos el estado de la imagen generada
        yield return StartCoroutine(CheckImageStatus(requestId));

        isGeneratingImage = false;
    }

    // Método separado para manejar la solicitud de generación de imagen
    private async Task<string> RequestImageGeneration(string prompt)
    {
        var generatorParameters = new ContentGeneration.Models.Gaxos.GaxosTextToImageParameters
        {
            Prompt = prompt,
            NSamples = 1,
            Width = 512,
            Height = 512
        };

        // Solicitar la generación de la imagen
        return await ContentGenerationApi.Instance.RequestGaxosTextToImageGeneration(
            generatorParameters,
            ContentGeneration.Models.GenerationOptions.None,
            new { player_id = playerId, asset_type = "Player Image" }
        );
    }

    // Corrutina para verificar el estado de la solicitud y obtener la URL de la imagen generada
    private IEnumerator CheckImageStatus(string requestId)
    {
        bool imageReady = false;
        while (!imageReady)
        {
            Task<Request> checkStatusTask = ContentGenerationApi.Instance.GetRequest(requestId);
            yield return new WaitUntil(() => checkStatusTask.IsCompleted);

            if (checkStatusTask.IsFaulted)
            {
                Debug.LogError("Error al verificar el estado: " + checkStatusTask.Exception.InnerException.Message);
                yield break;
            }

            var request = checkStatusTask.Result;

            // Verificamos si el estado es "Generated"
            if (request.Status == RequestStatus.Generated)
            {
                Debug.Log("La solicitud ha sido completada.");
                if (request.Assets != null && request.Assets.Length > 0)
                {
                    generatedImageUrl = request.Assets[0].URL;
                    Debug.Log("Imagen generada encontrada, URL: " + generatedImageUrl);
                    imageReady = true;
                    yield return StartCoroutine(DownloadAndDisplayImage(generatedImageUrl));
                }
            }
            else
            {
                Debug.Log("La imagen aún no está lista. Estado: " + request.Status);
                yield return new WaitForSeconds(5); // Esperar antes de intentar de nuevo
            }
        }
    }

    // Corrutina para descargar y mostrar la imagen generada
    private IEnumerator DownloadAndDisplayImage(string imageUrl)
    {
        Task<Texture2D> downloadTask = TextureHelper.DownloadImage(imageUrl, default);
        yield return new WaitUntil(() => downloadTask.IsCompleted);

        if (downloadTask.IsFaulted)
        {
            Debug.LogError("Error al descargar la imagen: " + downloadTask.Exception.InnerException.Message);
        }
        else
        {
            rawImage.texture = downloadTask.Result;
            rawImage.enabled = true;
            Debug.Log("Imagen generada descargada y aplicada.");
        }
    }
}
