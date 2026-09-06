using UnityEngine;

// Classe abstraite représentant une compétence dont l'effet dure plusieurs tours.
// Hérite de C_Skill et ajoute un cooldown d'effet (effect_cooldown) distinct du cooldown de recharge.
// Les sous-classes définissent ResetEffect() pour supprimer l'effet une fois le cooldown écoulé.
public abstract class C_OverTime : C_Skill
{
    // Nombre de tours restants avant que l'effet de la compétence se termine.
    protected int effect_cooldown;

    /// <summary>
    /// Constructeur. Initialise le cooldown d'effet à la valeur fournie.
    /// Entrée : data — données de configuration, cooldown — durée de l'effet en tours.
    /// </summary>
    public C_OverTime(SkillData data, int cooldown) : base(data)
    {
        effect_cooldown = cooldown;
    }

    /// <summary>
    /// Décrémente le cooldown d'effet d'un tour. Quand il atteint zéro, supprime l'effet.
    /// Appelée à chaque fin de tour via Player.over_time_cooldown().
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void cool()
    {
        Debug.Log("cool() appelé, effect_cooldown=" + effect_cooldown);
        if (effect_cooldown > 0)
            effect_cooldown--;
        else
        {
            // On utilise int.MinValue comme sentinelle pour éviter que ResetEffect()
            // soit appelé à nouveau lors du tour suivant si la suppression de la liste échoue.
            effect_cooldown = int.MinValue;
            ResetEffect();
        }
    }

    /// <summary>
    /// Supprime l'effet de la compétence une fois son cooldown écoulé.
    /// Doit être implémenté par chaque sous-classe pour définir comment l'effet est annulé.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    protected abstract void ResetEffect();
}
