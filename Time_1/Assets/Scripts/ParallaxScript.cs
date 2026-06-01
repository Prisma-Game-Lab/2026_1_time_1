using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxScript : MonoBehaviour
{
    [SerializeField] private GameObject cameraObject;
    [SerializeField] private ParallaxLayer[] parallaxEffect;

    private void Start()
    {
        foreach (var parallaxEffect in parallaxEffect)
        {
            parallaxEffect.StoreStartPos();
        }
    }

    private void FixedUpdate()
    {
        foreach (var parallaxEffect in parallaxEffect) 
        {
            parallaxEffect.UpdatePostion(cameraObject);
        }
    }
}

[System.Serializable]
class ParallaxLayer
{
    [SerializeField] private GameObject layerObject;
    [SerializeField] private float parallaxEffect;

    private float startPos;

    public void StoreStartPos() 
    {
        startPos = layerObject.transform.position.x;
    }

    public void UpdatePostion(GameObject cameraObject) 
    {
        float distance = cameraObject.transform.position.x * parallaxEffect;

        Vector3 pos = new Vector3(startPos + distance,
                                  layerObject.transform.position.y,
                                  layerObject.transform.position.z);
        layerObject.transform.position = pos;
    }
}
