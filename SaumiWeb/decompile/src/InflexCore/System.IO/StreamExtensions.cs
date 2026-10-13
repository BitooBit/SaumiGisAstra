using System.Threading;

namespace System.IO;

public static class StreamExtensions
{
	private const int maxBufferSize = 8192;

	public static void Copy(this Stream source, Stream output)
	{
		IAsyncResult asyncResult = null;
		byte[] array = new byte[8192];
		IAsyncResult asyncResult2 = null;
		byte[] array2 = null;
		for (int num = source.Read(array, 0, 8192); num != 0; num = source.EndRead(asyncResult))
		{
			array2 = new byte[num];
			Array.Copy(array, array2, num);
			asyncResult2 = output.BeginWrite(array2, 0, num, null, null);
			asyncResult = source.BeginRead(array, 0, 8192, null, null);
			WaitHandle.WaitAll(new WaitHandle[2] { asyncResult.AsyncWaitHandle, asyncResult2.AsyncWaitHandle });
			output.EndWrite(asyncResult2);
		}
	}
}
