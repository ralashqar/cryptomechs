using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleCollisionHandler : MonoBehaviour
{

    public ParticleSystem part;
    public List<ParticleCollisionEvent> collisionEvents;

    public delegate void OnPasrticleCollisionEvent(GameObject obj);
    public OnPasrticleCollisionEvent OnParticleCollisionTriggered;

    void Start()
    {
        part = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }

    public void OnParticleCollision(GameObject other)
    {
        OnParticleCollisionTriggered?.Invoke(other);
        //SendMessageUpwards("OnParticleCollisionTriggered", other, SendMessageOptions.DontRequireReceiver);
        int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);

        Rigidbody rb = other.GetComponent<Rigidbody>();
        int i = 0;

        while (i < numCollisionEvents)
        {
            if (rb)
            {
                Vector3 pos = collisionEvents[i].intersection;
                Vector3 force = collisionEvents[i].velocity * 10;
                rb.AddForce(force);
            }
            i++;
        }
    }
}
