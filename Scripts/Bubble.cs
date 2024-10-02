#define SOLUTION

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubble : MonoBehaviour
{

#if SOLUTION
    private const float LIFETIME = 2.0f;

    private float _timeElapsed;

    void Start()
    {
        _timeElapsed = 0.0f;
    }

    void FixedUpdate()
    {
        _timeElapsed += Time.fixedDeltaTime;

        if (_timeElapsed >= LIFETIME)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag.Equals("Obstacle"))
        {
            Destroy(gameObject);
        }
        else if (collider.gameObject.tag.Equals("Snake"))
        {
            Destroy(gameObject);
            collider.gameObject.GetComponent<Snake>().HandleEvent(Snake.SnakeEvent.HitByBubble);
        }
    }
#endif

}
