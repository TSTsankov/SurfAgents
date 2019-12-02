using System.Collections.Generic;
using UnityEngine;
public static class UTIL {
    public static float OnePlusLogValue (float value) {
        return 1 + Mathf.Log ((value < 1) ? 1 : value);
    }
}
public class Bubbles : MonoBehaviour {
    public float rewardValue = 0.1f;
    private bool active = true;
    private int bbL = 0;
    Dictionary<Collider, AgentLRT> characters = null;
    internal class AgentLRT {
        public Fragsurf.Movement.SurfCharacter surfCharacter;
        public float lastRewardTime = 0;
        public AgentLRT (Fragsurf.Movement.SurfCharacter surfCharacter) {
            this.surfCharacter = surfCharacter;
        }
    }
    void Start () {

        bbL = Resources.FindObjectsOfTypeAll<Bubbles> ().Length;
        rewardValue = 3f / ((float) bbL);

        if (characters == null) {
            characters = new Dictionary<Collider, AgentLRT> ();
            /* Fragsurf.Movement.SurfCharacter[] c = FindObjectsOfType<Fragsurf.Movement.SurfCharacter> ();
            for (int i = 0; i < c.Length; i++)
                characters.Add (c[i].GetComponent<Collider> (), new AgentLRT (c[i])); */
        }
    }
    //public float timeOut = 45;
    //private Dictionary<Fragsurf.Movement.SurfCharacter, float> characters = new Dictionary<Fragsurf.Movement.SurfCharacter, float>();

    /*  IEnumerator WaitAndPrint () {
         // suspend execution for 5 seconds
         active = false;
         yield return new WaitForSeconds (2);
         active = true;
     } */

    void OnTriggerEnter (Collider other) {
        //if(active == false)
        //return;
        if (other.tag != "Player")
            return;

        AgentLRT alrt;
        if (characters.TryGetValue (other, out alrt)) {
            //float lastRewardTime;
            //if (characters.TryGetValue(CC, out lastRewardTime) == false)
            //characters.Add(CC, -5);

            if (alrt.lastRewardTime > alrt.surfCharacter.lastFallTime)
                return;

            //if (Time.time > lastRewardTime + timeOut)
            //if (alrt.surfCharacter.multiplier < 15)
                alrt.surfCharacter.multiplier *=1.25f;
            //{

            /* Vector3 velocity = alrt.surfCharacter.MoveData.Velocity;
              alrt.surfCharacter.Reward (
                 rewardValue * alrt.surfCharacter.multiplier +
                 rewardValue *
                 UTIL.OnePlusLogValue (Mathf.Abs (velocity.y)) *
                 UTIL.OnePlusLogValue (Mathf.Abs (velocity.x) + Mathf.Abs (velocity.z))
             ); */

            //StartCoroutine(WaitAndPrint());
            //characters[CC] = Time.time;
            //}
            alrt.lastRewardTime = Time.time;
        } else {
            characters.Add (other, new AgentLRT (other.GetComponent<Fragsurf.Movement.SurfCharacter> ()));
            OnTriggerEnter(other);
        }

    }
}