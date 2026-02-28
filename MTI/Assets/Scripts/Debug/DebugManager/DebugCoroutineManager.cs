using UnityEngine;
using System.Collections;

namespace Game.QA.Shared
{
    public class DebugCoroutineManager : DebugSingleton<DebugCoroutineManager>
    {
        private void Start()
        {
            if (DebugManager.Instance != null && transform.parent == null)
            {
                transform.SetParent(DebugManager.Instance.transform);
            }
        }

        public Coroutine Run(IEnumerator routine)
        {
            return StartCoroutine(routine);
        }

        public IEnumerator RunAndWait(IEnumerator routine)
        {
            yield return StartCoroutine(routine);
        }

        public void Stop(Coroutine coroutine)
        {
            if (Instance != null)
                StopCoroutine(coroutine);
        }
    }
}