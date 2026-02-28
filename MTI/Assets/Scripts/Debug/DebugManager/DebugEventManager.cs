using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;


namespace Game.QA.Shared
{
    public class DebugEventManager : DebugSingleton<DebugEventManager>
    {
        private Dictionary<DebugEventID, Delegate> _eventTable = new Dictionary<DebugEventID, Delegate>();

        #region Interface

        private void Start()
        {
            if (DebugManager.Instance != null && this.transform.parent == null)
            {
                transform.SetParent(DebugManager.Instance.transform);
            }

            SyncMgr();
        }

        protected override void OnDestroy()
        {
            DesyncMgr();
        }

        #endregion

        #region Sync

        private void SyncMgr()
        {
        }

        private void DesyncMgr()
        {
            RemoveAll();
        }

        #endregion

        #region Register

        public void RegisterEvent<T>(DebugEventID evtType, Action<T> callback)
        {
            _eventTable.TryAdd(evtType, null);

            _eventTable[evtType] = (Action<T>)_eventTable[evtType] + callback;
        }


        public void RegisterEvent(DebugEventID evtType, Action callback)
        {
            _eventTable.TryAdd(evtType, null);

            _eventTable[evtType] = (Action)_eventTable[evtType] + callback;
        }

        #endregion

        #region Remove

        public void RemoveEvent<T>(DebugEventID evtType, Action<T> callback)
        {
            if (_eventTable.ContainsKey(evtType))
                _eventTable[evtType] = (Action<T>)_eventTable[evtType] - callback;
        }


        public void RemoveEvent(DebugEventID evtType, Action callback)
        {
            if (_eventTable.ContainsKey(evtType))
                _eventTable[evtType] = (Action)_eventTable[evtType] - callback;
        }

        public void RemoveEvent(DebugEventID evtType)
        {
            _eventTable.Remove(evtType);
        }

        public void RemoveAll()
        {
            _eventTable.Clear();
        }

        #endregion

        #region FireEvent

        public void FireEvent<T>(DebugEventID evtType, T arg)
        {
            if (_eventTable.TryGetValue(evtType, out var value))
            {
                Action<T> callback = value as Action<T>;
                callback?.Invoke(arg);
            }
        }


        public void FireEvent(DebugEventID evtType)
        {
            if (_eventTable.TryGetValue(evtType, out var value))
            {
                Action callback = value as Action;
                callback?.Invoke();
            }
        }


        private IEnumerator FireEvent(DebugEventID evtType, float delay)
        {
            yield return new WaitForSeconds(delay);
            FireEvent(evtType);
        }

        private IEnumerator FireEvent<T>(DebugEventID evtType, T arg, float delay)
        {
            yield return new WaitForSeconds(delay);
            FireEvent(evtType, arg);
        }

        #endregion

        #region DelayedFire

        public void DelayedFireEvent(DebugEventID evtType, float delay)
        {
            StartCoroutine(FireEvent(evtType, delay));
        }

        public void DelayedFireEvent<T>(DebugEventID evtType, T arg, float delay)
        {
            StartCoroutine(FireEvent(evtType, arg, delay));
        }

        #endregion

        #region FindEvent

        public bool FindEvent(DebugEventID evtType)
        {
            return _eventTable.ContainsKey(evtType);
        }

        #endregion
    }
}