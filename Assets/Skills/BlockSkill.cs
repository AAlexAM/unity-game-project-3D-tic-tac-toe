using NUnit.Framework.Internal;
using UnityEngine;


// Compétence qui bloque une cellule choisie pendant un nombre de tours défini.
// La cellule bloquée devient semi-transparente et ne peut plus être jouée.
// Hérite de C_OverTime car son effet dure plusieurs tours.
// Compatible avec la compétence Swap : si la cellule bloquée est dans une grille swappée,
// la référence est mise à jour automatiquement via change_property().
public class BlockSkill : C_OverTime
{
    private BlockData BlData;

    // Référence directe au composant GO_ClickableActor de la cellule bloquée.
    // Mise à jour par change_property() si la cellule est déplacée par un swap.
    internal GO_ClickableActor AC;

    // Référence au composant GO_selector de la cellule bloquée.
    // Désactivé pour empêcher la sélection de la cellule par d'autres compétences.
    private GO_selector SE;

    // Référence au joueur qui a utilisé cette compétence.
    // Utilisée pour retirer la compétence de sa liste d'effets actifs à la fin.
    Player p;

    /// <summary>
    /// Constructeur. Mémorise les données de configuration et la durée de l'effet.
    /// Entrée : data — données de configuration, cooldownEffect — durée du blocage en tours.
    /// </summary>
    public BlockSkill(BlockData data, int cooldownEffect) : base(data, cooldownEffect)
    {
        BlData = data;
    }

    /// <summary>
    /// Lance la demande de sélection de la cellule à bloquer.
    /// Appelée automatiquement par Utiliser().
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public override void DemanderCible()
    {
        Debug.Log("Choisis une cible à bloquer");
        InputManager.Instance.StartSelection(OnCibleChoisie);
    }

    /// <summary>
    /// Callback appelé quand le joueur clique sur une cellule à bloquer.
    /// Relance la sélection si l'objet cliqué est invalide.
    /// Entrée : obj — le GameObject de la cellule cliquée.
    /// Sortie : aucune.
    /// </summary>
    private void OnCibleChoisie(GameObject obj)
    {
        if (obj == null)
        {
            Debug.Log("Retry");
            InputManager.Instance.StartSelection(OnCibleChoisie);
            return;
        }

        AC = obj.GetComponent<GO_ClickableActor>();
        if (AC == null)
        {
            InputManager.Instance.StartSelection(OnCibleChoisie);
            return;
        }

        SE = obj.GetComponent<GO_selector>();

        ActiverEffet();
        Fin();
        button.cj.EndTurn();
    }

    /// <summary>
    /// Applique l'effet de blocage sur la cellule sélectionnée.
    /// Désactive les interactions, rend la cellule semi-transparente et l'ajoute aux effets actifs.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public override void ActiverEffet()
    {
        Debug.Log("Block : Activé");
        if (AC == null) return;

        // On désactive le sélecteur pour empêcher la cellule d'être choisie par d'autres compétences.
        SE?.Disable();
        // On désactive les clics normaux sur la cellule.
        AC.Disable();
        // On réactive le Collider pour que les clics soient interceptés (et non traversants).
        AC.GetComponent<Collider>().enabled = true;
        // On marque la cellule comme sous effet visuel pour la protéger de SetTransparency().
        AC.setUnderVF(true);
        // On applique la transparence pour indiquer visuellement le blocage.
        SetTransparency(0.5f);

        ControleurJeu cj = Object.FindFirstObjectByType<ControleurJeu>();
        p = cj.currentPlayer;
        // On évite que cool() soit appelé dans le même tour que l'activation.
        p.skipOverTimeCooldown = true;
        // On ajoute cette compétence aux effets actifs du joueur pour qu'elle soit décrémentée chaque tour.
        p.ovSkills.Add(this);

        Debug.Log("BLOCK POSÉ sur " + AC.gameObject.name +
          " pos=" + AC.pos[0] + "|" + AC.pos[1] + "|" + AC.pos[2]);
    }

    /// <summary>
    /// Modifie l'opacité de la cellule bloquée.
    /// Entrée : alpha — valeur de transparence entre 0 (invisible) et 1 (opaque).
    /// Sortie : aucune.
    /// </summary>
    private void SetTransparency(float alpha)
    {
        Debug.Log("another");
        if (AC == null) return;
        Renderer r = AC.GetComponent<Renderer>();
        if (r == null) return;
        Color c = r.material.color;
        c.a = alpha;
        r.material.color = c;
    }

    /// <summary>
    /// Supprime l'effet de blocage une fois le cooldown d'effet écoulé.
    /// Réactive la cellule et retire la compétence de la liste des effets actifs.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    protected override void ResetEffect()
    {
        // On retire d'abord la compétence de la liste pour éviter des appels répétés
        // si FindActor échoue ou si une exception survient.
        p.ovSkills.Remove(this);
        if (AC == null) { Debug.Log("ResetEffect : AC NULL"); return; }

        Debug.Log("ResetEffect : avant - isBlockedBySkill=" + AC.isBlockedBySkill +
                  " collider=" + AC.GetComponent<Collider>().enabled);

        // On lève la protection visuelle pour que SetTransparency() puisse à nouveau agir sur la cellule.
        AC.setUnderVF(false);
        // On réactive la cellule en forçant Enabled à true même si elle avait été swappée entre-temps.
        AC.ForceEnable();
        // On réactive le sélecteur pour permettre à d'autres compétences de la cibler.
        SE?.Enable();
        // On remet la cellule à pleine opacité.
        SetTransparency(1.0f);

        Debug.Log("ResetEffect : après - isBlockedBySkill=" + AC.isBlockedBySkill +
                  " collider=" + AC.GetComponent<Collider>().enabled +
                  " renderer alpha=" + AC.GetComponent<Renderer>().material.color.a);
        Debug.Log("Reset");
        AC.debugPrintPos();
    }

    /// <summary>
    /// Met à jour la référence à la cellule bloquée après un swap de grilles.
    /// Appelée par GO_ClickableActor.swap() quand la cellule bloquée change de position physique.
    /// Entrée : newAC — nouvelle cellule physique, newSE — nouveau sélecteur associé.
    /// Sortie : aucune.
    /// </summary>
    public void change_property(GO_ClickableActor newAC, GO_selector newSE)
    {
        AC = newAC;
        SE = newSE;
    }

    /// <summary>
    /// Indique si cette compétence bloque actuellement une cellule donnée.
    /// Utilisée par GO_ClickableActor.OnPointerUp() pour intercepter les clics sur la cellule bloquée.
    /// Entrée : actor — la cellule à tester.
    /// Sortie : bool — true si cette compétence bloque précisément cette cellule.
    /// </summary>
    public bool IsBlocking(GO_ClickableActor actor)
    {
        return AC == actor;
    }
}