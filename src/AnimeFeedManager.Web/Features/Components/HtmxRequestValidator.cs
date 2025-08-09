
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
        if (HttpContextAccessor.GetHtmxRequestType() is HtmxRequestType.HxForm)
        {
            CurrentEditContext?.Validate();
        }
    }
}