using UnityEngine;

public class ClimbSpot : MonoBehaviour
{
    public Transform[] ClimbSpotsLocal;

    private Vector3[] ClimbSpotsGlobal;

    private void Start()
    {
        for (int i = 0; i < ClimbSpotsLocal.Length; i++)
        {
            ClimbSpotsGlobal[i] = transform.TransformVector(ClimbSpotsLocal[i].position);
        }
    }

    public Vector3 GetValueStepForClimb(int step)
    {
        return ClimbSpotsGlobal[step];
    }
}
