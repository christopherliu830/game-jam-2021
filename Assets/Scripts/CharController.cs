using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharController : MonoBehaviour
{
    public SpriteRenderer sprite;
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
    private float _currentSeparation;
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
        sprite = GetComponentInChildren<SpriteRenderer>();
        _currentSeparation = GameManager.ROPE_LENGTH;
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
        }
    }
    void FixedUpdate()
    {
        float dx = 0, dy = 0;
        bool space = false;
        _velocity.y += GameManager.GRAVITY * Time.fixedDeltaTime;
        if (_velocity.y < -1) _velocity.y = -1; // Cap fall speed

        if (inputsEnabled) { 

            // If the right mouse isn't held, increase the separation distance until max
            if (!Input.GetMouseButton(1)) {
                if (_currentSeparation < maxSeparation) {
                    _currentSeparation += 0.1f;
                    if (_currentSeparation > maxSeparation ||
                        IsGrounded() && partner.IsGrounded()) _currentSeparation = maxSeparation;
                }
            }

            if (_focused) {
                if (!Input.GetMouseButton(1)) { // If pulling, freeze my movement
                    dx = Input.GetAxisRaw("Horizontal");
                    dy = Input.GetAxisRaw("Vertical");
                    space = _spacePressed;
                }
            } else if (Input.GetMouseButton(1)) { // I'm being pulled
                if (partner.IsGrounded()) PullTowards(partner.Position);
                return;
            } else if (Input.GetMouseButton(0)) { // I'm grabbed 
                if (Vector3.Distance(partner.Position, _position) < 1.0f) {
                    _position = MoveWithCollision(_position, partner.Position - _position);
                    _rb.MovePosition(_position);
                    Velocity = Vector2.zero;
                    return;
                }
            }
            else {
                (dx, space) = AI();
            }
        }

        Move(dx, dy, space);
    }

    public void PullTowards(Vector2 to) {
        Vector2 vel;
        float dx = Input.GetAxisRaw("Horizontal");
        if (IsGrounded()) {
            dx = 0;
            bool space = false;
            // AI Move to partner
            if (Mathf.Abs(to.x - Position.x) > 0.9f) {
                dx = Mathf.Sign(to.x - Position.x);
                space = IsOnWall() != 0;
            } else {
                space = (to.y - Position.y) > 1f;
                if (space) _currentSeparation = Vector2.Distance(Position, partner.Position);
            }
            Move(dx, 0, space);
            return;
        }
        if (IsOnWall() == 0) {
            _currentSeparation -= 0.1f;
            vel = _velocity + Vector2.right * dx * 0.05f;
        } else if (IsOnCeiling()) { 
            vel = _velocity + Vector2.right * dx * 0.05f;
            _currentSeparation = Mathf.Min(Vector2.Distance(Position, partner.Position), maxSeparation);
            vel.x = Mathf.Min(0.5f, vel.x);
        } else { // Pull me up the wall
            var dir = to.x - Position.x;
            if (dx != 0) dir = dx;
            vel = new Vector2(dir, Mathf.Sign(to.y - Position.y)) / 20f; 
            if (IsOnLedge() != 0) { 
                vel += Vector2.up / 3f; 
            }
            _currentSeparation = Vector2.Distance(Position, partner.Position);
        } 
        _velocity = ClampVelocity(vel, _currentSeparation);
        _position = MoveWithCollision(_position, _velocity);
        _rb.MovePosition(_position);
        if (dx < 0) sprite.flipX = true;
        else if (dx == 0 && _velocity.x < 0) sprite.flipX = true;
        else sprite.flipX = false;
    }

    public void Move(float dx, float dy, bool space) {
        bool grounded = IsGrounded();

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

        if (!_thrown && Mathf.Abs(_velocity.x) > maxSpeed) {
            _velocity.x = Mathf.Sign(_velocity.x) * maxSpeed;
        }

        if (_focused || !grounded) {
            _velocity = ClampVelocity(_velocity);
        }

        _position = MoveWithCollision(_position, _velocity);
        _rb.MovePosition(_position);
        if (dx < 0) sprite.flipX = true;
        else if (dx == 0 && _velocity.x < 0) sprite.flipX = true;
        else sprite.flipX = false;
    }
    public void Focus() {
        _focused = true;
        sprite.gameObject.layer = 6;
    }
    public void Unfocus() {
        _focused = false;
        sprite.gameObject.layer = 8;
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

    public bool IsOnCeiling() {
        var size = _collider.size;
        size.x *= 0.9f;
        RaycastHit2D hit = Physics2D.BoxCast(_position, size, 0, Vector2.up, 0.1f, blockedBy);
        return hit.collider != null;
    }

    public int IsOnLedge() {
        var dir = IsOnWall();
        if (dir == 0) return 0;
        Vector2 offset = new Vector2(0, _collider.size.y + 0.1f);
        var extent = _collider.bounds.extents.x;
        RaycastHit2D hit = Physics2D.Raycast(_position + offset, Vector2.left, extent + 1f, blockedBy);
        if (dir == -1 && hit.collider == null) return -1;

        offset = new Vector2(0, _collider.size.y + 0.1f);
        hit = Physics2D.Raycast(_position + offset, Vector2.right, extent + 1f, blockedBy);
        if (dir == 1 && hit.collider == null) return 1;

        return 0;

    }

    (float, bool) AI() {
        float dx = 0;
        bool space =  false;

        if (Mathf.Abs(Position.x - partner.Position.x) > followDistance * 0.75f) {
            dx = Mathf.Sign(partner.Position.x - Position.x);
            space = IsOnLedge() != 0 && partner.IsGrounded() && partner.Position.y - Position.y > 0.75f ||
                    partner.Position.y - Position.y > 10f;
        }
        if (Vector2.Distance(Position, partner.Position) > followDistance * .95f) {
            if (IsGrounded() && partner.IsGrounded()) {
                dx = Mathf.Sign(partner.Position.x - Position.x);
                space = true;
            }
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
        return ClampVelocity(vel, _currentSeparation);
    }
    Vector2 ClampVelocity(Vector2 vel, float separation) {
        Vector2 pos = Position;
        float dist = (pos + vel - partner.Position).magnitude;
        if (dist > separation) {
            Vector2 edge = (pos + vel - partner.Position).normalized * (separation) + partner.Position;
            Debug.DrawLine(pos, edge, Color.blue, 1, false);
            Vector2 ret = edge - pos;
            return ret;
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
            hit = Physics2D.CapsuleCast(pos, _collider.size, CapsuleDirection2D.Vertical, 0, dir, dir.magnitude, blockedBy);
            if (hit.collider == null) break;

            Debug.DrawRay(hit.point, hit.normal, Color.red, 1, false);
            var d = Vector2.Dot(Vector2.up, hit.normal);
            if (d > 0.8f) hit.normal = Vector2.up;
            else if (Mathf.Abs(d) < 0.4f) {
                if (hit.normal.x < 0) hit.normal = Vector2.left;
                else hit.normal = Vector2.right;
            }
            
            dir += hit.normal * 0.035f;
            i++;
        } 
        pos += dir;
        return pos;
    }
}
