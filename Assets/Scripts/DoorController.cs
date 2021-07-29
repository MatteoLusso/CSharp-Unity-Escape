using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DoorController : MonoBehaviour
{
// Le porte che separano le sezioni del livello si aprono e si chiudono in automatico quando rilevano un giocatore all'interno
// del proprio collider/trigger.
    public float doorTimeToOpen;
    // Tempo di apertura della porta.
    public float doorOpenHeight;
    // Massima altezza in cui la porta si solleva per aprirsi.

    private GameObject door;
    // Lo script va nel parent della porta che contiene il trigger (altrimenti il collider si sposterebbe assieme alla porta)
    private float doorActualHeight;

    private float closedPosition;

    public Text[] doorName;

    private string doorNumber;
    private char randomChar;

    private int colliderDetected;

    private void Start()
    {
        foreach(Transform child in this.transform)
        {
            if(child.CompareTag("Door"))
            {
                child.gameObject.SetActive(true);
                door = child.gameObject;
            }
        }
        closedPosition = door.transform.position.y;
        // È importante che il modello della porta sia istanziato nella posizione che deve avere quando la porta è chiusa.
        // PS: Si può altrimenti aggiungere una variabile pubblica per definire manualmente qual è l'altezza in cui la porta è chiusa e non preoccuparsi di dove si trovi quando è istanziata. 

        doorNumber = this.transform.parent.name;
        doorNumber = doorNumber.Replace("Connector(", "");
        doorNumber = doorNumber.Replace("-", "");
        doorNumber = doorNumber.Replace(")", "");
        doorNumber = doorNumber.Replace(" R", "");
        doorNumber = doorNumber.Replace(" D", "");

        randomChar = (char)Random.Range(65, 86);

        for(int i = 0; i < doorName.Length; i++)
        {
            doorName[i].text = "DOOR\n " + randomChar + "-" + doorNumber;
        }

        colliderDetected = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") || other.CompareTag("Enemy"))
        {

            //Debug.Log("Ingresso: " + other.name + " " + other.tag);
            StopCoroutine("ClosingDoor");
            StartCoroutine("OpeningDoor");
            // Quando il giocatore o il nemico entra dentro il trigger, si attiva la coroutine che sposta l'oggetto verso l'alto.

            colliderDetected++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if((other.CompareTag("Player") || other.CompareTag("Enemy")))
        {
            colliderDetected--;

            if(colliderDetected == 0)
            {
            // Ci può essere più di un collider all'interno del trigger che fa aprire e chiudere la porta,
            // quindi la porta si deve chiudere solo quando non c'è più nessuno nelle vicinanze. Faccio ciò
            // aggiornando un intero che viene aumentato di uno ogni volta che qualcosa o qualcuno entra nel trigger e diminuisce di uno quando qualcuno o qualcosa esce.
            // Così si evita che la porta si chiuda non appena qualcuno non è più rilevato ma c'è comunque un altro collider rilevato. Bisogna però fare attenzione a generare ogni porta sempre nello stato si chiusura e senza
            // collider all'interno, sennò il contatore si può sballare.
                StopCoroutine("OpeningDoor");
                StartCoroutine("ClosingDoor");
            }
        }
    }

    IEnumerator OpeningDoor()
    {
        float elapsedTime = 0.0f;
        Vector3 startingPos =  door.transform.position;
        Vector3 endingPos = new Vector3(startingPos.x, startingPos.y + (doorOpenHeight - doorActualHeight), startingPos.z);

        while (elapsedTime < doorTimeToOpen)
        {
        // Eseguo un'interpolazione lineare nel tempo scelto nell'editor che la porta deve metterci ad aprirsi.
            door.transform.position = Vector3.Lerp(startingPos, endingPos, elapsedTime / doorTimeToOpen);
            // Tuttavia, il giocatore può entrare nel trigger anche quando la porta non si trova nella sua posizione di chiusura, quindi la porta non deve sollevarsi sempre di un valore H,
            // ma della distanza che manca a raggiungere l'altezza H dalla posizione in cui si trova.
            elapsedTime += Time.deltaTime;
            doorActualHeight = door.transform.position.y;
            // Aggiorno il tempo trascorso del delta fra un frame e l'altro e l'altezza a cui si trova attualmente la porta.
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator ClosingDoor()
    {
        float elapsedTime = 0.0f;
        Vector3 startingPos =  door.transform.position;
        Vector3 endingPos = new Vector3(startingPos.x, closedPosition, startingPos.z);
        // La chiusura è più semplice, perché sappiamo già l'altezza in cui la porta deve spostarsi.

        while (elapsedTime < doorTimeToOpen)
        {
            door.transform.position = Vector3.Lerp(startingPos, endingPos, (elapsedTime / doorTimeToOpen));
            elapsedTime += Time.deltaTime;
            doorActualHeight = door.transform.position.y;
            // Tuttavia dobbiamo sempre aggiornare la variabile dell'altezza attuale perché possiamo uscire e rientrare dal trigger mentre si sta chiudendo ma prima che si chiuda del tutto,
            // quindi la porta deve aprirsi percorrendo solo la distanza che manca a raggiungere l'altezza H dalla posizione in cui si trova.
            yield return new WaitForEndOfFrame();
        }
    }
}
