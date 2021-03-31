using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class HingeJointConnect : MonoBehaviour
{
    public List<HingeJoint2D> joints;
    private CurvedLineRenderer _lr;

    public static HingeJointConnect Connect(Rigidbody2D a, Rigidbody2D b, int subdivisions, int length) {
        List<HingeJoint2D> joints = new List<HingeJoint2D>(subdivisions);
        GameObject line = new GameObject("Rope");

        for(int i = 0; i < subdivisions; i++) {

            if (i == subdivisions - 1) {
                HingeJoint2D joint = b.gameObject.AddComponent<HingeJoint2D>();
                joint.connectedBody = joints[joints.Count-1].GetComponent<Rigidbody2D>();
                joint.autoConfigureConnectedAnchor = false;
                joint.connectedAnchor = new Vector2(-length / (float)(subdivisions - 1), 0);
                joints.Add(joint);
                break;
            }

            GameObject go = new GameObject("Joint");
            go.transform.parent = line.transform;
            HingeJoint2D j = go.AddComponent<HingeJoint2D>();
            j.autoConfigureConnectedAnchor = false;
            go.transform.position = Vector3.Lerp(a.position, b.position, (i)/(float)(subdivisions - 1));

            if (i == 0) {
                j.connectedBody = a;
                j.transform.parent = a.transform;
                j.connectedAnchor = Vector2.zero;
            } else {
                j.connectedAnchor = new Vector2(-length / (float)(subdivisions - 1), 0);
                j.connectedBody = joints[joints.Count-1].GetComponent<Rigidbody2D>();
            } 

            joints.Add(j);
        }

        CurvedLineRenderer clr = line.AddComponent<CurvedLineRenderer>();
        LineRenderer lr = line.GetComponent<LineRenderer>();
        HingeJointConnect rope = line.AddComponent<HingeJointConnect>();
        clr.lineSegmentSize = 0.03f;
        lr.numCapVertices = 4;
        lr.startWidth = .2f;
        lr.endWidth = .2f;
        rope.joints = joints;
        return rope;
    }

    void OnEnable() {
        _lr = GetComponent<CurvedLineRenderer>();
    }

    void Update() {
        // UpdateLine(joints.Select(j => j.transform.position).ToList(), 40);
        _lr.linePositions = (joints.Select(j => j.transform.position).ToArray());
        _lr.SetPointsToLine();
    }
}
