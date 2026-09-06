using UnityEngine;
using UnityEngine.UI;

// Alexis 05/04 16h15
// Ce composant gère la musique de fond du jeu.
// Il persiste entre les scènes grâce à DontDestroyOnLoad et suit le patron singleton
// pour être accessible depuis n'importe quel script via MusiqueManager.Instance.
public class MusiqueManager : MonoBehaviour
{
    // Instance unique accessible depuis n'importe quel script
    // Permet d'éviter de devoir glisser une référence ans l'inspecteur
    public static MusiqueManager Instance;

    // Composant audio Unity
    private AudioSource audioSource;

    // La musique est désactivée par défaut
    private bool musiqueActive = false;

    /// <summary>
    /// Initialise le singleton et configure l'AudioSource.
    /// Si une instance existe déjà, détruit ce doublon.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    void Awake()
    {
        // Une seule instance dans tout le jeu
        if (Instance == null)
        {
            // Instance passe de null à un MusiqueManager
            Instance = this;
            // N'est pas détruit entre les scènes
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Si une instance existe déjà on détruit le doublon
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        // Désactivée par défaut
        audioSource.volume = 0.15f;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }

    // Alexis 05/04 17h
    /// <summary>
    /// Active ou désactive la musique.
    /// Appelée par le Toggle de l'interface des paramètres.
    /// Entrée : active — true pour lancer la musique, false pour l'arrêter.
    /// Sortie : aucune.
    /// </summary>
    public void SetMusique(bool active)
    {
        Debug.Log("SetMusique appelé : " + active);
        musiqueActive = active;
        if (active)
            audioSource.Play();
        else
            audioSource.Stop();
    }

    // Alexis 05/04 17h
    /// <summary>
    /// Règle le volume de la musique.
    /// Appelée par le Slider de l'interface des paramètres.
    /// Entrée : volume — valeur entre 0 (muet) et 1 (volume maximum).
    /// Sortie : aucune.
    /// </summary>
    public void SetVolume(float volume)
    {
        audioSource.volume = volume;
    }


    // Alexis 05/04 16h30
    /// <summary>
    /// Indique si la musique est actuellement activée.
    /// Utilisée par ParametresUI pour initialiser le Toggle au bon état.
    /// Entrée : aucune.
    /// Sortie : bool — true si la musique est active, false sinon.
    /// </summary>
    public bool IsMusiqueActive()
    {
        return musiqueActive;
    }

    /// <summary>
    /// Retourne le volume actuel de la musique.
    /// Utilisée par ParametresUI pour initialiser le Slider à la bonne valeur.
    /// Entrée : aucune.
    /// Sortie : float — volume entre 0 et 1.
    /// </summary>
    public float GetVolume()
    {
        return audioSource.volume;
    }
}
