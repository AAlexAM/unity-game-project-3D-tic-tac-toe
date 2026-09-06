using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Pierre 28/03/26
// Ce composant vérifie si un joueur a gagné en alignant des pions sur toutes les lignes possibles
// d'un morpion 3D (axes X, Y, Z, diagonales de face et diagonales d'espace).
// Pour optimiser, il ne teste que les lignes passant par le dernier coup joué.
public class Victory_Check : MonoBehaviour
{
    // Liste précalculée de toutes les lignes gagnantes possibles.
    // Chaque ligne est une liste de positions Vector3Int (x, y, z).
    List<List<Vector3Int>> allLines;

    /// <summary>
    /// Précalcule toutes les lignes gagnantes possibles selon la taille du terrain.
    /// Doit être appelée une fois au démarrage de la partie.
    /// Entrée : aucune. Sortie : aucune. Remplit allLines.
    /// </summary>
    public void Init()
    {
        allLines = GenerateAllLines();
    }

    /// <summary>
    /// Génère toutes les lignes gagnantes possibles pour la taille de terrain courante.
    /// Entrée : aucune.
    /// Sortie : List<List<Vector3Int>> — toutes les lignes gagnantes possibles.
    /// </summary>
    List<List<Vector3Int>> GenerateAllLines()
    {
        // Fonction qui renvoie une liste de toutes les lignes
        var lines = new List<List<Vector3Int>>();
        GO_FieldGenerator root = FindFirstObjectByType<GO_FieldGenerator>();
        int size = root.numberOfGrids;
        // Axes X
        for (int y = 0; y < size; y++) // Size Alexis 04/04 11h00
            for (int z = 0; z < size; z++) // Size Alexis 04/04 11h00
            {
                var line = new List<Vector3Int>();
                for (int n = 0; n < size; n++)
                    line.Add(new Vector3Int(n, y, z));
                lines.Add(line);
            }
        ;

        // Axes Y
        for (int x = 0; x < size; x++) // Size Alexis 04/04 11h00
            for (int z = 0; z < size; z++) // Size Alexis 04/04 11h00
            {
                var line = new List<Vector3Int>();
                for (int n = 0; n < size; n++)
                    line.Add(new Vector3Int(x, n, z));
                lines.Add(line);
            }
        ;

        // Axes Z
        for (int x = 0; x < size; x++) // Size Alexis 04/04 11h00 
            for (int y = 0; y < size; y++) // Size Alexis 04/04 11h00
            {
                var line = new List<Vector3Int>();
                for (int n = 0; n < size; n++)
                    line.Add(new Vector3Int(x, y, n));
                lines.Add(line);
            }
        ;

        // Diagonales XY
        for (int z = 0; z < size; z++) // Size Alexis 04/04 11h00
        {
            var line1 = new List<Vector3Int>();
            var line2 = new List<Vector3Int>();
            for (int n = 0; n < size; n++)
            {
                line1.Add(new Vector3Int(n, n, z));
                line2.Add(new Vector3Int(n, size - 1 - n, z));
            }
            lines.Add(line1);
            lines.Add(line2);
        }
        ;

        // Diagonales XZ
        for (int y = 0; y < size; y++) // Size Alexis 04/04 11h00
        {
            var line1 = new List<Vector3Int>();
            var line2 = new List<Vector3Int>();
            for (int n = 0; n < size; n++)
            {
                line1.Add(new Vector3Int(n, y, n));
                line2.Add(new Vector3Int(n, y, size - 1 - n));
            }
            lines.Add(line1);
            lines.Add(line2);
        }
        ;

        // Diagonales YZ
        for (int x = 0; x < size; x++) // Size Alexis 04/04 11h00
        {
            var line1 = new List<Vector3Int>();
            var line2 = new List<Vector3Int>();
            for (int n = 0; n < size; n++)
            {
                line1.Add(new Vector3Int(x, n, n));
                line2.Add(new Vector3Int(x, n, size - 1 - n));
            }
            lines.Add(line1);
            lines.Add(line2);
        }
        ;
        var d1 = new List<Vector3Int>();
        var d2 = new List<Vector3Int>();
        var d3 = new List<Vector3Int>();
        var d4 = new List<Vector3Int>();
        // Diagonales espace
        for (int n = 0; n < size; n++)
        {
            d1.Add(new Vector3Int(n, n, n));
            d2.Add(new Vector3Int(n, n, size - 1 - n));
            d3.Add(new Vector3Int(n, size - 1 - n, n));
            d4.Add(new Vector3Int(n, size - 1 - n, size - 1 - n));
        }
        lines.Add(d1);
        lines.Add(d2);
        lines.Add(d3);
        lines.Add(d4);

        return lines;
    }

    //Noam
    /// <summary>
    /// Vérifie si le dernier coup joué a permis à un joueur de gagner.
    /// Optimisé : ne teste que les lignes passant par le dernier coup.
    /// Si lastMove est [-1,-1,-1] (coup invalide ou swap), vérifie tout le plateau.
    /// Entrée : board — tableau 3D de l'état du terrain, lastMove — position du dernier coup.
    /// Sortie : bool — true si le joueur courant a gagné, false sinon.
    /// </summary>
    public bool CheckVictory(int[][][] board, int[] lastMove)
    {
        
        int player;
        // Si le dernier coup est valide, on récupère le joueur qui a joué.
        if (lastMove[0] != -1)
            player = board[lastMove[0]][lastMove[1]][lastMove[2]];
        else
            // Si le coup est invalide (ex: après un swap), on vérifie tout le plateau.
            return CheckAll(board);
        // Une cellule vide (0) ne peut pas gagner.
        if (player == 0) return false;

        foreach (var line in allLines)
        {
            // On ne teste que les lignes qui passent par le dernier coup
            if (!line.Contains(new Vector3Int(lastMove[2], lastMove[1], lastMove[0])) && lastMove[0] != -1) continue;

            bool win = true;

            foreach (var pos in line)
            {
                // Si une cellule de la ligne n'appartient pas au joueur, la ligne n'est pas gagnante.
                if (board[pos.z][pos.y][pos.x] != player)
                {
                    win = false;
                    break;
                }
            }

            if (win)
                return true;
        }

        return false;
    }

    //Noam
    /// <summary>
    /// Vérifie la victoire pour les deux joueurs sur l'ensemble du plateau.
    /// Utilisée quand le dernier coup est invalide (après un swap ou une compétence).
    /// Entrée : board — tableau 3D de l'état du terrain.
    /// Sortie : bool — true si l'un des deux joueurs a gagné, false sinon.
    /// </summary>
    private bool CheckAll(int[][][] board)
    {
        ControleurJeu cj = FindFirstObjectByType<ControleurJeu>();

        // On sauvegarde le joueur courant avant de le modifier
        Player savedPlayer = cj.currentPlayer;

        for (int player = 1; player <= 2; player++)
        {
            // On pointe temporairement le joueur courant sur le joueur testé
            // pour que la logique de victoire identifie correctement le gagnant.
            cj.currentPlayer = (player == 1) ? cj.player1 : cj.player2;
            foreach (var line in allLines)
            {
                bool win = true;

                foreach (var pos in line)
                {
                    if (board[pos.z][pos.y][pos.x] != player)
                    {
                        win = false;
                        break;
                    }
                }

                if (win)
                    return true;
            }
        }
        // On restaure le joueur courant si personne n'a gagné
        cj.currentPlayer = savedPlayer;
        return false;
    }
}