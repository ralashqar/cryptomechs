using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowCharacter : MonoBehaviour
{
    public Transform target;

    Vector3 targetLastPos;
    Vector3 targetCamPos;

    // Start is called before the first frame update
    void Start()
    {
        targetLastPos = target.transform.position;
        targetCamPos = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null) return;

        Vector3 delta = target.transform.position - targetLastPos;

        targetCamPos += delta;
        targetLastPos = target.transform.position;

        this.transform.position = Vector3.Slerp(this.transform.position, targetCamPos, Time.deltaTime * 5f);
    }
}
