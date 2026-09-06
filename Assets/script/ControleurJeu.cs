using UnityEngine;
using System.Linq;

// Alexis 02/04 10h20
// Pour pouvoir avoir attribu Image
using UnityEngine.UI;


// Ce composant est le chef d'orchestre de la partie. Il gère l'alternance des tours,
// l'initialisation des joueurs, la détection de victoire et de match nul,
// ainsi que les indicateurs visuels de tour (barres colorées).
// Il est unique dans la scène et référencé par la plupart des autres scripts.
public class ControleurJeu : MonoBehaviour
{
    [Header("Players")]
    // Données du joueur 1, assignées depuis l'inspecteur.
    public Player player1;
    // Données du joueur 2, assignées depuis l'inspecteur.
    public Player player2;

    // Indice du joueur courant (0 = joueur 1, 1 = joueur 2).
    // Utilisé pour identifier le propriétaire d'une cellule dans le Checker.
    // Internal se comporte comme public à la différence qu'un champ internal n'apparaît pas dans l'inspecteur Unity.
    internal int index = 0; //noam 25/03/26 

    // Référence au joueur dont c'est actuellement le tour.
    public Player currentPlayer; //noam //modif pour corriger bug victoire adverse

    // Alexis 02/04 10h20
    // Référence aux barres de tour, à assigner depuis l'inspecteur
    public Image barreGauche; // joueur 2 rouge
    public Image barreDroite; // joueur 1 bleu

    // Canvas affichant le message "aucune compétence sélectionnée" en mode sans compétences.
    public GameObject canvasAucuneCompetence;

    // Référence à la cellule centrale bloquée en mode sans compétences sur terrain impair.
    private GO_ClickableActor celluleCentrale;

    // Nombre de tours restants avant le déblocage de la cellule centrale.
    private int toursRestantsBloquage = 3;

    // Indique si la cellule centrale est actuellement bloquée
    private bool celluleBloquee = false;

    // Propriété publique en lecture seule pour accéder au joueur courant depuis d'autres scripts.
    public Player CurrentPlayer
    {
        get { return currentPlayer; }
    }

    /*
    /// <summary>
    /// Appelée automatiquement par Unity à chaque frame.
    /// Permet d'afficher l'écran de match nul en appuyant sur la touche N, uniquement à des fins de test.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            SetOpacite(barreGauche, 0f);
            SetOpacite(barreDroite, 0f);
            VictoireUI victoireUI = FindFirstObjectByType<VictoireUI>(FindObjectsInactive.Include);
            if (victoireUI != null)
                victoireUI.AfficherVictoire("Match Nul");
        }
    }
    */

    /// <summary>
    /// Initialise la partie au démarrage : applique la configuration, génère le terrain,
    /// initialise les joueurs et démarre le premier tour.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    void Start()
    {
        if (player1 == null || player2 == null)
        {
            Debug.LogError("Les deux joueurs doivent être configurés.");
            return;
        }

        // Alexis 11/04 13h
        // Si la partie a été configurée via le menu de personnalisation,
        // on applique les paramètres stockés dans GameConfig.
        if (GameConfig.modePersonnalise)
        {
            player1.playerName = GameConfig.nomJoueur1;
            player1.color = GameConfig.couleurJoueur1;
            player1.skillDatas = GameConfig.skillsJoueur1;

            player2.playerName = GameConfig.nomJoueur2;
            player2.color = GameConfig.couleurJoueur2;
            player2.skillDatas = GameConfig.skillsJoueur2;
        }

        // On génère le terrain avec la taille configurée.
        GO_FieldGenerator field = FindFirstObjectByType<GO_FieldGenerator>();
        if (field != null)
        {
           if (GameConfig.modePersonnalise)
              field.numberOfGrids = GameConfig.tailleTerrain;
                
           field.GenField();
        }

        // Alexis 13/04 21h30
        // On vérifie si aucune compétence n'a été sélectionnée par les deux joueurs.
        // All(s => s == null) retourne true si tous les slots de compétences sont vides.
        bool aucuneCompetence =
            (player1.skillDatas == null || player1.skillDatas.All(s => s == null)) &&
            (player2.skillDatas == null || player2.skillDatas.All(s => s == null));

        // En mode sans compétences sur terrain impair, on bloque la cellule centrale
        // pour éviter qu'un joueur ne la prenne dès le premier tour avec un avantage décisif.
        if (aucuneCompetence && field.numberOfGrids % 2 != 0)
        {
            // On affiche le message
            if (canvasAucuneCompetence != null)
                canvasAucuneCompetence.SetActive(true);

            // On trouve et bloque la cellule centrale
            // La cellule centrale = milieu du terrain
            // grids[taille/2] = grille du milieu en hauteur
            // actors[taille/2, taille/2] = cellule au centre de cette grille
            if (field != null && field.grids != null)
            {
                int milieu = field.numberOfGrids / 2;
                celluleCentrale = field.grids[milieu].actors[milieu, milieu];
                // On marque la cellule comme définitivement bloquée (ne peut pas être réactivée par Enable()).
                celluleCentrale.isDisabled = true;
                celluleCentrale.estBloquee = true;
                celluleBloquee = true;
            }
        }

        // On initialise les systèmes de vérification de victoire et de représentation du terrain.
        Checker chk = FindFirstObjectByType<Checker>();
        if (chk != null)
            chk.Init();

        Victory_Check vChk = FindFirstObjectByType<Victory_Check>();
        if (vChk != null)
            vChk.Init();

        player1.Init();
        player2.Init();
        currentPlayer = player1;

        // On trie les UIJoueur par leur ordre pour les assigner correctement aux joueurs.
        UIJoueur[] joueurs = Object.FindObjectsByType<UIJoueur>(FindObjectsSortMode.None);

        UIJoueur[] joueursTries = joueurs
            .OrderBy(j => j.order)
            .ToArray();
        joueursTries[0].player = player1;
        joueursTries[1].player = player2;
        foreach ( UIJoueur j in joueursTries)
        {
            j.Init();
        }
        StartTurn();
    }

