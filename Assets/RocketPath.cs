using UnityEngine;

public class RocketPath : MonoBehaviour
{
    public Transform rocket;
    public Vector2 startPos = new Vector2(0f, 0f);
    public Vector2 endPos = new Vector2(10f, 2f);
    public float duration = 5f;

    private float elapsed;

    void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);

        // Ease-in cubic
        float ease = t * t * t;

        // Interpolate position
        float x = Mathf.Lerp(startPos.x, endPos.x, ease);
        float y = Mathf.Lerp(startPos.y, endPos.y, ease);

        rocket.position = new Vector3(x, y, rocket.position.z);
    }
}