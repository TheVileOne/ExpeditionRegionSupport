using System;

namespace LogUtils
{
    public interface IAccessToken : IDisposable
    {
        IAccessToken Access();
        void RevokeAccess();
    }
}
