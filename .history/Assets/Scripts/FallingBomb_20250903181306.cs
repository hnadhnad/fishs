using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bom di chuyển không-dùng-physics (lerp từ spawn -> target trong fallDuration),
/// khi đến target sẽ nổ: gây damage trong radius, spawn meat pieces.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class FallingBomb : MonoBehaviour
{
    Vector3 _start;
    Vector3 _target;
    float _duration;
    float _t;
    float _explodeRadius;
    float _damage;
    GameObject _meatPrefab;
    int _meatCount;
    Boss _bossRef;
    bool _initialized = false;

    // gọi để setup
    public void Initialize(Vector3 target, float fallDuration, float explodeRadius, float damage, GameObject meatPrefab, int meatCount, Boss bossRef)
    {
        _start = transform.position;
        _target = target;
        _duration = Mathf.Max(0.05f, fallDuration);
        _explodeRadius = explodeRadius;
        _damage = damage;
        _meatPrefab = meatPrefab;
        _meatCount = Mathf.Max(0, meatCount);
        _bossRef = bossRef;
        _t = 0f;
        _initialized = true;

        // make sure collider is trigger (so it won't physically push)
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void Update()
    {
        if (!_initialized) return;

        _t += Time.deltaTime;
        float f = Mathf.Clamp01(_t / _duration);
        // ease-in for fall (feel natural)
        float ease = Mathf.SmoothStep(0f, 1f, f);
        transform.position = Vector3.Lerp(_start, _target, ease);

        if (f >= 1f)
        {
            Explode();
        }
    }

    void Explode()
    {
        // 1) Damage boss if inside radius (use boss ref if not null otherwise overlap)
        if (_bossRef != null)
        {
            float d = Vector3.Distance(_bossRef.transform.position, transform.position);
            if (d <= _explodeRadius)
            {
                _bossRef.TakeDamage(_damage);
            }
        }
        else
        {
            // fallback: overlap to find boss
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _explodeRadius);
            foreach (var h in hits)
            {
                Boss b = h.GetComponent<Boss>();
                if (b != null)
                {
                    b.TakeDamage(_damage);
                }
            }
        }

        // 2) Spawn meat pieces in 4 directions (or _meatCount evenly)
        if (_meatPrefab != null && _meatCount > 0)
        {
            float angleStep = 360f / _meatCount;
            float speed = 3.5f; // initial outward impulse for meat pieces
            for (int i = 0; i < _meatCount; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
                GameObject m = Instantiate(_meatPrefab, transform.position + dir * 0.2f, Quaternion.identity);

                // if meat has Rigidbody2D, give initial velocity
                if (m.TryGetComponent<Rigidbody2D>(out var rb))
                {
                    rb.velocity = dir * speed;
                }

                // ensure meat is configured: if Fish exists, set hungerValue etc as needed elsewhere
            }
        }

        // optional: spawn VFX, sound etc (user can attach on prefab)
        Destroy(gameObject);
    }

    // debug gizmo for explosion radius
    void OnDrawGizmosSelected()
    {
        if (_initialized)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _explodeRadius);
        }
    }
}
