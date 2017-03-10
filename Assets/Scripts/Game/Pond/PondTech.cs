using UnityEngine;

public class PondTech : MonoBehaviour 
{
	[SerializeField] private AnimationCurve _pondWalkCurve;

    const float APPROX_POND_RADIUS = 20f;
    public const float POND_MIN_Y = -3f;

    public float GetPondY(Vector3 pos)
    {
        pos.y = 0f;
        float dist = (pos - transform.position).magnitude;

        return _pondWalkCurve.Evaluate(dist / APPROX_POND_RADIUS) * POND_MIN_Y;
    }
}
