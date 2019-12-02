using MLAgents;
using UnityEngine;

public class SurfAgent : Agent {
	private SurfAcademy myAcademy;
	private Fragsurf.Movement.SurfCharacter surfComp;
	private RayPerceptionSurf rayPer;

	float[] frontA = { 55, 60, 65, 70, 75, 105, 110, 115, 120, 125 }; //x10
	float[] frontB = { 35, 40, 65, 90, 105, 130, 145 }; //x7
	float[] down = {-135, -90, -45, -0, 45, 90, 135, 180 }; //x8
	//float[] full = new float[] { 180, 165, 150, 135, 120, 105, 90, 75, 60, 45, 30, 15, 0, -15, -30, -45, -60, -75, -90, -105, -120, -135, -150, -165 }; //24
	float myActionReward = 0, v = 0, fDT = 0.0002f, lookAngle = 0, lookPenalty = 0, xAxis = 0, yAxis = 0;

	bool surfingBool = false, moveFwd = false, moveBack = false, moveLeft = false, moveRight = false, jump = false;
	int FwdBack = 0, LeftRight = 0, Jump = 0, rotateX = 0, rotateY = 0, cageFall = 0;

	const float rayDistance = 70;
	[System.NonSerialized] public Vector3[] portPositions, boostPositions;
	public LayerMask layerMask;

	public Vector3 ParentPosition {
		get {
			if (transform.parent)
				return transform.parent.position;
			else return transform.position;
		}
	}

	public override void InitializeAgent () {
		surfComp = transform.parent.GetComponent<Fragsurf.Movement.SurfCharacter> ();
		surfComp.rewardAction = AddReward;
		surfComp.fallInCage = () => {

			surfComp.lastFallTime = Time.time; // * .2f
			/* Set */
			//AddReward (-1f * surfComp.multiplier);

			SetReward (-.1f * Mathf.Max (1, 5 - surfComp.multiplier));
			surfComp.multiplier = 1;
			emaVelocity = null;
		};
		/* {
			AddReward (-.2f * ++cageFall);
			surfComp.multiplier = 1;
			emaVelocity = null;
			float stepRatio = Mathf.Clamp (20 * (((float) myAcademy.GetTotalStepCount ()) / 500000f), 0, 25);
			if (cageFall >= 30 - stepRatio) {
				cageFall = 0;
				AddReward (-20);
				Done ();
			}
			//Done ();
			//SetReward(-1f);
		}; */

		myAcademy = FindObjectOfType<SurfAcademy> ();
		rayPer = GetComponent<RayPerceptionSurf> ();

		for (int i = 0; i < frontB.Length; i++) {
			frontA[i] += 90;
			frontB[i] += 90;
		}
		frontA[5] += 90;
		GameObject[] otherGameObjects = GameObject.FindGameObjectsWithTag ("banana");
		portPositions = new Vector3[otherGameObjects.Length];
		for (int i = 0; i < otherGameObjects.Length; i++)
			portPositions[i] = otherGameObjects[i].transform.position;

		otherGameObjects = GameObject.FindGameObjectsWithTag ("stone");
		boostPositions = new Vector3[otherGameObjects.Length];
		for (int i = 0; i < otherGameObjects.Length; i++)
			boostPositions[i] = otherGameObjects[i].transform.position;

		fDT = 1f / agentParameters.maxStep; //Time.fixedDeltaTime * 0.002f; //Mathf.Pow (Time.fixedDeltaTime, 5); //

		layerMask = LayerMask.GetMask (new string[] { "SURFABLE" });
		Respawn ();
	}
	EMA<Vector3> emaVelocity = null;

	public override void CollectObservations () {
		//250 + 140 = 390 + 14 = 404 + 5
		//300 + 168 + 19 = 487
		string[] detectableObjects = { "surfable", "wall", "ground", "pit" }; //, "respawnzone", "teleport"};
		AddVectorObs (rayPer.Perceive (rayDistance, frontA, detectableObjects, 0, 0f)); //50 // new 60
		AddVectorObs (rayPer.Perceive (rayDistance, frontA, detectableObjects, 0f, -10f)); //50
		AddVectorObs (rayPer.Perceive (rayDistance, frontA, detectableObjects, 0f, 10f)); //50
		AddVectorObs (rayPer.Perceive (rayDistance, frontA, detectableObjects, 0f, -35f)); //50
		AddVectorObs (rayPer.Perceive (rayDistance, frontA, detectableObjects, 0f, 35f)); //50
		AddVectorObs (rayPer.Perceive (rayDistance, frontB, detectableObjects, 0f, -25f)); //35 // new 42
		AddVectorObs (rayPer.Perceive (rayDistance, frontB, detectableObjects, 0f, 25f)); //35
		AddVectorObs (rayPer.Perceive (rayDistance, frontB, detectableObjects, 0f, -50f)); //35
		AddVectorObs (rayPer.Perceive (rayDistance, frontB, detectableObjects, 0f, 50f)); //35
		surfingBool = Physics.Raycast (transform.parent.position, Vector3.down, 1.33f, layerMask);
		AddVectorObs (surfingBool); //x1

		if (emaVelocity == null) {
			emaVelocity = new EMA<Vector3> (surfComp.MoveData.Velocity * .02f);
			AddVectorObs (surfComp.MoveData.Velocity * .02f); //x3
		} else {
			AddVectorObs (emaVelocity.GetValueFor (surfComp.MoveData.Velocity * .02f)); //x3
		}
		AddVectorObs (transform.rotation); //x4
		AddVectorObs (surfComp._controller.Grounded); //x1

		System.Array.Sort (portPositions, delegate (Vector3 a, Vector3 b) { return Vector3.Distance (ParentPosition, a).CompareTo (Vector3.Distance (ParentPosition, b)); });
		System.Array.Sort (boostPositions, delegate (Vector3 a, Vector3 b) { return Vector3.Distance (ParentPosition, a).CompareTo (Vector3.Distance (ParentPosition, b)); });
		AddVectorObs (Vector3.Distance (ParentPosition, portPositions[0]) * .002f); //x1
		AddVectorObs (Quaternion.LookRotation (portPositions[0] - ParentPosition, Vector3.up)); //x4

		AddVectorObs (Vector3.Distance (ParentPosition, boostPositions[0]) * .002f); //x1
		AddVectorObs (Quaternion.LookRotation (boostPositions[0] - ParentPosition, Vector3.up)); //x4
	}

