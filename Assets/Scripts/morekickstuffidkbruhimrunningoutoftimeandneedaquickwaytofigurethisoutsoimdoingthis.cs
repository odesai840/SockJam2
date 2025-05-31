using UnityEngine;

public class morekickstuffidkbruhimrunningoutoftimeandneedaquickwaytofigurethisoutsoimdoingthis : MonoBehaviour
{
    [SerializeField] private GameObject kickCollider;

    public void DisableKickCollider()
    {
        kickCollider.GetComponent<CircleCollider2D>().enabled = false;
    }
}
