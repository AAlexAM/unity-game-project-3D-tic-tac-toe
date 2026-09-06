using UnityEngine;

// Alexis 11/04 10h30
// Classe statique de configuration de la partie.
// Stocke les paramètres choisis dans le menu de personnalisation avant de lancer la partie.
// Statique : pas de GameObject associé, persiste entre les scènes, non instanciable.
public static class GameConfig
{
    public static string nomJoueur1 = "Joueur Bleu";
    public static string nomJoueur2 = "Joueur Rouge";
    public static Color couleurJoueur1 = Color.blue;
    public static Color couleurJoueur2 = Color.red;
    public static int tailleTerrain = 3;
    // Indique si la partie a été configurée manuellement via le menu de personnalisation.
    public static bool modePersonnalise = false;
    // Indique si au moins une compétence a été sélectionnée (affecte la cellule centrale).
    public static bool competenceSelectionnee = false;
    // Compétences choisies par chaque joueur (4 max)
    public static SkillData[] skillsJoueur1 = new SkillData[4];
    public static SkillData[] skillsJoueur2 = new SkillData[4];

    /// <summary>
    /// Remet toutes les valeurs de configuration à leurs valeurs par défaut.
    /// Appelée lors du retour au menu principal pour ne pas contaminer la partie suivante.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public static void Reset()
    {
        nomJoueur1 = "Joueur Bleu";
        nomJoueur2 = "Joueur Rouge";
        couleurJoueur1 = Color.blue;
        couleurJoueur2 = Color.red;
        tailleTerrain = 3;
        modePersonnalise = false;
        skillsJoueur1 = new SkillData[4];
        skillsJoueur2 = new SkillData[4];
        competenceSelectionnee = false;
    }
}
