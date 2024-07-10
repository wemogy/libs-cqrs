using FastEndpoints;

namespace Wemogy.CQRS.Extensions.FastEndpoints.Extensions;

public static class PostProcessorContextExtensions
{
    /// <summary>
    /// The MarkExceptionAsHandled method add the CtxKey.EdiIsHandled key to the HttpContext.Items dictionary
    /// This method checks if the CtxKey.EdiIsHandled key is present in the HttpContext.Items dictionary
    /// </summary>
    public static bool IsExceptionHandled(this IPostProcessorContext context)
    {
        // The CtxKey class is an internal class of FastEndpoints which means that we can't access it
        // for that reason we hardcoded the value of the key
        // The PostProcessorContextExtensionsTests class tests this implementation, so if the key changes the tests will fail
        return context.HttpContext.Items.ContainsKey("3");
    }
}
