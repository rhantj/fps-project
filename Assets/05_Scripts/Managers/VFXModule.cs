using System.Collections;
using UnityEngine;

public class VFXModule : MonoBehaviour
{
    [SerializeField] ParticleSystem ps;

    private void Awake()
    {
        ps = GetComponentInChildren<ParticleSystem>();

    }

    private void OnEnable()
    {
        if (ps != null)
            ps.Play();
        StartCoroutine(Co_Return());
    }

    IEnumerator Co_Return()
    {
        yield return null;

        while (ps != null && ps.IsAlive(true))
        {
            yield return null;
        }

        ObjectPoolManager.Instance.Despawn(gameObject);
    }
}