using UnityEngine;
using UnityEngine.EventSystems;

// Alexis 07/04 13h00
// Ce composant permet à une cellule d'être sélectionnée par le système de compétences
// via l'InputManager. Il implémente IPointerDownHandler et IPointerUpHandler pour
// mesurer la distance entre l'appui et le relâché, afin de distinguer un clic d'un glissement.
// Il est placé sur le même GameObject que GO_ClickableActor.
public class GO_selector : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    //Noam
    // Indique si ce sélecteur peut transmettre des sélections à l'InputManager.
    // Désactivé par BlockSkill pour empêcher la sélection d'une cellule bloquée.
    internal bool _enabled = true;
    
    // Alexis 07/04 13h00
    // Position du curseur/doigt au moment du OnPointerDown
    private Vector2 pointerDownPosition;

    // Seuil en pixels : en dessous = clic, au dessus = drag (rotation caméra)
    // On utilise 20f comme dans GO_ClickableActor
    private float dragThreshold = 20f;

    /// <summary>
    /// Mémorise la position d'appui pour calculer le déplacement au relâché.
    /// Entrée : eventData — données de l'événement (position, identifiant du doigt, etc.).
    /// Sortie : aucune.
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDownPosition = eventData.position;
    }

    /// <summary>
    /// Si le déplacement est inférieur au seuil et que le sélecteur est actif,
    /// signale à l'InputManager que cet objet a été sélectionné.
    /// Entrée : eventData — données de l'événement (position au relâché, etc.).
    /// Sortie : aucune. Peut appeler InputManager.Instance.Select().
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        float dragDistance = Vector2.Distance(eventData.position, pointerDownPosition);

        // Si la distance est inférieure au seuil : c'est un vrai clic → on sélectionne
        // Sinon : c'était un glisser (rotation caméra) → on ignore
        if (dragDistance < dragThreshold && _enabled)
        {
            InputManager.Instance.Select(gameObject);
        }
    }

    /// <summary>
    /// Active ce sélecteur pour qu'il puisse transmettre des sélections.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void Enable ()
    {
        _enabled = true;
    }

    /// <summary>
    /// Désactive ce sélecteur pour empêcher toute sélection sur cette cellule.
    /// Utilisé par BlockSkill pour bloquer la sélection d'une cellule bloquée.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void Disable ()
    {
        _enabled = false;
    }
}
