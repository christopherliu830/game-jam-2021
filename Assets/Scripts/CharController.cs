using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharController : MonoBehaviour
{

    public CharController partner;
    public float maxSeparation = 2f;
    public float followDistance = 2f;
    [HideInInspector]
    public bool updates = true;
    public bool Focused => _focused;
    public bool inputsEnabled = true;
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
        followDistance = maxSeparation;
        _position = transform.position;
        _jumpFramesLeft = jumpFrames;
        _climbFramesLeft = climbFrames;
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
        _filter.SetLayerMask(blockedBy);
    }

    void Update() {
        if (Input.GetKeyUp(KeyCode.Space)) {
            _spacePressed = false;
            if (!IsGrounded()) _jumpFramesLeft = 0;
        }
        if (!inputsEnabled) return;
        if (Input.GetKeyDown(KeyCode.Space)) {
            _spacePressed = true;
        }
        if (_focused) {
            if (Input.GetMouseButtonUp(0)) {
                Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                if (Vector3.Distance(partner.Position, _position) < 1.0f) {
                    partner.Throw((pos - Position).normalized);
                } 
            }
            if (Input.GetMouseButton(0)) {
                if (Vector3.Distance(partner.Position, _position) < 1.0f) {
                    partner.Position = _position;
                    partner._velocity = Vector2.zero;
                }
            }
            if (Input.GetMouseButton(1)) {
                if (Vector3.Distance(partner.Position, _position) < 1.0f) {
                    partner.Position = _position;
                    partner._velocity = Vector2.zero;
                } else {
                    partner._velocity = (_position - partner._position).normalized / 5f; 
                }
            }
        }
    }
    void FixedUpdate()
    {
        if (!inputsEnabled) return;
        float dx = Input.GetAxisRaw("Horizontal");
        float dy = Input.GetAxisRaw("Vertical");

        bool space = _spacePressed;
        if (!_focused) {
            (dx, space) = AI();
        }
        Move(dx, dy, space);
    }

    public void Move(float dx, float dy, bool space) {
        bool grounded = IsGrounded();
        _velocity.y += GameManager.GRAVITY * Time.fixedDeltaTime;
        if (_velocity.y < -1) _velocity.y = -1; // Cap fall speed

        _velocity.x += dx * acceleration;

        if (grounded) {
            _velocity.x *= speedDecayRate;
        }

        if (grounded) {
            if (_velocity.y < 0) {
                _velocity.y = -0.1f;
                _thrown = false;
            }
            if (space) {
                _velocity.y = jumpForce;
                _jumpFramesLeft--;
            } else {
                _jumpFramesLeft = jumpFrames;
                _climbFramesLeft = climbFrames;
            }
        } else if (space && _jumpFramesLeft > 0)  {
            _velocity.y += jumpForce * ( 1 - (jumpFrames - _jumpFramesLeft) / (float)jumpFrames ) / 2f;
            _jumpFramesLeft--;
        }

        if (_jumpFramesLeft == 0) {
            space = false;
        }

        if (Mathf.Abs(_velocity.x) > maxSpeed) {
            _velocity.x = Mathf.Sign(_velocity.x) * maxSpeed;
        }

        if (_focused || !grounded) {
            _velocity = ClampVelocity(_velocity);
        }
        _position = MoveWithCollision(_position, _velocity);
        _rb.MovePosition(_position);
    }
    public void Focus() {
        _focused = true;
    }
    public void Unfocus() {
        _focused = false;
    }

    public bool IsGrounded() {
        Vector2 size = new Vector2(_collider.size.x / 5f, _collider.size.y);
        RaycastHit2D hit = Physics2D.CapsuleCast(_position, size, CapsuleDirection2D.Vertical, 0, Vector2.down, 0.1f, blockedBy);
        return hit.collider != null;
    }

    public void Throw(Vector2 velocity) {
        _thrown = true;
        _velocity = velocity;
    }

    public int IsOnWall() {
        RaycastHit2D hit = Physics2D.BoxCast(_position + new Vector2(-0.1f, 0.5f), _collider.size, 0, Vector2.left, Mathf.Epsilon, blockedBy);
        if (hit.collider != null) return -1;

        hit = Physics2D.BoxCast(_position + new Vector2(0.1f, 0.5f), _collider.size, 0, Vector2.right, Mathf.Epsilon, blockedBy);
        if (hit.collider != null) return 1;

        return 0;
    }

    public int IsOnLedge() {
        if (IsOnWall() == 0) return 0;
        RaycastHit2D hit = Physics2D.BoxCast(_position + new Vector2(-0.1f, _collider.size.y), _collider.size, 0, Vector2.left, Mathf.Epsilon, blockedBy);
        if (hit.collider == null) return -1;

        hit = Physics2D.BoxCast(_position + new Vector2(0.1f, _collider.size.y), _collider.size, 0, Vector2.right, Mathf.Epsilon, blockedBy);
        if (hit.collider == null) return 1;

        return 0;

    }

    (float, bool) AI() {
        float dx = 0;
        bool space =  false;

        if (Mathf.Abs(Position.x - partner.Position.x) > followDistance * 0.75f) {
            dx = Mathf.Sign(partner.Position.x - Position.x);
            space = IsOnLedge() != 0 && IsGrounded() && partner.IsGrounded() && partner.Position.y - Position.y > 0.75f;
        }

        // Attempt jump if partner is higher up
        return (dx, space);
    }

    public IEnumerator MoveTo(Vector2 position, float timeout) {
        float timer = 0;
        while (timer < timeout) {
            timer += Time.fixedDeltaTime;
            float dx = Mathf.Sign(position.x - Position.x);
            bool space = IsOnLedge() != 0 && IsGrounded() && position.y - Position.y > 0.75f;
            Move(dx, 0, space);
            yield return new WaitForFixedUpdate();
        }
        yield return null;
    }

    Vector2 ClampVelocity(Vector2 vel) {
        Vector2 pos = Position;

        float dist = (pos + vel - partner.Position).magnitude;

        if (dist > maxSeparation) {
            Vector2 edge = (pos + vel - partner.Position).normalized * (maxSeparation) + partner.Position;
            Vector2 ret = edge - pos;
            float damp = 0.1f;
            if (!_focused) damp = 1;
            return ret;
            // return Vector2.Lerp(vel, ret, damp);
        }

        return vel;
    }
    Vector2 MoveWithCollision(Vector2 position, Vector2 dir) {
        Vector2 pos = new Vector2(position.x, position.y);

        RaycastHit2D hit = Physics2D.CapsuleCast(pos, _collider.size, CapsuleDirection2D.Vertical, 0, dir, dir.magnitude, blockedBy);
        if (hit.collider == null) { return position + dir; }
        var dot = Vector2.Dot(dir, hit.normal);

        int i = 0; // Failsafe :)
        while (hit.collider != null && i < 1000) {
            hit = Physics2D.CapsuleCast(pos, _collider.size, CapsuleDirection2D.Vertical, 0, Vector2.zero, Mathf.Infinity, blockedBy);
            Debug.DrawRay(pos, hit.normal, Color.red, 1, false);
            if (Vector2.Dot(Vector2.up, hit.normal) > 0.8f)  {
                hit.normal = Vector2.up;
            }
            pos += hit.normal * 0.015f;
            i++;
        } 
        pos += dir;
        return pos;
    }
}
