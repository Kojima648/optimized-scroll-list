using UnityEngine;

public class SpinnerRotate : MonoBehaviour
{
    public float speed = 180f; // ÿ����ת�Ƕ�

    void Update()
    {
        transform.Rotate(Vector3.forward, speed * Time.deltaTime);
    }
}
