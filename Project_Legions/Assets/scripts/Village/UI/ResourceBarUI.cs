using UnityEngine;
using UnityEngine.UI;

namespace PPCorps
{
    public class ResourceBarUI : MonoBehaviour
    {
        [SerializeField] private Text _goldText;
        [SerializeField] private Text _gemsText;
        [SerializeField] private Text _techText;

        public void Refresh(CurrencyData currency)
        {
            if (_goldText != null)
                _goldText.text = currency.gold.ToString();
            if (_gemsText != null)
                _gemsText.text = currency.gems.ToString();
            if (_techText != null)
                _techText.text = currency.techPoints.ToString();
        }
    }
}
