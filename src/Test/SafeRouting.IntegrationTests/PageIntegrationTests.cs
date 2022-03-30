using Xunit;

namespace SafeRouting.IntegrationTests;

public sealed class PageIntegrationTests
{
  [Fact]
  public void AreasAreIncludedInRoute()
  {
    var route = Routes.Areas.Blog.Pages.Index.Get();

    Assert.Equal("/Index", route.PageName);
    Assert.Null(route.HandlerName);
    Assert.Collection(route.RouteValues, x =>
    {
      Assert.Equal("area", x.Key);
      Assert.Equal("Blog", x.Value);
    });
  }

  [Fact]
  public void ProductsEditGetRouteHasExpectedValues()
  {
    var route = Routes.Pages.Products_Edit.Get();

    Assert.Equal("/Products/Edit", route.PageName);
    Assert.Null(route.HandlerName);
    Assert.Collection(route.RouteValues, x =>
    {
      Assert.Equal("area", x.Key);
      Assert.Equal("", x.Value);
    });
  }

  [Fact]
  public void ProductsEditPostXRouteHasExpectedValues()
  {
    var route = Routes.Pages.Products_Edit.PostX();

    Assert.Equal("/Products/Edit", route.PageName);
    Assert.Equal("X", route.HandlerName);
    Assert.Collection(route.RouteValues, x =>
    {
      Assert.Equal("area", x.Key);
      Assert.Equal("", x.Value);
    });
  }
}
