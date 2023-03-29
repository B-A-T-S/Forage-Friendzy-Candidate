using UnityEngine;
using UnityEngine.AI;

public class RingbufferFootSteps1 : MonoBehaviour
{
    public ParticleSystem system;

    Vector3 lastEmit;

    public float delta = 1;
    public float gap = 0.5f;
    int dir = 1;
    static RingbufferFootSteps1 selectedSystem;

    void Start()
    {
        lastEmit = transform.position;

    }

    public void Update()
    {
        
        

        if (Vector3.Distance(lastEmit, transform.position) > delta)
        {
            
            var pos = transform.position + (transform.right * gap * dir);
            dir *= -1;
            ParticleSystem.EmitParams part = new ParticleSystem.EmitParams();
            part.position = pos;
            part.rotation = transform.rotation.eulerAngles.y;
            system.Emit(part, 1);
            lastEmit = transform.position;
        }

    }


}
