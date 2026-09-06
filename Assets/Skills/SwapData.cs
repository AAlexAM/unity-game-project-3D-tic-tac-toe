using UnityEngine;

// Données de configuration de la compétence Swap.
// Créé comme asset Unity via le menu "Skill/Swap".
[CreateAssetMenu(menuName = "Skill/Swap")]
public class SwapData : SkillData
{
    /// <summary>
    /// Crée une instance de SwapSkill avec ces données.
    /// Entrée : aucune.
    /// Sortie : C_Skill — une nouvelle instance de SwapSkill.
    /// </summary>
    public override C_Skill CreateSkill()
    {
        return new SwapSkill(this);
    }
}