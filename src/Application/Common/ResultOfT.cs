namespace BoldareBrewery.Application.Common
{
    public class Result<T> : Result
    {
        public T? Value { get; }

        internal Result(T? value, bool isSuccess, Error error) : base(isSuccess, error)
        {
            Value = value;
        }

        public static implicit operator Result<T>(T value) => Success(value);
        public static implicit operator Result<T>(Error error) => Failure<T>(error);

        public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Error, TResult> onFailure)
        {
            return IsSuccess ? onSuccess(Value!) : onFailure(Error);
        }
    }
}
