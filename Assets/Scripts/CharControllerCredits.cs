using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class CharControllerCredits : MonoBehaviour
{
    public SpriteRenderer sprite;
    public CharControllerCredits partner;
    public float maxSeparation = Mathf.Infinity;
    public float followDistance = Mathf.Infinity;
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

    [Header("Animations")]
    public AnimationClip runClip;
    public AnimationClip idleClip;
    public AnimationClip limpClip;
    public AnimationClip jumpRiseClip;
    public AnimationClip jumpFallClip;

    private Vector2 _velocity;
    private Vector2 _position;

    private Rigidbody2D _rb;
    private BoxCollider2D _collider;

    private AnimancerComponent _animancer;
    private bool _spacePressed;
    private bool _focused = false;
    private bool _thrown = false;
    private bool _updating = true;
    private int _jumpFramesLeft;
    private int _climbFramesLeft;
    private float _currentSeparation;
    private ContactFilter2D _filter = new ContactFilter2D();
    private RaycastHit2D[] _hits = new RaycastHit2D[4];
    private ContactPoint2D[] _contacts = new ContactPoint2D[4];
    private int _counter = 0;

    void Awake() {
        followDistance = maxSeparation;
        _position = transform.position;
        _jumpFramesLeft = jumpFrames;
        _climbFramesLeft = climbFrames;
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
        _filter.SetLayerMask(blockedBy);
        sprite = GetComponentInChildren<SpriteRenderer>();
        _currentSeparation = Mathf.Infinity;
        _animancer = GetComponentInChildren<AnimancerComponent>();
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

    void Animate() {
        if (IsGrounded()) {
          _animancer.Play(runClip);
        } 
        else { // Airborne and not being pulled
          if (_velocity.y > 0) _animancer.Play(jumpRiseClip, 0);
          else _animancer.Play(jumpFallClip, 0);
        }
    }

    void FixedUpdate()
    {
        if (!_updating) return;
        if (_position.x > 10) {
          _position.x = -10;
          transform.position = _position;
        }
        _counter++;

        bool space = _counter % 500 == 0;
        _velocity.y += GameManager.GRAVITY * Time.fixedDeltaTime;
        if (_velocity.y < -1) _velocity.y = -1; // Cap fall speed

        Move(1, 0, space);
        Animate();
    }

    public void PullTowards(Vector2 to) {
        Vector2 vel;
        float dx = Input.GetAxisRaw("Horizontal");
        float distX = Mathf.Abs(to.x - Position.x);
        Debug.Log(IsGrounded() + " " + IsOnWall() + " " + IsOnLedge() + " " + IsOnCeiling());
        if (IsGrounded() && distX > 1.0f ) {
            dx = 0;
            bool space = false;
            // AI Move to partner
            dx = Mathf.Sign(to.x - Position.x);
            space = IsOnWall() != 0;
            Move(dx, 0, space);
            return;
        }
        if (IsOnCeiling() && Position.y < partner.Position.y) {
            
            vel = _velocity + Vector2.right * dx * 0.05f;
            _currentSeparation = Mathf.Min(Vector2.Distance(Position, partner.Position) + 0.5f, maxSeparation);
            vel.x = Mathf.Clamp(vel.x, -maxSpeed, maxSpeed);
            vel.y = 1;
        }
        else if (IsOnWall() == 0) {
            _currentSeparation = Mathf.Max(_currentSeparation - 0.2f, 0.5f);
            Debug.Log(_currentSeparation);
            vel = _velocity + Vector2.right * dx * 0.05f;
        } else { // Pull me up the wall
            var dir = to.x - Position.x;
            vel = new Vector2(dir, Mathf.Sign(to.y - Position.y) * 1.5f) / 20f; 
            if (IsOnLedge() != 0) { 
                vel += Vector2.up / 3f; 
            }
            _currentSeparation = Mathf.Min(Vector2.Distance(Position, partner.Position) + 0.5f, maxSeparation);
        } 
        _velocity = ClampVelocity(vel, _currentSeparation);
        _position = MoveWithCollision(_position, _velocity);
        _rb.MovePosition(_position);
        if (dx < 0) sprite.flipX = true;
        else if (dx == 0 && _velocity.x < 0) sprite.flipX = true;
        else sprite.flipX = false;
        Animate();
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
        Vector2 size = new Vector2(_collider.size.x * .75f, _collider.size.y * .5f);
        var offset = new Vector2(0, -_collider.size.y * .25f);
        var extent = _collider.bounds.extents.y;
        RaycastHit2D hit = Physics2D.BoxCast(_position + offset, size, 0, Vector2.down, 0.075f, blockedBy);
        return hit.collider != null;
    }

    public void Throw(Vector2 velocity) {
        _thrown = true;
        _velocity = velocity;
    }

    public int IsOnWall() {
        var offset = new Vector2(-_collider.size.x * .35f, 0.0f);
        var size = _collider.size;
        size.y *= .6f;
        RaycastHit2D hit = Physics2D.BoxCast(_position + offset, size, 0, Vector2.left, 0.5f, blockedBy);
        if (hit.collider != null) return -1;

        offset = new Vector2(_collider.size.x * .35f, 0.05f);
        hit = Physics2D.BoxCast(_position + offset, size, 0, Vector2.right, 0.5f, blockedBy);
        if (hit.collider != null) return 1;

        return 0;
    }

    public bool IsOnCeiling() {
        var size = _collider.size;
        size.x *= 0.4f;
        RaycastHit2D hit = Physics2D.BoxCast(_position, size, 0, Vector2.up, 0.1f, blockedBy);
        return hit.collider != null;
    }

    public int IsOnLedge() {
        var dir = IsOnWall();
        if (dir == 0) return 0;
        Vector2 offset = new Vector2(0, _collider.bounds.extents.y + 0.1f);
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

        if (Vector2.Distance(Position, partner.Position) > followDistance * 0.9f && partner.IsGrounded()) {
            dx = Mathf.Sign(partner.Position.x - Position.x);
            space = IsOnLedge() != 0 && partner.IsGrounded() ||
                    partner.Position.y - Position.y > 10f;
        }

        // Attempt jump if partner is higher up
        return (dx, space);
    }

    public IEnumerator MoveTo(Vector2 position, float timeout) {
        float timer = 0;
        _updating = false;
        while (timer < timeout) {
            timer += Time.fixedDeltaTime;
            float dx = Mathf.Sign(position.x - Position.x);
            bool space = position.y > Position.y;
            _velocity.y += GameManager.GRAVITY * Time.fixedDeltaTime;
            if (_velocity.y < -1) _velocity.y = -1; // Cap fall speed
            Move(dx, 0, space);
            Animate();
            yield return new WaitForFixedUpdate();
        }
        _updating = true;
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

        // RaycastHit2D hit = Physics2D.CapsuleCast(pos, _collider.size, CapsuleDirection2D.Vertical, 0, dir, dir.magnitude, blockedBy);
        RaycastHit2D hit = Physics2D.BoxCast(pos, _collider.size, 0, dir, dir.magnitude, blockedBy);
        if (hit.collider == null) { return position + dir; }
        var dot = Vector2.Dot(dir, hit.normal);

        int i = 0; // Failsafe :)
        while (hit.collider != null && i < 1000) {
            // hit = Physics2D.CapsuleCast(pos, _collider.size, CapsuleDirection2D.Vertical, 0, dir, dir.magnitude, blockedBy);
            hit = Physics2D.BoxCast(pos, _collider.size, 0, dir, dir.magnitude, blockedBy);
            if (hit.collider == null) break;

            var d = Vector2.Dot(Vector2.up, hit.normal);
            if (d > 0.8f) hit.normal = Vector2.up;
            else if (Mathf.Abs(d) < 0.3f) {
                if (hit.normal.x < 0) hit.normal = Vector2.left;
                else hit.normal = Vector2.right;
            }
            Debug.DrawRay(hit.point, hit.normal, Color.red, 1, false);
            
            dir += hit.normal * 0.01f;
            i++;
        } 
        pos += dir;
        return pos;
    }
}
