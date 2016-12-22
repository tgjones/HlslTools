using System;
using System.Threading.Tasks;

namespace ShaderTools.Editor.VisualStudio.Core.Util
{
    internal static class ExceptionHelper
    {
        public static async Task TryCatchCancellation(Func<Task> callback)
        {
            try
            {
                await callback();
            }
            catch (OperationCanceledException)
            {

            }
            catch (ObjectDisposedException)
            {
                
            }
        }
    }
}