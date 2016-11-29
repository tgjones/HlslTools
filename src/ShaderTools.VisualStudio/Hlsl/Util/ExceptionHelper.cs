using System;
using System.Threading.Tasks;

namespace ShaderTools.VisualStudio.Hlsl.Util
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