namespace DesktopPet.Core
{
    public delegate bool HitTestCallback(int screenX, int screenY);

    public interface IPlatformIntegration
    {
        void InitializeTransparentWindow();
        void RegisterHitTestCallback(HitTestCallback callback);
        void Shutdown();
    }
}
