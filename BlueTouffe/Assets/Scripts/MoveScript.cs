using UnityEngine;
using System.Collections;
using LostPolygon.AndroidBluetoothMultiplayer.Examples;

public class MoveScript : MonoBehaviour {
    public float Speed = 100f;
    public double NetworkInterpolationBackTime = 0.11;
    private Vector3 _destination;
    private Transform _transform;
    private Renderer _renderer;
    private NetworkView _networkView;
    private NetworkTransformInterpolation _transformInterpolation;
    GameObject _obj;

    private void Awake()
    {
        _transform = GetComponent<Transform>();
        _renderer = GetComponent<Renderer>();
        _networkView = GetComponent<NetworkView>();
        _destination = transform.position;
        _transformInterpolation = new NetworkTransformInterpolation();
        _transformInterpolation.InterpolationBackTime = NetworkInterpolationBackTime;
    }
    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
    private void Update()
    {
        if ( _networkView.isMine )
        {
            _destination.z = 0f;
            Vector3 direction = _destination - transform.position;

            if ( direction.magnitude > 1f )
                transform.Translate(Speed * direction.normalized * Time.deltaTime);

            if ( Input.GetMouseButtonDown(0) )
            {
                Debug.Log(_transform.position);
                _destination = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
        }
        else {
            Vector3 interpolatedPosition = _transform.position;
            Quaternion interpolatedRotation = _transform.rotation;
            _transformInterpolation.Update(ref interpolatedPosition, ref interpolatedRotation);

            _transform.position = interpolatedPosition;
            _transform.rotation = interpolatedRotation;
        }
    }
    private void OnSerializeNetworkView( BitStream stream, NetworkMessageInfo info )
    {
        // Serialize the position and color
        
        _transformInterpolation.OnSerializeNetworkView(stream, info, _transform.position, _transform.rotation);
    }
}
