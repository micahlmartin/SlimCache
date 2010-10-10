using System.IO.IsolatedStorage;
public class test
{
    public void s()
    {
        var store = IsolatedStorageFile.GetUserStoreForApplication();
    }
}