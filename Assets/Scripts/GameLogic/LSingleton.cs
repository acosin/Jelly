using System;
public class LSingleton<T> where T : class, new()
{
    private static T instance_ = default(T);
    public static T Instance
    {
        get
        {
            if (null == LSingleton<T>.instance_)
            {
                LSingleton<T>.instance_ = Activator.CreateInstance<T>();
            }
            return LSingleton<T>.instance_;
        }
    }
    public void DeleteInstance()
    {
        LSingleton<T>.instance_ = default(T);
    }
}