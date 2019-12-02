using MLAgents;

public class SurfAcademy : Academy {

	public UnityEngine.AnimationCurve curve = new UnityEngine.AnimationCurve();
	public override void AcademyReset () {
		System.GC.Collect();
	}

	public override void AcademyStep () {
		//System.GC.Collect();
	}
}