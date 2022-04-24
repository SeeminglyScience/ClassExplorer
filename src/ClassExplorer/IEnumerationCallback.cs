namespace ClassExplorer;

internal interface IEnumerationCallback<T>
{
    void Invoke(T value);
}
