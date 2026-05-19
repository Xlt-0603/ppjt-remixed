using UnityEngine;

namespace PPCorps
{
    public class DeploySystem : MonoBehaviour
    {
        public static DeploySystem Instance { get; private set; }

        [SerializeField] private Transform _unitContainer;

        private void Awake() => Instance = this;

        public void PlaceUnit(UnitData data, Vector3 position)
        {
            if (data == null || data.prefab == null) return;

            GameObject unitObj = Instantiate(data.prefab, position, Quaternion.identity);
            if (_unitContainer != null)
                unitObj.transform.SetParent(_unitContainer);
        }
    }
}
