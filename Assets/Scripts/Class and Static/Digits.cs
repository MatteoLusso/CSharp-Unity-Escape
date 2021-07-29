using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Digits
{
// Classe statica con all'interno un'unica funzione
//che si occupa di restituire i valore della cifra di un intero positivo in una determinata posizione.
    public static int GetDigitInPosition(int number, int pos)
    {
        string stringInt = number.ToString();
        // Converto l'intero in testo;

        if(pos > stringInt.Length)
        {
            pos = stringInt.Length;
        }
        else if(pos < 1)
        {
            pos = 1;
        }
        // Controllo che il valore della posizione da controllare ricevuto in ingresso non sia minore di 1 o maggiore delle cifre del numero da analizzare.

        string invertedStringInt = "";

        for(int i = stringInt.Length - 1; i >= 0; i--)
        {
            invertedStringInt += stringInt[i];
        }
        // La posizione è contata da destra verso sinistra:
        // se ho 1234, la cifra in posizione 1 è 4 e così via.

        char zero = '0';
        // Carattere zero.

        //Debug.Log("Number int = " + number + " Number string = " + stringInt + " Digit in pos " + pos + " = " + ((int)invertedStringInt[pos - 1] - (int)zero));

        return (int)invertedStringInt[pos - 1] - (int)zero;
        // Converto il carattere zero nel suo corrispondente codice ASCII. Siccome per i numeri superiori a 0 questo valore si incrementa ogni volta di uno, la differenza rappresenta proprio il valore intero della cifra che sto cercando.
        // Grazie a https://stackoverflow.com/ per la dritta sulla conversione da intero a carattere.
    }
}
