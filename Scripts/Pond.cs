using UnityEngine;

public class Pond : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag.Equals("Frog"))
        {
            collider.gameObject.GetComponent<Frog>().EnterWater();
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.tag.Equals("Frog"))
        {
            collider.gameObject.GetComponent<Frog>().ExitWater();
        }
    }
}
