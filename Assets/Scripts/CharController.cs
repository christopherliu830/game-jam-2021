using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharController : MonoBehaviour
{

    public CharController partner;
    public float maxSeparation = 2f;
    [HideInInspector]
    public bool updates = true;
    public bool Focused => _focused;
    public Vector2 Position { get => _position; set => _position = value; }
    public Vector2 Velocity { get => _velocity; set => _velocity = value; }

    [Header("Physics")]
    public float maxSpeed = 2f;
    public float jumpForce;
    public int jumpFrames;
    public int climbFrames;
    public float acceleration = 2f;
    public float speedDecayRate = 0.9f;
    public LayerMask blockedBy;


    private Vector2 _velocity;
    private Vector2 _position;

    private Rigidbody2D _rb;
    private BoxCollider2D _collider;

    private bool _spacePressed;
    private bool _focused = false;
    private bool _thrown = false;
    private int _jumpFramesLeft;
    private int _climbFramesLeft;
    private ContactFilter2D _filter = new ContactFilter2D();
    private RaycastHit2D[] _hits = new RaycastHit2D[4];
    private ContactPoint2D[] _contacts = new ContactPoint2D[4];

    void Awake() {
        _position = transform.position;
        _jumpFramesLeft = jumpFrames;
        _climbFramesLeft = climbFrames;
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
        _filter.SetLayerMask(blockedBy);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            _spacePressed = true;
        }
        if (Input.GetKeyUp(KeyCode.Space)) {
            _spacePressed = false;
            if (!IsGrounded()) _jumpFramesLeft = 0;
        }
        if (_focused && Input.GetMouseButtonUp(1)) {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (Vector3.Distance(partner.Position, _position) < 0.2f) {
                partner.Throw((pos - Position).normalized);
            }
        }
        if (!_focused && Input.GetMouseButton(1)) {
            if (Vector3.Distance(partner.Position, _position) < 1.0f) {
                _position = partner.Position;
                _velocity = Vector2.zero;
            } else {
                _velocity = (partner.Position - _position).normalized / 5f; 
            }
        }
    }

    void FixedUpdate()
    {
        float dx = Input.GetAxisRaw("Horizontal");
        float dy = Input.GetAxisRaw("Vertical");
        bool grounded = IsGrounded();

        _velocity.y += GameManager.GRAVITY * Time.fixedDeltaTime;
        if (_velocity.y < -20) _velocity.y = -20;

        if (_focused) {
            _velocity.x += dx * acceleration;
        } else {
            dx = dy = 0;
            _spacePressed = false;
        }

        if (grounded) {
            _velocity.x *= speedDecayRate;
        }

        int dir = IsOnWall();
        if (_focused && dir != 0 && _climbFramesLeft > 0) {
            _velocity.y = dy * maxSpeed / 2;
            _velocity.x = dir;
            _climbFramesLeft--;

            // Give an extra boost at cliffs
            _position.y += 0.2f;
            if (IsOnWall() == 0) {  
                _velocity.y = jumpForce;
            }
            _position.y -= 0.2f;
        }

        if (grounded) {
            if (_velocity.y < 0) {
                _velocity.y = -0.1f;
                _thrown = false;
            }
            if (_spacePressed) {
                _velocity.y = jumpForce;
                _jumpFramesLeft--;
            } else {
                _jumpFramesLeft = jumpFrames;
                _climbFramesLeft = climbFrames;
            }
        } else if (_spacePressed && _jumpFramesLeft > 0)  {
            _velocity.y += jumpForce * ( 1 - (jumpFrames - _jumpFramesLeft) / (float)jumpFrames ) / 2f;
            _jumpFramesLeft--;
        }

        if (_jumpFramesLeft == 0) {
            _spacePressed = false;
        }

        if (Mathf.Abs(_velocity.x) > maxSpeed) {
            _velocity.x = Mathf.Sign(_velocity.x) * maxSpeed;
        }

        if (_focused || !grounded) {
            _velocity = ClampVelocity(_velocity);
        }
        Debug.DrawRay(_position, _velocity, Color.white, 1, false);
        _position = MoveWithCollision(_position, _velocity);
        transform.position = _position;


    }

    public void Focus() {
        _focused = true;
    }
    public void Unfocus() {
        _focused = false;
    }

    public bool IsGrounded() {
        RaycastHit2D hit = Physics2D.CapsuleCast(_position, _collider.size, CapsuleDirection2D.Vertical, 0, Vector2.down, 0.1f, blockedBy);
        return hit.collider != null;
    }

    public void Throw(Vector2 velocity) {
        _thrown = true;
        _velocity = velocity;
    }

    public int IsOnWall() {
        float extent = _collider.bounds.extents.x;
        RaycastHit2D hit = Physics2D.BoxCast(_position + Vector2.left * 0.1f, _collider.size, 0, Vector2.left, Mathf.Epsilon, blockedBy);
        if (hit.collider != null) return -1;

        hit = Physics2D.BoxCast(_position + Vector2.right * 0.1f, _collider.size, 0, Vector2.right, Mathf.Epsilon, blockedBy);
        if (hit.collider != null) return 1;

        return 0;
    }

    Vector2 ClampVelocity(Vector2 vel) {
        Vector2 pos = Position;

        float dist = (pos + vel - partner.Position).magnitude;

        if (dist > maxSeparation) {
            Vector2 edge = (pos + vel - partner.Position).normalized * (maxSeparation) + partner.Position;
            Vector2 ret = edge - pos;
            float damp = 0.1f;
            if (!_focused) damp = 1;
            return Vector2.Lerp(vel, ret, damp);
        }

        return vel;
    }
    Vector2 MoveWithCollision(Vector2 pos, Vector2 v) {

        Vector2 dy = new Vector2(0, v.y);
        Vector2 dx = new Vector2(v.x, 0);
        pos = MoveInDirection(pos, v, true);
        return pos;
    }

    Vector2 MoveInDirection(Vector2 position, Vector2 dir, bool isHorizontal) {
        Vector2 pos = new Vector2(position.x, position.y);


        RaycastHit2D hit = Physics2D.CapsuleCast(pos, _collider.size, CapsuleDirection2D.Vertical, 0, dir, dir.magnitude, blockedBy);
        if (hit.collider == null) { return position + dir; }
        var dot = Vector2.Dot(dir, hit.normal);

        int i = 0;
        while (hit.collider != null && i < 1000) {
            Debug.DrawRay(pos, hit.normal, Color.red, 1, false);
            hit = Physics2D.CapsuleCast(pos, _collider.size, CapsuleDirection2D.Vertical, 0, Vector2.zero, Mathf.Infinity, blockedBy);
            pos += hit.normal * 0.01f;
            i++;
        } 


        pos += dir;

        return pos;
    }
}
