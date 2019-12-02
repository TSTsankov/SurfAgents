using System.Collections;
using UnityEngine;

public class CollisionBoost : MonoBehaviour {
    [Range (0, 100)]
    public float boost = 20;
    //public float reawrd = 1;
    private Fragsurf.Movement.SurfCharacter surfCharacter;

    void OnTriggerEnter (Collider other) {
        Fragsurf.Movement.SurfCharacter CC = other.GetComponent<Fragsurf.Movement.SurfCharacter> ();
        if (surfCharacter != CC)
            surfCharacter = CC;

        Boost ();
    }
    void OnTriggerStay (Collider other) {
        Boost ();
    }

    void Boost () {
        if (surfCharacter != null) {
            surfCharacter.Boost (transform.forward * boost);
            //surfCharacter.Reward (reawrd);
        }
    }
}