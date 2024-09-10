using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Lista de eventos para arriba (up)
    private List<EventData> upEventsList = new List<EventData>();
    // Lista de eventos para abajo (down)
    private List<EventData> downEventsList = new List<EventData>();

    public float timeBetweenEvents = 20f;
    public float dificultyChangeSpeed = 0.05f;
    public bool playerUp = true;
    public float chaosLevel;
    public Image chaosBar;
    public GameObject taskCompleted;
    public AudioSource musicDefault;
    public AudioSource musicFast;
    public AudioSource startSound;
    public Animator CPAnim;
    public float musicLerp;
    public bool waitingForSpace;
    public Image endImg;

    [Header("Fuga de agua")]
    public GameObject water;
    public GameObject waterFlow1;
    public GameObject waterFlow2;
    public GameObject waterFlow3;
    public bool waterEventActive;

    [Header("Incendio")]
    public bool fireEventActive;
    public GameObject playerCube;
    public GameObject[] fires;

    [Header("Pelea de trispulantes")]
    public bool fightEventActive;
    public GameObject[] defaultFightTripulants;
    public GameObject[] fightingTripulants;

    [Header("Ratas")]
    public bool ratEventActive;
    public GameObject[] rats;

    [Header("DrunkPirate")]
    public bool drunkEventActive;
    public GameObject drunkPirate;
    public Vector3 dPLocalPos;

    [Header("Motín")]
    public bool motinEventActive;
    public bool motinHasOccurred; // Nueva variable para controlar si el motín ya ocurrió
    public GameObject[] defaultMotinTripulants;
    public GameObject[] motinTripulants;

    [Header("PirateAttack")]
    public bool pirateAttackEventActive;
    public GameObject pirateShip;
    public GameObject cannonBall;
    public AudioSource recieveSFX;
    public AudioSource shootSFX;
    public int attacksThrown;
    public int attacksThrownNeeded = 6;

    private PlayerController playerController;
    private void Awake()
    {
        Invoke("WaitForSpace", 8.25f);
        playerController = FindFirstObjectByType<PlayerController>();

        // Inicializar listas de eventos
        InitializeUpEvents();
        InitializeDownEvents();
    }

    private void WaitForSpace()
    {
        waitingForSpace = true;
    }

    public void StartGame()
    {
        musicDefault.Play();
        startSound.Play();
        CPAnim.SetTrigger("Start");
        drunkPirate.SetActive(true);
        playerController.canMove = true;
        motinHasOccurred = false;

        Invoke("ChooseRandomEvent", 10f);
    }

    private void ChooseRandomEvent()
    {
        if (!playerUp)
        {
            if (upEventsList.Count == 0)
                InitializeUpEvents();

            // Elegir un evento aleatorio basado en probabilidades
            EventData chosenEvent = ChooseRandomEventFromList(upEventsList);

            if (chosenEvent != null)
            {
                int eventID = chosenEvent.eventID;

                // Verificamos si hay conflictos entre eventos como motín y pelea
                if (eventID == 3 && motinEventActive) // Pelea pero el motín está activo
                {
                    upEventsList.Remove(chosenEvent);
                    ChooseRandomEvent(); // Volver a intentar
                    return;
                }
                else if (eventID == 6 && (fightEventActive || motinHasOccurred)) // Motín pero ya ocurrió o la pelea está activa
                {
                    upEventsList.Remove(chosenEvent);
                    ChooseRandomEvent(); // Volver a intentar
                    return;
                }

                upEventsList.Remove(chosenEvent);

                // Ejecutar el evento seleccionado
                switch (eventID)
                {
                    case 2:
                        UpEvent_2(); // Incendio
                        break;
                    case 3:
                        UpEvent_3(); // Pelea de tripulantes
                        break;
                    case 6:
                        UpEvent_6(); // Motín
                        motinHasOccurred = true; // Marcar que el motín ya ha ocurrido
                        break;
                    default:
                        break;
                }
            }
        }
        else
        {
            if (downEventsList.Count == 0)
                InitializeDownEvents();

            // Elegir un evento aleatorio basado en probabilidades
            EventData chosenEvent = ChooseRandomEventFromList(downEventsList);

            if (chosenEvent != null)
            {
                int eventID = chosenEvent.eventID;
                downEventsList.Remove(chosenEvent);

                // Ejecutar el evento seleccionado
                switch (eventID)
                {
                    case 1:
                        DownEvent_1(); // Fuga de agua
                        break;
                    case 4:
                        DownEvent_4(); // Tripulante borracho
                        break;
                    case 5:
                        DownEvent_5(); // Ratas
                        break;
                    case 7:
                        DownEvent_7(); // Ataque enemigo
                        break;
                    default:
                        break;
                }
            }
        }

        Invoke("ChooseRandomEvent", timeBetweenEvents);
    }

    private EventData ChooseRandomEventFromList(List<EventData> eventsList)
    {
        // Sumar todas las probabilidades
        float totalWeight = 0f;
        foreach (var eventData in eventsList)
        {
            totalWeight += eventData.weight;
        }

        // Seleccionar un valor aleatorio entre 0 y el total de probabilidades
        float randomValue = Random.Range(0, totalWeight);

        // Seleccionar el evento basado en el valor aleatorio
        float cumulativeWeight = 0f;
        foreach (var eventData in eventsList)
        {
            cumulativeWeight += eventData.weight;
            if (randomValue <= cumulativeWeight)
            {
                return eventData;
            }
        }

        return null; // Esto no debería ocurrir si los pesos están bien definidos
    }


    private void InitializeUpEvents()
    {
        upEventsList.Clear();
        upEventsList.Add(new EventData(2, 0.3f));  // Incendio con 30% de probabilidad
        upEventsList.Add(new EventData(3, 0.4f));  // Pelea de tripulantes con 40% de probabilidad
        upEventsList.Add(new EventData(6, 0.3f));  // Motín con 30% de probabilidad
    }

    private void InitializeDownEvents()
    {
        downEventsList.Clear();
        downEventsList.Add(new EventData(1, 0.25f)); // Fuga de agua con 25% de probabilidad
        downEventsList.Add(new EventData(4, 0.25f)); // Tripulante borracho con 25% de probabilidad
        downEventsList.Add(new EventData(5, 0.25f)); // Ratas con 25% de probabilidad
        downEventsList.Add(new EventData(7, 0.25f)); // Ataque enemigo con 25% de probabilidad
    }

    private void Update()
    {
        if (waitingForSpace)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartGame();

                waitingForSpace = false;
                return;
            }
        }
        else
        {
            timeBetweenEvents -= Time.deltaTime * dificultyChangeSpeed;
        }

        ChaosLevelLogic();

        if (waterEventActive)
        {
            bool endEvent_0 = !waterFlow1.activeSelf;
            bool endEvent_1 = !waterFlow2.activeSelf;
            bool endEvent_2 = !waterFlow3.activeSelf;

            bool endEvent = endEvent_0 && endEvent_1 && endEvent_2;

            if (endEvent)
            {
                water.SetActive(false);
                waterFlow1.SetActive(false);
                waterFlow2.SetActive(false);
                waterFlow3.SetActive(false);

                waterEventActive = false;

                Instantiate(taskCompleted);
            }
        }
        
        if (fireEventActive)
        {
            bool endEvent = true;

            foreach (GameObject obj in fires)
            {
                if (obj.activeSelf)
                {
                    endEvent = false;
                    break;
                }
            }

            if (endEvent)
            {
                fireEventActive = false;

                Instantiate(taskCompleted);
            }
        }

        if (ratEventActive)
        {
            bool endEvent = true;

            foreach (GameObject rat in rats)
            {
                if (rat.activeSelf)
                {
                    endEvent = false;
                    break;
                }
            }

            if (endEvent)
            {
                ratEventActive = false;

                Instantiate(taskCompleted);
            }
        }

        if (pirateAttackEventActive)
        {
            if (attacksThrown >= attacksThrownNeeded)
            {
                pirateAttackEventActive = false;
                pirateShip.SetActive(false);
                Instantiate(taskCompleted);
            }
        }
        else
        {
            cannonBall.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (playerCube.activeSelf && !fireEventActive) playerCube.SetActive(false);
    }

    public static void Death()
    {
        SceneManager.LoadScene(2);
    }

    private void ChaosLevelLogic()
    {
        float summary = 0;

        if (waterEventActive) summary += 0.3f;
        if (fireEventActive) summary += 0.3f;
        if (fightEventActive) summary += 0.15f;
        if (drunkEventActive) summary += 0.15f;
        if (ratEventActive) summary += 0.15f;
        if (motinEventActive) summary += 0.3f;
        if (pirateAttackEventActive) summary += 0.5f;

        chaosLevel = Mathf.Lerp(chaosLevel, summary,Time.deltaTime * 5);
        chaosBar.fillAmount = chaosLevel;

        if (summary >= 0.7f)
        {
            musicDefault.pitch = Mathf.Lerp(musicDefault.pitch, 1.2f, musicLerp * Time.deltaTime);
        }
        else
        {

            musicDefault.pitch = Mathf.Lerp(musicDefault.pitch, 1f, musicLerp * Time.deltaTime);
        }

        if (chaosLevel >= 0.99f)
        {
            Color color = endImg.color;

            color.a = Mathf.Lerp(endImg.color.a, 1, Time.deltaTime * 5);
            
            endImg.color = color;
        }

        if (endImg.color.a >= 0.99f)
        {
            Death();
        }
    }

    //

    private void DownEvent_1() // Fuga de agua
    {
        AlertSystem.SetAlert("Alerta de inundación");

        water.SetActive(true);
        waterFlow1.SetActive(true);
        waterFlow2.SetActive(true);
        waterFlow3.SetActive(true);

        waterEventActive = true;
    }

    private void UpEvent_2() // Incendio
    {
        AlertSystem.SetAlert("Alerta de incendio");

        foreach (GameObject fire in fires)
        {
            fire.SetActive(true);
        }

        fireEventActive = true;
    }

    private void UpEvent_3() // Pelea de tripulantes
    {
        AlertSystem.SetAlert("Pelea entre tripulantes");

        foreach (GameObject tripulant in defaultFightTripulants)
        {
            tripulant.SetActive(false);
        }

        foreach (GameObject tripulant in fightingTripulants)
        {
            tripulant.SetActive(true);
        }

        fightEventActive = true;
    }

    public void EndEvent_3()
    {
        for (int i = 0; i < defaultFightTripulants.Length; i++)
        {
            defaultFightTripulants[i].SetActive(true);
            defaultFightTripulants[i].transform.position = fightingTripulants[i].transform.position;
        }

        foreach (GameObject tripulant in fightingTripulants)
        {
            tripulant.SetActive(false);
        }

        fightEventActive = false;

        Instantiate(taskCompleted);
    }

    private void DownEvent_4() // Tripulante borracho (Hay que pillarlo)
    {
        AlertSystem.SetAlert("Pirata borracho en la bodega");

        drunkPirate.transform.localPosition = dPLocalPos;

        drunkEventActive = true;
    }

    public void EndEvent_4()
    {
        drunkEventActive = false;

        drunkPirate.transform.localPosition += Vector3.up * 10000;

        Instantiate(taskCompleted);
    }

    private void DownEvent_5() // Ratas
    {
        AlertSystem.SetAlert("Alerta de ratones en la bodega");

        foreach (GameObject rat in rats)
        {
            rat.SetActive(true);
        }

        ratEventActive = true;
    }

    private void UpEvent_6()
    {
        AlertSystem.SetAlert("Motín!!");

        foreach (GameObject tripulant in defaultMotinTripulants)
        {
            tripulant.SetActive(false);
        }

        foreach (GameObject tripulant in motinTripulants)
        {
            tripulant.SetActive(true);
        }

        motinEventActive = true;
    }

    public void EndEvent_6()
    {
        for (int i = 0; i < defaultMotinTripulants.Length; i++)
        {
            defaultMotinTripulants[i].SetActive(true);
            defaultMotinTripulants[i].transform.position = motinTripulants[i].transform.position;
        }

        foreach (GameObject tripulant in motinTripulants)
        {
            tripulant.SetActive(false);
        }

        motinEventActive = false;

        Instantiate(taskCompleted);
    }

    private void DownEvent_7()
    {
        AlertSystem.SetAlert("Ataque enemigo: Usa los cañones");

        attacksThrown = 0;

        Invoke("AttackRecieved", 9f);

        pirateShip.SetActive(true);

        pirateAttackEventActive = true;
    }

    public void AttackRecieved()
    {
        if (!pirateAttackEventActive) return;

        CPAnim.SetTrigger("Shake");
        recieveSFX.Play();

        Invoke("AttackRecieved", Random.Range(2f, 10f));
    }

    public void AttackThrown()
    {
        CPAnim.SetTrigger("Shake");
        shootSFX.Play();
        attacksThrown++;
    }

    //

    // Método para iniciar manualmente la Fuga de agua
    public void TriggerDownEvent1()
    {
        DownEvent_1();
    }

    // Método para iniciar manualmente el Incendio
    public void TriggerUpEvent2()
    {
        UpEvent_2();
    }

    // Método para iniciar manualmente la Pelea de tripulantes
    public void TriggerUpEvent3()
    {
        UpEvent_3();
    }

    // Método para iniciar manualmente el Tripulante borracho
    public void TriggerDownEvent4()
    {
        DownEvent_4();
    }

    // Método para iniciar manualmente el evento de Ratas
    public void TriggerDownEvent5()
    {
        DownEvent_5();
    }

    // Método para iniciar manualmente el Motín
    public void TriggerUpEvent6()
    {
        UpEvent_6();
    }

    // Método para iniciar manualmente el Ataque enemigo
    public void TriggerDownEvent7()
    {
        DownEvent_7();
    }
}

public class EventData
{
    public int eventID;
    public float weight;

    public EventData(int id, float probability)
    {
        eventID = id;
        weight = probability;
    }
}
