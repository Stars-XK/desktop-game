namespace DesktopPet.Core
{
    public interface IPlatformIntegration
    {
        void InitializeTransparentWindow();
        void SetClickThrough(bool passthrough);
    }
}
