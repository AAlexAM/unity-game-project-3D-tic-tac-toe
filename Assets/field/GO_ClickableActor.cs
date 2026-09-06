
using UnityEngine;
using UnityEngine.EventSystems; // nécessaire pour IPointerDownHandler, IPointerUpHandler et PointerEventData

// Ce composant représente une cellule cliquable du plateau de jeu.
// On implémente IPointerDownHandler et IPointerUpHandler :
// ces interfaces sont reconnues par l'EventSystem de Unity,
// ce qui permet de recevoir les événements souris ET touch de façon unifiée,
// et permet de mesurer la distance entre le point de clique et de relâché
// à condition qu'un PhysicsRaycaster soit présent sur la caméra principale
// Une cellule peut être occupée par un joueur, bloquée temporairement par une compétence,
// désactivée lors d'un focus de grille, ou définitivement occupée après un coup.
public class GO_ClickableActor : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // Renderer de l'objet, utilisé pour modifier sa couleur au clic
    // Déclaré internal pour être accessible depuis d'autres scripts du même assembly.
    internal Renderer rd;

    // Indique si cet objet peut recevoir des clics
    // Désactivé par exemple quand la grille n'est pas en focus
    private bool Enabled = true;

    // Attribut pour savoir si la case est occupée ou non, non occupée par défaut :
    public bool isOccupied = false;

    // noam 25/03/26
    // Identifiant du joueur qui occupe cette cellule.
    internal int occupiedBy = 0;

    // Position de cette cellule dans le terrain 3D.
    // Sert d'identifiant stable qui ne change pas lors d'un swap de grilles.
    public int[] pos;

    // Position du doigt/clic au moment du OnPointerDown
    // Utilisée pour mesurer si la souris/doigt a bougé avant le relâché
    private Vector2 pointerDownPosition;

    // Distance en pixels en dessous de laquelle on considère que c'est un vrai clic
    // Mis à 20f (au lieu de 5f pour la souris) car les doigts sont moins précis
    private float dragThreshold = 20f;


    // Alexis 02/04 10h00
    // Indicateur global statique partagé par toutes les cellules.
    // Mis à true lorsque la partie est terminée pour bloquer tous les clics.
    // Statique pour éviter d'avoir à parcourir toutes les cellules une par une.
    public static bool theEnd = false;
    public bool UnderVF = false; // Noam si un effet change la visibilité on change pas via setTransparency

    // Alexis 13/04 22h30
    // Empêche Enable() de réactiver une cellule bloquée
    public bool isDisabled = false;

    // Indique si cette cellule est bloquée par la règle de la cellule centrale
    // (mode sans compétences sur terrain de taille impaire).
    public bool estBloquee = false;

    // Mémorise si une compétence était active au moment où le joueur a appuyé sur cette cellule.
    // Permet d'ignorer le OnPointerUp si la compétence a déjà été résolue entre-temps.
    private bool skillWasActiveOnDown = false;

    // Valeur interne du flag isBlockedBySkill.
    // Accédée uniquement via la propriété publique ci-dessous.
    private bool _isBlockedBySkill = false;
    // Indique si cette cellule est actuellement bloquée par la compétence BlockSkill.
    // La propriété permet de tracer les changements de valeur via les logs de debug.
    public bool isBlockedBySkill
    {
        get { return _isBlockedBySkill; }
        set
        {
            Debug.Log(gameObject.name + " isBlockedBySkill : " + _isBlockedBySkill + " → " + value + "\n" + System.Environment.StackTrace);
            _isBlockedBySkill = value;
        }
    }


    //noam
    // Couleur d'origine de la cellule, mémorisée au démarrage.
    // Utilisée pour restaurer la couleur après une modification temporaire.
    internal Color base_color;

    /// <summary>
    /// Appelée par Unity au démarrage de l'objet.
    /// Récupère le Renderer et mémorise la couleur d'origine.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    void Start()
    {
        // On récupère le Renderer une seule fois au démarrage pour éviter
        // des appels répétés à GetComponent qui sont coûteux en performance
        rd = GetComponent<Renderer>();
        base_color = rd.material.color;
    }

    /// <summary>
    /// Remonte la hiérarchie de GameObjects pour appeler f_reset() sur la grille parente,
    /// ce qui déclenche la réinitialisation du focus du terrain.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    void f_reset()
    {
        // On cherche le composant GO_GridGenerator dans les parents de cet objet.
        GO_GridGenerator p = GetComponentInParent<GO_GridGenerator>();
        if (p != null)
            p.f_reset();
        else
            Debug.Log("GO_CA no parent");
    }

    /// <summary>
    /// Appelée par l'EventSystem Unity quand le doigt ou la souris touche cet objet.
    /// Mémorise la position de départ et l'état des compétences actives.
    /// Entrée : eventData — données de l'événement (position, identifiant du doigt, etc.).
    /// Sortie : aucune.
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        // eventData.position donne la position en pixels sur l'écran,
        // que ce soit un doigt ou une souris.
        pointerDownPosition = eventData.position;
        // On mémorise si une skill était active au moment du press
        ControleurJeu cj = FindFirstObjectByType<ControleurJeu>();
        skillWasActiveOnDown = (cj.CurrentPlayer.usingSkill != null);
    }

    /// <summary>
    /// Appelée par l'EventSystem Unity quand le doigt ou la souris est relâché sur cet objet.
    /// Vérifie toutes les conditions avant de placer un pion, et déclenche la fin de tour.
    /// Entrée : eventData — données de l'événement (position au relâché, etc.).
    /// Sortie : aucune. Peut modifier l'état de la cellule et appeler EndTurn().
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("OnPointerUp sur " + gameObject.name +
             " isBlockedBySkill=" + isBlockedBySkill +
             " Enabled=" + Enabled +
             " collider=" + GetComponent<Collider>().enabled +
             " skillWasActiveOnDown=" + skillWasActiveOnDown);
        // Si une skill était active au moment du clic, on ignore complètement
        if (skillWasActiveOnDown) return;
        
        
        // Noam
        ControleurJeu cj = FindFirstObjectByType<ControleurJeu>();
        // On entre dans le bloc de placement normal uniquement si aucune compétence n'est en cours.
        if (cj.CurrentPlayer.usingSkill == null) 
        {
            
            // On bloque les clics pendant la pause
            if (Time.timeScale == 0f) return;

            // Alexis 02/04 10h00
            // On bloque les clics si la partie est terminée
            // (on met theEnd true dans VictoireUI.AfficherVictoire())
            if (theEnd) return;
            // Vérifie si ce GameObject est actuellement bloqué par une BlockSkill
            // en parcourant les effets actifs des deux joueurs.
            ControleurJeu cjCheck = FindFirstObjectByType<ControleurJeu>();
            bool blockedByAnySkill = false;
            foreach (C_OverTime ov in cjCheck.player1.ovSkills)
            {
                BlockSkill bs = ov as BlockSkill;
                // Si cette compétence bloque précisément cette cellule, on le note.
                if (bs != null && bs.IsBlocking(this)) { blockedByAnySkill = true; break; }
            }
            if (!blockedByAnySkill)
            {
                foreach (C_OverTime ov in cjCheck.player2.ovSkills)
                {
                    BlockSkill bs = ov as BlockSkill;
                    if (bs != null && bs.IsBlocking(this)) { blockedByAnySkill = true; break; }
                }
            }
            // Si la cellule est bloquée par une compétence, on absorbe le clic sans rien faire.
            if (blockedByAnySkill)
            {
                Debug.Log("return blocked any");
                return;
            }
            // Si l'objet est désactivé on ne fait rien
            if (!Enabled) return;
            
            // Si la case est déjà prise on bloque le clic:
            if (isOccupied == true)
            {
                isOccupied = ((occupiedBy != 0) ? true : false ) ;
                return ;
            }

            // Cellule bloquée par le mode sans compétences
            if (estBloquee) return;

            // On vérifie si un long press vient de se terminer sur ce même objet
            // Si oui, on ignore le OnPointerUp pour éviter de déclencher un clic parasite
            // juste après la validation du long press
            ChildLongPress lp = GetComponent<ChildLongPress>();
            if (lp != null && lp.WasLongPress) return;

            // On mesure la distance en pixels entre la position au OnPointerDown et au OnPointerUp
            // eventData.position fonctionne aussi bien avec un doigt qu'avec la souris
            float dragDistance = Vector2.Distance(eventData.position, pointerDownPosition);
            
            

            if (dragDistance < dragThreshold)
            {
                Debug.Log("PLACEMENT D'UN PION sur " + gameObject.name);
                // On crée un nouveau matériau pour éviter de modifier le matériau partagé
                // entre toutes les cellules instanciées depuis le même prefab.
                rd.material = new Material(rd.material);
                // On colorie la cellule avec la couleur du joueur courant.
                rd.material.color = cj.CurrentPlayer.color;

                // On marque la cellule comme définitivement occupée.
                isOccupied = true;

                //noam 25/03/26
                occupiedBy = cj.index + 1;
                Checker chk = FindFirstObjectByType<Checker>();
                chk.make_grid(pos);
                //------------
                // On désactive la cellule pour qu'elle ne soit plus cliquable.
                Disable();
                // On réinitialise le focus du terrain.
                f_reset();
                // On signale la fin du tour au gestionnaire de jeu.
                cj.EndTurn();
            }
        }
        else
            Debug.Log("UsingSkill");
    }

    /// <summary>
    /// Désactive les clics sur cette cellule en désactivant son Collider.
    /// Un Collider désactivé n'est plus détecté par le PhysicsRaycaster de la caméra.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void Disable()
    {
        if (Enabled)
        {
            Debug.Log("Disable sur " + gameObject.name + "\n" + System.Environment.StackTrace);
            GetComponent<Collider>().enabled = false;
            Enabled = false;
        }
    }

    /// <summary>
    /// Réactive les clics sur cette cellule en réactivant son Collider.
    /// N'a aucun effet si la cellule est marquée isDisabled ou isBlockedBySkill.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void Enable()
    {
        // On ne réactive pas une cellule désactivée (cellule centrale bloquée).
        if (isDisabled) return; // Alexis 13/04 22h30
        // On ne réactive pas une cellule bloquée par une compétence BlockSkill.
        if (isBlockedBySkill) return;
        if (!Enabled)
        {
            GetComponent<Collider>().enabled = true;
            Enabled = true;
            Debug.Log("Enabled");
        }
    }

    /// <summary>
    /// Réactive les clics sur cette cellule en forçant Enabled à true,
    /// même si Enabled était déjà true (cas possible après un swap de grilles).
    /// N'a aucun effet si la cellule est marquée isDisabled ou isBlockedBySkill.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void ForceEnable()
    {
        // On ne réactive pas une cellule désactivée (cellule centrale bloquée).
        if (isDisabled) return;
        // On ne réactive pas une cellule encore bloquée par une compétence.
        if (isBlockedBySkill) return;
        Debug.Log("ForceEnable sur " + gameObject.name + " Enabled avant=" + Enabled);
        // On force la réactivation même si Enabled est déjà true,
        // car le swap a pu désynchroniser l'état logique et l'état du Collider.
        GetComponent<Collider>().enabled = true;
        Enabled = true;
        Debug.Log("ForceEnable sur " + gameObject.name + " Enabled après=" + Enabled + " collider=" + GetComponent<Collider>().enabled);
    }

    /* Renderer rd;

    // Indique si cet objet peut recevoir des clics
    // Désactivé par exemple quand la grille n'est pas en focus
    private bool Enabled = true;

    //Attribut pour savoir si la case est occupée ou non, non occupée par défaut :
    public bool isOccupied = false;

    // noam 25/03/26
    //sert a savoir a qui appartient la cellule
    internal int occupiedBy = 0;
    public int[] pos;*/


    //Noam
    /// <summary>
    /// Échange les données logiques de jeu entre cette cellule et une autre cellule.
    /// Utilisée lors d'un swap de grilles par la compétence SwapSkill.
    /// Les positions (pos[]) ne sont jamais échangées car elles identifient le GameObject physique.
    /// Si une compétence BlockSkill cible l'une de ces cellules, sa référence est mise à jour.
    /// Entrée : other — l'autre cellule avec laquelle échanger les données.
    /// Sortie : aucune.
    /// </summary>
    public void swap(GO_ClickableActor other)
    {
        Debug.Log("start self : " + pos[0] + "|" + pos[1] + "|" + pos[2] + " / other" + other.pos[0] + "|" + other.pos[1] + "|" + other.pos[2]);

        // On swap uniquement les données logiques de jeu
        bool tempIsOccupied = isOccupied;
        int tempOccupiedBy = occupiedBy;

        isOccupied = other.isOccupied;
        occupiedBy = other.occupiedBy;

        bool tempUnderVF = UnderVF;
        UnderVF = other.UnderVF;
        other.UnderVF = tempUnderVF;

        other.isOccupied = tempIsOccupied;
        other.occupiedBy = tempOccupiedBy;

        // On swap les couleurs SAUF l'alpha des cellules bloquées
        Color tempColor = rd.material.color;
        Color otherColor = other.rd.material.color;

        // On préserve l'alpha de chaque cellule bloquée
        float selfAlpha = isBlockedBySkill ? tempColor.a : otherColor.a;
        float otherAlpha = other.isBlockedBySkill ? otherColor.a : tempColor.a;

        rd.material.color = new Color(otherColor.r, otherColor.g, otherColor.b, selfAlpha);
        other.rd.material.color = new Color(tempColor.r, tempColor.g, tempColor.b, otherAlpha);

        bool TempIsDisabled = isDisabled;
        isDisabled = other.isDisabled;
        other.isDisabled = TempIsDisabled;
        // On synchronise l'état actif/inactif du Collider et du sélecteur de chaque cellule
        // en fonction de leur nouveau flag isDisabled après l'échange.
        if (isDisabled)
        {
            Disable();
            GetComponent<GO_selector>().Disable();
        }
        else
        {
            Enable();
            GetComponent<GO_selector>().Enable();
        }
        if (other.isDisabled)
        {
            other.Disable();
            other.GetComponent<GO_selector>().Disable();
        }
        else
        {
            other.Enable();
            other.GetComponent<GO_selector>().Enable();
        }
        // Si une compétence BlockSkill cible cette cellule (this), on met à jour sa référence
        // pour qu'elle pointe désormais vers le nouveau GameObject physique après le swap.
        // Cela garantit que le blocage suit visuellement la cellule échangée.
        ControleurJeu cj = Object.FindFirstObjectByType<ControleurJeu>();
        foreach (C_OverTime sk in cj.player1.ovSkills)
        {
            BlockSkill bsk = sk as BlockSkill;
            if (bsk != null && bsk.AC == this)
            {
                Debug.Log("Change");
                bsk.change_property(other, other.GetComponent<GO_selector>()); //null temp
            }
        }
        foreach (C_OverTime sk in cj.player2.ovSkills)
        {
            BlockSkill bsk = sk as BlockSkill;
            if (bsk != null && bsk.AC == this)
            {
                Debug.Log("Change");
                bsk.change_property(other, other.GetComponent<GO_selector>()); //null temp
            }
        }

        // On ne touche PAS à Enabled, isDisabled, UnderVF, isBlockedBySkill //noam -> non a l'origne enabled isdisabled et underVF son vraiment lier a la case ( donc si ont considere que la case bouge... vs captez
        // Ces états restent attachés au GameObject physique

        Debug.Log("end self : " + pos[0] + "|" + pos[1] + "|" + pos[2] + " / other" + other.pos[0] + "|" + other.pos[1] + "|" + other.pos[2]);
    }

    /// <summary>
    /// Modifie le flag UnderVF de cette cellule.
    /// Quand UnderVF est true, la cellule est exclue des modifications de SetTransparency().
    /// Entrée : state — true pour protéger la cellule, false pour la réintégrer.
    /// Sortie : aucune.
    /// </summary>
    public void setUnderVF( bool state)
    {
        UnderVF = state;
    }

    /// <summary>
    /// Affiche la position logique de cette cellule dans la console Unity.
    /// Utilisée pour le débogage.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void debugPrintPos()
    {
        Debug.Log("pos : " + pos[0] + " | " + pos[1] + " | " + pos[2]);
    }
}