using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Control_Cam : MonoBehaviour {

    public Transform focus;
    public float moveSpeed;
    public float rotateSpeed;
    public float min_vertical;
    public float max_vertical;
    public bool allowMove;

    private Vector3 _initial_offset;
    private Vector3 _forwardSide_dir;
    private Vector3 _vertical_dir;

    private float _horizontal;
    private float _vertical;

    
    // Use this for initialization
    void Start() {
        _initial_offset = focus.position - transform.position;
        transform.LookAt(focus);
        _horizontal = transform.eulerAngles.y;
        _vertical = transform.eulerAngles.x;
    }

    // Update is called once per frame
    void Update()
    {
        _HandleKeyboard();   
    }

    void FixedUpdate()
    {
        if (allowMove)
        {
            _HandleMouse();
            _Move();
        }
    }

    void _Move()
    {
        transform.Translate(_forwardSide_dir * moveSpeed * Time.fixedDeltaTime, Space.World);
    }

    void _HandleKeyboard()
    {
        _forwardSide_dir = Vector3.zero;
        _vertical_dir = Vector3.zero;

        if(!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
        {
            if (Input.GetKey(KeyCode.W))
            {
                _forwardSide_dir += transform.forward;
            }
            else if(Input.GetKey(KeyCode.S))
            {
                _forwardSide_dir += -1* transform.forward;
            }

            if (Input.GetKey(KeyCode.D))
            {
                _forwardSide_dir += transform.right;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                _forwardSide_dir += -1 * transform.right;
            }

            if (Input.GetKey(KeyCode.Q))
            {
                _forwardSide_dir += transform.up;
            }
            else if (Input.GetKey(KeyCode.E))
            {
                _forwardSide_dir += -1 * transform.up;
            }
        }
    }

    void _HandleMouse()
    {
        _horizontal += Input.GetAxis("Mouse X") * rotateSpeed;
        _vertical -= Input.GetAxis("Mouse Y") * rotateSpeed;
        _vertical = Mathf.Clamp(_vertical, min_vertical, max_vertical);

        if (_horizontal != 0.0f || _vertical != 0.0f)
        {
            transform.localEulerAngles = new Vector3(_vertical, _horizontal, transform.rotation.eulerAngles.z);
        }
    }
}
