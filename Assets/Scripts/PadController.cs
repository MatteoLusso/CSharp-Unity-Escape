using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PadController : MonoBehaviour
{
    public SettingsManager settings;
    public bool autoFindSettings;
    public MenuManager mM;
    public RawImage map;

    public RawImage scannerUp;
    public RawImage scannerDown;
    public RawImage scannerLeft;
    public RawImage scannerRight;
    public Texture[] scannerSignals;

    public Material ledGreen;
    public Material ledRed;

    public float ledSlowestTime;
    public float ledFastestTime;
    public float ledMaxRadius;

    private List<Transform> enemies;

    public Text noMap;
    public Transform playerPOV;
    // Posizione della telecamera del player.
    public Vector3 padOffset;
    // Spostamento del pad rispetto alla posizione delle telecamera.
    public Vector3 rotationOffset;
    // Rotazione del pad.
    public float closeUpDistance;

    public bool menu;

    private bool padVisible;

    private Vector3 startingOffset;
    private bool mapIsVisible;

    public float minDistance;
    public float maxDistance;

    private Transform player;

    private AudioSource beep;

    public bool autoFindPlayerPOV;

    private List<Transform> terminals;
    private List<Transform> objects;
    private List<Transform> lockers;

    private float ledGreenTimer;

    private bool ledGreenOn;

    private float ledRedTimer;

    private bool ledRedOn;

    private bool rigidbodyObject;

    private bool auxiliaryEnergyFound;

    public LoadManager loadManager;
    public bool autoFindLoadManager;

    public void PlayerIsDead(bool status)
    {
        rigidbodyObject = status;
    }

    void Awake()
    {
        startingOffset = padOffset;
        mapIsVisible = false;
        PadHidden();
    }

    void Start()
    {
        if(autoFindPlayerPOV)
        {
            playerPOV = GameObject.FindGameObjectWithTag("MainCamera").transform;
            player = playerPOV.parent.gameObject.transform;
        }

        if(autoFindSettings)
        {
            settings = GameObject.FindGameObjectWithTag("Settings").gameObject.GetComponent<SettingsManager>();
        }

        rigidbodyObject = false;

        beep = this.GetComponent<AudioSource>();

        enemies = new List<Transform>();
        foreach(GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            enemies.Add(enemy.transform);
        }

        terminals = new List<Transform>();
        foreach(GameObject terminal in GameObject.FindGameObjectsWithTag("Terminal"))
        {
           terminals.Add(terminal.transform);
        }

        lockers = new List<Transform>();
        foreach(GameObject locker in GameObject.FindGameObjectsWithTag("Locker"))
        {
           lockers.Add(locker.transform);
        }

        if(autoFindLoadManager)
        {
            loadManager = GameObject.FindGameObjectWithTag("Load Manager").GetComponent<LoadManager>();
        }  

        ledGreenTimer = 0.0f;
        ledGreenOn = false;

        auxiliaryEnergyFound = false;
        if(loadManager.IsLoading())
        {
            auxiliaryEnergyFound = loadManager.GetSavedData().pad_AuxiliaryEnergyActive;
        }
    }

    public bool AuxiliaryEnergyFound()
    {
        return auxiliaryEnergyFound;
    }

    public bool IsMapVisible()
    {
        return mapIsVisible;
    }

    /*public void ForceMapStatusTo(bool status)
    {
        mapIsVisible = status;
    }*/

    private void UpdateTerminalsScanner()
    {
        Transform nearestTerminal = null;

        foreach(Transform terminal in terminals)
        {
            if(nearestTerminal == null)
            {
                nearestTerminal = terminal;
            }
            else if((this.transform.position - nearestTerminal.transform.position).magnitude > (this.transform.position - terminal.transform.position).magnitude)
            {
                nearestTerminal = terminal;
            }
        }

        float timerOnOff;

        if(nearestTerminal == null)
        {
            ledGreen.DisableKeyword("_EMISSION");

            //Debug.Log("nearestTerminal è null");

            ledGreenTimer = 0.0f;
            timerOnOff = ledGreenTimer + 1.0f;
        }
        else
        {
            float distanceToTerminal = (this.transform.position - nearestTerminal.transform.position).magnitude;

            //Debug.Log("Distanza dal terminale più vicino: " + distanceToTerminal);

            if(distanceToTerminal >= ledMaxRadius)
            {
                ledGreen.DisableKeyword("_EMISSION");

                ledGreenTimer = 0.0f;
                timerOnOff = ledGreenTimer + 1.0f;
            }
            else
            {
                timerOnOff = (ledSlowestTime - ledFastestTime) * (distanceToTerminal / ledMaxRadius);
            }
        }

        if(ledGreenTimer >= timerOnOff)
        {
            if(ledGreenOn)
            {
                ledGreen.DisableKeyword("_EMISSION");
                ledGreenOn = false;

                ledGreenTimer = 0.0f;
            }
            else
            {
                ledGreen.EnableKeyword("_EMISSION");
                ledGreenOn = true;

                ledGreenTimer = 0.0f;
            }
        }

        ledGreenTimer += Time.deltaTime;
    }

    public void ForceAuxiliaryEnergyStatusTo(bool status)
    {
        auxiliaryEnergyFound = status;
    }

    private void UpdateAuxiliaryEnergy()
    {
        //Debug.Log(lockers.Count);
        if(!auxiliaryEnergyFound)
        {
            foreach(Transform locker in lockers)
            {
                //Debug.Log("Controllando armadietto");
                if(locker.gameObject.GetComponent<LockerController>().ImportantObjectFound())
                {
                    //Debug.Log("Trovata energia");
                    auxiliaryEnergyFound = true;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        foreach(GameObject importantObject in GameObject.FindGameObjectsWithTag("Important"))
        {
            Debug.DrawLine(player.transform.position, importantObject.transform.position, Color.white);
        }
        foreach(GameObject pod in GameObject.FindGameObjectsWithTag("PodDoor"))
        {
            Debug.DrawLine(player.transform.position, pod.transform.position, Color.cyan);
        }
    }

    private void UpdateImportantObjectsScanner()
    {
        Transform nearestObject = null;

        foreach(GameObject importantObject in GameObject.FindGameObjectsWithTag("Important"))
        {
            if(nearestObject == null)
            {
                nearestObject = importantObject.transform;
            }
            else if((this.transform.position - nearestObject.transform.position).magnitude > (this.transform.position - importantObject.transform.position).magnitude)
            {
                nearestObject = importantObject.transform;
            }
        }

        float timerOnOff;

        if(nearestObject == null)
        {
            ledRed.DisableKeyword("_EMISSION");

            //Debug.Log("nearestTerminal è null");

            ledRedTimer = 0.0f;
            timerOnOff = ledRedTimer + 1.0f;
        }
        else
        {
            float distanceToObject = (this.transform.position - nearestObject.transform.position).magnitude;

            //Debug.Log("Distanza dall'oggetto importante più vicino: " + distanceToObject);

            if(distanceToObject >= ledMaxRadius)
            {
                ledRed.DisableKeyword("_EMISSION");

                ledRedTimer = 0.0f;
                timerOnOff = ledRedTimer + 1.0f;
            }
            else
            {
                timerOnOff = (ledSlowestTime - ledFastestTime) * (distanceToObject / ledMaxRadius);
            }
        }

        if(ledRedTimer >= timerOnOff)
        {
            if(ledRedOn)
            {
                ledRed.DisableKeyword("_EMISSION");
                ledRedOn = false;

                ledRedTimer = 0.0f;
            }
            else
            {
                ledRed.EnableKeyword("_EMISSION");
                ledRedOn = true;

                ledRedTimer = 0.0f;
            }
        }

        ledRedTimer += Time.deltaTime;
    }

    void LateUpdate()
    {
        // Pad Visibility On/Off

        if(!menu)
        {
            if(!mapIsVisible && settings.loadGame)
            {
                mapIsVisible = loadManager.GetSavedData().pad_MapActive;
            }

            UpdateAuxiliaryEnergy();
            //Debug.Log("Energia ausiliariatrovata: " + auxiliaryEnergyFound);

            if(Input.GetKeyDown(KeyCode.Tab))
            {
            // Premendo Tab il pad, che mostra le informazioni come la mappa, diventa visibile o invisibile.
                if(!padVisible)
                {
                    PadInGameVisible();
                    UpdateMap();
                }
                else
                {
                    PadHidden();
                    UpdateMap();
                }
            }

            if(padVisible)
            {
                PadCloseUp(Input.GetKey(KeyCode.LeftControl));
                UpdateMap();
                UpdateEnemiesScanner();
                UpdateTerminalsScanner();
                UpdateImportantObjectsScanner();
            }
        }

        else
        {
            if(!padVisible && mM.isMenuVisible())
            {
                PadMenuVisible();
            }
        }

        if(!rigidbodyObject)
        {

            Vector3 padNewPosition = playerPOV.transform.TransformPoint(padOffset);
            this.transform.position = padNewPosition;
            // Calcolo della nuova posizione nello spazio globale.

            Vector3 directionToLook = playerPOV.transform.position - this.transform.position;
            this.transform.forward = directionToLook;
            // Calcolo della nuova rotazione dopo il movimento del player.

            this.transform.Rotate(rotationOffset);
        }
    }

    private void UpdateEnemiesScanner()
    {
        Transform nearestEnemyInQ1 = null;
        Transform nearestEnemyInQ2 = null;
        Transform nearestEnemyInQ3 = null;
        Transform nearestEnemyInQ4 = null;

        foreach(Transform enemy in enemies)
        {
            float angleWithForward = Vector3.Angle(player.transform.forward, enemy.position - player.transform.position);
            float angleWithRight = Vector3.Angle(player.transform.right, enemy.position - player.transform.position);


            if(angleWithForward >= 0.0f && angleWithForward <= 70.0f)
            {
                if(nearestEnemyInQ1 == null)
                {
                    nearestEnemyInQ1 = enemy;

                }
                else if((enemy.transform.position - player.transform.position).magnitude < (nearestEnemyInQ1.position - player.transform.position).magnitude)
                {
                    nearestEnemyInQ1 = enemy;
                }
            }
            else if(angleWithRight >= 0.0f && angleWithRight <= 70.0f)
            {
                if(nearestEnemyInQ2 == null)
                {
                    nearestEnemyInQ2 = enemy;
                }
                else if((enemy.transform.position - player.transform.position).magnitude < (nearestEnemyInQ2.position - player.transform.position).magnitude)
                {
                    nearestEnemyInQ2 = enemy;
                }
            }
            else if(angleWithForward >= 110.0f && angleWithForward <= 180.0f)
            {
                if(nearestEnemyInQ3 == null)
                {
                    nearestEnemyInQ3 = enemy;
                }
                else if((enemy.transform.position - player.transform.position).magnitude < (nearestEnemyInQ3.position - player.transform.position).magnitude)
                {
                    nearestEnemyInQ3 = enemy;
                }
            }
            else if(angleWithRight >= 110.0f && angleWithRight <= 180.0f)
            {
                if(nearestEnemyInQ4 == null)
                {
                    nearestEnemyInQ4 = enemy;
                }
                else if((enemy.transform.position - player.transform.position).magnitude < (nearestEnemyInQ4.position - player.transform.position).magnitude)
                {
                    nearestEnemyInQ4 = enemy;
                }
            }
        }

        float distanceQ1 = maxDistance + 1.0f;
        float distanceQ2 = maxDistance + 1.0f;
        float distanceQ3 = maxDistance + 1.0f;
        float distanceQ4 = maxDistance + 1.0f;

        if(nearestEnemyInQ1 == null)
        {
            scannerUp.texture = scannerSignals[0];
        }
        else
        {
            distanceQ1 = (nearestEnemyInQ1.position - player.transform.position).magnitude;

            if(distanceQ1 >= maxDistance)
            {
                scannerUp.texture = scannerSignals[0];
                //Debug.Log("Level signal: 0");
            }
            else if(distanceQ1 <= minDistance)
            {
                scannerUp.texture = scannerSignals[scannerSignals.Length - 1];
                //Debug.Log("Level signal: " + (scannerSignals.Length - 1));
            }
            else
            {
                scannerUp.texture = scannerSignals[(scannerSignals.Length - 1) - (int)((distanceQ1 - minDistance) / ((maxDistance - minDistance) / (scannerSignals.Length - 1)))];
                //Debug.Log("Level signal: " + ((scannerSignals.Length - 1) - (int)((distanceQ1 - minDistance) / ((maxDistance - minDistance) / (scannerSignals.Length - 1)))));
            }
        }

        if(nearestEnemyInQ2 == null)
        {
            scannerRight.texture = scannerSignals[0];
        }
        else
        {
            distanceQ2 = (nearestEnemyInQ2.position - player.transform.position).magnitude;

            if(distanceQ2 >= maxDistance)
            {
                scannerRight.texture = scannerSignals[0];
            }
            else if(distanceQ2 <= minDistance)
            {
                scannerRight.texture = scannerSignals[scannerSignals.Length - 1];
            }
            else
            {
                scannerRight.texture = scannerSignals[(scannerSignals.Length - 1) - (int)((distanceQ2 - minDistance) / ((maxDistance - minDistance) / (scannerSignals.Length - 1)))];
            }
        }

        if(nearestEnemyInQ3 == null)
        {
            scannerDown.texture = scannerSignals[0];
        }
        else
        {
            distanceQ3 = (nearestEnemyInQ3.position - player.transform.position).magnitude;

            if(distanceQ3 >= maxDistance)
            {
                scannerDown.texture = scannerSignals[0];
            }
            else if(distanceQ3 <= minDistance)
            {
                scannerDown.texture = scannerSignals[scannerSignals.Length - 1];
            }
            else
            {
                scannerDown.texture = scannerSignals[(scannerSignals.Length - 1) - (int)((distanceQ3 - minDistance) / ((maxDistance - minDistance) / (scannerSignals.Length - 1)))];
            }
        }

        if(nearestEnemyInQ4 == null)
        {
            scannerLeft.texture = scannerSignals[0];
        }
        else
        {
            distanceQ4 = (nearestEnemyInQ4.position - player.transform.position).magnitude;

            if(distanceQ4 >= maxDistance)
            {
                scannerLeft.texture = scannerSignals[0];
            }
            else if(distanceQ4 <= minDistance)
            {
                scannerLeft.texture = scannerSignals[scannerSignals.Length - 1];
            }
            else
            {
                scannerLeft.texture = scannerSignals[(scannerSignals.Length - 1) - (int)((distanceQ4 - minDistance) / ((maxDistance - minDistance) / (scannerSignals.Length - 1)))];
            }
        }

        float[] distancesQ = new float[4];
        distancesQ[0] = distanceQ1;
        distancesQ[1] = distanceQ2;
        distancesQ[2] = distanceQ3;
        distancesQ[3] = distanceQ4;

        float lowestDistance = Mathf.Min(distancesQ);

        float newPitch = 2.0f - ((lowestDistance - minDistance) / ((maxDistance - minDistance) / 2.0f));
        if(lowestDistance > maxDistance)
        {
            newPitch = 0.0f;
        }
        else if(lowestDistance < minDistance)
        {
            newPitch = 2.0f;
        }
        beep.pitch = newPitch;
    }

    private void PadMenuVisible()
    {
        this.GetComponent<Renderer>().enabled = true;
        this.GetComponent<AudioSource>().enabled = false;
        /*foreach(Transform child in this.transform)
        {
            child.gameObject.SetActive(true);
            if(child.CompareTag("PadCanvas"))
            {
                foreach(Transform canvasElement in child)
                {
                    canvasElement.gameObject.SetActive(true);

                    if(canvasElement.CompareTag("Map") ||canvasElement.CompareTag("Scanner"))
                    {
                        canvasElement.gameObject.SetActive(false);
                    }
                }
            }
        }*/
        padVisible = true;
    }
    private void PadInGameVisible()
    {
        this.GetComponent<Renderer>().enabled = true;
        this.GetComponent<AudioSource>().enabled = true;
        foreach(Transform child in this.transform)
        {
            child.gameObject.SetActive(true);
            if(child.CompareTag("PadCanvas"))
            {
                foreach(Transform canvasElement in child)
                {
                    canvasElement.gameObject.SetActive(true);

                    if(canvasElement.CompareTag("MenuElement"))
                    {
                        canvasElement.gameObject.SetActive(false);
                    }
                }
            }
        }
        padVisible = true;
    }

    private void PadHidden()
    {
        this.GetComponent<Renderer>().enabled = false;
        this.GetComponent<AudioSource>().enabled = false;
        foreach(Transform child in this.transform)
        {
            child.gameObject.SetActive(false);
        }
        padVisible = false;
    }

    public void MapVisible(bool visibility)
    {
        mapIsVisible = visibility;
    }

    private void UpdateMap()
    {
        if(mapIsVisible)
        {
            map.gameObject.SetActive(true);
            noMap.gameObject.SetActive(false);
        }
        else
        {
            map.gameObject.SetActive(false);
            noMap.gameObject.SetActive(true);
        }
    }

    private void PadCloseUp(bool input)
    {
        if(input)
        {
            padOffset = new Vector3(0.0f, 0.0f, closeUpDistance);
            //Se tengo premuto CTRL il pad è spostato al centro dello schermo per una visione più chiara.
        }
        else
        {
            padOffset = startingOffset;
        }
    }
}