	//[SerializeField] UnityEngine.UI.Text text;
	public override void AgentAction (float[] vectorAction, string textAction) {

		if (brain.brainParameters.vectorActionSpaceType == SpaceType.continuous) {
			moveFwd = vectorAction[0] > 0.33f;
			moveBack = vectorAction[0] < -0.33f;
			moveLeft = vectorAction[1] > 0.33f;
			moveRight = vectorAction[1] < -0.33f;
			xAxis = vectorAction[2];
			yAxis = vectorAction[3];
			jump = vectorAction[4] > 0.5f;
		} else {
			FwdBack = Mathf.FloorToInt (vectorAction[0]);
			LeftRight = Mathf.FloorToInt (vectorAction[1]);
			Jump = Mathf.FloorToInt (vectorAction[2]);
			rotateX = Mathf.FloorToInt (vectorAction[3]);
			rotateY = Mathf.FloorToInt (vectorAction[4]);

			moveFwd = FwdBack == 1;
			moveBack = FwdBack == 2;
			moveLeft = LeftRight == 1;
			moveRight = LeftRight == 2;
			jump = Jump == 1;

			xAxis = myAcademy.curve.Evaluate (Mathf.Ceil (rotateX / 2));
			if (rotateX % 2 == 0)
				xAxis = -xAxis;

			yAxis = myAcademy.curve.Evaluate (Mathf.Ceil (rotateY / 2));
			if (rotateY % 2 == 0)
				yAxis = -yAxis;
		}

		myActionReward = 0;

		Vector3 velocity = surfComp.MoveData.Velocity;
		if (velocity != Vector3.zero) {
			float absX = Mathf.Abs (velocity.x);
			float absZ = Mathf.Abs (velocity.z);
			float absY = Mathf.Abs (velocity.y);
			v = absX + absZ; 
			lookAngle = Quaternion.Angle (Quaternion.LookRotation (velocity, Vector3.up), transform.rotation);
			lookPenalty = (lookAngle <= 33) ? 0 : Mathf.Log (lookAngle);

			surfingBool = Physics.Raycast (transform.parent.position, Vector3.down, 1.33f, layerMask);

			if (surfingBool)
				myActionReward += surfComp.multiplier *
				(((v > 6 || velocity.y > 0) ? fDT * UTIL.OnePlusLogValue (v) * Mathf.Max (1, velocity.y) :
					-fDT * 10 * Mathf.Max (1, 10 - surfComp.multiplier)));
			else
				myActionReward += (surfComp._controller.Grounded) ?
				-fDT * 7.5f * Mathf.Max (1, 12 - surfComp.multiplier) :
				((velocity.y > 0 && !jump) ?
					surfComp.multiplier * fDT * UTIL.OnePlusLogValue (v * 1.5f) * Mathf.Max (1, velocity.y * 3) :
					(
						(absY - .33f < v) ?
						surfComp.multiplier * fDT * UTIL.OnePlusLogValue (v * 1.5f) :
						-fDT * UTIL.OnePlusLogValue (absY) * 5 * Mathf.Max (1, 5 - surfComp.multiplier)
					)
				);


			if (lookPenalty > 0 && !surfComp._controller.Grounded)
				myActionReward -= fDT * lookPenalty;
		} else AddReward (-fDT * 20);

		if (moveFwd || moveBack)
			myActionReward -= fDT;
		if (moveLeft || moveRight)
			myActionReward -= fDT;

		myActionReward -= (Mathf.Abs (xAxis) + Mathf.Abs (yAxis)) * fDT * .5f;

		AddReward (myActionReward);

		surfComp.UpdateMoveData (moveFwd, moveBack, moveLeft, moveRight, jump);
		surfComp.UpdateRotation (xAxis, yAxis);
	}

	void Respawn () {
		emaVelocity = null;
		Transform to = GameObject.Find ("CageSpawn").transform;
		if (to.childCount > 1)
			surfComp.SpawnAt (to.GetChild (Random.Range (0, to.childCount)).position);
		else
			surfComp.SpawnAt (to.position);
	}
	public override void AgentReset () {
		Respawn ();
		surfComp.multiplier = 1;
		cageFall = 0;
	}
}

public class EMA<T> {
	private T previous;
	private dynamic alpha = 0.6f;

	public T GetValueFor (T value) {
		value = (1 - alpha) * value + alpha * previous;
		previous = value;
		return value;
	}
	public EMA (T value) {
		previous = value;
	}
}