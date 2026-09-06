using UnityEngine;


// Alexis 13/04 15h30
// Ce composant gère l'affichage du tutoriel sous forme de diaporama.
// Il affiche les slides une par une et permet de naviguer entre elles.
// N'est actif que si le joueur a cliqué sur "Tutoriel" depuis le menu principal.
public class TutoManager : MonoBehaviour
{
    // Tableau de tous les GameObjects représentant les slides du tutoriel.
    // À remplir dans l'inspecteur dans l'ordre d'affichage souhaité.
    public GameObject[] slides;

    // Indice de la slide actuellement affichée.
    private int currentSlide = 0;

    // Permet d'activer/désactiver le GameObject selon l'indice de la slide courante.
    public GameObject boutonPrecedent; // à assigner depuis l'inspecteur

    // Permet d'activer/désactiver le GameObject selon l'indice de la slide courante.
    public GameObject boutonSuivant; // à assigner depuis l'inspecteur

    /// <summary>
    /// Affiche le tutoriel si on vient du bouton Tutoriel, sinon désactive ce composant.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    void Start()
    {
        // On affiche le tuto seulement si on vient du bouton Tutoriel
        if (!Boutons.isTuto)
        {
            gameObject.SetActive(false);
            return;
        }
        AfficherSlide(0);
    }

    /// <summary>
    /// Passe à la slide suivante. Si c'est la dernière, quitte le tutoriel.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void Suivant()
    {
        if (currentSlide < slides.Length - 1)
        {
            currentSlide++;
            AfficherSlide(currentSlide);
        }
        else
        {
            // Dernière slide : on quitte le tuto
            Boutons.isTuto = false;
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Revient à la slide précédente si elle existe.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void Precedent()
    {
        if (currentSlide > 0)
        {
            currentSlide--;
            AfficherSlide(currentSlide);
        }
    }

    /// <summary>
    /// Ferme le tutoriel immédiatement sans attendre la dernière slide.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void Passer()
    {
        Boutons.isTuto = false;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Affiche uniquement la slide à l'indice donné et masque toutes les autres.
    /// Entrée : index — indice de la slide à afficher.
    /// Sortie : aucune.
    /// </summary>
    private void AfficherSlide(int index)
    {
        // On désactive toutes les slides d'abord pour repartir d'un état propre.
        foreach (GameObject slide in slides)
            slide.SetActive(false);

        // On active uniquement la slide demandée.
        slides[index].SetActive(true);

        // On cache le bouton précédent sur la première slide
        if (boutonPrecedent != null)
            boutonPrecedent.SetActive(index > 0);

        // On cache le bouton suivant sur la dernière slide
        if (boutonSuivant != null)
            boutonSuivant.SetActive(index < slides.Length - 1);
    }
}