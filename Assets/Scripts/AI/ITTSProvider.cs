using System;
using UnityEngine;

namespace DesktopPet.AI
{
    public interface ITTSProvider
    {
        /// <summary>
        /// Converts text to an AudioClip asynchronously.
        /// </summary>
        /// <param name="text">The text to synthesize.</param>
        /// <param name="onSuccess">Callback with the generated AudioClip.</param>
        /// <param name="onError">Callback with the error message.</param>
        void SynthesizeAudioAsync(string text, Action<AudioClip> onSuccess, Action<string> onError);
    }
}
