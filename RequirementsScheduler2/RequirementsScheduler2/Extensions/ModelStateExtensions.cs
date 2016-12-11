using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace RequirementsScheduler2.Extensions
{
    public static class ModelStateExtensions
    {
        public static string ErrorsToString(this ModelStateDictionary modelState)
        {
            return string.Join("; ", modelState.Values
                                               .SelectMany(x => x.Errors)
                                               .Select(x => x.ErrorMessage));
        }
    }
}
