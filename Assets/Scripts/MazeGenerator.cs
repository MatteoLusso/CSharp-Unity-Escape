using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MazeGenerator : MonoBehaviour
{
    public LoadManager loadManager;
    public bool autoFindLoadManager;

    public int seed;
    public bool randomSeed;

    public SettingsManager playerParameters;
    public bool autoFindPlayerParameters;
    public bool randomLevelSize;
    public bool randomRoomSize;
    public bool usePlayerParameters;

    //----//

    [Range(0.0f, 100.0f)]
    public float lockersDensity;
    public GameObject[] lockers;
    [Range(0.0f, 100.0f)]
    public float lockersObjectsDensity;
    [Range(0.0f, 100.0f)]
    public float lockersImportantObjectDensity;

    //----//

    public bool addVents;
    [Range(0.0f, 100.0f)]
    public float ventsDensity;

    //----//

    public GameObject[] props;

    [Range(0.0f, 100.0f)]
    public float propsDensity;
    public float propScale;

    //----//

    public GameObject pod;
    [Range(0.0f, 100.0f)]
    public float podsDensity;
    private bool podGenerated = false;
    public Vector3 podLeftCorrection;
    public Vector3 podRightCorrection;
    public Vector3 podUpCorrection;
    public Vector3 podDownCorrection;

    //----//

    public bool addPipes;
    [Range(0.0f, 100.0f)]
    public float pipesDensity;

    //----//

    public GameObject speaker;
    public bool addSpeakers;
    [Range(0.0f, 100.0f)]
    public float speakersDensity;

    //----//

    public bool generateAtStart;
    // Generazione del livello ad ogni avvio.

    //----//

    public GameObject player;

    public float playerHeight;

    //----//

    public int maxX;
    public int maxY;
    // Massima estensione del livello.

    //----//

    [Range(0,100)]
    public float blocksDensity;
    [Range(0,100)]
    public float roomDensity;
    // Limite massimo entro il quale è generato una parte del livello o una parte di una stanza.

    //----//

    public bool shortHallways;
    // Le parti di corridoio che non collegano a un altro blocco sono trasformate in parti di stanza.

    //----//

    public int roomSize;
    // Massima estensione di una stanza.

    //----//

    public bool thereIsSpaceBetweenBlocks;
    // Esiste un intersitizio fra i vari blocchi?

    //----//

    public float distanceBetweenBlocks;
    // Distanza fra i vari blocchi che compongono il livello.

    //----//

    public GameObject blocksConnector;
    // Modello tridimensione del separatore fra i blocchi.
    public float connectorRightXCorrection;
    public float connectorRightYCorrection;
    public float connectorDownXCorrection;
    public float connectorDownYCorrection;
    // Valori necessari a correggere la posizione del separatore fra blocchi nella generazione.

    //----//

    public GameObject floorHallway;
    public GameObject floorCargo;
    public GameObject roof;
    public GameObject wall;
    // Modelli 3D dei componenti base di una stanza.
    public GameObject terminal;
    // Modello del terminale.
    public GameObject separatorWall;
    // Muro con passaggio aperto che separa i corridoi dalle stanze.
    public float separatorRightXCorrection;
    public float separatorRightYCorrection;
    public float separatorLeftXCorrection;
    public float separatorLeftYCorrection;
    public float separatorUpXCorrection;
    public float separatorUpYCorrection;
    public float separatorDownXCorrection;
    public float separatorDownYCorrection;
    // Valori per correggere la posizione finale del separatore fra stanze e corridoi.

    //----//

    public bool curvedWalls;
    // Ci sono muri curvi?
    public GameObject curvedWall;
    //Modello del muro curvo.

    //----//

    public bool addCargo;
    public GameObject[] cargo;
    public int cargoHeight;
    public float cargoYPos;

    [Range(0.0f, 100.0f)]
    public float cargoDensity;

    //----//

    public int mazeScalingForNavMesh;
    // Siccome ogni singolo elemento deve essere inscrivibile all'interno di un cubo, una volta generato il livello fa scalato in modo tale che la NavMesh riesca a essere generata.

    public bool onlyRapresentation;
    // Rappresentazione più semplice del livello fatta di planes e cubi.

    public GameObject room;
    public GameObject block;
    // Planes e cubi.

    private int[,] maze;
    // Matrice che contiene la forma del livello generato casualmente sottoforma di 0 e 1. 

    public Block[,] blockMatrix;
    // Matrice di Block. La forma finale è identica alla matrice di interi, ma contiene più informazioni.

    private bool[,] wasHere;
    private bool[,] correctPath;
    // Variabili utilizzate dall'algoritmo che "risolve" il labirinto.

    private int noLoopCounter = 0;
    // Variabile che evita loop infiniti se la generazione del livello fallisce.

    private List<Transform> roomCenters;
    // Lista di tutte le posizione dei centri di ogni stanzastanza (o centri dei blocchi). È utilizzata per posizionare casualmente il giocatore all'inizio della partita.

    private bool generationStop = false;

    private bool terminalAlreadyGeneratedInThisBlock;
    // Variabile usata per evitare di generare più di un terminale per blocco.

    private int podCounter;
    private int importantObjectsCounter;

    private bool loadingSavegame;

    public GameObject enemy;
    public int maxEnemies;
    public float enemyHeight;

    private void Awake()
    {
        if(generateAtStart)
        {
            Generation();
        }
    }

    public bool IsGenerationEnded()
    {
        return generationStop;
    }

    private void LoadPlayer()
    {

    }

    public void Generation()
    {
    // Algoritmo per la generazione del livello.

        if(autoFindLoadManager)
        {
            loadManager = GameObject.FindGameObjectWithTag("Load Manager").GetComponent<LoadManager>();
        }

        //Debug.Log(loadManager.IsLoading());

        /*loadingSavegame = loadManager.IsLoading();
        loadManager.SetLoading(false);*/

        //----// Questi sono i parametri impostati nel menù principale.

        if(autoFindPlayerParameters)
        {
            playerParameters = GameObject.FindGameObjectWithTag("Settings").GetComponent<SettingsManager>();
        }
        if(usePlayerParameters)
        {
            //Debug.Log("Impostazioni del giocatore");
            blocksDensity = playerParameters.blocksDensity;
            //Debug.Log("nuovo blockdensity: " + blocksDensity);
            roomDensity = playerParameters.roomsDensity;
            propsDensity = playerParameters.objectsDensity;

            maxEnemies -= (int)playerParameters.enemiesDensity;
            loadingSavegame = playerParameters.loadGame;
        }

        //----// Per generare il livello uso un seme casuale. Se invece carico la partita, il seme deve essere caricato da file.

        if(randomSeed && !loadingSavegame)
        {
            seed = Random.Range(int.MinValue, int.MaxValue);
        }
        else if(loadingSavegame)
        {
            loadManager.Load();
            seed = loadManager.GetSavedData().level_Seed;
        }

        Random.InitState(seed);

        //----// L'estensione del livello e di ogni stanza è sempre casuale entro certi parametri impostati nello script. Il giocatore non può cambiarli.

        if(randomLevelSize)
        {
            maxX = (int)Random.Range(3.0f, 6.99f);
            maxY = (int)Random.Range(3.0f, 6.99f);
            //Debug.Log("Dimensione livello casuale");
        }
        if(randomRoomSize)
        {
            roomSize = (int)Random.Range(5.0f, 10.99f);
            // Questi valori sono un buon compromesso tra un'estensione sufficientemente grande ma non troppo da rendere il caricamento iniziale troppo lento.
            //Debug.Log("Dimensione stanza casuale");
        }

        //----// 

        ClearEditor();

        //----//

        GenerateMaze:
        // Utilizzo le etichette al posto di un ciclo while o for.

        if(noLoopCounter > 10000)
        {
            goto GenerationFailed;
        }

        maze = Maze.GenerateRandomMaze(maxX, maxY, blocksDensity);
        // Generazione del labirinto.

        if(!ExitIsReachable(Vector2.zero))
        {
        // Controllo se dal blocco in posizione (0, 0) si può raggiungere l'uscita che, per semplicità,
        // è sempre situata nel blocco più in basso a destra.

            //IncreaseDensity();
            noLoopCounter++;
            goto GenerateMaze;
            // Se l'uscita non è raggiungibile, rigenero il livello;
        }
        else
        {
            goto GenerationEnded;
            // altrimenti esco da questo loop.
        }

        GenerationFailed:

        Debug.Log("Generation failed");
        noLoopCounter = 0;
        return;

        //----//

        GenerationEnded:

        PolishMaze();
        // Algoritmo che elimina i blocchi irraggiungibili.

        //----//

        ConvertToBlocks();

        //----//

        GenerateRooms();
        // Conversione della matrice di interi in una matrice di blocchi e generazione delle stanze di ogni blocco.

        //----//

        if(onlyRapresentation)
        {
            DrawMaze();
            // Questo algoritmo serve solo quando si genera un livello nell'editor e si vuole una semplice rappresentazione del livello per osservarne la forma.
        }
        else
        {
            podCounter = 0;
            importantObjectsCounter = 0;
            DrawAdvancedMaze();

            if(podCounter == 0 || importantObjectsCounter == 0)
            {
                noLoopCounter++;
                ClearEditor();
                seed = (int)Random.Range(0.0f, Mathf.Infinity);
                goto GenerateMaze;
                //Se mancano gli oggetti fondamentali ai fini del gameplay rigenero da capo il livello con un seme diverso;
            }

            AddProps();

            GenerateNavMesh(GameObject.Find("Level").GetComponent<NavMeshSurface>());
            // Genero la NavMesh (sono stati scaricati dei componenti per poter fare ciò attraverso uno script e non il tasto Bake dell'editor).

            if(!loadingSavegame)
            {
                InstantiatePlayerInStartingPosition();

                InstantiateEnemies();
            }
            else
            {
                InstantiatePlayerInLoadedPosition();
                InstantiateLoadedEnemies();
            }

            generationStop = true;
        }

    }

    //----//

    private void ActivateLockerObjects(GameObject locker)
    {
        // I prefeab degli armadietti contengono in gerarchia degli oggetti disattivati.
        // Attivandoli in maniera casuale il contenuto di ogni armadietto istanziato appare diverso.

        for(int i = 1; i <= 13; i++)
        {
            if(Random.Range(0.0f, 100.0f) <= lockersObjectsDensity)
            {
                foreach(Transform child in locker.transform)
                {
                    if(child.gameObject.name == ("Object" + i))
                    {
                        child.gameObject.SetActive(true);
                    }
                }
            }
        }

        if(Random.Range(0.0f, 100.0f) <= lockersImportantObjectDensity)
        {
            foreach(Transform child in locker.transform)
            {
                if(child.gameObject.name == "Important")
                {
                    child.gameObject.SetActive(true);
                    importantObjectsCounter++;
                    // QUesto oggetto è fondamentale per terminare la partita, per questo ho un contatore che conta quanti ne sono stati attivati.
                }
            }
        }
    }

    //----//

    private void ScaleMaze(GameObject toScale, int newScale)
    {
    // Funzione che "scala" l'intero livello in modo che si possa generare la NavMesh.
        toScale.transform.localScale = new Vector3(newScale, newScale, newScale);
    }

    public void ClearEditor()
    {
        GameObject levelToDestroy = GameObject.Find("Level");
        GameObject playerToDestroy = GameObject.Find("Player");
        GameObject enemiesToDestroy = GameObject.Find("Enemies");

        if(levelToDestroy != null)
        {
            DestroyImmediate(levelToDestroy);
        }

        if(playerToDestroy != null)
        {
            DestroyImmediate(playerToDestroy);
        }

        if(enemiesToDestroy != null)
        {
            DestroyImmediate(enemiesToDestroy);
        }
        // Elimina il livello generato nell'editor.

    }
    private void PolishMaze()
    {
        for(int line = 0; line < maxX; line++)
        {
            for(int column = 0; column < maxY; column++)
            {
                if(maze[line, column] == 1)
                {
                    if(!ExitIsReachable(new Vector2(line, column)))
                    {
                    // Da qualsiasi blocco che esista cerco di raggiunge l'uscita in basso a destra, se non si può fare elimino quel blocco.
                        maze[line, column] = 0;
                    }
                }
            }
        }
    }

    public void DrawAdvancedMaze()
    {
        GameObject mazeParent = new GameObject("Level");
        // GameObject vuoto che contiene tutti gli elementi del livello.
        mazeParent.transform.position = Vector3.zero;
        roomCenters = new List<Transform>();
        // Lista delle posizioni dei centri stanza (utile per spostare casualmente il player a inizio partita).
        NavMeshSurface navSurface = mazeParent.AddComponent(typeof(NavMeshSurface)) as NavMeshSurface;

        for(int line = 0; line < maxX; line++)
        {
            for(int column = 0; column < maxY; column++)
            {
            // Scorro la matrice di Block.
                terminalAlreadyGeneratedInThisBlock = false;
                // Per ogni blocco posso generare almeno un terminale.

                if(blockMatrix[line, column] != null)
                {
                    //GameObject blockParent = new GameObject("Block " + (((line * maxY) + column)));
                    GameObject blockParent = new GameObject("Block(" + line + "-" + column + ")");
                    // GameObject vuoto che contiene tutti gli elementi del singolo blocco.
                    blockParent.transform.position = new Vector3((line + 1) * roomSize / 2, 0.0f, (column + 1) * roomSize / 2);
                    blockParent.transform.parent = mazeParent.transform;
                    // Che poi diventa figlio dell'oggetto Maze.

                    GameObject wallsParent = new GameObject("Walls");
                    wallsParent.transform.position = new Vector3((line + 1) * roomSize / 2, 0.0f, (column + 1) * roomSize / 2);
                    wallsParent.transform.parent = blockParent.transform;

                    GameObject floorsParent = new GameObject("Floors");
                    floorsParent.transform.position = new Vector3((line + 1) * roomSize / 2, 0.0f, (column + 1) * roomSize / 2);
                    floorsParent.transform.parent = blockParent.transform;

                    GameObject roofsParent = new GameObject("Roofs");
                    roofsParent.transform.position = new Vector3((line + 1) * roomSize / 2, 0.0f, (column + 1) * roomSize / 2);
                    roofsParent.transform.parent = blockParent.transform;
                    // Stessa cosa per muri, pavimenti e soffitti.

                    GameObject lockersParent = new GameObject("Lockers");
                    lockersParent.transform.position = new Vector3((line + 1) * roomSize / 2, 0.0f, (column + 1) * roomSize / 2);
                    lockersParent.transform.parent = blockParent.transform;


                    float newWallX;
                    float newWallY;
                    GameObject newWall;
                    GameObject newTerminal;

                    for(int x = 0; x < roomSize; x++)
                    {
                        for(int y = 0; y < roomSize; y++)
                        {
                        // Ora scorro la matrice di Room di ogni singolo blocco.
                            if(blockMatrix[line, column].roomsMatrix[x, y].roomExists)
                            {
                                float newPavementX = (line * roomSize) + (1 * x) + 0.5f;
                                float newPavementY = (column * roomSize) + (1 * y) + + 0.5f;
                                // Questa formule sono usate per convertire le righe e le colonne delle due matrici in coordinate nello spazio in cui istanziare l'oggetto.

                                GameObject newFloor = null;
                                //bool isHallway = false;

                                if(blockMatrix[line, column].roomsMatrix[x, y].roomHallway)
                                {
                                    newFloor = Instantiate(floorHallway, new Vector3(newPavementX, 0.0f,newPavementY), Quaternion.identity);
                                    //isHallway = true;

                                    if(x == (int)roomSize / 2 && y == (int)roomSize / 2)
                                    {
                                        roomCenters.Add(newFloor.transform);
                                        // Se questo oggetto è stato istanziato quando ci troviavamo nel "centro" della matrice, allora è un centro stanza.
                                    }
                                }
                                else
                                {
                                    newFloor = Instantiate(floorCargo, new Vector3(newPavementX, 0.0f,newPavementY), Quaternion.identity);
                                    
                                    // ZONA CARGO

                                    Vector3 cargoPos;
                                    int randomCargo;
                                    if(addCargo)
                                    {
                                        if(x > 0 && x < roomSize - 1 && y > 0 && y < roomSize - 1)
                                        {
                                        // Non devo andare oltre gli indici, quindi controllo solo i pavimenti che hanno indice X e Y compresi fra 1 e roomSize - 1.
                                            if(blockMatrix[line, column].roomsMatrix[x - 1, y - 1].roomExists && !blockMatrix[line, column].roomsMatrix[x - 1, y - 1].roomHallway)
                                            {
                                            // Presa una casella della matrice di Room, per prima cosa controllo che la Room una riga e colonna indietro esista e non sia un corridoio...
                                                if((blockMatrix[line, column].roomsMatrix[x - 1, y].roomExists && !blockMatrix[line, column].roomsMatrix[x - 1, y].roomHallway) && (blockMatrix[line, column].roomsMatrix[x, y - 1].roomExists && !blockMatrix[line, column].roomsMatrix[x, y - 1].roomHallway))
                                                {
                                                // ... poi controllo se anche la Room sopra e quella a sinistra 
                                                // O O
                                                // O X -> X è la casella in cui siamo e O sono le caselle adiacenti che controlliamo.

                                                    cargoPos = newFloor.transform.Find("CargoPos3").transform.position;
                                                    // nei prefab dei pavimenti ci sono dei GameObject vuoti la cui posizione X e Z viene usata per istanzare il modello del carico.

                                                    if(Random.Range(0.0f, 100.0f) <= cargoDensity)
                                                    {
                                                    // con quest'if decido la densità del carico.

                                                        for(int i = 1; i <= (int)Random.Range(1.0f, ((float)cargoHeight + 0.99f)); i++)
                                                        {
                                                        // Mentre ora è casualmente impostato quante casse ci saranno accatastate una sopra l'altra.
                                                            randomCargo = (int)Random.Range(0.0f, ((float)cargo.Length - 0.01f));
                                                            cargoPos.y = (2 * cargoYPos * (i - 1)) + cargoYPos;
                                                            // I diversi modelli di casse devono avere altezza costante, così si può calcolare in base al punto in cui devono trovarsi.
                                                            Instantiate(cargo[randomCargo], cargoPos, Quaternion.Euler(0.0f, 90.0f * (int)Random.Range(0.0f, 3.99f), 0.0f), newFloor.transform);
                                                        }
                                                    }
                                                }
                                            }

                                            if(blockMatrix[line, column].roomsMatrix[x - 1, y + 1].roomExists && !blockMatrix[line, column].roomsMatrix[x - 1, y + 1].roomHallway)
                                            {
                                            // Stesso ragionamento per gli altri tre angoli.

                                                if((blockMatrix[line, column].roomsMatrix[x - 1, y].roomExists && !blockMatrix[line, column].roomsMatrix[x - 1, y].roomHallway) && (blockMatrix[line, column].roomsMatrix[x, y + 1].roomExists && !blockMatrix[line, column].roomsMatrix[x, y + 1].roomHallway))
                                                {
                                                    cargoPos = newFloor.transform.Find("CargoPos1").transform.position;

                                                    if(Random.Range(0.0f, 100.0f) <= cargoDensity)
                                                    {
                                                        for(int i = 1; i <= (int)Random.Range(1.0f, ((float)cargoHeight + 0.99f)); i++)
                                                        {
                                                            randomCargo = (int)Random.Range(0.0f, ((float)cargo.Length - 0.01f));
                                                            cargoPos.y = (2 * cargoYPos * (i - 1)) + cargoYPos;
                                                            Instantiate(cargo[randomCargo], cargoPos, Quaternion.Euler(0.0f, 90.0f * (int)Random.Range(0.0f, 3.99f), 0.0f), newFloor.transform);
                                                        }
                                                    }
                                                }
                                            }

                                            if(blockMatrix[line, column].roomsMatrix[x + 1, y + 1].roomExists && !blockMatrix[line, column].roomsMatrix[x + 1, y + 1].roomHallway)
                                            {
                                                if((blockMatrix[line, column].roomsMatrix[x, y + 1].roomExists && !blockMatrix[line, column].roomsMatrix[x, y + 1].roomHallway) && (blockMatrix[line, column].roomsMatrix[x + 1, y].roomExists && !blockMatrix[line, column].roomsMatrix[x + 1, y].roomHallway))
                                                {
                                                    cargoPos = newFloor.transform.Find("CargoPos2").transform.position;

                                                    if(Random.Range(0.0f, 100.0f) <= cargoDensity)
                                                    {
                                                        for(int i = 1; i <= (int)Random.Range(1.0f, ((float)cargoHeight + 0.99f)); i++)
                                                        {
                                                            randomCargo = (int)Random.Range(0.0f, ((float)cargo.Length - 0.01f));
                                                            cargoPos.y = (2 * cargoYPos * (i - 1)) + cargoYPos;
                                                            Instantiate(cargo[randomCargo], cargoPos, Quaternion.Euler(0.0f, 90.0f * (int)Random.Range(0.0f, 3.99f), 0.0f), newFloor.transform);
                                                        }
                                                    }
                                                }
                                            }

                                            if(blockMatrix[line, column].roomsMatrix[x + 1, y - 1].roomExists && !blockMatrix[line, column].roomsMatrix[x + 1, y - 1].roomHallway)
                                            {   
                                                if((blockMatrix[line, column].roomsMatrix[x, y - 1].roomExists && !blockMatrix[line, column].roomsMatrix[x, y - 1].roomHallway) && (blockMatrix[line, column].roomsMatrix[x + 1, y].roomExists && !blockMatrix[line, column].roomsMatrix[x + 1, y].roomHallway))
                                                {
                                                    cargoPos = newFloor.transform.Find("CargoPos4").transform.position;

                                                    if(Random.Range(0.0f, 100.0f) <= cargoDensity)
                                                    {
                                                        for(int i = 1; i <= (int)Random.Range(1.0f, ((float)cargoHeight + 0.99f)); i++)
                                                        {
                                                            randomCargo = (int)Random.Range(0.0f, ((float)cargo.Length - 0.01f));
                                                            cargoPos.y = (2 * cargoYPos * (i - 1)) + cargoYPos;
                                                            Instantiate(cargo[randomCargo], cargoPos, Quaternion.Euler(0.0f, 90.0f * (int)Random.Range(0.0f, 3.99f), 0.0f), newFloor.transform);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    // END ZONA CARGO
                                }

                                newFloor.name = "Floor(" + line + "-" + column + " | " + x + "-" + y + ")";
                                newFloor.transform.parent = floorsParent.transform;
                                blockMatrix[line, column].roomsMatrix[x, y].roomTransform = newFloor.transform;

                                GameObject newRoof = Instantiate(roof, new Vector3(newPavementX, 0.0f,newPavementY), Quaternion.identity, roofsParent.transform);
                                // Il soffitto ha le stesse coordinate della stanza, tranne che per Y.
                                AddVents(newRoof);

                                if(curvedWalls && blockMatrix[line, column].roomsMatrix[x, y].roomLayout == 81100)
                                {
                                // Le mura curve sono istanziate in base al valore di layout della Room che stiamo analizzando.
                                        newWallX = (line * roomSize) + (1 * x) + 0.5f - 1.0f;
                                        newWallY = (column * roomSize) + (1 * y) + 0.5f + 1.0f;
                                        // 0.5f è un valore correttivo costante perché ogni oggetto è inscrivibile in un cubo 1x1x1. Successivamente si trasla il necessario per X e Z (principalmente sono andato per tentativi).
                                        newWall = Instantiate(curvedWall, new Vector3(newWallX, 0.0f,newWallY), Quaternion.Euler(0.0f, -90.0f, 0.0f), wallsParent.transform);
                                        newWall.name = "CurvedWall(" + line + "-" + column + " | " + x + "-" + y + " UL)";
                                }
                                else if(curvedWalls && blockMatrix[line, column].roomsMatrix[x, y].roomLayout == 81010)
                                {
                                        newWallX = (line * roomSize) + (1 * x) + 0.5f - 1.0f;
                                        newWallY = (column * roomSize) + (1 * y) + 0.5f;
                                        newWall = Instantiate(curvedWall, new Vector3(newWallX, 0.0f,newWallY), Quaternion.Euler(0.0f, 180.0f, 0.0f), wallsParent.transform);
                                        newWall.name = "CurvedWall(" + line + "-" + column + " | " + x + "-" + y + " DL)";
                                }
                                else if(curvedWalls && blockMatrix[line, column].roomsMatrix[x, y].roomLayout == 80101)
                                {
                                    newWallX = (line * roomSize) + (1 * x) + 0.5f;
                                    newWallY = (column * roomSize) + (1 * y) + 0.5f + 1.0f;
                                    newWall = Instantiate(curvedWall, new Vector3(newWallX, 0.0f,newWallY), Quaternion.identity, wallsParent.transform);
                                    newWall.name = "CurvedWall(" + line + "-" + column + " | " + x + "-" + y + " UR)";
                                }
                                else if(curvedWalls && blockMatrix[line, column].roomsMatrix[x, y].roomLayout == 80011)
                                {
                                    newWallX = (line * roomSize) + (1 * x) + 0.5f;
                                    newWallY = (column * roomSize) + (1 * y) + 0.5f;
                                    newWall = Instantiate(curvedWall, new Vector3(newWallX, 0.0f,newWallY), Quaternion.Euler(0.0f, 90.0f, 0.0f), wallsParent.transform);
                                    newWall.name = "CurvedWall(" + line + "-" + column + " | " + x + "-" + y + " DR)";
                                }

                                else{

                                    for(int k = 1; k <= 4; k++)
                                    {
                                        if(Digits.GetDigitInPosition(blockMatrix[line, column].roomsMatrix[x, y].roomLayout, k) == 0)
                                        {
                                        //Debug.Log(Digits.GetDigitInPosition(blockMatrix[line, column].roomsMatrix[x, y].roomLayout, k) + " | " +  Digits.GetDigitInPosition(blockMatrix[line, column].blockLayout, k));

                                        // Utilizzo la funzione per conoscere il valore di una cifra in una determinata posizione per "leggere" in quale lato bisogna istanziare un muro.
                                            switch(k)
                                            {
                                            // Siccome sono in un ciclo for che scorre 4 cifre, avrò 4 (le cifre) casi ognuno con due possibilità (le cifre possono valere 0 o 1).

                                                case 1:     newWallX = (line * roomSize) + (1 * x) + 0.5f - 1.0f;
                                                            newWallY = (column * roomSize) + (1 * y) + 0.5f + 1.0f;

                                                            if(!podGenerated && Random.Range(0.0f, 100.0f) <= podsDensity && x == (int)roomSize / 2 && y == roomSize - 1 && blockMatrix[line, column].IsBorder() &&  column + 1 == maxY)
                                                            {
                                                                newWall = Instantiate(pod, new Vector3(newWallX + podRightCorrection.x, 0.0f + podRightCorrection.y, newWallY + podRightCorrection.z), Quaternion.Euler(0.0f, 90.0f, 0.0f), wallsParent.transform);
                                                                newWall.name = "Pod(" + line + "-" + column + " | " + x + "-" + y + " R)";
                                                                podCounter++;
                                                                //podGenerated = true;
                                                            }
                                                            else
                                                            {
                                                                newWall = Instantiate(wall, new Vector3(newWallX, 0.0f,newWallY), Quaternion.Euler(0.0f, 90.0f, 0.0f), wallsParent.transform);
                                                                newWall.name = "Wall(" + line + "-" + column + " | " + x + "-" + y + " R)";

                                                                if(blockMatrix[line, column].roomsMatrix[x, y].roomHaveTerminal && !terminalAlreadyGeneratedInThisBlock)
                                                                {
                                                                    newTerminal = Instantiate(terminal, new Vector3(newWallX, 0.0f,newWallY), Quaternion.Euler(0.0f, 90.0f, 0.0f), wallsParent.transform);
                                                                    newTerminal.name = "Terminal(" + line + "-" + column + " | " + x + "-" + y + " R)";
                                                                    terminalAlreadyGeneratedInThisBlock = true;
                                                                    // Istanzio un terminale se la Room che sto analizando ha la variabile roomHaveTerminal a true.
                                                                    //Per evitare di avere un terminale in ogni muro di quella porzione di stanza, rendo true la variabile terminalAlreadyGeneratedInThisBlock;
                                                                }
                                                                else
                                                                {
                                                                    AddPipes(newWall);

                                                                    if(Random.Range(0.0f, 100.0f) <= lockersDensity)
                                                                    {
                                                                        int lockersIndex = (int)Random.Range(0.0f, lockers.Length - 0.01f);
                                                                        GameObject newLocker = Instantiate(lockers[lockersIndex], newWall.transform.Find("LockerPos1").transform.position, Quaternion.Euler(0.0f, 90.0f, 0.0f), lockersParent.transform);
                                                                        ActivateLockerObjects(newLocker);
                                                                    }
                                                                }

                                                                if(addSpeakers && Random.Range(0.0f, 100.0f) <= speakersDensity)
                                                                {
                                                                    Instantiate(speaker, newWall.transform.Find("SpeakerPos" + (int)Random.Range(1.0f, 2.99f)).transform.position, Quaternion.Euler(0.0f, 90.0f, 0.0f), newWall.transform);
                                                                }
                                                            }
                                                            break;

                                                case 2:     newWallX = (line * roomSize) + (1 * x) + 0.5f;
                                                            newWallY = (column * roomSize) + (1 * y) + 0.5f + 1.0f;

                                                            if(!podGenerated && Random.Range(0.0f, 100.0f) <= podsDensity && x == roomSize - 1 && y == (int)roomSize / 2 && blockMatrix[line, column].IsBorder() && line + 1 == maxX)
                                                            {
                                                                newWall = Instantiate(pod, new Vector3(newWallX + podDownCorrection.x, 0.0f + podDownCorrection.y, newWallY + podDownCorrection.z), Quaternion.Euler(0.0f, 180.0f, 0.0f), lockersParent.transform);
                                                                newWall.name = "Pod(" + line + "-" + column + " | " + x + "-" + y + " D)";
                                                                podCounter++;
                                                                //podGenerated = true;
                                                            }
                                                            else
                                                            {
                                                                newWall = Instantiate(wall, new Vector3(newWallX, 0.0f,newWallY), Quaternion.Euler(0.0f, 180.0f, 0.0f), wallsParent.transform);
                                                                newWall.name = "Wall(" + line + "-" + column + " | " + x + "-" + y + " D)";

                                                                if(blockMatrix[line, column].roomsMatrix[x, y].roomHaveTerminal && !terminalAlreadyGeneratedInThisBlock)
                                                                {
                                                                    newTerminal = Instantiate(terminal, new Vector3(newWallX, 0.0f,newWallY), Quaternion.Euler(0.0f, 180.0f, 0.0f), wallsParent.transform);
                                                                    newTerminal.name = "Terminal(" + line + "-" + column + " | " + x + "-" + y + " D)";
                                                                    terminalAlreadyGeneratedInThisBlock = true;
                                                                }
                                                                else
                                                                {
                                                                    AddPipes(newWall);

                                                                    if(Random.Range(0.0f, 100.0f) <= lockersDensity)
                                                                    {
                                                                        int lockersIndex = (int)Random.Range(0.0f, lockers.Length - 0.01f);
                                                                        GameObject newLocker = Instantiate(lockers[lockersIndex], newWall.transform.Find("LockerPos1").transform.position, Quaternion.Euler(0.0f, 180.0f, 0.0f), lockersParent.transform);
                                                                        ActivateLockerObjects(newLocker);
                                                                    }
                                                                }

                                                                if(addSpeakers && Random.Range(0.0f, 100.0f) <= speakersDensity)
                                                                {
                                                                    Instantiate(speaker, newWall.transform.Find("SpeakerPos" + (int)Random.Range(1.0f, 2.99f)).transform.position, Quaternion.Euler(0.0f, 180.0f, 0.0f), newWall.transform);
                                                                }
                                                            }
                                                            break;

                                                case 3:     newWallX = (line * roomSize) + (1 * x) + 0.5f - 1.0f;
                                                            newWallY = (column * roomSize) + (1 * y) + 0.5f;

                                                            if(!podGenerated && Random.Range(0.0f, 100.0f) <= podsDensity && x == 0 && y == (int)roomSize / 2 && blockMatrix[line, column].IsBorder() && line - 1 == -1)
                                                            {
                                                                newWall = Instantiate(pod, new Vector3(newWallX - podUpCorrection.x, 0.0f + podUpCorrection.y ,newWallY - podUpCorrection.z), Quaternion.identity, wallsParent.transform);
                                                                newWall.name = "Pod(" + line + "-" + column + " | " + x + "-" + y + " U)";
                                                                podCounter++;
                                                                //podGenerated = true;
                                                            }
                                                            else
                                                            {
                                                                newWall = Instantiate(wall, new Vector3(newWallX, 0.0f,newWallY), Quaternion.identity, wallsParent.transform);
                                                                newWall.name = "Wall(" + line + "-" + column + " | " + x + "-" + y + " U)";

                                                                if(blockMatrix[line, column].roomsMatrix[x, y].roomHaveTerminal && !terminalAlreadyGeneratedInThisBlock)
                                                                {
                                                                    newTerminal = Instantiate(terminal, new Vector3(newWallX, 0.0f,newWallY), Quaternion.identity, wallsParent.transform);
                                                                    newTerminal.name = "Terminal(" + line + "-" + column + " | " + x + "-" + y + " U)";
                                                                    terminalAlreadyGeneratedInThisBlock = true;
                                                                } 
                                                                else
                                                                {
                                                                    AddPipes(newWall);

                                                                    if(Random.Range(0.0f, 100.0f) <= lockersDensity)
                                                                    {
                                                                        int lockersIndex = (int)Random.Range(0.0f, lockers.Length - 0.01f);
                                                                        GameObject newLocker = Instantiate(lockers[lockersIndex], newWall.transform.Find("LockerPos1").transform.position, Quaternion.identity, lockersParent.transform);
                                                                        ActivateLockerObjects(newLocker);
                                                                    }
                                                                } 

                                                                if(addSpeakers && Random.Range(0.0f, 100.0f) <= speakersDensity)
                                                                {
                                                                    Instantiate(speaker, newWall.transform.Find("SpeakerPos" + (int)Random.Range(1.0f, 2.99f)).transform.position, Quaternion.identity, newWall.transform);
                                                                } 
                                                            }
                                                            break;

                                                case 4:     newWallX = (line * roomSize) + (1 * x) + 0.5f;
                                                            newWallY = (column * roomSize) + (1 * y) + 0.5f;

                                                            if(!podGenerated && Random.Range(0.0f, 100.0f) <= podsDensity && x == (int)roomSize / 2 && y == 0 && blockMatrix[line, column].IsBorder() && column - 1 == -1)
                                                            {
                                                                newWall = Instantiate(pod, new Vector3(newWallX + podLeftCorrection.x, 0.0f + podLeftCorrection.y, newWallY + podLeftCorrection.z), Quaternion.Euler(0.0f, -90.0f, 0.0f), wallsParent.transform);
                                                                newWall.name = "Pod(" + line + "-" + column + " | " + x + "-" + y + " L)";
                                                                podCounter++;
                                                                //podGenerated = true;
                                                            }
                                                            else
                                                            {
                                                                newWall = Instantiate(wall, new Vector3(newWallX, 0.0f,newWallY), Quaternion.Euler(0.0f, -90.0f, 0.0f), wallsParent.transform);
                                                                newWall.name = "Wall(" + line + "-" + column + " | " + x + "-" + y + " L)";

                                                                if(blockMatrix[line, column].roomsMatrix[x, y].roomHaveTerminal && !terminalAlreadyGeneratedInThisBlock)
                                                                {
                                                                    newTerminal = Instantiate(terminal, new Vector3(newWallX, 0.0f,newWallY), Quaternion.Euler(0.0f, -90.0f, 0.0f), wallsParent.transform);
                                                                    newTerminal.name = "Terminal(" + line + "-" + column + " | " + x + "-" + y + " L)";
                                                                    terminalAlreadyGeneratedInThisBlock = true;
                                                                }
                                                                else
                                                                {
                                                                    AddPipes(newWall);
                                                                    
                                                                    if(Random.Range(0.0f, 100.0f) <= lockersDensity)
                                                                    {
                                                                        int lockersIndex = (int)Random.Range(0.0f, lockers.Length - 0.01f);
                                                                        GameObject newLocker = Instantiate(lockers[lockersIndex], newWall.transform.Find("LockerPos1").transform.position, Quaternion.Euler(0.0f, -90.0f, 0.0f), lockersParent.transform);
                                                                        ActivateLockerObjects(newLocker);
                                                                    }
                                                                }

                                                                if(addSpeakers && Random.Range(0.0f, 100.0f) <= speakersDensity)
                                                                {
                                                                    Instantiate(speaker, newWall.transform.Find("SpeakerPos" + (int)Random.Range(1.0f, 2.99f)).transform.position, Quaternion.Euler(0.0f, -90.0f, 0.0f), newWall.transform);
                                                                } 
                                                            }
                                                            break;
                                            }
                                        }
                                    }

                                    for(int k = 1; k <= 4; k++)
                                    {
                                        if(Digits.GetDigitInPosition(blockMatrix[line, column].roomsMatrix[x, y].hallwayLayout, k) == 1)
                                        {
                                        // Ripeto lo stesso ciclo, stavolta però per i corridoi.
                                            switch(k)
                                            {
                                                case 1:     newWallX = (line * roomSize) + (1 * x) + separatorRightXCorrection;
                                                            newWallY = (column * roomSize) + (1 * y) + separatorRightYCorrection;
                                                            newWall = Instantiate(separatorWall, new Vector3(newWallX, 0.0f,newWallY), Quaternion.Euler(0.0f, 90.0f, 0.0f), wallsParent.transform);
                                                            newWall.name = "DoorWall(" + line + "-" + column + " | " + x + "-" + y + " R)";
                                                            break;
                                                case 2:     newWallX = (line * roomSize) + (1 * x) + separatorDownXCorrection;
                                                            newWallY = (column * roomSize) + (1 * y) + separatorDownYCorrection;
                                                            newWall = Instantiate(separatorWall, new Vector3(newWallX, 0.0f,newWallY), Quaternion.Euler(0.0f, 180.0f, 0.0f), wallsParent.transform);
                                                            newWall.name = "DoorWall(" + line + "-" + column + " | " + x + "-" + y + " D)";
                                                            break;
                                                case 3:     newWallX = (line * roomSize) + (1 * x) + separatorUpXCorrection;
                                                            newWallY = (column * roomSize) + (1 * y) + separatorUpYCorrection;
                                                            newWall = Instantiate(separatorWall, new Vector3(newWallX, 0.0f,newWallY), Quaternion.identity, wallsParent.transform);
                                                            newWall.name = "DoorWall(" + line + "-" + column + " | " + x + "-" + y + " U)";
                                                            break;
                                                case 4:     newWallX = (line * roomSize) + (1 * x) + separatorLeftXCorrection;
                                                            newWallY = (column * roomSize) + (1 * y) + separatorLeftYCorrection;
                                                            newWall = Instantiate(separatorWall, new Vector3(newWallX, 0.0f,newWallY), Quaternion.Euler(0.0f, -90.0f, 0.0f), wallsParent.transform);
                                                            newWall.name = "DoorWall(" + line + "-" + column + " | " + x + "-" + y + " L)";
                                                            break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Generazione interstizi fra blocchi.

                    if(thereIsSpaceBetweenBlocks)
                    {
                        blockParent.transform.position = blockParent.transform.position = new Vector3(blockParent.transform.position.x + (distanceBetweenBlocks * line), 0.0f, blockParent.transform.position.z + (distanceBetweenBlocks * column));
                        // Dopo aver instanziato tutti i modelli 3D che compongono una stanza bisogna aggiungere, se lo si vuole, degli spazi fra un blocco e l'altro. Per far ciò sposto tutti i parent che contengono questi GameObject in modo che la posizione
                        // locale di ognuno non cambi.
                        for(int k = 1; k <= 2; k++)
                        {
                        // Scorro solo le prime due cifre da destra perché è sufficiente istanziare, se necessario, un separatore a destra e in basso.

                            if(Digits.GetDigitInPosition(blockMatrix[line, column].blockLayout, k) == 1)
                            {
                                GameObject newConnector;
                                if(k == 1)
                                {
                                    float newConnectorX = (line * roomSize) + (line * distanceBetweenBlocks) + (roomSize / 2) + connectorRightXCorrection;
                                    float newConnectorY = ((column + 1) * roomSize) + (column * distanceBetweenBlocks) + (distanceBetweenBlocks / 2) + connectorRightYCorrection;
                                    newConnector = Instantiate(blocksConnector, new Vector3(newConnectorX, 0.0f, newConnectorY), Quaternion.Euler(0.0f, -90.0f, 0.0f), blockParent.transform);
                                    newConnector.name = "Connector(" + line + "-" + column + " R)";
                                    // I modelli 3D dei separatori hanno una rotazione di 0 o 90 gradi, mentre per calcolare la posizione nello spazio float si usa questa formula:
                                    // per X si moltiplica la riga della matrice in cui siamo per la dimensione della matrice della stanza (il livello non è ancora stato scalato, quindi ogni singola porzione del livello ha ogni lato che vale 1)
                                    // poi si somma il prodotto fra riga e spazio fra i blocchi più la metà della dimensione della matrice della stanza ed infine si aggiunge un piccolo valore di correzione per far combaciare tutto.
                                    // Il ragionamento è simile per Y, solo che, poiché adesso stiamo instanziando i separatori a destra del blocco, non posso far partire l'indice della colonna da zero.
                                }
                                if(k == 2)
                                {
                                    float newConnectorX = ((line + 1) * roomSize) + (line * distanceBetweenBlocks) + (distanceBetweenBlocks / 2) + connectorDownXCorrection;
                                    float newConnectorY = (column * roomSize) + (column * distanceBetweenBlocks) + (roomSize / 2) + connectorDownYCorrection;
                                    newConnector = Instantiate(blocksConnector, new Vector3(newConnectorX, 0.0f, newConnectorY), Quaternion.identity, blockParent.transform);
                                    newConnector.name = "Connector(" + line + "-" + column + " D)";
                                    // Ragionamento invertito quando si istanziano i separatori in basso. In questo caso, per Y dobbiamo considerare la prima riga a partire dall'indice 1 e le colonne a partire da 0.
                                }
                            }

                        }
                    }
                }
            }
        }
        ScaleMaze(mazeParent, mazeScalingForNavMesh);
        // Scalo l'intero livello modificando la scala del GameObject che raccoglie tutti gli altri.
    }

    public void DrawMaze()
    {
        GameObject mazeParent = new GameObject("Level");
        mazeParent.transform.position = Vector3.zero;

        for(int line = 0; line < maxX; line++)
        {
            for(int column = 0; column < maxY; column++)
            {
                if(maze[line, column] == 1)
                {
                    GameObject newBlock = Instantiate(block, Vector3.zero, Quaternion.identity, mazeParent.transform);
                    newBlock.transform.parent = mazeParent.transform;
                    newBlock.transform.localPosition = new Vector3((float)line * roomSize / 2, 0.0f, (float)column * roomSize / 2);
                    
                    int counter = 0;
                    for(int i = 0; i < roomSize; i++){
                        for(int j = 0; j < roomSize; j++)
                        {
                            if(blockMatrix[line, column].roomsMatrix[i, j].roomExists)
                            {
                                counter++;
                                GameObject newRoom = Instantiate(room, Vector3.zero, Quaternion.identity, newBlock.transform);
                                newRoom.transform.localPosition = new Vector3((float)i - (int)roomSize / 2, 0.0f, (float)j - (int)roomSize / 2);

                                newRoom.GetComponent<Renderer>().material.color = Color.blue;
                                if(blockMatrix[line, column].roomsMatrix[i, j].roomHallway)
                                {
                                    newRoom.GetComponent<Renderer>().material.color = Color.black;
                                }
                                if(blockMatrix[line, column].roomsMatrix[i, j].roomBlockConnection)
                                {
                                    newRoom.GetComponent<Renderer>().material.color = Color.yellow;
                                }
                            }
                        }
                    }
                    
                    if(line == 0 && column == 0)
                    {
                        newBlock.GetComponent<Renderer>().material.color = Color.red;
                    }
                    if(line == maxX - 1 && column == maxY - 1)
                    {
                        newBlock.GetComponent<Renderer>().material.color = Color.green;
                    }
                }
            }
        }

        ScaleMaze(mazeParent, mazeScalingForNavMesh);
    }
    // Questa funziona genera un livello più semplice fatto solo di blocchi e planes. Il posizionamento è meno preciso visto che non è una funzionalità necessaria per il gioco.

    private bool ExitIsReachable(Vector2 start)
    {
    // Funziona ricorsiva (https://en.wikipedia.org/wiki/Maze_solving_algorithm#Recursive_algorithm) per la navigazione nel livello.
        wasHere = new bool[maxX, maxY];
        correctPath = new bool[maxX, maxY];
        
        for(int line = 0; line < maxX; line++)
        {
            for(int column = 0; column < maxY; column++)
            {
                wasHere[line, column] = false;
                correctPath[line, column] = false;
            }
        }

        return RecursiveNavigation((int)start.x, (int)start.y);
    }

    private bool RecursiveNavigation(int x, int y)
    {
        if (x == maxX - 1 && y == maxY - 1)
        // L'uscita si trova sempre in basso a destra.
        {
            return true;
        }

        if (maze[x, y] != 1 || wasHere[x, y]) 
        {
            return false;  
        }

        wasHere[x, y] = true;

        if (x != 0)
        {
            if (RecursiveNavigation(x-1, y))
            {
                correctPath[x, y] = true;
                return true;
            }
        }

        if (x != maxX - 1)
        {
            if (RecursiveNavigation(x + 1, y))
            {
                correctPath[x, y] = true;
                return true;
            }
        }

        if (y != 0)
        {
            if (RecursiveNavigation(x, y - 1))
            {
                correctPath[x, y] = true;
                return true;
            }
        }

        if (y != maxY - 1)
        {
            if (RecursiveNavigation(x, y + 1))
            {
                correctPath[x, y] = true;
                return true;
            }
        }
        return false;
    }

    public void ConvertToBlocks()
    {
    // A partire dalla matrice di interi che rappresenta il livello genero quella di Block.
        blockMatrix = new Block[maxX, maxY];

        for(int line = 0; line < maxX; line++)
        {
            for(int column = 0; column < maxY; column++)
            {
                if(maze[line, column] == 1)
                {
                    blockMatrix[line, column] = new Block(maze, maxX, maxY, roomSize, line, column);
                }
            }
        }
    }

    public void GenerateRooms()
    {
        for(int line = 0; line < maxX; line++)
        {
            for(int column = 0; column < maxY; column++)
            {
                if(blockMatrix[line, column] != null)
                {
                    blockMatrix[line, column].GenerateBlockRooms(blockMatrix, roomSize, line, column, roomDensity, shortHallways);
                    // Per ogni blocco genero casualmente la matrice di Room.
                }
            }
        }
    }

    private void InstantiatePlayerInStartingPosition()
    {
    // Il giocatore inizia la partita in una posizione casuale basata sulla lista che raccoglie le Transform del centro di ogni blocco del livello.
        //Debug.Log("Nuova posizione");
        int randomPos = (int)Random.Range(0, (roomCenters.Count - 1));
        float randomRotation = Random.Range(0.0f, 360.0f);

        //Debug.Log("Centri stanza del livello: " + roomCenters.Count);
        //Debug.Log("Posizione prima dello spostamento: " + player.transform.position);

        Vector3 newPos = new Vector3(roomCenters[randomPos].transform.position.x - (0.5f * mazeScalingForNavMesh), playerHeight, roomCenters[randomPos].transform.position.z + (0.5f * mazeScalingForNavMesh));
        Quaternion newRot = Quaternion.Euler(0.0f, randomRotation, 0.0f);

        GameObject newPlayer = Instantiate(player, newPos, newRot);
        newPlayer.tag = "Player";
        newPlayer.name = "Player";

        //Debug.Log("Posizione dopo lo spostamento: " + newPlayer.transform.position);

    }

    private void InstantiatePlayerInLoadedPosition()
    {

        Vector3 newPos = new Vector3(loadManager.GetSavedData().player_Position[0], loadManager.GetSavedData().player_Position[1], loadManager.GetSavedData().player_Position[2]);
        Quaternion newRot = new Quaternion(loadManager.GetSavedData().player_Rotation[0], loadManager.GetSavedData().player_Rotation[1], loadManager.GetSavedData().player_Rotation[2], loadManager.GetSavedData().player_Rotation[3]);

        GameObject newPlayer = Instantiate(player, newPos, newRot);
        newPlayer.tag = "Player";
        newPlayer.name = "Player";

        //Debug.Log("Posizione dopo lo spostamento: " + newPlayer.transform.position);

    }

    private void InstantiateLoadedEnemies()
    {
        GameObject enemiesParent = new GameObject("Enemies");

        for(int i = 0; i < loadManager.GetSavedData().enemies_Count; i++)
        {
            Vector3 loadedPosition = new Vector3(loadManager.GetSavedData().enemies_XPositions[i], loadManager.GetSavedData().enemies_YPositions[i], loadManager.GetSavedData().enemies_ZPositions[i]);
            Quaternion loadedRotation = new Quaternion(loadManager.GetSavedData().enemies_XRotations[i], loadManager.GetSavedData().enemies_YRotations[i], loadManager.GetSavedData().enemies_ZRotations[i], loadManager.GetSavedData().enemies_WRotations[i]);

            Instantiate(enemy, loadedPosition, loadedRotation, enemiesParent.transform);
        }
    }

    private void InstantiateEnemies()
    {
        GameObject enemiesParent = new GameObject("Enemies");
        Vector3 playerPosition = GameObject.Find("Player").transform.position;
        int enemiesCounter = 0;
        int enemiesNoLoop = 0;

        InstantiateEnemies:

        if(enemiesNoLoop > 100)
        {
            goto EnemiesSpawnEnding;
        }

        for(int i = 0; i < maxEnemies; i++)
        {
            int randomPos = (int)Random.Range(0, (roomCenters.Count - 1));
            float randomRotation = Random.Range(0.0f, 360.0f);

            Vector3 newPos = new Vector3(roomCenters[randomPos].transform.position.x - (0.5f * mazeScalingForNavMesh), enemyHeight, roomCenters[randomPos].transform.position.z + (0.5f * mazeScalingForNavMesh));
            Quaternion newRot = Quaternion.Euler(0.0f, randomRotation, 0.0f);

            if(newPos.x != playerPosition.x && newPos.z != playerPosition.z)
            {
                GameObject newEnemy = Instantiate(enemy, newPos, newRot);
                newEnemy.tag = "Enemy";
                newEnemy.name = "Enemy";
                newEnemy.transform.parent = enemiesParent.transform;

                enemiesCounter++;
            }
        }

        if(enemiesCounter == 0)
        {
            enemiesNoLoop++;
            goto InstantiateEnemies;
        }

        EnemiesSpawnEnding:

        if(enemiesCounter == 0)
        {
            Debug.Log("Enemies generation failed.");
        }

    }

    public void GenerateNavMesh(NavMeshSurface surface)
    {
        surface.BuildNavMesh();
    }

    private void AddProps()
    {
        GameObject propsParent = new GameObject("Props");
        propsParent.transform.parent = GameObject.Find("Level").transform;

        foreach(GameObject roof in GameObject.FindGameObjectsWithTag("Roof"))
        {
            if(propsDensity > Random.Range(0.0f, 100.0f))
            {
                GameObject newProp = Instantiate(props[(int)Random.Range(0.0f, props.Length - 0.01f)], roof.transform.Find("SpawnPoint" + (int)Random.Range(1.0f, 4.99f)).position, Quaternion.Euler(Random.Range(-180.0f, 180.0f), Random.Range(-180.0f, 180.0f), Random.Range(-180.0f, 180.0f)), propsParent.transform);
                newProp.transform.localScale = new Vector3(propScale, propScale, propScale);
            }
        }
    }

    private void AddVents(GameObject roofWithVent)
    {
        if(addVents && Random.Range(0.0f, 100.0f) < ventsDensity)
        {
            foreach(Transform vent in roofWithVent.transform.Find("Vent").transform)
            {
                vent.gameObject.SetActive(true);
            }
        }
    }

    private void AddPipes(GameObject wallWithPipes)
    {
        if(addPipes)
        {
            for(int i = 1; i <= 6; i++)
            {
                if(Random.Range(0.0f, 100.0f) < pipesDensity)
                {
                    foreach(Transform pipe in wallWithPipes.transform.Find("Pipe" + i).transform)
                    {
                        pipe.gameObject.SetActive(true);
                    }
                }
            }
        }
    }
}