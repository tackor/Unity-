using UnityEngine;

public class CameraCtrl : MonoBehaviour {

    public Transform Maincamera;
    Vector3 SetPoint;
    Vector3 CameraOffset;

    public void CameraInit() {
        Maincamera.position = CameraOffset;
        transform.position = Vector3.zero;
    }

    public void SetPosition(Vector3 position) {
        SetPoint.x = position.x;
        SetPoint.z = position.z;
        SetPoint.y = 0;
    }

	// Use this for initialization
	void Start () {
        CameraOffset = Maincamera.position;
	}
	
	// Update is called once per frame
	void Update () {
        Maincamera.position = Vector3.Lerp(Maincamera.position, SetPoint + CameraOffset, 0.1f);
        transform.position = Vector3.Lerp(transform.position, SetPoint, 0.1f);
	}
}