using UnityEngine;

// Compétence qui intervertit toutes les cellules de deux grilles choisies par le joueur.
// Nécessite deux clics successifs pour sélectionner les deux grilles à échanger.
// Interdit si l'une des grilles contient une cellule bloquée par BlockSkill.
public class SwapSkill : C_Skill
{
    private SwapData swData;

    // Première grille sélectionnée par le joueur.
    private GO_GridGenerator cible1;
    // Deuxième grille sélectionnée par le joueur.
    private GO_GridGenerator cible2;

    /// <summary>
    /// Constructeur. Mémorise les données de configuration.
    /// Entrée : data — données de configuration de la compétence Swap.
    /// </summary>
    public SwapSkill(SwapData data) : base(data)
    {
        swData = data;
    }

    /// <summary>
    /// Lance la demande de sélection de la première grille.
    /// Appelée automatiquement par Utiliser().
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public override void DemanderCible()
    {
        Debug.Log("Choisis la première cible");
        InputManager.Instance.StartSelection(OnFirstSelected);
    }

    /// <summary>
    /// Callback appelé quand le joueur clique sur une cellule pour choisir la première grille.
    /// Si la grille est invalide, relance la sélection.
    /// Entrée : obj — le GameObject de la cellule cliquée.
    /// Sortie : aucune.
    /// </summary>
    private void OnFirstSelected(GameObject obj)
    {
        // On remonte depuis la cellule cliquée jusqu'à la grille parente.
        cible1 = obj.GetComponent<GO_ClickableActor>().GetComponentInParent<GO_GridGenerator>();

        if (!EstValide(cible1))
        {
            Debug.Log("Retry cible 1");
            InputManager.Instance.StartSelection(OnFirstSelected);
            return;
        }

        Debug.Log("Choisis la deuxième cible");
        InputManager.Instance.StartSelection(OnSecondSelected);
    }

    /// <summary>
    /// Callback appelé quand le joueur clique sur une cellule pour choisir la deuxième grille.
    /// Si la grille est invalide ou identique à la première, relance la sélection.
    /// Déclenche l'effet si les deux cibles sont valides.
    /// Entrée : obj — le GameObject de la cellule cliquée.
    /// Sortie : aucune.
    /// </summary>
    private void OnSecondSelected(GameObject obj)
    {
        cible2 = obj.GetComponent<GO_ClickableActor>().GetComponentInParent<GO_GridGenerator>();

        if (!EstValide(cible2) || cible2 == cible1)
        {
            Debug.Log("Retry cible 2");
            InputManager.Instance.StartSelection(OnSecondSelected);
            return;
        }

        ActiverEffet();
        Fin();
        button.cj.EndTurn();
    }

    /// <summary>
    /// Vérifie si une grille peut être sélectionnée pour le swap.
    /// Une grille est invalide si elle est nulle, non initialisée,
    /// ou contient une cellule bloquée par BlockSkill.
    /// Entrée : cible — la grille à valider.
    /// Sortie : bool — true si la grille est éligible au swap, false sinon.
    /// </summary>
    private bool EstValide(GO_GridGenerator cible)
    {
        if (cible == null || cible.actors == null) return false;

        // On refuse le swap si l'une des cellules de la grille est bloquée par une compétence.
        foreach (GO_ClickableActor actor in cible.actors)
            if (actor.isBlockedBySkill) return false;

        return true;
    }

    /// <summary>
    /// Échange les données logiques des deux grilles sélectionnées.
    /// Met à jour le Checker après l'échange pour refléter le nouvel état du terrain.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public override void ActiverEffet()
    {
        Debug.Log("Swap activé");

        if (cible1 == null || cible2 == null)
            return;

        Checker chk = Object.FindFirstObjectByType<Checker>();
        chk?.print_grid();
        // On délègue l'échange à GO_GridGenerator qui parcourt toutes ses cellules.
        cible1.swap(cible2);
        // On met à jour la représentation interne avec -1 pour indiquer un coup de compétence.
        chk?.make_grid(null);
        chk?.print_grid();
    }
}
