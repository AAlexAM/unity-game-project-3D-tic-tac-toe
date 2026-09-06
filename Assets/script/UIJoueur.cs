using UnityEngine;
using UnityEngine.UI;



//Alexis 24/03 10h00
// Ce composant gère le panneau de compétences d'un joueur dans l'interface.
// Il affiche les boutons de compétences et gère l'ouverture/fermeture du menu déroulant.
public class UIJoueur : MonoBehaviour
{
    // Panel contenant les boutons de compétences, affiché ou masqué selon l'état du menu.
    public GameObject menuPanel;
    
    // Alexis 27/03 13h00 ajout :
    // Le bouton qui ouvre/ferme le panel 
    // Permet de gérer la position du bouton (suit le panel)
    // Accrocher le bouton enfant du Canvas Menu où se trouve le script 
    public RectTransform boutonPanel;   
    
    // Indique si le menu est ouvert ou fermé
    private bool isOpen = false;

    // Alexis 27/03 13h00
    // Garde la position initiale du bouton
    private Vector2 boutonInitialPos;

    // Noam 01/04 ---
    // Prefab d'un bouton de compétence à instancier pour chaque skill du joueur.
    public GameObject skillButtonPrefab;

    // Conteneur dans lequel les boutons de compétences seront instanciés.
    public Transform skillContainer;

    // Ordre d'affichage de ce UIJoueur (0 = premier, 1 = deuxième).
    // Utilisé pour associer correctement les UIJoueur aux joueurs.
    public int order;

    // Référence au joueur dont ce UIJoueur affiche les compétences.
    public Player player;
    // ------------

    /// <summary>
    /// Initialise l'interface du joueur : colorie le bouton, ferme le menu et crée les boutons de compétences.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void Init()
    {
        // On colorie le bouton avec la couleur du joueur pour l'identifier visuellement.
        boutonPanel.GetComponent<Image>().color = player.color;
        // Au démarrage le menu est fermé
        menuPanel.SetActive(false);
        // Alexis 27/03 13h00
        // On garde la position initiale du bouton
        boutonInitialPos = boutonPanel.anchoredPosition;

        // Noam 01/04
        // Affiche les boutons
        CreerBoutonsSkills();
    }

    /// <summary>
    /// Ouvre ou ferme le menu de compétences selon son état actuel.
    /// Bloquée pendant la pause (timeScale = 0).
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void OuvrirFermer()
    {

        // On bloque l'ouverture/fermeture du menu pendant la pause
        if (Time.timeScale == 0f) return;

        isOpen = !isOpen;
        menuPanel.SetActive(isOpen);
        // Alexis 27/03 13h00
        UpdateBoutonPosition();
    }

    // Alexis 27/03 13h00
    /// <summary>
    /// Déplace le bouton pour qu'il suive le bord du panel ouvert.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    private void UpdateBoutonPosition()
    {
        if (isOpen)
        {
            // Déplacer le bouton pour qu'il "suit" le panel
            // Recupere le transform du menuPanel et le cast en RectTransform (pour utiliser .rect.width
            // .rect.width recupere la largeur du panel
            float largeur = ((RectTransform)menuPanel.transform).rect.width;
            // Ajoute la largeur du panel à la position initiale
            // Ici le bouton s'ouvre sur la droite
            // Pour ouvrir sur la gauche 2 options :
            // Soit compléter ce script avec -largeur
            // Soit pivoter de 180° et modifier l'anchor dans unity --> option adoptée
            boutonPanel.anchoredPosition = boutonInitialPos + new Vector2(largeur, 0);
        }
        else
        {
            // Retour à sa position initiale
            boutonPanel.anchoredPosition = boutonInitialPos;
        }
    }

    /// <summary>
    /// Ferme le menu de compétences de force.
    /// Appelée depuis Boutons.cs lors de l'ouverture de la pause.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void FermerMenu()
    {
        isOpen = false;
        menuPanel.SetActive(false);
        // Alexis 27/03 13h00
        // Retour à sa position initiale
        boutonPanel.anchoredPosition = boutonInitialPos;
    }

    /// <summary>
    /// Instancie un bouton de compétence pour chaque skill du joueur dans le conteneur.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void CreerBoutonsSkills()
    {
        Debug.Log("creation");

        //  Vérifications de sécurité
        if (player == null)
        {
            Debug.LogError("Player est null !");
            return;
        }

        if (player.skills == null)
        {
            Debug.LogError("player.skills est null !");
            return;
        }

        if (skillContainer == null)
        {
            Debug.LogError("skillContainer non assigné !");
            return;
        }

        if (skillButtonPrefab == null)
        {
            Debug.LogError("skillButtonPrefab non assigné !");
            return;
        }

        // Nettoyage
        foreach (Transform child in skillContainer)
        {
            Destroy(child.gameObject);
        }

        // Création des boutons
        foreach (C_Skill skill in player.skills)
        {
            if (skill == null)
            {
                Debug.LogWarning("Skill null ignoré");
                continue;
            }

            GameObject obj = Instantiate(skillButtonPrefab, skillContainer);

            SkillButton btn = obj.GetComponent<SkillButton>();

            if (btn == null)
            {
                Debug.LogError("Le prefab n'a pas de SkillButton !");
                continue;
            }

            btn.Init(skill, player);

            // Alexis 07/04 14h15
            // Permet à SkillButton de fermer le menu à la victoire
            // ( skillMenuPanel est un gameObject dans SkillButton.cs ) 
            btn.skillMenuPanel = menuPanel; 

        }
    }
    /*Debug.Log("creation");
        // Sécurité : vider les anciens boutons si jamais
        if (skillContainer)
        {
            foreach (Transform child in skillContainer)
            {
                Destroy(child.gameObject);
            }
        }

        // Créer un bouton par skill
        foreach (C_Skill skill in player.skills)
        {
            GameObject obj = Instantiate(skillButtonPrefab, skillContainer);

            SkillButton btn = obj.GetComponent<SkillButton>();
            btn.Init(skill);

        }
    }*/
}