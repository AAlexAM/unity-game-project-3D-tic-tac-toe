/*using UnityEngine;
using UnityEngine.EventSystems;
using System;

public abstract class C_Skill
{
    public Action onUpdate;
    protected SkillData data;

    public int stack = 0;
    public int cooldown;

    public SkillButton button;

    public C_Skill(SkillData data)
    {
        this.data = data;
        cooldown = data.cooldownMax;
    }

    public void Attente()
    {
        cooldown--;
        if (cooldown < 0)
        {
            cooldown = data.cooldownMax;
            stack++;
            Debug.Log("En cooldown");
        }

        onUpdate?.Invoke();
        button.UpdateTexte();
    }

    public bool Utiliser()
    {
        if (stack == 0)
            return false;

        stack--;
        //demander cible
        ActiverEffet();

        onUpdate?.Invoke();

        return true;
    }

    public Texture2D GetIcone()
    {
        return data.icone;
    }

    public abstract void ActiverEffet();
}*/

using UnityEngine;
using System;


// Classe de base abstraite pour toutes les compétences du jeu.
// Une compétence dispose d'un système de charges (stack) et d'un cooldown.
// Elle suit le patron de conception Template Method : les sous-classes définissent
// ActiverEffet() et optionnellement DemanderCible() pour personnaliser le comportement.
public abstract class C_Skill
{
    // Événement déclenché à chaque mise à jour de la compétence (cooldown, stack).
    // Permet à SkillButton de se mettre à jour automatiquement sans couplage fort.
    public Action onUpdate;

    // Données de configuration de cette compétence (icône, cooldown, stackMax).
    protected SkillData data;

    // Alexis 10/04 11h15
    // Permet à SkillButton d'accéder au cooldownMax sans exposer tout l'objet data.
    public int CooldownMax => data.cooldownMax;

    // Nombre de charges disponibles
    public int stack = 0;

    // Nombre de tours restants avant qu'une nouvelle charge soit accordée.
    public int cooldown;

    // Référence au bouton UI associé à cette compétence pour mettre à jour l'affichage.
    public SkillButton button;

    /// <summary>
    /// Constructeur. Initialise le cooldown à sa valeur maximale.
    /// Entrée : data — les données de configuration de la compétence.
    /// </summary>
    public C_Skill(SkillData data)
    {
        this.data = data;
        // On commence avec le cooldown plein, la compétence n'est pas encore disponible.
        cooldown = data.cooldownMax;
    }

    /// <summary>
    /// Décrémente le cooldown d'un tour. Si le cooldown atteint zéro,
    /// accorde une charge supplémentaire (dans la limite de stackMax).
    /// Appelée à chaque fin de tour pour le joueur courant.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary
    public void Attente()
    {
        cooldown--;

        if (cooldown < 0)
        {
            cooldown = data.cooldownMax;

            // Alexis 07/04 14h30
            // On ne dépasse pas le stackMax
            if (stack < data.stackMax)
                stack++;
            Debug.Log("Skill chargée +1");
        }
        // On notifie les abonnés (notamment SkillButton) que l'état a changé.
        onUpdate?.Invoke();
        button.UpdateTexte();
    }

    /// <summary>
    /// Tente d'utiliser la compétence. Si une charge est disponible, la consomme
    /// et lance la demande de cible ou l'effet direct.
    /// Entrée : aucune.
    /// Sortie : bool — true si la compétence a été utilisée, false si aucune charge disponible.
    /// </summary>
    public bool Utiliser()
    {
        if (stack == 0)
        {
            Debug.Log("Pas de stack !");
            return false;
        }

        stack--;

        // On entre en mode sélection au lieu d'exécuter direct
        DemanderCible();

        onUpdate?.Invoke();

        return true;
    }

    /// <summary>
    /// Demande au joueur de choisir une cible pour la compétence.
    /// Par défaut, exécute directement l'effet sans demander de cible.
    /// Les sous-classes peuvent surcharger cette méthode pour attendre un clic.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public virtual void DemanderCible()
    {
        // Par défaut : pas de cible → exécution directe
        ActiverEffet();
    }

    /// <summary>
    /// Exécute l'effet réel de la compétence. Doit être implémenté par chaque sous-classe.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public abstract void ActiverEffet();

    /// <summary>
    /// Retourne l'icône de la compétence pour l'affichage sur le bouton UI.
    /// Entrée : aucune.
    /// Sortie : Texture2D — la texture de l'icône.
    /// </summary>
    public Texture2D GetIcone()
    {
        return data.icone;
    }

    /// <summary>
    /// Termine l'utilisation de la compétence : réinitialise l'icône et libère le slot de compétence active.
    /// Doit être appelée après qu'une compétence a produit son effet.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void Fin()
    {
        // Alexis 10/04 13h
        // AJOUT : on dégrisse l'icône après utilisation
        button.icon.color = Color.white;
        button.owner.usingSkill = null;
    }
}

// en theorie si besoin de pls inputs
/*
int step = 0;
GameObject cible1, cible2, cible3;

public override void DemanderCible()
{
    step = 0;
    NextStep();
}

void NextStep()
{
    InputManager.Instance.StartSelection(OnSelect);
}

void OnSelect(GameObject obj)
{
    if (step == 0) cible1 = obj;
    if (step == 1) cible2 = obj;
    if (step == 2) cible3 = obj;

    step++;

    if (step < 3)
        NextStep();
    else
        ActiverEffet();
}*/