using System;

namespace Agatha.Common
{
	public abstract class Disposable : IDisposable
	{
		private bool isDisposed;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public bool IsDisposed
		{
			get { return isDisposed; }
		}

		protected void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (disposing)
				{
					DisposeManagedResources();
				}

				DisposeUnmanagedResources();
				isDisposed = true;
			}
		}

		protected void ThrowExceptionIfDisposed()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(GetType().FullName);
			}
		}

		protected abstract void DisposeManagedResources();
		protected virtual void DisposeUnmanagedResources() { }
	}
}