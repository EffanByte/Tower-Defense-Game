using System.Collections.Generic;
using UnityEngine;

public class EnemyPathAgent : MonoBehaviour
{
    [Header("Motion")]
    public float speed = 3.5f;         // world units / second
    public float yFixed = 0.1f;        // keep enemies on your gameplay plane
    public bool orientIn3D = true;     // true for XZ top-down; false if you want 2D XY rotation
    public float turnSpeed = 12f;      // rotation smoothing

    private IReadOnlyList<Vector3> _wps;
    private int _i;
    private bool _active;

    public System.Action<EnemyPathAgent> OnReachedEnd; // hook your base damage / pooling here

    public void Init(IReadOnlyList<Vector3> waypoints, float? overrideSpeed = null, float? overrideY = null)
    {
        if (waypoints == null || waypoints.Count == 0)
        {
            Debug.LogError("[EnemyPathAgent] Empty waypoints.");
            enabled = false;
            return;
        }

        _wps = waypoints;
        _i = 0;
        _active = true;

        if (overrideSpeed.HasValue) speed = overrideSpeed.Value;
        if (overrideY.HasValue) yFixed = overrideY.Value;

        // Start exactly at the first waypoint (prevents a visible snap on first Update)
        var start = _wps[0]; start.y = yFixed;
        transform.position = start;

        // Optionally face the next segment
        if (_wps.Count > 1)
        {
            var dir = FlatDir(_wps[1] - start);
            ApplyOrientation(dir, instant: true);
        }
    }

    void Update()
    {
        if (!_active || _wps == null) return;

        float move = speed * Time.deltaTime;

        // Step across as many segments as needed this frame
        while (move > 0f && _i < _wps.Count)
        {
            var pos = transform.position;
            var target = _wps[_i]; target.y = yFixed;

            var to = target - pos;
            var dist = to.magnitude;

            if (dist <= 1e-5f)
            {
                // Reached current waypoint
                _i++;
                if (_i >= _wps.Count)
                {
                    _active = false;
                    OnReachedEnd?.Invoke(this);
                    return;
                }
                continue;
            }

            if (move >= dist)
            {
                // Land exactly on this waypoint and continue with leftover distance
                transform.position = target;
                ApplyOrientation(FlatDir(to)); // face where we were going
                move -= dist;
                _i++;
                if (_i >= _wps.Count)
                {
                    _active = false;
                    OnReachedEnd?.Invoke(this);
                    return;
                }
            }
            else
            {
                // Advance partway to the target
                var dir = to / dist;
                transform.position = pos + dir * move;
                ApplyOrientation(FlatDir(dir));
                move = 0f;
            }
        }
    }

    Vector3 FlatDir(Vector3 v)
    {
        if (orientIn3D)
        {
            v.y = 0f;
            return v.normalized;
        }
        else
        {
            // for 2D XY projects you'd rotate in XY; here we still move in XZ plane
            v.z = 0f;
            return v.normalized;
        }
    }

    void ApplyOrientation(Vector3 dir, bool instant = false)
    {
        if (dir.sqrMagnitude < 1e-6f) return;

        if (orientIn3D)
        {
            var look = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = instant ? look
                                         : Quaternion.Slerp(transform.rotation, look, turnSpeed * Time.deltaTime);
        }
        else
        {
            // 2D: face right along dir.x/dir.y (if you render in XY)
            float angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            var rot = Quaternion.Euler(0, 0, angleDeg);
            transform.rotation = instant ? rot
                                         : Quaternion.Slerp(transform.rotation, rot, turnSpeed * Time.deltaTime);
        }
    }
}
