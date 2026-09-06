using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Ce composant gère un bouton de compétence dans l'interface utilisateur.
// Il affiche l'icône, le nombre de charges disponibles et la barre de progression du cooldown.
// Il gère aussi l'activation et la désactivation de la compétence au clic.
public class SkillButton : MonoBehaviour
{
    // Instance de compétence associée à ce bouton.
    private C_Skill skill;

    // Image affichant l'icône de la compétence.
    public RawImage icon;
    // Composant Button Unity pour détecter les clics.
    public Button button;
    // Texte affichant le nombre de charges disponibles (ex: "x2").
    public TMP_Text texte;
    // Référence au gestionnaire de jeu pour accéder au joueur courant.
    public ControleurJeu cj;

    // Joueur propriétaire de cette compétence.
    public Player owner;

    // Alexis 07/04 13h45
    // Panel du menu de compétences contenant ce bouton.
    // Utilisé pour fermer le menu automatiquement à la victoire.
    public GameObject skillMenuPanel;

    // Alexis 10/04 11h00
    // Référence à la barre de progression du cooldown de compétence
    public ProgressBar cooldownBar;


    // Alexis 07/04 13h45
    /// <summary>
    /// Vérifie à chaque frame si la partie est terminée pour fermer le menu automatiquement.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    void Update()
    {
        if (GO_ClickableActor.theEnd && skillMenuPanel != null && skillMenuPanel.activeSelf)
        {
            skillMenuPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Initialise ce bouton avec la compétence et le joueur associés.
    /// S'abonne aux mises à jour de la compétence et configure l'affichage initial.
    /// Entrée : skill — la compétence à afficher, p — le joueur propriétaire.
    /// Sortie : aucune.
    /// </summary>
    public void Init(C_Skill skill, Player p)
    {
        this.skill = skill;
        skill.button = this;
        owner = p;
        cj = FindFirstObjectByType<ControleurJeu>();

        // On s'abonne à l'événement onUpdate de la compétence pour mettre à jour l'affichage
        // automatiquement à chaque changement de cooldown ou de charges.
        skill.onUpdate += UpdateTexte;

        // On affiche l'icône si elle est disponible.
        if (icon != null && skill.GetIcone() != null)
        {
            icon.texture = skill.GetIcone();
        }

        UpdateTexte();
        // On branche le clic du bouton Unity sur notre méthode OnClick.
        button.onClick.AddListener(OnClick);
    }

    /// <summary>
    /// Met à jour l'affichage du nombre de charges et de la barre de cooldown.
    /// Appelée automatiquement par l'événement onUpdate de la compétence.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void UpdateTexte()
    {
        texte.text = "x" + skill.stack.ToString();

        // Alexis 10/04 11h15
        // Mise à jour de la barre de chargement
        // On calcule la progression du cooldown : 0 = cooldown plein, 1 = chargée.
        // Le cooldown diminue de cooldownMax jusqu'à 0, donc on inverse pour la barre.
        if (cooldownBar != null)
            cooldownBar.SetValue(1f - ((float)skill.cooldown / skill.CooldownMax));
    }


    /// <summary>
    /// Gère le clic sur ce bouton : active ou désactive la compétence selon son état courant.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    void OnClick()
    {
        // On ne traite le clic que si c'est bien le tour du propriétaire de cette compétence.
        if (cj.CurrentPlayer == owner)
        {
            if (owner.usingSkill == skill)
            {
                // Si la compétence était déjà active, on la désactive et on rembourse la charge.
                owner.disable_skill();
                skill.stack++;
                // Alexis 10/04 13h
                // On dégrisse l'icône quand on désélectionne
                icon.color = Color.white;
            }
            else if (owner.usingSkill != skill)
            {
                // Dégriser l'ancienne compétence avant d'en activer une nouvelle
                if (owner.usingSkill != null && owner.usingSkill.button != null)
                {
                    owner.usingSkill.stack++;
                    owner.usingSkill.button.icon.color = Color.white;
                    owner.usingSkill.button.UpdateTexte();
                }

                // On tente d'activer cette compétence.
                if (owner.enable_skill(skill))
                {
                    skill.Utiliser();
                    // Alexis 10/04 13h
                    // On grise l'icône quand la compétence est sélectionnée
                    icon.color = new Color(0.4f, 0.4f, 0.4f, 1f);
                }
            }
        }
        else
            Debug.Log("Error cannot enable skill");
        UpdateTexte();
    }


    /// <summary>
    /// Appelée automatiquement par Unity quand ce GameObject est détruit.
    /// Se désabonne de l'événement onUpdate pour éviter les fuites mémoire.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    void OnDestroy()
    {
        if (skill != null)
            skill.onUpdate -= UpdateTexte;
    }
}