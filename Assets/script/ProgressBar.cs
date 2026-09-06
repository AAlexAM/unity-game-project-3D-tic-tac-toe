using UnityEngine;
using UnityEngine.UI;

// Alexis 10/04 10h45
// Composant générique de barre de progression UI.
// Reçoit une valeur entre 0 et 1 et remplit proportionnellement une Image en mode "Filled".
// Réutilisable pour les cooldowns de compétences, les barres de vie, etc.
public class ProgressBar : MonoBehaviour
{
    // Image en mode "Filled" dans l'Inspector (type : Filled, fill method : Horizontal)
    // Permet d'avoir .fillAmount qui accpte une valeur entre 0 et 1 et remplit l'image proportionnellement
    public Image fillImage;

    /// <summary>
    /// Met à jour le remplissage de la barre avec une valeur normalisée.
    /// Entrée : value — valeur entre 0 (vide) et 1 (plein). Clampée automatiquement.
    /// Sortie : aucune.
    /// </summary>
    public void SetValue(float value)
    {
        // Clamp01 force la valeur entre 0 et 1 pour éviter les débordements visuels.
        fillImage.fillAmount = Mathf.Clamp01(value); 
    }
}
