using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block
{
    public int blockLayout;
    // blockLayout è un intero inizializzato a 70000. La prima cifra non rappresenta nulla. Le altre quattro invece sono legate alla posizione del singolo blocco
    // nella matrice di Block che rappresenta la forma del livello.
    // NB: Per il momento questa variabile è pubblica, successivamente diventerà privata e ci sarà un metodo pubblico per accedervi attraverso il GameManager.

    public Room[,] roomsMatrix;
    // Ogni Block contiene una matrice di variabili Room definite a parte. In pratica, il livello finale è composto da tanti Block,
    //la cui forma varia in base alla generazione di questa matrice di Rooms.

    private bool isBorder = false;

    bool[,] wasHere;
    bool [,] correctPath;

    public Block(int[,] maze, int mazeXExt, int mazeYExt, int roomSize, int posX, int posY)
    // Per generare un Block è necessario ricevere in ingresso i seguenti parametri:
    // - una matrice di interi che rappresenta la prima generazione basilare del livello (in questo script avviene la generazione della matrice Room);
    // - il numero di righe della matrice;
    // - il numero di colonne;
    // - la dimensione della matrice di Room (non esiste un controllo, ma ovviamente deve essere costante per ogni Block);
    // - l'indice della riga della matrice di interi in cui siamo;
    // - l'indice della colonna in cui siamo.
    {
        roomsMatrix = new Room[roomSize, roomSize];
        // Generazione della matrice di stanze vuota.

        blockLayout = 70000; // North 100, East 1, South 10, West 1000
        // Inizializzazione a zero del layout del blocco,
        // e immediatamente dopo è calcolato il layout finale (la generazione del livello è già avvenuta, ora si sta convertendo la matrice di interi in una di Block casella per casella)

        if(posX == 0 || posX == mazeXExt - 1 || posY == 0 || posY == mazeYExt - 1)
        {
            isBorder = true;
        }

        if((posX - 1) >= 0)
        // Bisogna fare attenzione agli indici, quindi bisogna sempre controllare
        // di non avere un indice negativo o superiore al numero di righe o colonne - 1;
        {
            if(maze[posX - 1, posY] == 1)
            // Controllo "sopra" il blocco.
            {
                blockLayout += 100;
                //Se ne esiste uno nella stessa colonna una riga prima, aggiungo 100.
            }
        }
        if((posX + 1) < mazeXExt)
        {
            if(maze[posX + 1, posY] == 1)
            // Controllo "sotto" il blocco.
            {
                blockLayout += 10; 
                //Se esiste aggiungo 10.
            }
        }

        if((posY - 1) >= 0)
        {
            if(maze[posX, posY - 1] == 1)
            //  Controllo "a sinistra".
            {
                blockLayout += 1000;
                // Aggiungo 1000;
            }
        }
        if((posY + 1) < mazeYExt)
        {
            if(maze[posX, posY + 1] == 1)
            // "A destra".
            {
                blockLayout += 1;
                // Aggiungo 1.
            }
        }
        //Debug.Log("Valore layout blocco (" + (((posX * maze.GetLength(1)) + posY)) + ") : " + blockLayout);
    }

    public bool IsBorder()
    {
        return isBorder;
    }

    private bool RoomCenterIsReachable(int roomSize, Vector2 start)
    {
    // Funziona ricorsiva (https://en.wikipedia.org/wiki/Maze_solving_algorithm#Recursive_algorithm) per la navigazione nel livello.
        wasHere = new bool[roomSize, roomSize];
        correctPath = new bool[roomSize, roomSize];
        
        for(int line = 0; line < roomSize; line++)
        {
            for(int column = 0; column < roomSize; column++)
            {
                wasHere[line, column] = false;
                correctPath[line, column] = false;
            }
        }

        return RecursiveNavigation((int)start.x, (int)start.y, roomSize);
    }

    private bool RecursiveNavigation(int x, int y, int roomSize)
    {
        if (x == (int)roomSize / 2 && y == (int)roomSize / 2)
        {
            return true;
        }

        if (!roomsMatrix[x, y].roomExists || wasHere[x, y]) 
        {
            return false;  
        }

        wasHere[x, y] = true;

        if (x != 0)
        {
            if (RecursiveNavigation(x-1, y, roomSize))
            {
                correctPath[x, y] = true;
                return true;
            }
        }

        if (x != roomSize - 1)
        {
            if (RecursiveNavigation(x + 1, y, roomSize))
            {
                correctPath[x, y] = true;
                return true;
            }
        }

        if (y != 0)
        {
            if (RecursiveNavigation(x, y - 1, roomSize))
            {
                correctPath[x, y] = true;
                return true;
            }
        }

        if (y != roomSize - 1)
        {
            if (RecursiveNavigation(x, y + 1, roomSize))
            {
                correctPath[x, y] = true;
                return true;
            }
        }
        return false;
    }

    public void GenerateBlockRooms(Block[,] blocksMatrix, int roomSize, int posX, int posY, float density, bool reduceHallways)
    // Funzione pubblica che genera la matrice Room del Block. Da richiamare una volta completata la generazione della matrice di Block.
    // In ingresso riceve:
    // - la matrice di Block finale;
    // - il numero di righe e colonne della matrice Room;
    // - la riga e la colonna della matrice di Block in cui ci troviamo attualmente;
    // - la percentuale massima entro la quale potrebbe essere generata una stanza vuota (0 = il blocco del livello è una stanza perfettamente rettangolare, 100 = sono generati solo i corridoi)
    // - variabile booleana che trasforma le parti di corridoio che non confinano con un altro Block in parte della stanza.
    {
        //  GENERAZIONE CASUALE STANZE

        Room newRoom = new Room();
        // Inizializzazione della singola variabile Room.

        for(int x = 0; x < roomSize; x++)
        {
            for(int y = 0; y < roomSize; y++)
            {
                //Scorro le righe e le colonne della matrice di Block.
                newRoom = new Room();
                // newRoom è una variabile Room temporanea che rappresenta a tutti gli effetti una parte vuota della stanza finale, quindi questa assegnazione va ripetuta.

                int cellType = (int)Random.Range(0.0f, 100.0f);
                // Genero un valore da 0 a 100 che "deciderà" se la Room sarà vuota o no.
                if(cellType <= density)
                {
                    // Se questo valore è minore del limite che abbiamo imposto come parametro in ingresso, allora questo pezzo di stanza esiste.
                    newRoom.roomExists = true;
                }

                roomsMatrix[x, y] = newRoom;
                // Assegno la variabile Room alla matrice nella posizione corrispondente.
            }
        }

        // Generazione dei corridoi con un primo aggiornamento del layout aggiungendo i passaggi che collegano a un altro blocco,
        // poiché non vogliamo che un muro blocchi il passaggio fra un corridoio di un blocco e l'altro.

        int halfSize = (int)roomSize / 2;
        // Per comodità i corridoi sono sempre generati al "centro" della matrice di Room;
        
        for(int k = 0; k < roomSize; k++)
        {
            // quindi è sufficiente scorrere un solo valore k.
            Room newRoomUpDown = new Room();
            newRoomUpDown.roomExists = true;
            // Nella generazione precedente la stanza, che in realtà è il nostro corridoio potrebbe non essere stata generata, quindi ora imponiamo che esista.
            newRoomUpDown.roomHallway = true;
            // Questa variabile indica che questa porzione della stanza è un corridoio.


            Room newRoomLeftRight = new Room();
            newRoomLeftRight.roomExists = true;
            newRoomLeftRight.roomHallway = true;
            // Per necessità legate allo scorrimento del ciclo for, esistono due variabile temporanee che rappresentano le singole parti del corridoio verticale e orizzontale.



            if(k == 0 && posX - 1 >= 0)
            // Se siamo ai margini della matrice, allora in questo punto non dovrà esserci un muro,
            {
                if(blocksMatrix[posX - 1, posY] != null)
                {
                    newRoomUpDown.roomBlockConnection = true;
                    newRoomUpDown.roomLayout += 100;
                    // ma una porta. Quindi, se esiste un altro blocco dal lato che sto controllando (stessa logica del calcolo del layout), aggiungo il valore corrispondente alla variabile intera che rappresenta il layout
                    // e rendo vera la variabile che mi indica che è un punto di collegamento fra un blocco e láltro del livello.
                }
            }
            else if(k == roomSize - 1 && posX + 1 < blocksMatrix.GetLength(0))
            {
                if(blocksMatrix[posX + 1, posY] != null)
                {
                    newRoomUpDown.roomBlockConnection = true;
                    newRoomUpDown.roomLayout += 10;
                }
            }

            if(k == 0 && posY - 1 >= 0)
            {
                if(blocksMatrix[posX, posY - 1] != null)
                {
                    newRoomLeftRight.roomBlockConnection = true;
                    newRoomLeftRight.roomLayout += 1000;
                }
            }
            else if(k == roomSize - 1 && posY + 1 < blocksMatrix.GetLength(1))
            {
                if(blocksMatrix[posX, posY + 1] != null)
                {
                    newRoomLeftRight.roomBlockConnection = true;
                    newRoomLeftRight.roomLayout += 1;
                }
            }

            roomsMatrix[k, halfSize] = newRoomUpDown;
            roomsMatrix[halfSize, k] = newRoomLeftRight;
        }

        // Trasformazione in stanza dei corridoi che non collegano ad un altro blocco.

            if(reduceHallways)
            {
            // In questo caso si usano quattro cicli for che "scorrono" i corridoi dall'altro fino al centro, dal centro fino al basso,
            // da destra fino al centro e da sinistra fino al centro.

                if(!roomsMatrix[0, halfSize].roomBlockConnection)
                {
                    for(int k = 0; k < halfSize; k++)
                    {
                        roomsMatrix[k, halfSize].roomHallway = false;
                    }
                }

                if(!roomsMatrix[roomSize - 1, halfSize].roomBlockConnection)
                {
                    for(int k = roomSize - 1; k > halfSize; k--)
                    {
                        roomsMatrix[k, halfSize].roomHallway = false;
                    }
                }

                if(!roomsMatrix[halfSize, 0].roomBlockConnection)
                {
                    for(int k = 0; k < halfSize; k++)
                    {
                        roomsMatrix[halfSize, k].roomHallway = false;
                    }
                }

                if(!roomsMatrix[halfSize, roomSize - 1].roomBlockConnection)
                {
                    for(int k = roomSize - 1; k > halfSize; k--)
                    {
                        roomsMatrix[halfSize, k].roomHallway = false;
                    }
                }

            // Se quella parte di corridoio non ha un collegamento con l'altra stanza (ce lo dice la varibile roomBlockConnection), allora quella porzione di
            // corridio è convertita in parte della stanza.

            }


        // Calcolo del layout finale di ogni stanza e corridoi.

        for(int x = 0; x < roomSize; x++)
        {
            for(int y = 0; y < roomSize; y++)
            {
                roomsMatrix[x, y].CalculateRoomLayout(roomsMatrix, x, y, roomSize);
                roomsMatrix[x, y].CalculateHallwayLayout(roomsMatrix, x, y, roomSize);
                // Queste due funzioni si trovano nella classe Room.
            }
        }

        // Eliminazione stanze inacessibili (Bug noto: L'eliminazione è incompleta incompleta, restano parti irraggiungibili)

        for(int x = 0; x < roomSize; x++)
        {
            for(int y = 0; y < roomSize; y++)
            {
                if(roomsMatrix[x, y].roomLayout == 80000)
                // 0000 vuol dire un muro da ogni lato, quindi quel pezzo di stanza è irraggiungibile.
                {
                    roomsMatrix[x, y].roomExists = false;
                }
                else if(!RoomCenterIsReachable(roomSize, new Vector2(x, y)))
                {
                    roomsMatrix[x, y].roomExists = false;
                }
            }
        }

        // Aggiunta di terminali nei blocchi.
        
        bool terminalAlreadyGenerated = false;
        // Ci può essere un solo terminale per blocco, questa variabile evita ciò.

        for(int x = 0; x < roomSize; x++)
        {
            for(int y = 0; y < roomSize; y++)
            {
                if(roomsMatrix[x, y].roomExists && !roomsMatrix[x, y].roomHallway && !terminalAlreadyGenerated && (int)Random.Range(0.0f, 1.99f) == 1)
                {
                //  Scorro tutte le stanze della matrice di Room del blocco e, se la stanza esiste e non è un corridoio, ho il 50% di probabilità di generare un terminale.
                // C'è un minimo rischio che in nessun blocco sia generato un terminale, ma è piuttosto raro, in ogni caso andrebbe implementato un controllo per far si che ci sia almeno un terminale nel livello.
                    roomsMatrix[x, y].roomHaveTerminal = true;
                    terminalAlreadyGenerated = true;
                }
            }
        }
    }
}
