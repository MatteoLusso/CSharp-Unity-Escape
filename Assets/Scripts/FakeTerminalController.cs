//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FakeTerminalController : MonoBehaviour
{
    /*public MazeGenerator mazeGen;
    public bool autoFindMazeGenerator;*/

    public Text worldUI;

    public Transform enemyReachPoint;
    public Transform terminalPoV;
    // È un GameObject vuoto dentro il prefab del terminale. La telecamera del giocatore è spostata in quella posizione quando si usa il terminale.
    public Transform player;
    public bool autoFindPlayer;
    // Siccome i terminali sono istanziati in tempo reale, il giocatore deve essere trovato automaticamente.
    public PadController pad;
    public bool autoFindPad;
    public float maxTerminalActivationDistance;
    // Massima distanza entro la quale è possibile usare il terminale premendo E.
    public string[] validHelpInputs;
    public string[] validEnergyInputs;
    public string[] validLightInputs;
    public string[] validEmergencyInputs;
    public string[] validShutdownInputs;
    public string[] validMapInputs;
    // Poiché il terminale è simile ai vecchi PC con MS-DOS, i comandi si inseriscono da tastiera.
    //Questi vettori di stringhe servono a riconoscere tutti i possibili comandi che si possono inserire.
    public string[] intro;
    // Le stringhe di questo vettore sono mostrate "all'avvio" del terminale. È solo un elemento scenico.
    public float introLineLoadingTime;
    // Tempo che passa fra il "print" di un stringa introduttiva e l'altra.
    public int maxStringOnScreen;
    // Poiché questo script è applicato a una variabile di tipo Text, dobbiamo far combaciare il numero di righe...
    public int maxCharsInString;
    // ... e caratteri con lo spazio disponibile.
    public char terminalCursor;
    // È il carattere del cursore che lampeggia. Elemento scenico.
    public float terminalCursorSpeed;
    // Velocità del lampeggio del cursore.

    public LightsManager lightsManager;
    public bool autoFindLightsManager;

    private List<string> outputText = new List<string>();
    // Questa lista di stringhe raccoglie tutto il testo visibile sullo "schermo" del falso terminale.

    private Text screen;

    private List<SimpleAI> enemiesAI;

    private int actualLine;
    private int fakeLine;
    private string lineIntro;
    private bool terminalIsRunning;
    private bool cursorVisible;
    private bool terminalStarting;
    private bool terminalIsIdling;

    private bool lockCamera;
    private int fakeMinutesTimer;
    private float fakeSecondsTimer;

    private bool mapActive;

    private Vector3 playerCameraOldPostion;

    private void OnDrawGizmos()
    {
        Debug.DrawLine(this.transform.position, player.transform.position, Color.yellow);
    }

    void Start()
    {
        if(autoFindPlayer)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
        if(autoFindLightsManager)
        {
            lightsManager = GameObject.FindGameObjectWithTag("LightsManager").GetComponent<LightsManager>();
        }
        if(autoFindPad)
        {
            pad = GameObject.FindGameObjectWithTag("Pad").GetComponent<PadController>();
        }

        enemiesAI = new List<SimpleAI>();
        foreach(GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            enemiesAI.Add(enemy.GetComponent<SimpleAI>());
        }

        playerCameraOldPostion = player.Find("MainCamera").transform.localPosition;

        fakeMinutesTimer = Random.Range(9999, 99999);
        fakeSecondsTimer = 0.0f;
        // Valori random di un falso comando che può essere inserito nel terminale.

        actualLine = 0;
        // La riga in cui ci troviamo nella lista di stringhe.
        fakeLine = 0;
        // Ogni volta che inseriamo un comando questo valore è incrementato di 1. Elemento scenico.

        cursorVisible = false;
        terminalIsIdling = true;
        terminalIsRunning = false;
        terminalStarting = false;
        // Variabili bool che controllano il funzionamento del terminale
        // Principalmente può essere a riposo o in funzione.

        lockCamera = false;
        // Quando si usa il terminale non si può spostare il giocatore.
        mapActive = false;
        // Ancora da implementare. La mappa sul pad non è visibile finché non si interagisce con un terminale.

        screen = this.GetComponent<Text>();

        StartCoroutine("TerminalIdlingCursor");
        // Questa Corutine fa lampeggiare il cursore.
    }

    void TerminalInput()
    {
    // Funzione che controlla l'inserimento di input da tastiera.

        lineIntro = "" + fakeLine + ".  ";

        foreach (char c in Input.inputString)
        {
            if (c == '\b')
            // Se si preme backspace...
            {
                if (outputText[actualLine].Length > lineIntro.Length)
                {
                // ... e abbiamo già scritto qualcosa...
                    if(outputText[actualLine].Contains("" + terminalCursor) && outputText[actualLine].Length > lineIntro.Length + 1)
                    {
                        outputText[actualLine] = outputText[actualLine].Substring(0, outputText[actualLine].Length - 2) + terminalCursor;
                        // ... nal caso nella stringa sia presente il cursore, cancelliamo gli ultimi due caratteri e aggiungiamo il cursore,
                    }
                    else if(!outputText[actualLine].Contains("" + terminalCursor))
                    {
                        outputText[actualLine] = outputText[actualLine].Substring(0, outputText[actualLine].Length - 1);
                        // altrimenti cancelliamo solo un carattere.
                        // Per far ciò dividiamo la stringa con Substring dal primo carattere fino all'ultimo -1 o -2.
                    }
                }
            }
            else if ((c == '\n') || (c == '\r'))
            {
            // Se si preme invio...
                if(outputText[actualLine].Contains("" + terminalCursor))
                {
                // ...ed è presente il cursore nella riga,
                    string temp = "";
                    foreach(char letter in outputText[actualLine])
                    {
                        if(letter != terminalCursor)
                        temp += letter;
                        // copiamo tutta la riga in una stringa temporanea tranne il cursore
                    }
                    outputText[actualLine] = temp;
                    // e infine la ricopiamo nella riga originale (altrimenti andando a capo rimarebbe il carattere del cursore alla fine della riga precedente). 
                }

                bool printError = true;
                // Però premendo Invio si sta anche immettendo un comando, quindi dobbiamo controllare se abbiamo un'input valido
                // di vettori di stringhe che rappresentano i vari input validi e "stampare" a schermo le informazioni legate a quell'input.

                foreach(string validHelpInput in validHelpInputs)
                {
                    if(outputText[actualLine].Contains(validHelpInput))
                    {
                        printError = false;
                        PrintHelp();
                    }
                }

                foreach(string validEnergyInput in validEnergyInputs)
                {
                    if(outputText[actualLine].Contains(validEnergyInput))
                    {
                        printError = false;
                        PrintEnergy();
                    }
                }

                foreach(string validEmergencyInput in validEmergencyInputs)
                {
                    if(outputText[actualLine].Contains(validEmergencyInput))
                    {
                        printError = false;
                        PrintEmergency();
                    }
                }

                foreach(string validShutdownInput in validShutdownInputs)
                {
                    if(outputText[actualLine].Contains(validShutdownInput))
                    {
                        printError = false;
                        ShutdownTerminal();
                    }
                }

                foreach(string validLightInput in validLightInputs)
                {
                    if(outputText[actualLine].Contains(validLightInput))
                    {
                        printError = false;
                        PrintLight();
                    }
                }

                foreach(string validMapInput in validMapInputs)
                {
                    if(outputText[actualLine].Contains(validMapInput))
                    {
                        printError = false;
                        if(!mapActive)
                        {
                            ActivateMap();
                        }
                        else
                        {
                            outputText.Add("        MAP ALREADY DOWNLOADED.");
                            UpdateLineIndex();
                        }
                    }
                } 
                PrintError(printError);
                // Se l'input non è valido "stampo" un falso errore.

                outputText.Add(lineIntro);
                // Andando a capo aggiungo una nuova riga alla lista di stringhe...
                UpdateLineIndex();
                // ... e, se la lista è "piena" (ovvero ha raggiunto il numero massimo di righe che abbiamo impostato),
                // questa si comporta come una coda FIFO. La prima riga è cancellata e all'indice di tutte le altre è sottratto 1. 

                fakeLine++;
                // Ogni volta che premiamo Invio il valore della falsa riga è incrementato di 1.
            }
            else
            {
            //Se non abbiamo premuto Invio o Backspace, quel carattere va stampato, mantenendo però sempre il cursore in ultima posizione e lampeggiante.
                if(outputText[actualLine].Contains("" + terminalCursor))
                {
                // Se la riga contiene il cursore (questo lo controllo perché ho una Coroutine che mette e toglie il cursore ogni tot di tempo,
                // quindi se sono nel frame in cui c'è il cursore, questo deve rimanere quando premo un tasto)
                    string temp = "";
                    // Anche stavolta uso una stringa temporanea...
                    foreach(char letter in outputText[actualLine])
                    {
                        if(letter != terminalCursor)
                        {
                            temp += letter;
                            // ... in cui copio il contenuto della stringa attuale meno il cursore...
                        }
                    }

                    if(outputText[actualLine].Length < maxCharsInString)
                    {
                        outputText[actualLine] = temp + c + terminalCursor;
                        // ... e infine aggiungo il nuovo carattere c e il cursore in ultima posizione.
                    }
                }
                else
                {
                    if(outputText[actualLine].Length < maxCharsInString)
                    {
                        outputText[actualLine] += c;
                        // Altriment, se sono in un frame senza cursore aggiungo solo il carattere.
                    }
                }
            }
        }
        screen.text = GenerateTextFromList(outputText);
        // Ora la lista di stringhe va convertita in un'unica stringa che verrà applicata al componente Text.
    }

    

    void LateUpdate()
    {
        fakeSecondsTimer += Time.deltaTime;
        if(fakeSecondsTimer > 60.0f)
        {
            fakeMinutesTimer --;
            fakeSecondsTimer = 0.0f;
        }
        // Aggiorno il falso timer che non serve a nulla se non a fare scena.

        if(Input.GetKeyDown(KeyCode.E) && (this.transform.position - player.transform.position).magnitude < maxTerminalActivationDistance && terminalIsIdling)
        {
        // In ogni frame controllo se il giocatore ha tentato di accedere al terminale premendo E ed era abbastanza vicino.
            StopCoroutine("TerminalIdlingCursor");
            // Se il giocatore accede al PC gli viene mostrato il testo introduttivo e il cursore è disattivato per non vedersi mentre questo testo viene "stampato".
            lockCamera = true;

            terminalIsIdling = false;
            terminalStarting = true;
            terminalIsRunning = true;
            // Il terminale non è quindi più in idling ma si sta accedendo e non è ancora utilizzabile.

            StartCoroutine("TerminalStart");
            // Questa coroutine stampa il testo introduttivo poi si disattiva in automatico.
            StartCoroutine("TerminalRunningCursor");
            // Attivo il cursore lampeggiante in modalità testo.

            worldUI.gameObject.SetActive(false);
        }

        if(lockCamera)
        {

            //Debug.Log(terminalIsRunning);
            Debug.Log(playerCameraOldPostion);

            player.Find("MainCamera").transform.position = terminalPoV.position;
            player.Find("MainCamera").transform.rotation = terminalPoV.rotation;
            // Sposto il giocatore davanti al terminale.

            player.GetComponent<CharacterController>().enabled = false;
            // Mentre si usa il terminale i movimenti sono disattivati.

            if(!terminalStarting && terminalIsRunning)
            {
                TerminalInput();
                // Se il terminale ha smesso di avviarsi ed è in funzione, allora inizio ad accettare gli input da tastiera.
            }
        }

        /*if(Input.GetKeyDown(KeyCode.Escape) && terminalIsRunning)
        {
            StopCoroutine("TerminalRunningCursor");
            // Se smetto di usare il terminale, disattivo il cursore lampeggiante in modalità testo.
            lockCamera = false;

            terminalIsIdling = true;
            terminalStarting = false;
            terminalIsRunning = false;
            // Aggiorno le bool che rappresentano lo stato del terminale (ora è in idle).

            Cursor.lockState = CursorLockMode.Locked;

            player.Find("MainCamera").transform.localPosition = playerCameraOldPostion;

            player.GetComponent<CharacterController>().enabled = true;
            // Riattivo il movimento del giocatore.

            StartCoroutine("TerminalIdlingCursor");
            // E attivo il cursore lampeggiante in modalità idle (semplicemente si vede solo il cursore che lampeggia su schermo).

            worldUI.gameObject.SetActive(true);
        }*/
    }

    private void ShutdownTerminal()
    {
        StopCoroutine("TerminalRunningCursor");
        // Se smetto di usare il terminale, disattivo il cursore lampeggiante in modalità testo.
        lockCamera = false;

        terminalIsIdling = true;
        terminalStarting = false;
        terminalIsRunning = false;
        // Aggiorno le bool che rappresentano lo stato del terminale (ora è in idle).

        Cursor.lockState = CursorLockMode.Locked;

        player.Find("MainCamera").transform.localPosition = playerCameraOldPostion;

        player.GetComponent<CharacterController>().enabled = true;
        // Riattivo il movimento del giocatore.

        StartCoroutine("TerminalIdlingCursor");
        // E attivo il cursore lampeggiante in modalità idle (semplicemente si vede solo il cursore che lampeggia su schermo).

        worldUI.gameObject.SetActive(true);
    }

    IEnumerator TerminalStart()
    {
        outputText = new List<string>();
        // All'avvio del terminale resetto la lista di stringhe.
        fakeLine = 0;

        for(int i = 0; i < intro.Length; i++)
        {
            outputText.Add(intro[i]);
            actualLine++;
            screen.text = GenerateTextFromList(outputText);
            yield return new WaitForSeconds(introLineLoadingTime);
            // Stampo l'intro aspettando per ogni riga un tempo prestabilito.
        }
        outputText.Add("");
        outputText.Add("" + fakeLine + ".   ");
        fakeLine++;

        screen.text = GenerateTextFromList(outputText);
        // Stampo la riga vuota dopo l'intro.

        terminalStarting = false;
        // Il terminale ha finito di accendersi.
    }

    IEnumerator TerminalIdlingCursor()
    {   
        while(terminalIsIdling)
        {
            outputText = new List<string>();
            //Debug.Log("Idling");
            if(!terminalStarting)
            {
                if(!cursorVisible)
                {
                    outputText.Add("" + terminalCursor);
                    cursorVisible = true;
                }
                else
                {
                    outputText.Add("");   
                    cursorVisible = false;
                }
            }

            screen.text = GenerateTextFromList(outputText);

            yield return new WaitForSeconds(terminalCursorSpeed);
        }
        // Semplicemente, mentre il terminale è in idle viene mostrato e nascosto il cursore ogni tot tempo.
    }

    IEnumerator TerminalRunningCursor()
    {
    // Quando si scrive del testo il cursore deve restare come ultimo carattere della riga e lampeggiare sempre alla stessa velocità.
        while(terminalIsRunning)
        {
            if(!terminalStarting)
            {
                if(!cursorVisible)
                {
                // Se stiamo per rendere il cursore visibile...
                    bool cursorAlreadyExist = false;

                    foreach(char letter in outputText[outputText.Count - 1])
                    {
                        // ... dobbiamo controllare se nella riga è già presente il cursore (altrimenti ne avremmo due di seguito)...
                        if(letter == terminalCursor)
                        {
                            cursorAlreadyExist = true;
                        }
                    }

                    if(!cursorAlreadyExist)
                    {
                        outputText[outputText.Count - 1] += terminalCursor;
                        // ... e solo nel caso che non ci sia, lo aggiungiamo.
                    }
                    cursorVisible = true;
                    // Ora siamo nel periodo in cui il cursore è visibile.
                }
                else
                {
                // Altrimenti, se stiamo rendendo il cursore invisibile (cambio di stato),
                    string temp = "";
                    foreach(char letter in outputText[outputText.Count - 1])
                    {
                        if(letter != terminalCursor)
                        {
                            temp += letter;
                            // allora dobbiamo copiare il testo della riga eccetto il carattere del cursore.
                        }
                    }

                    outputText[outputText.Count - 1] = temp;
                    // e poi ricopiarlo nella riga originale.
                    cursorVisible = false;
                }
            }
            yield return new WaitForSeconds(terminalCursorSpeed);
        }

    }

    private void UpdateLineIndex()
    {
    // Questa funzione va chiamata ogni volta che si inserisce una nuova riga...
        if(actualLine + 1 <= maxStringOnScreen)
        {
            actualLine++;
        }
        ScrollText();
        // ... e poi controlliamo se abbiamo superato il numero massimo di righe.
    }

    private void PrintError(bool print)
    {
    // Errore mostrato a schermo se si inserisce un comando non valido.
        if(print)
        {
            outputText.Add("        ERROR: INVALID INPUT. USE <color=red>/HELP</color> TO DISPLAY COMMANDS LIST.");
            UpdateLineIndex();
        }
    }

    private void PrintLight()
    {
        outputText.Add("        UNAVAILABLE (ERROR 0x2ff09): Contact sistem administrator.");
        UpdateLineIndex();
        outputText.Add("        Thanks for using our services. Have a nice day.");
        UpdateLineIndex();
    }

    private void PrintEmergency()
    {
        outputText.Add("        WAIT FOR EMERGENCY SERVICES.");
        UpdateLineIndex();
        outputText.Add("        [Emergency Service ETA: " + fakeMinutesTimer  + " minutes]");
        UpdateLineIndex();
        outputText.Add("        Thanks for using our services. Have a nice day.");
        UpdateLineIndex();

        foreach(SimpleAI enemyAI in enemiesAI)
        {
            enemyAI.SetDestination(enemyReachPoint.position);
        }
    }

    private void ActivateMap()
    {
    // Funzione che si occuperà di rendere visibile la mappa sul pad una volta chiamata (Incompleta).
        outputText.Add("        MAP DOWNLOADED.");
        UpdateLineIndex();
        outputText.Add("        Thanks for using our services. Have a nice day.");
        UpdateLineIndex();
        pad.MapVisible(true);
        mapActive = true;
    }




    private void PrintHelp()
    {
    // Scrivendo /HELP nel terminale sono mostrati i comandi disponibili.

        outputText.Add("        BASIC COMANDS:");
        UpdateLineIndex();
        outputText.Add("");
        UpdateLineIndex();
        outputText.Add("        -> <color=white>/CALL_EMERGENCY</color>");
        UpdateLineIndex();
        outputText.Add("        -> <color=white>/DOWNLOAD_MAP</color>");
        UpdateLineIndex();
        outputText.Add("        -> <color=white>/LIGTHS_ON</color>");
        UpdateLineIndex();
        outputText.Add("        -> <color=white>/DEVIATE_POWER</color>   [WARNING: Emergency lights will be shutdown]");
        UpdateLineIndex();
        outputText.Add("        -> <color=white>/SHUTDOWN_TERMINAL</color>");
        UpdateLineIndex();
        outputText.Add("");
        UpdateLineIndex();
        outputText.Add("        Thanks for using our services. Have a nice day.");
        UpdateLineIndex();
 
    }

    private void PrintEnergy()
    {
        outputText.Add("        POWER DEVIATE TO EMERGENCY SHUTTLES.");
        UpdateLineIndex();
        outputText.Add("        Thanks for using our services. Have a nice day.");
        UpdateLineIndex();

        foreach(GameObject pod in GameObject.FindGameObjectsWithTag("Pod"))
        {
            pod.GetComponent<PodController>().UnlockPodDoor();
        }

        lightsManager.LightsOff(true);
    }

        

    private void ScrollText()
    {
        if(outputText.Count > maxStringOnScreen)
        {
        // Se le stringhe nella lista hanno superato il valore massimo impostato,
            List<string> temp = new List<string>();
            // si crea una nuova lista ...

            for(int i = 1; i < maxStringOnScreen + 1; i++)
            {
            // che parte dall'indice 1 fino all'ultima riga.*
                temp.Add(outputText[i]);
            }

            outputText = temp;

            actualLine--;
            // La riga attuale viene ridotta di 1.
            // *È fondamentale chiamare questa funzione ogni volta che è inserita una nuova riga perché parto sempre dall'indice 1,
            // quindi devo controllare se sono andato oltre il numero massimo di righe ogni volta che le righe sono aggiornate.
        }
    }

    private string GenerateTextFromList(List<string> lines)
    {
        actualLine = -1;

        string textToPrint = "";

        foreach(string line in lines)
        {
            actualLine++;
            if(actualLine < lines.Count - 1)
            {
                textToPrint += (line + "\n");
            }
            else
            {
                textToPrint += line;
            }
        }
        // Trasformazione della lista di stringhe in un'unica stringa (alla fine di ogni riga tranne l'ultima si aggiunge /n).

        return textToPrint;
    }
}