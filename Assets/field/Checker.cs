using UnityEngine;


// Ce composant maintient une représentation interne du terrain sous forme de tableau 3D d'entiers.
// Il sert d'intermédiaire entre l'état visuel du terrain (les GO_ClickableActor)
// et le système de détection de victoire (Victory_Check).
//noam 25/03/26
public class Checker : MonoBehaviour
{
    // Tableau 3D représentant l'état de toutes les cellules du terrain.
    // grid[i][j][k] correspond à la cellule à l'index de grille i, colonne j, ligne k.
    // Internal se comporte comme public à la différence qu'un champ internal n'apparaît pas dans l'inspecteur Unity.
    internal int[][][] grid;
    // Position de la dernière cellule jouée.
    // Utilisée par Victory_Check pour n'analyser que les lignes passant par ce point.
    internal int[] last_pos;


    /// <summary>
    /// Initialise le tableau grid avec des zéros (terrain vide).
    /// Doit être appelée une fois au démarrage de la partie.
    /// Entrée : aucune.
    /// Sortie : aucune. Initialise grid et last_pos.
    /// </summary>
    public void Init()
    {
        // On récupère le terrain pour connaître le nombre de grilles.
        GO_FieldGenerator root = FindFirstObjectByType<GO_FieldGenerator>();
        int size = root.numberOfGrids;

        last_pos = new int[3];
        // On alloue un tableau 3D
        grid = new int[size][][];
        for (int i = 0; i < size; i++)
        {
            grid[i] = new int[size][];
            for (int j = 0; j < size; j++)
            {
                grid[i][j] = new int[size];
            }
        }
    }


    /// <summary>
    /// Met à jour le tableau grid en lisant l'état actuel de toutes les cellules du terrain.
    /// Doit être appelée après chaque coup joué.
    /// Entrée : new_pos — position de la cellule jouée.
    /// Sortie : aucune. Met à jour grid et last_pos.
    /// </summary>
    public void make_grid(int[] new_pos) // on init field
    {
        GO_FieldGenerator root = FindFirstObjectByType<GO_FieldGenerator>();
        ControleurJeu game_manager = FindFirstObjectByType<ControleurJeu>();
        GO_GridGenerator current;
        GO_ClickableActor[,] A_current;
        int size = root.grids.Length;

        // On enregistre la position du dernier coup.
        // Si new_pos est null (coup invalide ou swap), on met [-1, -1, -1]
        // pour indiquer à Victory_Check de vérifier tout le plateau.
        if (new_pos != null)
        {
            // Les indices sont réordonnés car pos[] est stocké en [x, y, index_grille]
            // mais last_pos est attendu en [index_grille, y, x] par Victory_Check.
            last_pos[0] = new_pos[2];
            last_pos[1] = new_pos[1];
            last_pos[2] = new_pos[0];
        }
        else
        {
            last_pos[0] = -1 ;
            last_pos[1] = -1;
            last_pos[2] = -1;
        }
        
        // On parcourt toutes les grilles et toutes les cellules pour remplir le tableau.
        for (int i = 0; i < size; i++)
        {
            current = root.grids[i];
            A_current = current.actors;
            grid[i] = new int[size][];
            for (int j = 0; j < size; j++)
            {
                grid[i][j] = new int[size];
                for (int k = 0; k < size; k++)
                {
                    if (!A_current[k, j].isOccupied)
                        grid[i][j][k] = 0;
                    else
                    {
                        grid[i][j][k] = A_current[k, j].occupiedBy;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Affiche le contenu du tableau grid dans la console Unity, pour le débogage.
    /// Entrée : aucune.
    /// Sortie : aucune. Affiche un log dans la console Unity.
    /// </summary>
    public void print_grid()
    {
        GO_FieldGenerator root = FindFirstObjectByType<GO_FieldGenerator>();
        int size = root.grids.Length;
        // On construit une chaîne lisible affichant le dernier coup et l'état complet du terrain.
        string str = "Dernier click : " + last_pos[0] + " | " + last_pos[1] + " | " + last_pos[2] + "\n";
        for (int i = 0; i < size; i++)
        {
            str += "[";
            for (int j = 0; j < size; j++)
            {
                str += "[";
                for (int k = 0; k < size; k++)
                {
                    if ( k > 0)
                        str += ", ";
                    str += grid[i][j][k];
                }
                str += "]";
            }
            str += "]\n";
        }
        Debug.Log(str);
    }

    public void convert_grid() // on init grid
    {

    }

    // Alexis 02/04 10h50
    /// <summary>
    /// Vérifie si toutes les cellules du terrain sont occupées.
    /// Utilisée pour détecter un match nul en fin de tour.
    /// Entrée : aucune.
    /// Sortie : bool — true si toutes les cellules sont occupées (match nul), false sinon.
    /// </summary>
    public bool IsGridFull()
    {
        for (int i = 0; i < grid.Length; i++)
            for (int j = 0; j < grid[i].Length; j++)
                for (int k = 0; k < grid[i][j].Length; k++)
                    if (grid[i][j][k] == 0) // 0 = cellule vide
                        return false;
        return true;
    }
}
