using PlatformTemplate.ServiceName.Domain.Models;

namespace PlatformTemplate.ServiceName.UnitTests;

public sealed class ServiceNameItemTests
{
    [Fact]
    public void Create_TrimsNameAndInitializesIdentity()
    {
        var item = ServiceNameItem.Create("  sample  ");

        Assert.NotEqual(Guid.Empty, item.Id);
        Assert.Equal("sample", item.Name);
    }

    [Fact]
    public void Create_RejectsEmptyName()
    {
        Assert.Throws<ArgumentException>(() => ServiceNameItem.Create("  "));
    }
}
