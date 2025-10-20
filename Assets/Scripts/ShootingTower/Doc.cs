using UnityEngine;

public class Doc : MonoBehaviour
{
    public Transform docTransform;
    public bool isReserved;
    
    public void ReleaseDoc()
    {
        isReserved = false;
    }
}
