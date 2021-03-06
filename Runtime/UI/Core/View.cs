using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Framework.Assets;
using Framework.Asynchronous;
using Framework.Execution;
using Framework.UI.Core.Bind;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework.UI.Core
{
    public enum UILevel
    {
        None,
        Bg,
        Common,
        Pop,
        Toast,
        Guide,
        FullScreen,
    }
    
    public abstract class View
    {
        private List<View> _subViews;
        private CanvasGroup _canvasGroup;
        public GameObject Go { get; private set; }
        public ViewModel ViewModel { get; private set; }
        protected readonly UIBindFactory Binding;

        public View()
        {
            _subViews = new List<View>();
            Binding = new UIBindFactory();
        }

        public void SetGameObject(GameObject obj)
        {
            Go = obj;
            _canvasGroup = Go.GetOrAddComponent<CanvasGroup>();
            SetComponent();
            Start();
            GameLoop.Ins.OnUpdate += Update;
        }

        private static Dictionary<Type, List<Tuple<FieldInfo, string>>> _type2TransPath =
            new Dictionary<Type, List<Tuple<FieldInfo, string>>>();

        private void SetComponent()
        {
            if (!_type2TransPath.TryGetValue(GetType(), out var paths))
            {
                paths = new List<Tuple<FieldInfo, string>>();
                var fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
                foreach (var fieldInfo in fields)
                {
                    var path = fieldInfo.GetCustomAttribute<TransformPath>();
                    if (path != null)
                    {
                        paths.Add(new Tuple<FieldInfo, string>(fieldInfo, path.Path));
                        
                    }
                }
            }
            foreach (var tuple in paths)
            {
                try
                {
                    tuple.Item1.SetValue(this, Go.transform.Find(tuple.Item2).GetComponent(tuple.Item1.FieldType));
                }
                catch (Exception e)
                {
                    Log.Error(e);
                    Debug.Log(tuple.Item2 + " not found", Go);
                }
            }
        }
        
        public void SetVm(ViewModel vm)
        {
            if (vm == null || ViewModel == vm) return;
            ViewModel = vm;
            Binding.Reset();
            if (ViewModel != null)
            {
                OnVmChange();
            }
        }

        #region 界面显示隐藏的调用和回调方法

        protected virtual void Start()
        {
            
        }

        protected virtual void Update()
        {
        }

        public void Show()
        {
            Visible(true);
            OnShow();
        }

        public void Hide()
        {
            Visible(false);
            OnHide();
            _subViews.ForEach((subView) => subView.Hide());
        }

        protected virtual void OnShow()
        {
        }

        protected virtual void OnHide()
        {
        }

        public void Destroy()
        {
            Hide();
            GameLoop.Ins.OnUpdate -= Update;
            _subViews.ForEach(subView => subView.Destroy());
            Object.Destroy(Go.gameObject);
        }

        public void Visible(bool visible)
        {
            _canvasGroup.interactable = visible;
            _canvasGroup.alpha = visible ? 1 : 0;
            _canvasGroup.blocksRaycasts = visible;
        }

        #endregion

        public IProgressResult<float, T> AddSubView<T>(ViewModel viewModel = null) where T : View
        {
            foreach (var subView in _subViews)
            {
                if(subView is T)
                    return null;
            }
            var progressResult = UIManager.Ins.CreateView<T>(viewModel);
            progressResult.Callbackable().OnCallback((result => _subViews.Add(result.Result)));
            return progressResult;
        }

        public void AddSubView(View view)
        {
            if(_subViews.Contains(view))
                return;
            view.Go.transform.SetParent(Go.transform, true);
            _subViews.Add(view);
        }

        public T GetSubView<T>() where T : View
        {
            foreach (var subView in _subViews)
            {
                if (subView is T view)
                    return view;
            }
            return null;
        }

        public void Close()
        {
            UIManager.Ins.Close(GetType());
        }

        protected abstract void OnVmChange();
        public virtual UILevel UILevel { get; } = UILevel.Common;
        public virtual bool IsSingle { get; } = true;
    }
}