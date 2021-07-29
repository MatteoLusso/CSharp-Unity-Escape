using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Maze
{
// Prima generazione del livello/labirinto.
    public static int[,] GenerateRandomMaze(int mazeX, int mazeY, float density)
    {
    // In ingresso ricevo la dimensione N*M della matrice di interi che andrò a generare e il limite massimo entro cui non è generato un blocco.
        int[,] maze = new int[mazeX, mazeY];

        for(int line = 0; line < mazeX; line++)
        {
            for(int column = 0; column < mazeY; column++)
            {
                int cellType = (int)Random.Range(0.0f, 100.0f);
                if(cellType <= density)
                {
                    maze[line, column] = 1;
                    // Nessun blocco.
                }
                else
                {
                    maze[line, column] = 0;
                    // Esiste un blocco, successivamente questo 1 diventa una variabile Block.
                }
            }
        }
        maze[0, 0] = 1;
        maze[mazeX - 1, mazeY - 1] = 1;
        // Il blocco in posizione (0,0) e (mazeX - 1, mazeY - 1) esistono sempre.
        // Il secondo blocco è anche quello dove si trova l'uscita. 

        return maze;
    }
}
