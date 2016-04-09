using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Threading;

namespace Loxonator.Common.Helpers
{
    public class AsyncObservableCollection<T> : ObservableCollection<T>
    {
        public override event NotifyCollectionChangedEventHandler CollectionChanged;

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            using (BlockReentrancy()) // like MSDN said
            {
                NotifyCollectionChangedEventHandler eventHandler = CollectionChanged;
                if (eventHandler == null)
                    return;
                Delegate[] delegates = eventHandler.GetInvocationList();
                foreach (NotifyCollectionChangedEventHandler handler in delegates)
                {
                    DispatcherObject dispatcherObject = handler.Target as DispatcherObject;
                    if (dispatcherObject != null && !dispatcherObject.CheckAccess())
                        dispatcherObject.Dispatcher.Invoke(DispatcherPriority.DataBind, handler, this, e); // invoke in different thread
                    else
                        handler(this, e); // execute as is
                }
            }
        }
    }
}
