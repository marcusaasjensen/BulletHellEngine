using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
[RequireComponent(typeof(ProjectileController))]
public class ProjectileCollision : MonoBehaviour
{
    [SerializeField] ProjectileController _projectileController;
    [SerializeField] Collider2D _projectileCollider;
    [SerializeField] List<AudioClip> _collisionSounds;

    void Awake()
    {
        if(!_projectileCollider)
            _projectileCollider = GetComponent<Collider2D>();
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (_projectileController == null)
        {
            Debug.LogWarning("Projectile Controller needs to be referenced in Projectile Collision script.", this);
            return;
        }

        if (!collider.CompareTag(_projectileController.Target.tag)) return;

        SoundManager.Instance.PlayRandomSound(_collisionSounds, true);

        if (_projectileController.DisappearWhenTouchingTarget)
            gameObject.SetActive(false);
    }

    public void EnableCollider(bool value)
    {
        if (_projectileCollider == null) return;
        _projectileCollider.enabled = value;
    }
}