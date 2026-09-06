using UnityEngine;


// Données de configuration de la compétence Remove.
// Créé comme asset Unity via le menu "Skill/Remove".
[CreateAssetMenu(menuName = "Skill/Remove")]
public class RemoveData : SkillData
{
    /// <summary>
    /// Crée une instance de RemoveSkill avec ces données.
    /// Entrée : aucune.
    /// Sortie : C_Skill — une nouvelle instance de RemoveSkill.
    /// </summary>
    public override C_Skill CreateSkill()
    {
        return new RemoveSkill(this);
    }
}