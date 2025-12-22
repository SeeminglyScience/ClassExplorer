namespace ClassExplorer;

internal interface IEnumerationCallback<T>
{
    void Invoke(T value);

    void Invoke(T value, object? source);
}
