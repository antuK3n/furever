using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FurEver.Web.TagHelpers;

/// <summary>
/// Appends a red asterisk to labels whose bound model property is required.
/// Runs after the built-in LabelTagHelper so the generated label text is preserved.
/// </summary>
[HtmlTargetElement("label", Attributes = ForAttributeName)]
public class RequiredLabelTagHelper : TagHelper
{
    private const string ForAttributeName = "asp-for";

    [HtmlAttributeName(ForAttributeName)]
    public ModelExpression For { get; set; } = default!;

    // Built-in LabelTagHelper uses the default order (0); run after it.
    public override int Order => 100;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (For.Metadata.IsRequired)
        {
            output.PostContent.AppendHtml(" <span class=\"required-star\" aria-hidden=\"true\">*</span>");
        }
    }
}
