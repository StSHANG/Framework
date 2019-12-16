using System;
using System.Collections.Generic;
using UnityEngine;

namespace AD
{
    /// <summary>
    /// 提供注册，单个释放和全部释放接口
    /// 可以使用lambda来注册
    /// 当传入MonoBehaviour的时候，可以在物体销毁的时候自动释放事件
    /// </summary>
    public class EventService
    {
        public EventService(MonoBehaviour mono)
        {
            var trigger = mono.GetComponent<MonoDestroyTrigger>();
            if (trigger == null)
                trigger = mono.gameObject.AddComponent<MonoDestroyTrigger>();
            trigger.AddDisposeOnDestroy(UnRegisterAll);
        }

        public EventService()
        {
        }

        private List<Action> unRegisterEvent = new List<Action>();
        
        public void Register<T>(Action<T> listener)
        {
            MsgMgr.Register(listener);
            unRegisterEvent.Add(() => MsgMgr.UnRegister(listener));
        }

        public void Register(int tag, Action listener)
        {
            MsgMgr.Register(tag, listener);
            unRegisterEvent.Add(() => MsgMgr.UnRegister(tag, listener));
        }

        public void UnRegister<T>(Action<T> listener)
        {
            MsgMgr.UnRegister(listener);
        }

        public void UnRegister(int tag, Action listener)
        {
            MsgMgr.UnRegister(tag, listener);
        }

        public void UnRegisterAll()
        {
            unRegisterEvent.ForEach((action) => action?.Invoke());
            unRegisterEvent.Clear();
        }

    }
}