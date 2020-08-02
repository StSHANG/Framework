﻿using System;
using System.Globalization;
using Framework.UI.Wrap.Base;
using UnityEngine.UI;

namespace Framework.UI.Wrap
{
    public class TextWrapper : BaseWrapper<Text>, IFieldChangeCb<string>, IFieldChangeCb<int>, IFieldChangeCb<float>,
        IFieldChangeCb<double>
    {
        public TextWrapper(Text text) : base(text)
        {
            view = text;
        }

        Action<string> IFieldChangeCb<string>.GetFieldChangeCb()
        {
            return (value) => view.text = value;
        }

        public Action<int> GetFieldChangeCb()
        {
            return value => view.text = value.ToString();
        }

        Action<float> IFieldChangeCb<float>.GetFieldChangeCb()
        {
            return value => view.text = value.ToString(CultureInfo.InvariantCulture);
        }

        Action<double> IFieldChangeCb<double>.GetFieldChangeCb()
        {
            return value => view.text = value.ToString(CultureInfo.InvariantCulture);
        }
    }
}