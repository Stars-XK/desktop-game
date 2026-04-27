using System;

namespace DesktopPet.AI
{
    public interface ILLMProvider
    {
        /// <summary>
        /// Sends a message to the LLM and returns the response asynchronously.
        /// </summary>
        /// <param name="message">The user's input text.</param>
        /// <param name="onSuccess">Callback with the AI's response text and an optional emotion tag.</param>
        /// <param name="onError">Callback with the error message.</param>
        void SendMessageAsync(string message, Action<string, string> onSuccess, Action<string> onError);
    }
}
