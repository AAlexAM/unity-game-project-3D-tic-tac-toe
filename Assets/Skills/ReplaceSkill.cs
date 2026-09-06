using NUnit.Framework.Internal;
using UnityEngine;


// Compétence qui remplace le pion d'une cellule adverse par un pion du joueur courant.
// La cellule change de couleur et de propriétaire sans passer par un tour normal.
// On ne peut pas cibler une cellule vide ou une cellule déjà occupée par le joueur courant.
public class ReplaceSkill : C_Skill
{
    private ReplaceData rpData;

    // Référence à la cellule ciblée par le joueur.
    private GO_ClickableActor cible;

    /// <summary>
    /// Constructeur. Mémorise les données de configuration.
    /// Entrée : data — données de configuration de la compétence Replace.
    /// </summary>
    public ReplaceSkill(ReplaceData data) : base(data)
    {
        rpData = data;
    }

    /// <summary>
    /// Lance la demande de sélection de la cellule à remplacer.
    /// Appelée automatiquement par Utiliser().
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public override void DemanderCible()
    {
        Debug.Log("Choisis une cible à Remplacer");
        InputManager.Instance.StartSelection(OnCibleChoisie);
    }

    /// <summary>
    /// Callback appelé quand le joueur clique sur une cellule à remplacer.
    /// Rejette la sélection si la cellule est vide ou appartient déjà au joueur courant.
    /// Entrée : obj — le GameObject de la cellule cliquée.
    /// Sortie : aucune.
    /// </summary>
    private void OnCibleChoisie(GameObject obj)
    {
        cible = obj.GetComponent<GO_ClickableActor>();

        // Alexis 07/04 13h15
        // On rejette aussi si la cellule est vide(occupiedBy == 0)
        // car remplacer une case vide n'a pas de sens et cause un bug
        if (cible == null || cible.occupiedBy == button.cj.index + 1 || cible.occupiedBy == 0)
        {
            
            Debug.Log("Retry");
            InputManager.Instance.CancelSelection();
            InputManager.Instance.StartSelection(OnCibleChoisie);
            return;
        }

        ActiverEffet();
        Fin();
        button.cj.EndTurn();
    }

    /// <summary>
    /// Remplace le pion de la cellule ciblée par un pion du joueur courant.
    /// Met à jour la couleur, le propriétaire et notifie le Checker.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public override void ActiverEffet()
    {
        Debug.Log("Replace : Activé");
        if (cible != null)
        {
            // On change la couleur de la cellule pour celle du joueur courant.
            cible.rd.material.color = button.cj.CurrentPlayer.color;
            // On met à jour le propriétaire de la cellule.
            cible.occupiedBy = button.cj.index + 1;

            // Alexis 07/04 13h30
            // On ne modfiait que ocupiedBy et la couleur, alors
            // on notifie aussi le cheker comme dans GO_ClickableActor
            // Cela permet de réévaluer le terrain et vérifier un éventuel alignement
            Checker chk = Object.FindFirstObjectByType<Checker>();
            if (chk != null)
                chk.make_grid(cible.pos);

            Debug.Log("Objet Remplacé");
        }
    }
}