using UnityEngine;

// Ce composant permet à une grille de réagir à un appui long en demandant au terrain
// de la mettre en focus. Il sert de pont entre l'événement d'appui long
// et le système de focus du GO_FieldGenerator.
public class GO_GridFocusController : MonoBehaviour
{
    // Référence à la grille que ce contrôleur représente.
    // Assignée automatiquement dans Awake() ou manuellement depuis l'inspecteur.
    public GO_GridGenerator generator;

    // Référence au générateur de terrain parent, qui gère le focus global.
    public GO_FieldGenerator fieldGenerator;


    /// <summary>
    /// Initialise la référence à la grille parente si elle n'est pas déjà assignée.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    [HideInInspector]
    private void Awake()
    {
        if (generator == null)
            generator = GetComponentInParent<GO_GridGenerator>();
    }


    /// <summary>
    /// Appelée quand un appui long est détecté sur cette grille.
    /// Demande au terrain de mettre cette grille en avant-plan.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void OnGridLongClick()
    {
        if (generator == null)
            Debug.Log("GridFocusGen");
        if (fieldGenerator == null)
            Debug.Log("GridFocusField");
        if (fieldGenerator != null)
            fieldGenerator.FocusGrid(generator);
    }
}

