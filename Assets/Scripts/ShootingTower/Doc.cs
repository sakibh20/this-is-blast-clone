using UnityEngine;

public class Doc : MonoBehaviour
{
    [SerializeField] private Transform docTransform;
    private bool isReserved;
    public bool IsReserved => isReserved;
    public Transform DocTransform => docTransform;

    private Player _currentPlayer; 
    public Player CurrentPlayer => _currentPlayer; 
    
    public void ReserveDoc(Player currentPlayer)
    {
        _currentPlayer = currentPlayer;
        isReserved = true;
    }
    
    public void ReleaseDoc()
    {
        _currentPlayer = null;
        isReserved = false;
    }
}
