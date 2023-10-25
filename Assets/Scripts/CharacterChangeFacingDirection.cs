using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class CharacterChangeFacingDirection : MonoBehaviour
{
    
    
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 LookAtDirection = Vector3.Scale(transform.parent.gameObject.GetComponent<Rigidbody>().velocity, new Vector3(1,0,1));

        if (LookAtDirection == Vector3.zero) return;

        transform.rotation = Quaternion.LookRotation(LookAtDirection);

       

    }
}
