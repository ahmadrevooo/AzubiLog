using System.ComponentModel.DataAnnotations;

namespace AzubiLog.Services.Shared;

public static class ValidationHelper
{
    public static void ValidateModel(object model)
    {
        var context = new ValidationContext(model);
        Validator.ValidateObject(model, context, validateAllProperties: true);
    }
}
