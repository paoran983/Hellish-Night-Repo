using UnityEngine;

[CreateAssetMenu]
public class MoveParameterSO : ScriptableObject {
    [field: SerializeField] string parameterName { get; set; }
}
