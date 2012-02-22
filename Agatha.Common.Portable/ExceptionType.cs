using System;

namespace Agatha.Common
{
    [CLSCompliant(true)]
	public enum ExceptionType
	{
		None,
		Business,
		Security,
		EarlierRequestAlreadyFailed,
		Unknown
	}
}