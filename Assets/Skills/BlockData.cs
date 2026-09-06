using UnityEngine;


// Données de configuration de la compétence Block.
// Créé comme asset Unity via le menu "Skill/Block".
[CreateAssetMenu(menuName = "Skill/Block")]
public class BlockData : SkillData
{
    // Durée en tours pendant laquelle la cellule reste bloquée après activation.
    public int effectCooldown;

    /// <summary>
    /// Crée une instance de BlockSkill avec ces données.
    /// Entrée : aucune.
    /// Sortie : C_Skill — une nouvelle instance de BlockSkill.
    /// </summary>
    public override C_Skill CreateSkill()
    {
        return new BlockSkill(this, effectCooldown);
    }
}