using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardBase : MonoBehaviour
{
    public int HandCardIndex;

    public bool CardInDrag;

    RaycastHit hit;

    private void Update()
    {
        
        if(CardInDrag)
        {

            if (Physics.Raycast(transform.position, Vector3.forward, out hit, 100.0f))
            {
                Debug.Log(hit.collider.gameObject.name);
            }
        }
    }

    public GameObject MouseOff()
    {
        if (Physics.Raycast(transform.position, Vector3.forward, out hit, 100.0f))
        {
            return hit.collider.gameObject;
        }

        return null;
    }
}
