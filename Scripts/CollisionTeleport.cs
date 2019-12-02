using UnityEngine;

public class CollisionTeleport : MonoBehaviour {
    public Transform to;
    public bool goesToCage = false;
    public bool reward = true;
    //Rand random = new Rand ();
    void OnTriggerEnter (Collider other) {
        Fragsurf.Movement.SurfCharacter CC = other.GetComponent<Fragsurf.Movement.SurfCharacter> ();
        if (goesToCage) {
            CC.FallInCage ();

            if (to.childCount > 1)
                CC.SpawnAt (to.GetChild (Random.Range (0, to.childCount)).position);
            else
                CC.SpawnAt (to.position);
        } else {
            CC.SpawnAt (to.position);
            if (reward)
                CC.Reward (5f);//10 + CC.multiplier * 3);
        }
    }
}