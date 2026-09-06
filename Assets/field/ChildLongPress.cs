
using UnityEngine;
using UnityEngine.EventSystems; // nécessaire pour IPointerDownHandler, IPointerUpHandler et PointerEventData

[RequireComponent(typeof(Collider))] // Unity force l'ajout d'un Collider sur cet objet
// Ce composant détecte un appui long (long press) sur une cellule de la grille.
// On implémente IPointerDownHandler et IPointerUpHandler :
// ces interfaces sont reconnues par l'EventSystem de Unity,
// ce qui permet de recevoir les événements souris ET touch de façon unifiée,
// à condition qu'un PhysicsRaycaster soit présent sur la caméra principale
// Lorsqu'un appui long est détecté, il demande au terrain de mettre en focus la grille concernée.
public class ChildLongPress : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // Durée en secondes que l'utilisateur doit maintenir le doigt/clic pour valider un long press
    public float holdTime = 1f;

    // Référence au générateur de champ parent, assignée automatiquement dans Awake() via GetComponentInParent.
    public GO_FieldGenerator fieldGenerator;

    // Référence à la grille parente, permet de savoir sur quelle grille l'appui long a été effectué.
    private GO_GridGenerator parentGrid;

    // Indique si le doigt/clic est actuellement maintenu sur cet objet
    private bool isPointerDown = false;

    // Compteur de temps écoulé depuis que le doigt/clic a été posé
    private float timer = 0f;

    // Flag public lisible de l'extérieur (notamment par GO_ClickableActor)
    // mais modifiable uniquement depuis ce script (private set)
    // Permet à GO_ClickableActor de savoir si un long press vient de se terminer
    // et donc d'ignorer le OnPointerUp pour éviter un clic parasite
    public bool WasLongPress { get; private set; } = false;


    /// <summary>
    /// Initialise les références aux composants parents au démarrage.
    /// Entrée : aucune.
    /// Sortie : aucune. Assigne fieldGenerator et parentGrid.
    /// </summary>
    void Awake()
    {
        // On cherche automatiquement les composants parents au démarrage
        // pour ne pas avoir à les assigner manuellement dans l'inspecteur
        fieldGenerator = GetComponentInParent<GO_FieldGenerator>();
        parentGrid = GetComponentInParent<GO_GridGenerator>();
    }


    /// <summary>
    /// Appelée à chaque frame par Unity. Incrémente le timer si un appui est en cours
    /// et déclenche le focus de grille si la durée seuil est atteinte.
    /// Entrée : aucune.
    /// Sortie : aucune. Peut appeler fieldGenerator.FocusGrid().
    /// </summary>
    void Update()
    {
        // On incrémente le timer uniquement si le doigt/clic est maintenu
        if (isPointerDown)
        {
            // Time.deltaTime représente le temps écoulé depuis le dernier frame.
            timer += Time.deltaTime;

            // Si le timer dépasse le seuil défini, on valide le long press
            if (timer >= holdTime)
            {
                if (parentGrid == null)
                    Debug.Log("ChildLongPress ParentGrid null");
                if (fieldGenerator == null)
                    Debug.Log("ChildLongPress FieldGenerator null");

                if (parentGrid != null && fieldGenerator != null)
                {
                    // On marque le long press comme validé AVANT d'appeler FocusGrid
                    // pour que GO_ClickableActor puisse le détecter dans son OnPointerUp
                    // et ignorer le clic parasite qui suivrait.
                    WasLongPress = true;
                    // On demande au terrain de mettre cette grille en "avant"
                    // et de rendre les autres grilles semi-transparentes.
                    fieldGenerator.FocusGrid(parentGrid);
                }

                // On remet le timer et isPointerDown à zéro après validation
                ResetState();
            }
        }
    }

    /// <summary>
    /// Appelée par l'EventSystem Unity quand le doigt ou la souris touche cet objet.
    /// Démarre le compteur d'appui long.
    /// Entrée : eventData — données de l'événement (position, identifiant du doigt, etc.).
    /// Sortie : aucune.
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        // On bloque le long press pendant la pause
        // 0f pour éviter une conversion implicite de type : Time.timeScale est un float.
        if (Time.timeScale == 0f) return;
        // Alexis 02/04 10h15
        // On bloque le long press si la partie est terminée
        if (GO_ClickableActor.theEnd) return;

        // On remet WasLongPress à false à chaque nouvel appui
        // pour ne pas bloquer les clics suivants après un long press
        WasLongPress = false;
        isPointerDown = true;
        timer = 0f;
    }

    /// <summary>
    /// Appelée par l'EventSystem Unity quand le doigt ou la souris est relâché.
    /// Si le seuil n'a pas été atteint, annule simplement l'appui en cours.
    /// Entrée : eventData — données de l'événement (position, identifiant du doigt, etc.).
    /// Sortie : aucune.
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        // Le doigt est relâché avant la fin du holdTime : on annule le long press
        ResetState();
    }

    /// <summary>
    /// Réinitialise l'état interne du détecteur d'appui long.
    /// Renommée de Reset() en ResetState() pour éviter un conflit avec la méthode
    /// Reset() native de Unity, appelée automatiquement par l'éditeur.
    /// Entrée : aucune.
    /// Sortie : aucune.
    /// </summary>
    private void ResetState()
    {
        isPointerDown = false;
        timer = 0f;
    }
}