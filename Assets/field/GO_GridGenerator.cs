using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

//noam
// Ce composant génère et gère une grille de cellules cliquables.
// Il peut être utilisé en mode éditeur (ExecuteInEditMode) pour prévisualiser la grille
// directement dans la scène Unity sans lancer le jeu.
// Chaque grille est composée d'un tableau 2D de GO_ClickableActor (cellules).
[ExecuteInEditMode]
public class GO_GridGenerator : MonoBehaviour
{
    // Nombre de cellules par côté de la grille (ex : 3 pour une grille 3x3).
    public int size = 3;

    // Espacement en unités Unity entre les centres de deux cellules adjacentes.
    public int padding = 12;

    // Prefab de cellule à instancier pour remplir la grille.
    public GO_ClickableActor actorPrefab;

    // Tableau 2D contenant toutes les cellules générées.
    // Accessible depuis d'autres scripts pour lire ou modifier l'état des cellules.
    public GO_ClickableActor[,] actors;

    // Indice de cette grille dans le terrain global (0 = grille du bas, size-1 = grille du haut).
    public int index;


    /// <summary>
    /// Génère toutes les cellules de la grille en les instanciant depuis le prefab.
    /// Efface d'abord les cellules existantes pour éviter les doublons.
    /// Entrée : aucune.
    /// Sortie : aucune. Remplit le tableau actors[][].
    /// </summary>
    [ContextMenu("Generate Grid")]
    public void gen()
    {
        // On efface les cellules existantes avant de régénérer pour éviter les doublons.
        clear();
        // On vérifie que le prefab est bien assigné avant de continuer.
        if (actorPrefab == null) 
            return;
        actors = new GO_ClickableActor[size, size];

        // On parcourt les deux dimensions de la grille pour placer chaque cellule.
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                // On calcule la position 3D de la cellule en fonction de ses indices et de l'espacement.
                Vector3 pos = new Vector3(i * padding, 0, j * padding) + transform.position;
                // On instancie une cellule depuis le prefab, en la rattachant à cette grille.
                GO_ClickableActor actor = Instantiate(actorPrefab, transform);

                actor.transform.position = pos;
                actor.transform.SetParent(this.transform);

                // On assigne la position logique de la cellule : [x, y, index_grille].
                // Cette position sert d'identifiant stable et ne change pas lors d'un swap.
                actor.pos = new int[3]; 
                actor.pos[0] = i;
                actor.pos[1] = j;
                actor.pos[2] = index;

                // On stocke la référence dans le tableau pour y accéder plus tard.
                actors[i, j] = actor;
            }
        }
    }


    /// <summary>
    /// Supprime toutes les cellules enfants de cette grille.
    /// Compatible éditeur (DestroyImmediate) et runtime (Destroy).
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void clear()
    {
        int child = transform.childCount;
        // On itère en sens inverse pour éviter les décalages d'indices lors de la suppression.
        for (int i = child - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            DestroyImmediate(transform.GetChild(i).gameObject);
#else
            Destroy(transform.GetChild(i).gameObject);
#endif
        }

        // On nettoie aussi le tableau actors au cas où des références y subsisteraient.
        if (actors == null)
            return;
        foreach (GO_ClickableActor ac in actors)
        {
            if (ac != null)
            {
#if UNITY_EDITOR
                DestroyImmediate(ac);
#else
                Destroy(ac);
#endif
            }
        }
    }


    /// <summary>
    /// Applique un niveau de transparence à toutes les cellules de la grille.
    /// Les cellules sous effet visuel (UnderVF) ou bloquées par une compétence sont ignorées.
    /// Entrée : alpha — valeur de transparence entre 0 (invisible) et 1 (opaque).
    /// Sortie : aucune.
    /// </summary>
    public void SetTransparency(float alpha)
    {
        Debug.Log("Transparency");
        if (actors == null) return;

        foreach (GO_ClickableActor actor in actors)
        {
            // On exclut les cellules sous effet visuel pour ne pas écraser leur état.
            if (actor.UnderVF == true) continue;
            // On exclut les cellules bloquées par la compétence BlockSkill.
            if (actor.isBlockedBySkill) continue;

            // On désactive les clics si la grille est en arrière-plan (alpha < 1),
            // et on les réactive si elle revient au premier plan (alpha = 1).
            if (alpha != 1.0f)
                actor.Disable();
            else
                actor.Enable();

            // On modifie l'alpha du matériau de la cellule pour changer sa transparence visuelle.
            Renderer r = actor.GetComponent<Renderer>();
            if (r == null) continue;
            Color c = r.material.color;
            c.a = alpha;
            r.material.color = c;
        }
    }

    /// <summary>
    /// Remonte la hiérarchie pour demander au terrain parent de réinitialiser le focus.
    /// Appelée après qu'un coup a été joué sur cette grille.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void f_reset()
    {
        GO_FieldGenerator p = GetComponentInParent<GO_FieldGenerator>();
        if (p != null)
            p.reset_focus();
        else
            Debug.Log("GO_GG no parent");
    }

    
    // noam 25/03/26
    /// <summary>
    /// Retourne le tableau 2D de toutes les cellules de cette grille.
    /// Entrée : aucune.
    /// Sortie : GO_ClickableActor[,] — le tableau des cellules.
    /// </summary>
    public GO_ClickableActor[,] get_cells()
    {
        return actors;
    }


    /// <summary>
    /// Échange les données logiques de toutes les cellules de cette grille
    /// avec celles d'une autre grille, cellule par cellule.
    /// Utilisée par la compétence SwapSkill pour intervertir deux grilles.
    /// Entrée : other — la grille avec laquelle échanger les données.
    /// Sortie : aucune.
    /// </summary>
    public void swap(GO_GridGenerator other)
    {
        for (int i = 0; i < size;i++)
        {
            for (int j = 0; j < size; j++)
            {
                actors[j, i].swap(other.actors[j, i]);
            }
        }
    }
}

    
