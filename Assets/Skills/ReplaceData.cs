using UnityEngine;


// Données de configuration de la compétence Replace.
// Créé comme asset Unity via le menu "Skill/Replace".
[CreateAssetMenu(menuName = "Skill/Replace")]
public class ReplaceData : SkillData
{
    /// <summary>
    /// Crée une instance de ReplaceSkill avec ces données.
    /// Entrée : aucune.
    /// Sortie : C_Skill — une nouvelle instance de ReplaceSkill.
    /// </summary>
    public override C_Skill CreateSkill()
    {
        return new ReplaceSkill(this);
    }
}
