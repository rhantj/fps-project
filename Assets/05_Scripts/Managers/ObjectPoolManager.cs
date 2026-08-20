using System;
using System.Collections.Generic;
using UnityEngine;

public enum PoolId
{
    Target,
    SoundPlayer,
    Grenade,
    GrenadeExplosionVFX,
    HitVFX,
    MuzzleVFX
}

public sealed class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    [Serializable]
    public class PoolConfig
    {
        public PoolId id;
        public GameObject prefab;
        public int prewarmCount;
    }

    [Header("Pool Configs")]
    [SerializeField] private List<PoolConfig> configs = new();

    [Header("Root")]
    [SerializeField] private Transform poolRoot;

    private readonly Dictionary<PoolId, Pool> pools = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (poolRoot == null)
        {
            var root = new GameObject("[PoolRoot]");
            root.transform.SetParent(transform);
            poolRoot = root.transform;
        }

        BuildPools();
    }

    private void BuildPools()
    {
        pools.Clear();

        foreach (var cfg in configs)
        {
            if (cfg == null || cfg.prefab == null) continue;
            if (pools.ContainsKey(cfg.id)) continue;

            var pool = new Pool(cfg, poolRoot);
            pools.Add(cfg.id, pool);

            if (cfg.prewarmCount > 0)
                pool.Prewarm(cfg.prewarmCount);
        }
    }
    public GameObject Spawn(PoolId id, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (!pools.TryGetValue(id, out var pool))
        {
            return null;
        }

        return pool.Get(position, rotation, parent);
    }

    public void Despawn(GameObject instance)
    {
        if (!instance) return;

        var member = instance.GetComponent<PoolMember>();
        if (member == null)
        {
            Destroy(instance);
            return;
        }

        if (!pools.TryGetValue(member.Id, out var pool))
        {
            Destroy(instance);
            return;
        }

        pool.Release(instance);
    }

    private  class Pool
    {
        private PoolConfig cfg;
        private Transform root;
        private Stack<GameObject> stack = new();

        // 스택 안에 들어 있는 인스턴스 집합. 이중 반납을 막는다.
        private readonly HashSet<GameObject> pooled = new();

        public Pool(PoolConfig cfg, Transform poolRoot)
        {
            this.cfg = cfg;

            var child = new GameObject($"[{cfg.id}]");
            child.transform.SetParent(poolRoot);
            root = child.transform;
        }

        public void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var go = CreateNew();
                go.SetActive(false);
                stack.Push(go);
                pooled.Add(go);
            }
        }

        public GameObject Get(Vector3 pos, Quaternion rot, Transform parent)
        {
            if (stack.Count == 0)
            {
                var go = CreateNew();
                go.SetActive(false);
                stack.Push(go);
                pooled.Add(go);
            }

            var inst = stack.Pop();
            pooled.Remove(inst);
            return SpawnInstance(inst, pos, rot, parent);
        }

        public void Release(GameObject inst)
        {
            if (!inst) return;

            // 이미 반납된 인스턴스를 다시 넣으면 스택에 중복으로 쌓여
            // 서로 다른 두 곳에 같은 오브젝트가 동시에 대여된다
            if (!pooled.Add(inst)) return;

            if (inst.TryGetComponent<IPoolable>(out var p))
                p.OnDespawned();

            inst.transform.SetParent(root, false);
            inst.SetActive(false);
            stack.Push(inst);
        }

        private GameObject SpawnInstance(GameObject inst, Vector3 pos, Quaternion rot, Transform parent)
        {
            var t = inst.transform;

            if (parent != null) t.SetParent(parent, false);
            else t.SetParent(null, true);

            t.SetPositionAndRotation(pos, rot);

            inst.SetActive(true);

            if (inst.TryGetComponent<IPoolable>(out var p))
                p.OnSpawned();

            return inst;
        }

        private GameObject CreateNew()
        {
            var go = UnityEngine.Object.Instantiate(cfg.prefab, root);
            go.name = $"{cfg.prefab.name} (Pooled)";

            var member = go.GetComponent<PoolMember>();
            if (member == null) member = go.AddComponent<PoolMember>();
            member.Initialize(cfg.id);

            return go;
        }
    }

    private class PoolMember : MonoBehaviour
    {
        public PoolId Id { get; private set; }
        public void Initialize(PoolId id) => Id = id;
    }
}
