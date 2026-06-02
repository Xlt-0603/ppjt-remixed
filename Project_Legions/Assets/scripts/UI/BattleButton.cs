using UnityEngine;
using UnityEngine.SceneManagement;

namespace PPCorps
{
    public class BattleButton : MonoBehaviour
    {
        private bool _isLoading;

        private void OnMouseDown()
        {
            if (_isLoading) return;
            _isLoading = true;
            Invoke(nameof(LoadBattle), 0.15f);
        }

        private void LoadBattle()
        {
            SceneManager.LoadScene("battlescene");
        }
    }
}
