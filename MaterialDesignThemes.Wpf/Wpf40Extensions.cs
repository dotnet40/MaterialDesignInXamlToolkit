using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Windows.Threading
{
    internal class DispatcherOperation<TResult>
    {
        private readonly DispatcherOperation _operation;
        private readonly TaskCompletionSource<TResult> _tcs;
        public Task<TResult> Task
        {
            get
            {
                return _tcs.Task;
            }
        }
        
        [SecurityCritical]
        internal DispatcherOperation(Dispatcher dispatcher, DispatcherPriority priority, Func<TResult> func)
        {
            _tcs = new TaskCompletionSource<TResult>();
            _operation = dispatcher.BeginInvoke(func, priority);
            _operation.Completed += operation_Completed;
            _operation.Aborted += operation_Aborted;
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public TaskAwaiter<TResult> GetAwaiter()
        {
            return this.Task.GetAwaiter();
        }

        private void operation_Aborted(object sender, EventArgs e)
        {
            _tcs.SetCanceled();
        }

        private void operation_Completed(object sender, EventArgs e)
        {
            _tcs.SetResult((TResult)_operation.Result);
        }
    }

    internal static class DispatcherOperationExtensions
    {

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public static TaskAwaiter GetAwaiter(this DispatcherOperation operation)
        {
            var tcs = new TaskCompletionSource<object>();
            operation.Completed += (s, e) => tcs.SetResult(null);
            operation.Aborted += (s, e)=>tcs.SetCanceled();
            return ((Task)tcs.Task).GetAwaiter();
        }
    }

    internal static class DispatcherExtensions
    {
        public static DispatcherOperation InvokeAsync(this Dispatcher dispather, Action callback)
        {
            return dispather.InvokeAsync(callback, DispatcherPriority.Normal, CancellationToken.None);
        }

        public static DispatcherOperation InvokeAsync(this Dispatcher dispather, Action callback, DispatcherPriority priority)
        {
            return dispather.InvokeAsync(callback, priority, CancellationToken.None);
        }

        public static DispatcherOperation InvokeAsync(this Dispatcher dispather, Action callback, DispatcherPriority priority, CancellationToken cancellationToken)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));
            return dispather.BeginInvoke(priority, callback);
        }

        public static DispatcherOperation<TResult> InvokeAsync<TResult>(this Dispatcher dispather, Func<TResult> callback)
        {
            return dispather.InvokeAsync(callback, DispatcherPriority.Normal, CancellationToken.None);
        }

        public static DispatcherOperation<TResult> InvokeAsync<TResult>(this Dispatcher dispather, Func<TResult> callback, DispatcherPriority priority, CancellationToken cancellationToken)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));
            return new DispatcherOperation<TResult>(dispather, priority, callback);
        }
    }
}
