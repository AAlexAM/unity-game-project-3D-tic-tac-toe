using UnityEngine;

// Classe de base abstraite pour toutes les données de compétences.
// Hérite de ScriptableObject pour pouvoir être créée comme asset Unity (fichier .asset)
// et assignée depuis l'inspecteur sans code supplémentaire.
// Chaque type de compétence crée une sous-classe de SkillData (ex: SwapData, BlockData).
public abstract class SkillData : ScriptableObject
{
    // Icône affichée sur le bouton de la compétence dans l'interface.
    public Texture2D icone;

    // Nombre de tours d'attente entre deux charges de cette compétence.
    // Plus ce nombre est élevé, plus la compétence est rare à utiliser.
    public int cooldownMax = 3;

    // Alexis 07/04 14h30
    // Nombre maximum de charges que le joueur peut accumuler simultanément.
    public int stackMax = 2;

    /// <summary>
    /// Crée et retourne une instance de compétence correspondant à ce type de données.
    /// Chaque sous-classe implémente cette méthode pour instancier la bonne classe de compétence.
    /// Entrée : aucune.
    /// Sortie : C_Skill — l'instance de compétence créée.
    /// </summary>
    public abstract C_Skill CreateSkill();
}
