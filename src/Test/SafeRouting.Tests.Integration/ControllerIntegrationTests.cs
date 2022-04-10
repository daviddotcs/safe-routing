using Xunit;

namespace SafeRouting.Tests.Integration;

public sealed class ControllerIntegrationTests
{
  [Fact]
  public void AccountBlogAccountHasCorrectArea()
  {
    var route = Routes.Controllers.Account.BlogAccount();

    Assert.Equal("Account", route.ControllerName);
    Assert.Equal("BlogAccount", route.ActionName);
    Assert.Collection(route.RouteValues, x =>
    {
      Assert.Equal("area", x.Key);
      Assert.Equal("Blog", x.Value);
    });
  }

  [Theory]
  [InlineData("test")]
  [InlineData("foo")]
  public void AccountIndexRouteAcceptsProperty(string headerValue)
  {
    var route = Routes.Controllers.Account.Index();
    route[route.Properties.CustomHeader] = headerValue;

    Assert.Equal("Account", route.ControllerName);
    Assert.Equal("Index", route.ActionName);
    Assert.Collection(route.RouteValues, x =>
    {
      Assert.Equal("area", x.Key);
      Assert.Equal("", x.Value);
    }, x =>
    {
      Assert.Equal(route.Properties.CustomHeader.Name, x.Key);
      Assert.Equal(headerValue, x.Value);
    });
  }

  [Fact]
  public void AccountIndexRouteHasExpectedValues()
  {
    var route = Routes.Controllers.Account.Index();

    Assert.Equal("Account", route.ControllerName);
    Assert.Equal("Index", route.ActionName);
    Assert.Collection(route.RouteValues, x =>
    {
      Assert.Equal("area", x.Key);
      Assert.Equal("", x.Value);
    });
  }

  [Theory]
  [InlineData(5)]
  [InlineData(int.MinValue)]
  [InlineData(int.MaxValue)]
  public void AccountListRouteHasExpectedValues(int page)
  {
    var route = Routes.Controllers.Account.List(page);

    Assert.Equal("Account", route.ControllerName);
    Assert.Equal("List", route.ActionName);
    Assert.Collection(route.RouteValues, x =>
    {
      Assert.Equal("area", x.Key);
      Assert.Equal("", x.Value);
    }, x =>
    {
      Assert.Equal(route.Parameters.Page.Name, x.Key);
      Assert.Equal(page, x.Value);
    });
  }

  [Fact]
  public void AreasAreIncludedInRoute()
  {
    var route = Routes.Areas.Blog.Controllers.Post.Index();

    Assert.Equal("Post", route.ControllerName);
    Assert.Equal("Index", route.ActionName);
    Assert.Collection(route.RouteValues, x =>
    {
      Assert.Equal("area", x.Key);
      Assert.Equal("Blog", x.Value);
    });
  }

  [Fact]
  public void ParameterValuesCanBeChanged()
  {
    var page = 5;
    var route = Routes.Controllers.Account.List(page);

    Assert.Equal("Account", route.ControllerName);
    Assert.Equal("List", route.ActionName);
    Assert.Collection(route.RouteValues, x =>
    {
      Assert.Equal("area", x.Key);
      Assert.Equal("", x.Value);
    }, x =>
    {
      Assert.Equal(route.Parameters.Page.Name, x.Key);
      Assert.Equal(page, x.Value);
    });

    page = 10;
    route[route.Parameters.Page] = page;

    Assert.Equal("Account", route.ControllerName);
    Assert.Equal("List", route.ActionName);
    Assert.Collection(route.RouteValues, x =>
    {
      Assert.Equal("area", x.Key);
      Assert.Equal("", x.Value);
    }, x =>
    {
      Assert.Equal(route.Parameters.Page.Name, x.Key);
      Assert.Equal(page, x.Value);
    });
  }

  [Fact]
  public void ParameterValuesCanBeRemoved()
  {
    var page = 5;
    var route = Routes.Controllers.Account.List(page);

    Assert.Equal("Account", route.ControllerName);
    Assert.Equal("List", route.ActionName);
    Assert.Collection(route.RouteValues, x =>
    {
      Assert.Equal("area", x.Key);
      Assert.Equal("", x.Value);
    }, x =>
    {
      Assert.Equal(route.Parameters.Page.Name, x.Key);
      Assert.Equal(page, x.Value);
    });

    route.Remove(route.Parameters.Page);

    Assert.Equal("Account", route.ControllerName);
    Assert.Equal("List", route.ActionName);
    Assert.Collection(route.RouteValues, x =>
    {
      Assert.Equal("area", x.Key);
      Assert.Equal("", x.Value);
    });
  }
}
