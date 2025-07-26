namespace NativeAOTJS;

public class MyEventSource
{
    public event EventHandler<string>? Message;

    public void Raise(string msg)
    {
        Message?.Invoke(this, msg);
    }
}