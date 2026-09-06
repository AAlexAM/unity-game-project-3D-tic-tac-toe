using UnityEngine;
using UnityEngine.SceneManagement;

// Composant simple d'initialisation de partie depuis le menu principal.
public class Jeu : MonoBehaviour
{
    /// <summary>
    /// Charge la scène de jeu pour démarrer une nouvelle partie.
    /// Entrée : aucune. Sortie : aucune.
    /// </summary>
    public void InitialiserPartie()
    {
        SceneManager.LoadScene("Game");
    }
}