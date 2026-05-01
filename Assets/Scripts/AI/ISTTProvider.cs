using System;

namespace DesktopPet.AI
{
    public interface ISTTProvider
    {
        void TranscribeWavAsync(byte[] wavData, Action<string> onSuccess, Action<string> onError);
    }
}

