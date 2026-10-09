using UnityEngine;

/// Simplified Sonic 1 style controller (flat terrain, movement along Z).
/// Constants are the Sonic 1 values converted from px/frame @ 60fps to units/sec,
/// assuming 16 px = 1 Unity unit (so Sonic is ~2.5 units tall).
/// If your character is smaller/larger, change worldScale instead of every value.
[RequireComponent(typeof(Rigidbody))]
public class SonicController : MonoBehaviour {
    [Header("References")]
    public Rigidbody rb;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.25f;

    [Header("Scale (1 = Sonic 1 size at 16px per unit)")]
    public float worldScale = 1f;

    [Header("Sonic 1 physics (units/sec, units/sec^2)")]
    public float topSpeed = 22.5f; // 6 px/frame
    public float acceleration = 10.5f; // 0.046875 px/frame^2
    public float airAccel = 21.1f; // 0.09375  px/frame^2 (double ground accel)
    public float friction = 10.5f;  // same as acceleration
    public float braking = 112.5f; // 0.5 px/frame^2 (pushing against movement)
    public float jumpSpeed = 24.4f; // 6.5 px/frame
    public float jumpCap = 15f; // 4 px/frame, upward speed cap on early release
    public float gravity = 49.2f; // 0.21875 px/frame^2

    public int rings = 0;

    private float horizontalInput;
    private bool jumpPressed;
    private bool jumpHeld;
    private bool isJumping;

    private void Awake() {
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX;
    }

    private void Update() {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        jumpPressed |= Input.GetButtonDown("Jump");
        jumpHeld = Input.GetButton("Jump");
    }

    private void FixedUpdate() {
        float dt = Time.fixedDeltaTime;
        float s = worldScale;
        bool grounded = IsGrounded();

        // Use the real velocity so walls and collisions stop us naturally
        Vector3 v = rb.linearVelocity;
        float speed = v.z;
        float dir = Mathf.Sign(horizontalInput);

        if (horizontalInput != 0f) {
            // Braking when pushing against current movement (ground only)
            float accel;
            if (grounded) accel = (dir * speed < 0f) ? braking : acceleration;
            else          accel = airAccel;

            // Only add speed while below top speed in the input direction.
            // Never pulls you back down if something else pushed you above it.
            if (dir * speed < topSpeed * s) {
                speed += dir * accel * s * dt;
                if (dir * speed > topSpeed * s) speed = dir * topSpeed * s;
            }
        }
        else if (grounded) {
            speed = Mathf.MoveTowards(speed, 0f, friction * s * dt);
        }

        if (jumpPressed && grounded) {
            v.y = jumpSpeed * s;
            isJumping = true;
        }
        jumpPressed = false;

        if (isJumping && !jumpHeld && v.y > jumpCap * s) {
            v.y = jumpCap * s;
        }

        if (!grounded && v.y > 0f && v.y < jumpCap * s && Mathf.Abs(speed) > 0.5f * s) {
            speed *= Mathf.Pow(0.96875f, dt * 60f); // original: x0.96875 per frame
        }

        v.y -= gravity * s * dt;

        // Clear jump state once landed
        if (grounded && v.y <= 0.1f * s) isJumping = false;

        rb.linearVelocity = new Vector3(0f, v.y, speed);
    }

    private bool IsGrounded() {
        return Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
    }

    public void CollectRing() {
        rings++;
        Debug.Log("Rings: " + rings);
    }
}