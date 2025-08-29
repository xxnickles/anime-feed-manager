
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace AnimeFeedManager.Web.Features.Components;

public class HtmxRequestValidator : ComponentBase
{
    [Inject]
    public required IHttpContextAccessor HttpContextAccessor { get; set; }

    // Get the EditContext from the parent component (EditForm)
    [CascadingParameter]
    private EditContext? CurrentEditContext { get; set; }

    protected override void OnParametersSet()
    {
        // Validate that this component is used within an EditForm
        if (CurrentEditContext == null)
        {
            throw new InvalidOperationException(
                $"{nameof(HtmxRequestValidator)} must be placed inside an EditForm component.");
        }
        
        if (HttpContextAccessor.GetHtmxRequestType() is HxForm)
        {
            CurrentEditContext?.Validate();
        }
    }
}