using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FurEver.Web.TagHelpers;

[HtmlTargetElement("label", Attributes = ForAttributeName)]
public class RequiredLabelTagHelper : TagHelper
{
    private const string ForAttributeName = "asp-for";

    [HtmlAttributeName(ForAttributeName)]
    public ModelExpression For { get; set; } = default!;

    public override int Order => 100;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (For.Metadata.IsRequired)
        {
            output.PostContent.AppendHtml(" <span class=\"required-star\" aria-hidden=\"true\">*</span>");
        }
    }
}
