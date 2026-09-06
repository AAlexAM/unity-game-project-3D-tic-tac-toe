using UnityEngine;
using TMPro; // Système de texte moderne de Unity

// Alexis 28/03 11h30
// Ce composant gère l'affichage de l'écran de victoire ou de match nul.
// Il désactive les interactions restantes et affiche le nom du gagnant.
public class VictoireUI : MonoBehaviour
{
    // Assigner depuis l'inspecteur pour que le script sache quel texte il doit modifier
    public TextMeshProUGUI textGagnant;

    // Assigner le bouton pause depuis l'inspecteur
    public GameObject boutonPause;

    // Alexis 02/04 10h10
    // Assigner dans l'inspecteur sur le canvas victoire
    public GameObject boutonMenuHautGauche; // le bouton flèche du menu compétences
    public GameObject boutonMenuBasDroite; // le bouton flèche du menu compétences

    /// <summary>
    /// Affiche l'écran de victoire avec le nom du gagnant et bloque toutes les interactions.
    /// Entrée : nomGagnant — nom du joueur gagnant ou "Match Nul".
    /// Sortie : aucune.
    /// </summary>
    public void AfficherVictoire(string nomGagnant)
    {
        textGagnant.text = nomGagnant + " a gagné !";
        // On active ce GameObject (le canvas de victoire) pour l'afficher.
        gameObject.SetActive(true);

        //noam
        // On s'assure que currentPlayer pointe bien sur le bon gagnant.
        ControleurJeu cj = FindFirstObjectByType<ControleurJeu>();
        cj.currentPlayer = (cj.index == 0) ? cj.player1 : cj.player2;

        // Alexis 2/04 9h45 et 10h05
        // On ne gèle plus le temps pour garder la rotation de caméra
        // Time.timeScale = 0f; → supprimé
        // On désactive juste les interactions
        GO_ClickableActor.theEnd = true;
        if (boutonPause != null)
            boutonPause.SetActive(false);
        // Alexis 02/04 10h10
        // Desactivation des panels de compétences à la fin de la partie
        if (boutonMenuHautGauche != null)
            boutonMenuHautGauche.SetActive(false);
        if (boutonMenuBasDroite != null)
            boutonMenuBasDroite.SetActive(false);
    }
}