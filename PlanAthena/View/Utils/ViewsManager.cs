// Fichier: PlanAthena/View/Utils/ViewManager.cs
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PlanAthena.View.Utils
{
    public class ViewsManager
    {
        private readonly Dictionary<Type, UserControl> _viewCache = new Dictionary<Type, UserControl>();

        public void RegisterView(UserControl view)
        {
            if (view != null)
            {
                _viewCache[view.GetType()] = view;
            }
        }

        public T GetView<T>() where T : UserControl
        {
            _viewCache.TryGetValue(typeof(T), out var view);
            return (T)view;
        }
    }
}