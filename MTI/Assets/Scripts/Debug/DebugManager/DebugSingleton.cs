using UnityEngine;

namespace Game.QA.Shared
{
    public class DebugSingleton<T> : MonoBehaviour where T : DebugSingleton<T>
    {
        protected static GameObject root;

        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    // 尝试在场景中找
                    _instance = FindObjectOfType<T>();
                    if (_instance == null)
                    {
                        // 找不到则自动创建
                        root = new GameObject(typeof(T).Name);
                        _instance = root.AddComponent<T>();
                        DontDestroyOnLoad(root);
                    }
                }

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
        }

        protected virtual void Update()
        {
        }
    }
}










