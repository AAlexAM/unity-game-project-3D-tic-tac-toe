using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// Alexis 11/04 10h45
// Ce composant gère le menu de personnalisation d'une partie.
// Il permet de configurer les noms, couleurs, compétences et taille du terrain
// pour les deux joueurs avant de lancer une partie personnalisée.
public class MenuPersonnalisation : MonoBehaviour
{
    // Panel affiché quand le joueur clique sur "Jouer" pour choisir le mode.
    public GameObject panelChoixMode;
    // Panel principal de personnalisation.
    public GameObject panelPersonnalisation;

    // Champ de saisie du nom du joueur 1.
    public TMP_InputField inputNomJ1;
    // Image affichant la couleur actuellement choisie pour le joueur 1.
    public Image couleurSelectionneeJ1;
    public Image fondCouleurSelectionneeJ1;
    // Conteneur des boutons de compétences du joueur 1.
    public Transform parentBoutonsSkillsJ1;

    public TMP_InputField inputNomJ2;
    public Image couleurSelectionneeJ2;
    public Image fondCouleurSelectionneeJ2;
    public Transform parentBoutonsSkillsJ2;

    // Texte affichant la taille actuelle du terrain.
    public TMP_Text texteTaille;
    // Taille courante du terrain (entre 3 et 5).
    private int tailleCourante = 3;

    // Prefab d'un bouton de compétence à instancier dans le menu.
    public GameObject skillButtonPrefab;

    // Toutes les compétences disponibles dans le projet.
    public SkillData[] toutesLesSkills;

    // Compétences sélectionnées par chaque joueur (jusqu'à 4 slots).
    private SkillData[] skillsJ1 = new SkillData[4];
    private SkillData[] skillsJ2 = new SkillData[4];

    // Palette de couleurs disponibles pour les joueurs.
    private Color[] couleursDisponibles = new Color[]
    {
        Color.blue, Color.red, Color.green, Color.yellow,
        new Color(1f, 0.5f, 0f), // orange
        Color.magenta
    };

    // Couleurs actuellement sélectionnées par chaque joueur.
    private Color couleurJ1 = Color.blue;
    private Color couleurJ2 = Color.red;

    /// <summary>
    /// Initialise l'affichage du menu de personnalisation.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    void Start()
    {
        texteTaille.text = tailleCourante.ToString();
        MettreAJourCouleur();
    }

    /// <summary> Retourne au panel de choix de mode depuis la personnalisation. </summary>
    public void OnClickRetourChoixMode()
    {
        panelPersonnalisation.SetActive(false);
        panelChoixMode.SetActive(true);
    }

    /// <summary> Ferme le panel de choix de mode. </summary>
    public void OnClickRetourMenu()
    {
        Boutons.isTuto = false;
        panelChoixMode.SetActive(false);
    }

    /// <summary> Affiche le panel de choix de mode quand le joueur clique sur Jouer. </summary>
    public void OnClickJouer()
    {
        panelChoixMode.SetActive(true);
    }

    /// <summary>
    /// Lance une partie en mode normal avec les paramètres par défaut.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void OnClickModeNormal()
    {
        Boutons.isTuto = false;
        GameConfig.Reset();
        GameConfig.competenceSelectionnee = true;
        LancerPartie();
    }

    /// <summary>
    /// Affiche le panel de personnalisation pour configurer la partie.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void OnClickModePersonnalise()
    {
        panelChoixMode.SetActive(false);
        panelPersonnalisation.SetActive(true);
        GenererBoutonsSkills();
    }

    /// <summary>
    /// Génère les boutons de compétences pour les deux joueurs en vidant les anciens.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    private void GenererBoutonsSkills()
    {
        foreach (Transform child in parentBoutonsSkillsJ1) Destroy(child.gameObject);
        foreach (Transform child in parentBoutonsSkillsJ2) Destroy(child.gameObject);

        foreach (SkillData skill in toutesLesSkills)
        {
            CreerBoutonSkill(skill, parentBoutonsSkillsJ1, skillsJ1);
            CreerBoutonSkill(skill, parentBoutonsSkillsJ2, skillsJ2);
        }
    }

