
using UnityEngine;

// Ce composant gère la rotation de la caméra par glissement du doigt ou de la souris.
// Il utilise les événements souris Unity qui, avec "Active Input Handling : Both",
// couvrent automatiquement les événements tactiles sur Android.
public class CameraController : MonoBehaviour
{
    // Multiplicateur de vitesse de rotation, ajustable depuis l'inspecteur Unity
    public float rotationSpeed = 0.2f;

    // Position du doigt/souris lors du dernier frame
    // utilisée pour calculer le déplacement depuis le frame précédent
    private Vector2 lastInputPosition;

    // Indique si un glisser est en cours (doigt/souris maintenu enfoncé)
    private bool isDragging = false;

    // Permet d'activer/désactiver la rotation depuis un autre script
    // utile par exemple pour bloquer la rotation quand un menu est ouvert
    private bool rotationEnabled = true;

    // Alexis 04/04 11h45
    // à assigner depuis l'inspecteur
    // car ce script est attaché sur un pivot et non sur la caméra
    // donc besoin de référence à la caméra
    public Camera mainCamera;

    // Alexis 04/04 11h30
    /// <summary>
    /// Ajuste la position et le champ de vision de la caméra selon la taille du terrain.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    void Start()
    {
        GO_FieldGenerator root = FindFirstObjectByType<GO_FieldGenerator>();
        int size = root.numberOfGrids;

        // On ne modifie que X car c'est l'axe de profondeur
        mainCamera.transform.position = new Vector3(
            // -27.7(position X pour du 3x3x3) / 3 ≈ -9.2f 
            // donc 3 * -9.2f  = -27.7
            size * -9.2f,
            12.3f,  // valeur fixe Y
            -12.6f  // valeur fixe Z
        );

        // Ajuste le FOV selon la taille du terrain
        // + 0 * 10
        // + 1 * 10
        // ainsi de suite
        mainCamera.fieldOfView = 83f + (size - 3) * 10f;
    }

    /// <summary>
    /// Détecte les glissements et applique la rotation à chaque frame.
    /// Bloqué si la rotation est désactivée ou si le jeu est en pause.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    void Update()
    {
        // Time.timeScale = 0 signifie que le jeu est en pause
        // on bloque la rotation même si les inputs continuent de fonctionner
        if (!rotationEnabled || Time.timeScale == 0f) return;

        // Le delta représente le déplacement du doigt/souris depuis le dernier frame
        Vector2 delta = Vector2.zero;

        // Avec "Active Input Handling : Both" dans Project Settings,
        // Unity convertit automatiquement les événements touch en événements souris
        // Ce seul bloc couvre donc les trois cas sans code supplémentaire :
        //   - souris dans l'éditeur Unity
        //   - souris sur émulateur Android Studio
        //   - doigt sur smartphone Android réel
        if (Input.GetMouseButtonDown(0))
        {
            // Le bouton/doigt vient d'être pressé : on démarre le drag
            // et on mémorise la position de départ pour calculer le delta au prochain frame
            isDragging = true;
            lastInputPosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            // Le bouton/doigt vient d'être relâché : on stoppe le drag
            isDragging = false;
        }

        if (isDragging)
        {
            // On calcule le déplacement entre la position actuelle et celle du dernier frame
            // puis on met à jour lastInputPosition pour le calcul du prochain frame
            delta = (Vector2)Input.mousePosition - lastInputPosition;
            lastInputPosition = Input.mousePosition;
        }

        // On applique la rotation seulement si le doigt/souris a bougé ce frame
        if (delta != Vector2.zero)
        {
            RotateCamera(delta);
        }
    }

    /// <summary>
    /// Calcule et applique la rotation en fonction du déplacement détecté.
    /// Ne tourne que selon l'axe dominant (horizontal OU vertical) pour éviter les rotations diagonales.
    /// Entrée : delta — vecteur de déplacement en pixels depuis le frame précédent.
    /// Sortie : aucune.
    /// </summary>
    private void RotateCamera(Vector2 delta)
    {
        float rotX = 0f;
        float rotY = 0f;

        // On ne tourne que selon l'axe dominant (horizontal OU vertical)
        // pour éviter les rotations diagonales incontrôlables
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            // Mouvement majoritairement horizontal → rotation autour de l'axe Y (gauche/droite)
            // Le -1f inverse le sens pour que ce soit naturel (comme faire tourner un globe)
            rotY = delta.x * rotationSpeed * -1f;
        }
        else
        {
            // Mouvement majoritairement vertical → rotation autour de l'axe X (haut/bas)
            rotX = delta.y * rotationSpeed;
        }

        // On applique les rotations en Space.World pour éviter les dérives d'axes
        // qui surviennent quand on enchaîne des rotations en espace local
        transform.Rotate(Camera.main.transform.right, rotX, Space.World); // rotation verticale
        transform.Rotate(Camera.main.transform.up, rotY, Space.World);    // rotation horizontale
    }

    /// <summary>
    /// Active ou désactive la rotation de la caméra.
    /// Entrée : enable — true pour activer, false pour désactiver.
    /// Sortie : aucune.
    /// </summary>
    public void ActivateRotation(bool enable)
    {
        rotationEnabled = enable;
    }
}
