
using UnityEngine;
using UnityEngine.SceneManagement;

// Alexis
// Ce composant centralise toutes les actions des boutons de l'interface :
// navigation entre scènes, gestion de la pause, paramètres, rejouer, tutoriel.
// Il gère aussi l'état du dégradé d'assombrissement de l'écran lors des menus.
public class Boutons : MonoBehaviour
{
    // Accrocher sur bouton manager
    // Permet d'afficher les canvas pause et parametres
    // Permet de gérer dynamiquement (si un canvas s'ouvre l'autre se ferme)
    public GameObject pauseCanvas;
    public GameObject parametresCanvas;

    // Accrocher sur bouton manager
    // Menus des compétences
    // Permet de gérer dynamiquement (si un canvas s'ouvre l'autre se ferme)
    public UIJoueur MenuHautGauche;
    public UIJoueur MenuBasDroite;

    // Alexis 05/04 18h30
    // Assigner depuis l'inspecteur
    public GameObject boutonPause;

    // Alexis 10/04 12h15
    // AJOUT : image noire semi-transparente pour assombrir le jeu derrière les menus
    // À assigner dans l'Inspector
    public GameObject degrade;

    // Alexis 13/04 15h30
    // AJOUT : indique si on est en mode tutoriel
    // Static pour être accessible depuis TutoManager sans référence directe
    public static bool isTuto = false;


    // Bouton "OK" du canvas d'information sur l'absence de compétences.
    public GameObject boutonOk;
    // Indique si le joueur a cliqué sur "OK" pour fermer le message.
    private bool aCliqueOk = false;
    // Canvas affiché quand aucune compétence n'est sélectionnée.
    public GameObject canvasAucuneCompetence;

    /// <summary>
    /// Quitte l'application. En éditeur Unity, arrête le mode lecture.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void OnClickQuitter()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }


    /// <summary>
    /// Depuis le menu principal, charge la scène des paramètres du menu.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void OnClickParametresMenu()
    {
        SceneManager.LoadScene("Parametres Menu");
    }

    /// <summary>
    /// Depuis les paramètres du menu, retourne au menu principal.
    /// Remet le temps à la normale et réinitialise la configuration.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void OnClickRetourMenu()
    {
        // On remet le temps à la normale avant de quitter
        // sinon la scène Menu (et la prochaine partie) seront gelées
        Time.timeScale = 1f;
        
        // Alexis 05/04 18h45 
        // On remet theEnd à false pour la prochaine partie
        GO_ClickableActor.theEnd = false;
        SceneManager.LoadScene("Menu");

        GameConfig.Reset();
    }


    /// <summary>
    /// Met le jeu en pause et affiche le canvas de pause.
    /// Ferme les menus de compétences s'ils sont ouverts.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void OnClickPause()
    {
        // Fermer menus dépiables des compétences lorsque la pause est demandée
        // Alexis 24/03 11h00 
        if(MenuHautGauche != null)
            MenuHautGauche.FermerMenu();
        if (MenuBasDroite != null)
            MenuBasDroite.FermerMenu();


        if (pauseCanvas != null)
            pauseCanvas.SetActive(true);
        // Alexis 10/04 12h15
        if (degrade != null)
            degrade.SetActive(true);

        if(canvasAucuneCompetence != null)
            canvasAucuneCompetence.SetActive(false);

        // timeScale = 0 gèle le jeu sans recharger la scène,
        // ce qui préserve l'état du plateau (couleurs, positions).
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Reprend la partie depuis la pause sans recharger la scène.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void OnClickRetourGame()
    {
        if (pauseCanvas != null)
            pauseCanvas.SetActive(false);
        // Alexis 10/04 12h15
        if (degrade != null)
            degrade.SetActive(false);

        GO_FieldGenerator field = FindFirstObjectByType<GO_FieldGenerator>();
        // On réaffiche le message "aucune compétence" si nécessaire.
        if (!aCliqueOk && canvasAucuneCompetence != null && GameConfig.competenceSelectionnee == false 
            && field.numberOfGrids % 2 != 0)
            canvasAucuneCompetence.SetActive(true);

        Time.timeScale = 1f;
    }

    /// <summary>
    /// Depuis la pause, ouvre les paramètres in-game sans recharger la scène.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void OnClickParametresGame()
    {
        if (pauseCanvas != null)
            pauseCanvas.SetActive(false);
        if (parametresCanvas != null)
            parametresCanvas.SetActive(true);

        // Alexis 05/04 18h30 
        // On cache le bouton pause quand on entre dans les paramètres
        if (boutonPause != null)
            boutonPause.SetActive(false);

        // Alexis 10/04 12h15
        if (degrade != null)
            degrade.SetActive(true);
    }

    /// <summary>
    /// Depuis les paramètres in-game, retourne au canvas de pause.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void OnClickRetourPause()
    {
        if (parametresCanvas != null)
            parametresCanvas.SetActive(false);
        if (pauseCanvas != null)
            pauseCanvas.SetActive(true);

        // Alexis 05/04 18h30 
        // On réaffiche le bouton pause
        if (boutonPause != null)
            boutonPause.SetActive(true);

        // Alexis 10/04 12h15
        if (degrade != null)
            degrade.SetActive(true);
    }

    // Alexis 28/03 11h30
    /// <summary>
    /// Recharge la scène de jeu pour rejouer une nouvelle partie.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void OnClickRejouer()
    {
        Time.timeScale = 1f;
        // Alexis 02/04 10h05
        // On remet theEnd à false pour pouvoir rejouer.
        GO_ClickableActor.theEnd = false;
        isTuto = false;
        aCliqueOk = false;
        SceneManager.LoadScene("Game");
    }

    // Alexis 28/03 15h00
    /// <summary>
    /// Charge la scène des crédits.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void OnClickCredits()
    {
        SceneManager.LoadScene("Crédits");
    }

    // Alexis 28/03 15h00
    /// <summary>
    /// Retourne aux paramètres du menu depuis les crédits.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void OnClickRetourParametresMenu()
    {
        SceneManager.LoadScene("Parametres Menu");
    }

    // Alexis 13/04 15h45
    /// <summary>
    /// Lance le tutoriel en mode jeu.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void OnClickTutoriel()
    {
        isTuto = true;
        SceneManager.LoadScene("Game");
        GameConfig.competenceSelectionnee = true;
    }

    /// <summary>
    /// Ferme le canvas "aucune compétence" quand le joueur clique sur OK.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void OnClickBoutonOk()
    {
        if(canvasAucuneCompetence != null)
            canvasAucuneCompetence.SetActive(false);
        aCliqueOk = true;
    }

}
