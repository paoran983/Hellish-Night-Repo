using UnityEngine;

namespace Inventory.Model {
    [CreateAssetMenu]
    public class ItemParameterSO : ScriptableObject {
        [field: SerializeField] private string parameterName { get; set; }

        public string ParameterName {
            get { return parameterName; }
            set { parameterName = value; }
        }
    }
}
