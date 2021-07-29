using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public int roomLayout;
    public int hallwayLayout;
    public bool roomExists;
    public bool roomHaveTerminal;
    public bool roomBlockConnection;
    public bool roomHallway;
    public bool roomCargo;
    public Transform roomTransform;

    public Room()
    { 
    // La singola variabile Room rappresenta una singola porzione della stanza (la matrice di Room) associata a un Block.
    // Il nome Room può essere fuorviante visto che ciò rappresenta solo un "tassello" della stanza.

        roomLayout = 80000;
        hallwayLayout = 70000;
        // Per stanze e corridoi si segue la stessa identica logica usata per il layout del singolo blocco.
        roomExists = false;
        roomBlockConnection = false;
        roomHallway = false;
        roomHaveTerminal = false;
        roomCargo = false;
        roomTransform = null;
        // Informazioni extra su ciò che si trova nella stanza. Ne saranno aggiunte altre per generare automaticamente ulteriori dettagli come tubi, luci, ventole, ecc...

    }

    public void CalculateRoomLayout(Room[,] rooms, int posX, int posY, int size)
    {
        // Nord +100, Est +1, Sud +10, Ovest +1000

        if((posX - 1) >= 0)
        {
            if(rooms[posX - 1, posY].roomExists == true)
            {
                roomLayout += 100; 
                //C'è un'altro blocco a nord.
            }
        }
        if((posX + 1) < size)
        {
            if(rooms[posX + 1, posY].roomExists == true)
            {
                roomLayout += 10;
                //C'è un'altro blocco a sud.
            }
        }

        if((posY - 1) >= 0)
        {
            if(rooms[posX, posY - 1].roomExists == true)
            {
                roomLayout += 1000;
                //C'è un'altro blocco a ovest.
            }
        }
        if((posY + 1) < size)
        {
            if(rooms[posX, posY + 1].roomExists == true)
            {
                roomLayout += 1;
                //C'è un'altro blocco a est.
            }
        }

        //Debug.Log("Valore layout stanza (" + posX + " " + posY + ") : " + roomLayout);

    }

    public void CalculateHallwayLayout(Room[,] rooms, int posX, int posY, int size)
    {
    // Se la variabile Room è in realtà un pezzo di corridoio, il layout deve essere calcolato a parte, perché voglio
    // dei muri con un passaggio aperto per separarli dalle stanze. Quindi, in questo caso controllo solo se ci sono stanze adiacenti.

        if((posX - 1) >= 0)
        {
            if(rooms[posX - 1, posY].roomExists && !rooms[posX - 1, posY].roomHallway && rooms[posX, posY].roomHallway)
            {
                hallwayLayout += 100;
            }
        }
        if((posX + 1) < size)
        {
            if(rooms[posX + 1, posY].roomExists && !rooms[posX + 1, posY].roomHallway && rooms[posX, posY].roomHallway)
            {
                hallwayLayout += 10;
            }
        }

        if((posY - 1) >= 0)
        {
            if(rooms[posX, posY - 1].roomExists && !rooms[posX, posY - 1].roomHallway && rooms[posX, posY].roomHallway)
            {
                hallwayLayout += 1000;
            }
        }
        if((posY + 1) < size)
        {
            if(rooms[posX, posY + 1].roomExists && !rooms[posX, posY + 1].roomHallway && rooms[posX, posY].roomHallway)
            {
                hallwayLayout += 1;
            }
        }

        //Debug.Log("Valore layout corridoio (" + posX + " " + posY + ") : " + roomLayout);
    }
}
