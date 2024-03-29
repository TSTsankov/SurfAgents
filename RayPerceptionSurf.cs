using System;
using System.Collections.Generic;
using UnityEngine;

namespace MLAgents {

    /// <summary>
    /// Ray perception component. Attach this to agents to enable "local perception"
    /// via the use of ray casts directed outward from the agent. 
    /// </summary>
    public class RayPerceptionSurf : RayPerception {
        Vector3 endPosition;
        RaycastHit hit;
        private float[] subList;
        [SerializeField] LayerMask layerMask;

        /// <summary>
        /// Creates perception vector to be used as part of an observation of an agent.
        /// Each ray in the rayAngles array adds a sublist of data to the observation.
        /// The sublist contains the observation data for a single ray. The list is composed of the following:
        /// 1. A one-hot encoding for detectable objects. For example, if detectableObjects.Length = n, the
        ///    first n elements of the sublist will be a one-hot encoding of the detectableObject that was hit, or
        ///    all zeroes otherwise.
        /// 2. The 'length' element of the sublist will be 1 if the ray missed everything, or 0 if it hit
        ///    something (detectable or not).
        /// 3. The 'length+1' element of the sublist will contain the normalised distance to the object hit.
        /// NOTE: Only objects with tags in the detectableObjects array will have a distance set.
        /// </summary>
        /// <returns>The partial vector observation corresponding to the set of rays</returns>
        /// <param name="rayDistance">Radius of rays</param>
        /// <param name="rayAngles">Angles of rays (starting from (1,0) on unit circle).</param>
        /// <param name="detectableObjects">List of tags which correspond to object types agent can see</param>
        /// <param name="startOffset">Starting height offset of ray from center of agent.</param>
        /// <param name="endOffset">Ending height offset of ray from center of agent.</param>
        public override List<float> Perceive (float rayDistance,
            float[] rayAngles, string[] detectableObjects,
            float startOffset, float endOffset) {
            if (subList == null || subList.Length != detectableObjects.Length + 2)// + 3)
                subList = new float[detectableObjects.Length + 2];// + 3];

            perceptionBuffer.Clear ();
            perceptionBuffer.Capacity = subList.Length * rayAngles.Length;

            // For each ray sublist stores categorical information on detected object
            // along with object distance.
            foreach (float angle in rayAngles) {
                endPosition = Quaternion.Euler (0, angle, 0) * (Quaternion.AngleAxis (endOffset, -Vector3.right)) * -Vector3.forward;
                endPosition = transform.rotation * endPosition;
                //Debug.DrawRay(transform.parent.position, transform.TransformDirection(endPosition), Color.magenta);

                /* endPosition = transform.TransformDirection (
                    PolarToCartesian (rayDistance, angle));
                endPosition.y = endOffset; */
                if (Application.isEditor) {
                    Debug.DrawRay (transform.parent.position + new Vector3 (0f, startOffset, 0f),
                        endPosition * rayDistance, Color.cyan, 0.01f, true);
                }

                Array.Clear (subList, 0, subList.Length);

                if (Physics.SphereCast (transform.parent.position +
                        new Vector3 (0f, startOffset, 0f), 0.1f,
                        endPosition, out hit, rayDistance, layerMask.value)) {
                    for (int i = 0; i < detectableObjects.Length; i++) {
                        if (hit.collider.gameObject.CompareTag (detectableObjects[i])) {
                            subList[i] = 1;
                            subList[detectableObjects.Length + 1] = hit.distance / rayDistance;
                            //subList[detectableObjects.Length + 2] = hit.normal[0];
                            //subList[detectableObjects.Length + 3] = hit.normal[1];
                            //subList[detectableObjects.Length + 4] = hit.normal[2];
                            
                            /*           
                            int[] triangles = (hit.collider as MeshCollider).sharedMesh.triangles;
                            Vector3[] vertices = (hit.collider as MeshCollider).sharedMesh.vertices;
                            Vector3 p0 = vertices[triangles[hit.triangleIndex * 3 + 0]];
                            Vector3.Distance(transform.parent.position, hit.barycentricCoordinate);
                            Vector3.Distance(transform.parent.position, hit.barycentricCoordinate);
                            Vector3 p1 = vertices[triangles[hit.triangleIndex * 3 + 1]];
                            Vector3 p2 = vertices[triangles[hit.triangleIndex * 3 + 2]];
                            */
                            break;
                        }
                    }
                } else {
                    subList[detectableObjects.Length] = 1f;
                }

                Utilities.AddRangeNoAlloc (perceptionBuffer, subList);
            }

            return perceptionBuffer;
        }

        /// <summary>
        /// Converts polar coordinate to cartesian coordinate.
        /// </summary>
        public static Vector3 PolarToCartesian (float radius, float angle) {
            float x = radius * Mathf.Cos (DegreeToRadian (angle));
            float z = radius * Mathf.Sin (DegreeToRadian (angle));
            return new Vector3 (x, 0f, z);
        }

    }
}