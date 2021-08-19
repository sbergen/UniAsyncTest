using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace UniAsyncTest
{
	internal class EnumeratorSynchronizationContext : SynchronizationContext
	{
		private readonly ConcurrentQueue<(SendOrPostCallback callback, object state)> callbacks =
			new ConcurrentQueue<(SendOrPostCallback, object)>();

		private bool done;
		private Exception exception;

		public override void Send(SendOrPostCallback d, object state)
		{
			throw new NotSupportedException("Synchronous completion is not supported");
		}

		public override void Post(SendOrPostCallback d, object state) =>
			this.callbacks.Enqueue((d, state));

		public override SynchronizationContext CreateCopy() => this;

		public static IEnumerator RunTask(Func<Task> taskFactory)
		{
			var oldContext = Current;
			var context = new EnumeratorSynchronizationContext();

			SetSynchronizationContext(context);

			context.Post(async _ =>
			{
				try
				{
					await taskFactory();
				}
				catch (Exception e)
				{
					context.exception = e;
					throw;
				}
				finally
				{
					context.Post(__ => context.done = true, null);
				}
			}, null);

			SetSynchronizationContext(oldContext);

			return context.Run();
		}

		private IEnumerator Run()
		{
			while (!this.done)
			{
				if (this.callbacks.TryDequeue(out var data))
				{
					data.callback?.Invoke(data.state);
					if (this.exception != null)
					{
						throw this.exception;
					}
				}

				yield return null;
			}
		}
	}
}
