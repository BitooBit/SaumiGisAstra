namespace InflexCore.Components;

public class ValidateResult
{
	public bool IsSuccess { get; private set; }

	public bool IsFiled => !IsSuccess;

	public ValidateResult(bool isSuccess)
	{
		IsSuccess = isSuccess;
	}
}
