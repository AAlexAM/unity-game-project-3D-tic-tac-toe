using UnityEngine;
using System;

// Ce composant centralise la gestion des sélections de cellules par les compétences.
// Les compétences s'enregistrent via StartSelection() pour recevoir un callback
// dès qu'une cellule est cliquée. Cela découple les compétences du système de clics.
// Il existe en un seul exemplaire dans la scène (singleton via Instance).
public class InputManager : MonoBehaviour
{
    // Instance unique accessible depuis n'importe quel script sans référence directe.
    public static InputManager Instance;

    // Callback enregistré par la compétence en attente d'une sélection.
    // Null si aucune compétence n'attend de sélection.
    private Action<GameObject> onSelect;

    /// <summary>
    /// Initialise l'instance singleton au démarrage.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Enregistre un callback à appeler dès qu'une cellule sera sélectionnée.
    /// Appelée par une compétence quand elle attend que le joueur choisisse une cible.
    /// Entrée : callback — fonction à appeler avec le GameObject sélectionné.
    /// Sortie : aucune.
    /// </summary>
    public void StartSelection(Action<GameObject> callback)
    {
        onSelect = callback;
    }


    /// <summary>
    /// Notifie la compétence en attente qu'une cellule vient d'être cliquée.
    /// Appelée par GO_selector.OnPointerUp() quand une cellule est cliquée.
    /// Entrée : actor — le GameObject de la cellule cliquée.
    /// Sortie : aucune. Appelle le callback enregistré si présent.
    /// </summary>
    public void Select(GameObject actor)
    {
        if (onSelect == null)
            return;

        var callback = onSelect;
        onSelect = null;

        callback.Invoke(actor);
    }

    /// <summary>
    /// Annule la sélection en cours sans appeler le callback.
    /// Utilisée quand une compétence est désactivée avant qu'une cible soit choisie.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void CancelSelection()
    {
        onSelect = null;
        Debug.Log("Sélection annulée");
    }
}