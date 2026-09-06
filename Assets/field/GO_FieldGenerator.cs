using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// Ce composant génère et gère l'ensemble du terrain de jeu, constitué de plusieurs grilles empilées.
// Il gère également le système de focus : quand un appui long est détecté sur une grille,
// les autres grilles deviennent semi-transparentes pour mettre en avant la grille sélectionnée.
// Il peut être utilisé en mode éditeur (ExecuteInEditMode) pour prévisualiser le terrain.
[ExecuteInEditMode]
public class GO_FieldGenerator : MonoBehaviour
{
    // Nombre de grilles composant le terrain (correspond aussi à la taille de chaque grille).
    // Un terrain 3x3x3 a numberOfGrids = 3.
    public int numberOfGrids = 3;

    // Espacement horizontal en unités Unity entre les cellules d'une même grille.
    public int padding = 12;

    // Espacement vertical en unités Unity entre les grilles superposées.
    public int uppadding = 20;

    // Prefab de grille à instancier pour constituer le terrain.
    public GO_GridGenerator gridPrefab;

    // Position du doigt ou de la souris au moment de l'appui, pour détecter les glissements.
    private Vector2 mouseDownPosition;

    // Distance maximale en pixels pour distinguer un clic d'un glissement.
    private float dragThreshold = 20f;

    // Tableau de toutes les grilles générées.
    internal GO_GridGenerator[] grids;

    void Start()
    {
            //if (grids == null)
            //Debug.Log("Grids est null");
    }


    /// <summary>
    /// Génère l'ensemble du terrain en instanciant numberOfGrids grilles empilées verticalement.
    /// Efface d'abord le terrain existant pour éviter les doublons.
    /// Entrée : aucune.
    /// Sortie : aucune. Remplit le tableau grids[].
    /// </summary>
    [ContextMenu("Generate Field")]
    public void GenField()
    {
        ClearField();

        if (gridPrefab == null)
            return;

        grids = new GO_GridGenerator[numberOfGrids];

        for (int i = 0; i < numberOfGrids; i++)
        {
            Vector3 pos = new Vector3(
                (0 - (numberOfGrids - 1) / 2.0f) * padding, // X centré
                (i - (numberOfGrids - 1) / 2.0f) * uppadding, // Y centré
                (0 - (numberOfGrids - 1) / 2.0f) * padding) + transform.position;
            GO_GridGenerator gridInstance = Instantiate(gridPrefab);
            gridInstance.transform.position = pos;
            gridInstance.transform.SetParent(transform);
            gridInstance.index = i;
            // Configure la grille
            gridInstance.size = numberOfGrids;
            gridInstance.padding = padding;
            gridInstance.gen(); // génère les acteurs de la grille

            grids[i] = gridInstance;
        }
    }


    /// <summary>
    /// Supprime toutes les grilles enfants du terrain.
    /// Compatible éditeur (DestroyImmediate) et runtime (Destroy).
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    [ContextMenu("Clear Field")]
    public void ClearField()
    {
        int childCount = transform.childCount;
        // On itère en sens inverse pour éviter les décalages d'indices lors de la suppression.
        for (int i = childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            DestroyImmediate(transform.GetChild(i).gameObject);
#else
            Destroy(transform.GetChild(i).gameObject);
#endif
        }

        // On vide aussi le tableau de références
        if (grids != null)
        {
            for (int i = 0; i < grids.Length; i++)
            {
                grids[i] = null;
            }
        }
    }


    /// <summary>
    /// Met en avant une grille cliquée en la rendant opaque et les autres semi-transparentes.
    /// Appelée lors d'un appui long sur une cellule d'une grille.
    /// Entrée : clicked — la grille à mettre en avant-plan.
    /// Sortie : aucune.
    /// </summary>
    public void FocusGrid(GO_GridGenerator clicked)
    {
        if (grids == null)
            Debug.Log("nuhuh");
        if (clicked == null)
            Debug.Log("null");
        foreach (GO_GridGenerator grid in grids)
        {
            if (grid != null)
            {
                Debug.Log("ici");
                // La grille sélectionnée devient opaque (alpha = 1), les autres semi-transparentes (alpha = 0.3).
                if (grid == clicked)
                    grid.SetTransparency(1.0f);
                else
                    grid.SetTransparency(0.3f);
            }
            else
                Debug.Log("pas ici");
        }
    }


    /// <summary>
    /// Réinitialise toutes les grilles à leur opacité normale (alpha = 1).
    /// Appelée après qu'un coup a été joué ou qu'un clic en dehors du terrain est détecté.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void reset_focus()
    {
        Debug.Log("ResetFocus");
        foreach (GO_GridGenerator grid in grids)
        {
            if (grid != null)
            {
                grid.SetTransparency(1.0f);
            }
        }
    }

    // noam 20/03/26
    /// <summary>
    /// Retourne le tableau de toutes les grilles du terrain.
    /// Entrée : aucune.
    /// Sortie : GO_GridGenerator[] — le tableau des grilles.
    /// </summary>
    public GO_GridGenerator[] get_tb()
    {
        return grids;
    }


    // Yanis 08/04/2026 13h30
    /// <summary>
    /// Appelée à chaque frame par Unity. Détecte les clics en dehors du terrain
    /// pour réinitialiser le focus si le joueur clique dans le vide.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    void Update()
    {
        // clic souris ou tap écran
        if (Input.GetMouseButtonDown(0))
        {
            mouseDownPosition = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Si on touche quelque chose
            if (Physics.Raycast(ray, out hit))
            {
                // Si ce n'est PAS une grille (ni un enfant de grille)
                if (hit.collider.GetComponentInParent<GO_GridGenerator>() == null)
                {
                    //reset_focus();
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            float dragDistance = Vector2.Distance(Input.mousePosition, mouseDownPosition);

            if (dragDistance < dragThreshold)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.GetComponentInParent<GO_GridGenerator>() == null)
                        reset_focus();
                }
                else
                {
                    reset_focus();
                }
            }
        }
        
    }
}
