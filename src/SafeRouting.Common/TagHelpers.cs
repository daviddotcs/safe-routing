﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SafeRouting.Extensions;

namespace SafeRouting.TagHelpers;

/// <summary>
/// <see cref="ITagHelper"/> implementation targeting <c>&lt;a&gt;</c>, <c>&lt;form&gt;</c>, or <c>&lt;img&gt;</c> elements.
/// </summary>
[HtmlTargetElement(AnchorTagName, Attributes = ForRouteAttributeName)]
[HtmlTargetElement(ButtonTagName, Attributes = ForRouteAttributeName)]
[HtmlTargetElement(FormTagName, Attributes = ForRouteAttributeName)]
[HtmlTargetElement(ImageTagName, Attributes = ForRouteAttributeName)]
public sealed class RouteValuesTagHelper : TagHelper
{
  /// <summary>
  /// The route values with which to assign the <c>href</c>, <c>action</c>, or <c>src</c> attribute.
  /// </summary>
  [HtmlAttributeName(ForRouteAttributeName)]
  public IRouteValues? ForRoute { get; set; }

  /// <summary>
  /// The current <see cref="Microsoft.AspNetCore.Mvc.Rendering.ViewContext"/>.
  /// </summary>
  [HtmlAttributeNotBound]
  [ViewContext]
  public ViewContext ViewContext { get; set; } = default!;

  /// <summary>
  /// Initialises a new instance of the <see cref="RouteValuesTagHelper"/> class.
  /// </summary>
  /// <param name="urlHelperFactory">A factory to be injected.</param>
  public RouteValuesTagHelper(IUrlHelperFactory urlHelperFactory)
  {
    this.urlHelperFactory = urlHelperFactory;
  }

  /// <inheritdoc/>
  public override void Process(TagHelperContext context, TagHelperOutput output)
  {
    var urlAttributeName = context.TagName switch
    {
      AnchorTagName => AnchorAttributeName,
      ButtonTagName => ButtonAttributeName,
      FormTagName => FormAttributeName,
      ImageTagName => ImageAttributeName,
      _ => null
    };

    if (urlAttributeName is null
      || ForRoute is null
      || ForRoute.Url(Url) is not { } url)
    {
      return;
    }

    output.Attributes.SetAttribute(urlAttributeName, url);
  }

  private IUrlHelper Url => urlHelperFactory.GetUrlHelper(ViewContext);

  private readonly IUrlHelperFactory urlHelperFactory;

  private const string ForRouteAttributeName = "for-route";
  private const string AnchorTagName = "a";
  private const string AnchorAttributeName = "href";
  private const string ButtonTagName = "button";
  private const string ButtonAttributeName = "formaction";
  private const string FormTagName = "form";
  private const string FormAttributeName = "action";
  private const string ImageTagName = "img";
  private const string ImageAttributeName = "src";
}