    /// <summary>
    /// Instancie un bouton pour une compétence dans un conteneur donné.
    /// Entrée : skill — compétence à afficher, parent — conteneur, skills — tableau de sélection.
    /// Sortie : aucune.
    /// </summary>
    private void CreerBoutonSkill(SkillData skill, Transform parent, SkillData[] skills)
    {
        GameObject go = Instantiate(skillButtonPrefab, parent);
        Button btn = go.GetComponent<Button>();
        TMP_Text texte = go.GetComponentInChildren<TMP_Text>();
        RawImage icon = go.GetComponentInChildren<RawImage>();

        texte.text = skill.name;
        if (icon != null && skill.icone != null) icon.texture = skill.icone;

        // On capture skill et skills par valeur pour le lambda (closure).
        btn.onClick.AddListener(() => SelectionnerSkill(skill, skills, icon));
    }

    /// <summary>
    /// Sélectionne ou désélectionne une compétence pour un joueur.
    /// Entrée : skill — compétence cliquée, skills — tableau du joueur, boutonImage — icône du bouton.
    /// Sortie : aucune.
    /// </summary>
    private void SelectionnerSkill(SkillData skill, SkillData[] skills, RawImage boutonImage)
    {
        // Si la compétence est déjà sélectionnée, on la désélectionne.
        for (int i = 0; i < skills.Length; i++)
        {
            if (skills[i] == skill)
            {
                skills[i] = null;
                boutonImage.color = Color.white;
                GameConfig.competenceSelectionnee = false;
                return;
            }
        }

        // Sinon on cherche un slot libre pour l'ajouter.
        for (int i = 0; i < skills.Length; i++)
        {
            if (skills[i] == null)
            {
                skills[i] = skill;
                boutonImage.color = Color.green;
                GameConfig.competenceSelectionnee = true;
                return;
            }
        }

        Debug.Log("2 compétences max !");
    }

    /// <summary> Augmente la taille du terrain (maximum 5). </summary>
    public void OnClickAugmenterTaille()
    {
        if (tailleCourante < 5) { tailleCourante++; texteTaille.text = tailleCourante.ToString(); }
        else Debug.Log("Taille maximum : 5 !");
    }

    /// <summary> Diminue la taille du terrain (minimum 3). </summary>
    public void OnClickDiminuerTaille()
    {
        if (tailleCourante > 3) { tailleCourante--; texteTaille.text = tailleCourante.ToString(); }
        else Debug.Log("Taille minimum : 3 !");
    }

    /// <summary> Passe à la couleur suivante pour le joueur 1 dans la palette disponible. </summary>
    public void OnClickCouleurJ1()
    {
        int index = System.Array.IndexOf(couleursDisponibles, couleurJ1);
        couleurJ1 = couleursDisponibles[(index + 1) % couleursDisponibles.Length];
        MettreAJourCouleur();
    }

    /// <summary> Passe à la couleur suivante pour le joueur 2 dans la palette disponible. </summary>
    public void OnClickCouleurJ2()
    {
        int index = System.Array.IndexOf(couleursDisponibles, couleurJ2);
        couleurJ2 = couleursDisponibles[(index + 1) % couleursDisponibles.Length];
        MettreAJourCouleur();
    }

    /// <summary>
    /// Met à jour les images de prévisualisation des couleurs des deux joueurs.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    private void MettreAJourCouleur()
    {
        if (couleurSelectionneeJ1 != null && fondCouleurSelectionneeJ1 != null)
        {
            couleurSelectionneeJ1.color = couleurJ1;
            fondCouleurSelectionneeJ1.color = couleurJ1;
        }
        if (couleurSelectionneeJ2 != null && fondCouleurSelectionneeJ2 != null)
        {
            couleurSelectionneeJ2.color = couleurJ2;
            fondCouleurSelectionneeJ2.color = couleurJ2;
        }
    }

    /// <summary>
    /// Valide la configuration et lance la partie personnalisée.
    /// Stocke tous les paramètres dans GameConfig avant de charger la scène de jeu.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void OnClickLancerPersonnalise()
    {
        GameConfig.nomJoueur1 = string.IsNullOrEmpty(inputNomJ1.text) ? "Joueur 1" : inputNomJ1.text;
        GameConfig.nomJoueur2 = string.IsNullOrEmpty(inputNomJ2.text) ? "Joueur 2" : inputNomJ2.text;
        GameConfig.couleurJoueur1 = couleurJ1;
        GameConfig.couleurJoueur2 = couleurJ2;
        GameConfig.tailleTerrain = tailleCourante;
        GameConfig.skillsJoueur1 = skillsJ1;
        GameConfig.skillsJoueur2 = skillsJ2;
        GameConfig.modePersonnalise = true;
        LancerPartie();
    }

    /// <summary>
    /// Charge la scène de jeu pour démarrer la partie.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    private void LancerPartie()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
    }
}