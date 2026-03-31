using UnityEngine;

public class PlayerTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Circle"))
        {
            Debug.Log("Player chạm circle");
        }
    }
}