    //noam 25/03/26
    /// <summary>
    /// Termine le tour courant : met à jour les cooldowns, vérifie la victoire ou le match nul,
    /// puis passe la main à l'autre joueur si la partie continue.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void EndTurn()
    {
        // Alexis 13/04 21h45
        // On décrémente le compteur si la cellule centrale est bloquée
        // et on la débloque après 3 tours
        if (celluleBloquee)
        {
            toursRestantsBloquage--;

            if (toursRestantsBloquage <= 0)
            {
                // On débloque la cellule centrale après le nombre de tours défini.
                celluleCentrale.isDisabled = false;

                celluleCentrale.estBloquee = false;

                celluleBloquee = false;
            }
        }

        // On décrémente le cooldown de recharge des compétences du joueur courant.
        currentPlayer.skills_cooldown();
        // On décrémente le cooldown des effets actifs des deux joueurs.
        // skipOverTimeCooldown évite de décrémenter dans le tour même où l'effet a été activé.
        if (!player1.skipOverTimeCooldown) player1.over_time_cooldown();
        if (!player2.skipOverTimeCooldown) player2.over_time_cooldown();
        player1.skipOverTimeCooldown = false;
        player2.skipOverTimeCooldown = false;

        // On met à jour la représentation interne du terrain pour la vérification de victoire.
        Checker chk = FindFirstObjectByType<Checker>();
        chk.print_grid();
        Victory_Check V_chk = FindFirstObjectByType<Victory_Check>();
        if (V_chk.CheckVictory(chk.grid, chk.last_pos))
        {   
            // Alexis 02/04 10h40
            // On cache les barres de tour à la victoire.
            SetOpacite(barreGauche, 0f);
            SetOpacite(barreDroite, 0f);

            // Alexis 28/03 11h50
            // On affiche l'écran de victoire avec le nom du gagnant.
            // Un peu coûteux en performance car Unity doit scanner toute la scène(FindAnyObjectByType).
            // Mais pose pas de problèmes car appelé rarement.
            // FindAnyObjectByType scan aussi les objets desactivés de l'inspecteur grâce à FindObjectsInactive.Include
            // Pour ça que nous n'utilisons pas FindFirstObjectByType qui ne dispose pas de du paramètre FindObjectsInactive.Include
            VictoireUI victoireUI = FindAnyObjectByType<VictoireUI>(FindObjectsInactive.Include);

            // On désactive le canvas à la victoire.
            if(canvasAucuneCompetence != null)
                canvasAucuneCompetence.SetActive(false);
            // On affiche l'écran de victoire.
            if (victoireUI != null)
                victoireUI.AfficherVictoire(currentPlayer.playerName);
            else
                Debug.Log("VictoireUI non trouvé !");
            Debug.Log("Victoire!");
            // On affiche l'état du terrain
            chk.print_grid();

        }
        // Alexis 02/04 11h00
        // Verifie le cas de match nul à la fin d'un tour
        else if (chk.IsGridFull())
        {
            // On cache les barres de tour quand la les grilles sont pleines.
            SetOpacite(barreGauche, 0f);
            SetOpacite(barreDroite, 0f);
            // FindAnyObjectByType avec FindObjectsInactive.Include permet de trouver
            // le canvas de victoire même s'il est désactivé dans la hiérarchie.
            VictoireUI victoireUI = FindAnyObjectByType<VictoireUI>(FindObjectsInactive.Include);
            if (victoireUI != null)
                victoireUI.AfficherVictoire("Match Nul"); // réutilise le même écran
            else
                Debug.Log("VictoireUI non trouvé !");
            Debug.Log("GridFull!");
        }
        else
        {
            Debug.Log("NormalMode");
            // Alternance stricte entre les deux joueurs
            currentPlayer = (currentPlayer == player1) ? player2 : player1;
            StartTurn();
            // On met à jour l'indice (0 ou 1) pour l'identification des cellules.
            index = (index + 1) % 2;
        }
    }

    

    /// <summary>
    /// Démarre le tour du joueur courant en mettant à jour les indicateurs visuels.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    private void StartTurn()
    {
        Debug.Log("Tour de : " + currentPlayer.playerName);

        // Donne les bonnes couleurs à l'indicateur de tour
        if (barreGauche != null) barreGauche.color = player2.color;
        if (barreDroite != null) barreDroite.color = player1.color;
        // Alexis 02/04 10h40
        // On met à jour l'opacité des barres selon le joueur actif
        bool joueur1Actif = currentPlayer == player1;
        SetOpacite(barreDroite, joueur1Actif ? 1f : 0.05f);
        SetOpacite(barreGauche, joueur1Actif ? 0.05f : 1f);
    }

    // Alexis 02/04 10h30 
    /// <summary>
    /// Modifie l'opacité (canal alpha) d'une Image UI.
    /// Entrée : image — l'image UI à modifier, opacite — valeur entre 0 (invisible) et 1 (opaque).
    /// Sortie : aucune.
    /// </summary>
    private void SetOpacite(Image image, float opacite)
    {
        if (image == null) return;
        Color c = image.color;
        c.a = opacite; // Le canal alpha (a) contrôle la transparence de l'image.
        image.color = c;
    }
}