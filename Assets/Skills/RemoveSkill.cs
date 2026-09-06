using UnityEngine;

// Compétence qui supprime un pion adverse d'une cellule choisie par le joueur.
// La cellule redevient vide et cliquable après utilisation.
// On ne peut pas cibler une cellule vide ou une cellule occupée par le joueur courant.
public class RemoveSkill : C_Skill
{
    private RemoveData rmData;

    // Référence à la cellule ciblée par le joueur.
    private GO_ClickableActor cible;

    /// <summary>
    /// Constructeur. Mémorise les données de configuration.
    /// Entrée : data — données de configuration de la compétence Remove.
    /// </summary>
    public RemoveSkill(RemoveData data) : base(data)
    {
        rmData = data;
    }

    /// <summary>
    /// Lance la demande de sélection de la cellule à vider.
    /// Appelée automatiquement par Utiliser().
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public override void DemanderCible()
    {
        Debug.Log("Choisis une cible à supprimer");
        InputManager.Instance.StartSelection(OnCibleChoisie);
    }

    /// <summary>
    /// Callback appelé quand le joueur clique sur une cellule à vider.
    /// Rejette la sélection si la cellule est vide ou appartient au joueur courant.
    /// Entrée : obj — le GameObject de la cellule cliquée.
    /// Sortie : aucune.
    /// </summary>
    private void OnCibleChoisie(GameObject obj)
    {
        cible = obj.GetComponent< GO_ClickableActor>();
        // Alexis 10/04 13h15 On rejette si la cellule est vide ou appartient au joueur courant
        if (cible == null || cible.occupiedBy == 0 || cible.occupiedBy == button.cj.index + 1) 
        {
            InputManager.Instance.StartSelection(OnCibleChoisie);
            return;
        }

        ActiverEffet();
        Fin();
        button.cj.EndTurn();
    }

    /// <summary>
    /// Vide la cellule ciblée : remet sa couleur d'origine, réinitialise son propriétaire
    /// et la rend à nouveau cliquable.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public override void ActiverEffet()
    {
        Debug.Log("Remove : Activé");
        if (cible != null)
        {
            // On remet la couleur d'origine de la cellule (blanc ou couleur de base).
            cible.rd.material.color = cible.base_color;
            // On indique que la cellule n'appartient plus à personne.
            cible.occupiedBy = 0;
            // On réactive les clics sur la cellule (elle peut être rejouée).
            cible.Enable();
            // Note : isOccupied n'est pas remis à false intentionnellement ici.
            Debug.Log("Objet supprimé");
        }
    }
}