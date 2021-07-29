using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    public Camera playerPOV;
    public float lightSpeed;
    // Velocità di inseguimento della torcia
    public float lightSmoothFactor;
    public bool manualLightOffset;
    public Vector3 lightOffset;
    public bool flickering;
    // Effetto sfarfallio della torcia.
    public float flickeringMinWait;
    public int flickeringMaxDuration;
    public float lightMaxRange;
    private float flickeringTimer;
    public float flickeringMaxWait;
    private bool isFlickering;
    private bool lightOn;

    public bool autoFindPlayerPOV;

    void Start()
    {
        if(autoFindPlayerPOV)
        {
            playerPOV = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        }

        if(!manualLightOffset)
        {
        // La posizione della luce può essere impostata attraverso l'editor oppure attraverso un vettore direzione.
            lightOffset = this.transform.position - playerPOV.transform.position;
            // Se la torcia combacia con la posizione della telecamera non sono proiettate le ombre (forse un bug di Unity).
        }

        isFlickering = false;
        lightOn = true;

        this.GetComponent<Light>().range = lightMaxRange;

    }

    void LightSwitch()
    {
        if(Input.GetKeyDown(KeyCode.F))
        {
        // Con F si accende e spegne la luce.
            if(lightOn)
            {
                this.GetComponent<Collider>().enabled = false;

                this.GetComponent<Light>().range = 0;
                flickering = false;
                lightOn = false;

                StopCoroutine("Flickering");
            }
            else
            {
                this.GetComponent<Collider>().enabled = true;

                this.GetComponent<Light>().range = lightMaxRange;
                flickering = true;
                lightOn = true;
            }
        }
    }
    
    void LateUpdate() {
        
        Ray mouseRay = playerPOV.ScreenPointToRay(Input.mousePosition);

        this.transform.forward = Vector3.Lerp(this.transform.forward, mouseRay.direction.normalized, lightSpeed * Time.deltaTime);
        // La asse Z della torcia, la Spotlight, va a combaciare con un vettore direzione tracciato dalla posizione della telecamera al cursore del mouse.

        Vector3 lightNewPosition;
        lightNewPosition = playerPOV.transform.TransformPoint(lightOffset);
        // La torcia si deve spostare in un nuovo punto ogni volta che la telecamera ruota mantenendo sempre la stessa distanza.
        this.transform.position = Vector3.Lerp(this.transform.position, lightNewPosition, lightSmoothFactor * Time.deltaTime);
        // Le funzioni Lerp servono a rendere questi spostamenti non istantanei, in modo che la luce segua la direzione in cui stiamo guardando.

        LightSwitch();

        if(flickering)
        {
            if(flickeringTimer >= Random.Range(flickeringMinWait, flickeringMaxWait) && !isFlickering)
            {
                StartCoroutine("Flickering");
                // Per avere un effetto di sfarfallio casuale mi affido a una Coroutine che è avviata dopo un tempo casuale, compreso fra un valore minimo e massimo.
            }

            if(isFlickering)
            {
                flickeringTimer = 0.0f;
            }
            else
            {
                flickeringTimer += Time.deltaTime;
            }
        }
    }

    IEnumerator Flickering()
    {
        isFlickering = true;

        int flickeringDuration = (int)Random.Range(4, flickeringMaxDuration);
        // In pratica lo sfarfallio è un'accensione e spegnimento della torcia molto veloce più volte di file, da un minimo di 4 (sennò l'effetto non rende).
        // a un valore intero impostato nell'editor.

        if(flickeringDuration % 2 != 0)
        {
            flickeringDuration += 1;
            // Siccome la torcia deve prima spegnersi (o diminuire d'intensità), poi riaccendersi, ciò deve ripetersi un numero pari di volte (altrimenti resterebbe spenta).
            // Se il valore impostato dall'editor è dispari, sommo 1.
        }

        for(int i = 0; i <= flickeringDuration; i++)
        {
            flickeringTimer = 0.0f;
            if(i % 2 != 0)
            {
                this.GetComponent<Light>().range = Random.Range(0.0f, lightMaxRange);
                yield return new WaitForSeconds(Random.Range(0.01f, 0.05f));
                // Se i è dispari la luce diminuisce di intesità (assume un valore compreso fra 0 e quello di normale funzionamanto).
            }
            else
            {
                this.GetComponent<Light>().range = lightMaxRange;
                yield return new WaitForSeconds(Random.Range(0.01f, 0.05f));
                // Altrimenti, quando i è pari, la luce ritorna al suo normale funzionamento.
            }
        }
        isFlickering = false;
    }
}