using UnityEngine;

public class SpinnerRotate : MonoBehaviour
{
    public float speed = 180f; // Ã¿ÃëÐý×ª½Ç¶È

    void Update()
    {
        transform.Rotate(Vector3.forward, speed * Time.deltaTime);
    }
}
