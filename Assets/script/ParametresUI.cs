using UnityEngine;
using UnityEngine.UI;

// Alexis 05/04 16h30
// Ce composant gère l'interface des paramètres audio (toggle musique et slider volume).
// Il lit l'état actuel du MusiqueManager pour initialiser les contrôles,
// puis se branche sur leurs événements pour transmettre les changements.
public class ParametresUI : MonoBehaviour
{
    // À assigner depuis l'inspecteur
    // Toggle Unity pour activer ou désactiver la musique.
    public Toggle toggleMusique;
    // Slider Unity pour régler le volume entre 0 et 1.
    public Slider sliderVolume;

    /// <summary>
    /// Initialise les contrôles avec les valeurs actuelles du MusiqueManager
    /// et branche les événements de l'interface.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    void Awake()
    {
        Debug.Log(MusiqueManager.Instance == null ? "MusiqueManager null !" : "MusiqueManager OK !");
        if (MusiqueManager.Instance == null) return;

        // On initialise l'UI avec les valeurs actuelles du MusiqueManager
        toggleMusique.isOn = MusiqueManager.Instance.IsMusiqueActive();
        sliderVolume.value = MusiqueManager.Instance.GetVolume();

        // Alexis 05/04 17h45
        // On branche les événements
        toggleMusique.onValueChanged.AddListener(OnToggleMusique);// Quand la valeur du toggle change (clic), appelle OnToggleMusique
        sliderVolume.onValueChanged.AddListener(OnSliderVolume);// Quand la valeur du slider change (slide), appelle OnToggleVolume
    }


    // Alexis 05/04 17h
    /// <summary>
    /// Appelée automatiquement quand le toggle change d'état.
    /// Active ou désactive la musique et le slider.
    /// Entrée : active — true si le toggle est coché, false sinon.
    /// Sortie : aucune.
    /// </summary>
    private void OnToggleMusique(bool active)
    {
        if (MusiqueManager.Instance != null)
            MusiqueManager.Instance.SetMusique(active);

        // Active ou désactive le slider selon l'état de la musique
        sliderVolume.interactable = active;
    }

    // Alexis 05/04 17h
    /// <summary>
    /// Appelée automatiquement quand le slider change de valeur.
    /// Transmet le nouveau volume au MusiqueManager.
    /// Entrée : volume — nouvelle valeur du slider entre 0 et 1.
    /// Sortie : aucune.
    /// </summary>
    private void OnSliderVolume(float volume)
    {
        if (MusiqueManager.Instance != null)
            MusiqueManager.Instance.SetVolume(volume);
    }
}