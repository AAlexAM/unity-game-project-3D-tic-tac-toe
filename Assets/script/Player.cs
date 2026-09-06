using UnityEngine;
using System.Linq;
using System.Collections.Generic;


// Représente les données et l'état d'un joueur pendant une partie.
// Marqué [System.Serializable] pour être configurable directement dans l'inspecteur Unity.
[System.Serializable]
public class Player
{
    // Nom affiché dans l'interface et à la victoire.
    public string playerName;

    // Couleur des pions de ce joueur.
    public Color color;

    // Tableau des données de compétences choisies avant la partie (peut contenir des nulls).
    public SkillData[] skillDatas;

    // Tableau des instances de compétences actives en jeu, créées à partir de skillDatas.
    // Internal se comporte comme public à la différence qu'un champ internal n'apparaît pas dans l'inspecteur Unity.
    internal C_Skill[] skills;

    // Liste des effets de compétences durables actuellement actifs pour ce joueur.
    internal List<C_OverTime> ovSkills = new List<C_OverTime>();

    // Compétence actuellement en cours d'utilisation (en attente d'une cible). Null si aucune.
    internal C_Skill usingSkill = null;

    // Si true, le cooldown des effets durables n'est pas décrémenté ce tour-ci.
    // Utilisé pour éviter de décrémenter dans le même tour qu'une activation.
    internal bool skipOverTimeCooldown = false;

    /// <summary>
    /// Décrémente le cooldown de recharge de toutes les compétences de ce joueur.
    /// Appelée à chaque fin de tour pour le joueur courant.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void skills_cooldown()
    {
        foreach (C_Skill c in skills)
            c.Attente();
    }

    /// <summary>
    /// Décrémente le cooldown d'effet de tous les effets durables actifs de ce joueur.
    /// Appelée à chaque fin de tour pour les deux joueurs.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void over_time_cooldown()
    {
        // On itère en sens inverse pour pouvoir supprimer des éléments sans décalage d'indices.
        for (int i = ovSkills.Count - 1; i >= 0; i--)
            ovSkills[i].cool();
    }

    /// <summary>
    /// Active une compétence si elle appartient au joueur et si elle a au moins une charge.
    /// Entrée : skill — la compétence à activer.
    /// Sortie : bool — true si la compétence a été activée, false sinon.
    /// </summary>
    public bool enable_skill(C_Skill skill)
    {
        if (skills.Contains(skill) && skill.stack > 0)
        {
            usingSkill = skill;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Désactive la compétence en cours et annule la sélection de cible en attente.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void disable_skill()
    {
        usingSkill = null;
        InputManager.Instance.CancelSelection();
    }

    /// <summary>
    /// Initialise les compétences du joueur à partir de skillDatas.
    /// Filtre les slots vides (null) et crée les instances via CreateSkill().
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void Init()
    {
        int count = 0;
        foreach (SkillData sd in skillDatas)
            if (sd != null) count++;

        skills = new C_Skill[count];
        int index = 0;
        for (int i = 0; i < skillDatas.Length; i++)
        {
            if (skillDatas[i] != null)
                skills[index++] = skillDatas[i].CreateSkill();
        }
        ovSkills.Clear();
    }
}
