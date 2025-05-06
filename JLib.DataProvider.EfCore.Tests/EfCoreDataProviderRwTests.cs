using FluentAssertions;
using JLib.DataProvider.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace JLib.DataProvider.EfCore.Tests;

public class EfCoreDataProviderRwTests(ITestOutputHelper toh) : EfCoreDataProviderRwTestBase(toh)
{
    protected override IServiceCollection ModifyServices(IServiceCollection services)
    {
        base.ModifyServices(services);
        return services.AddDataAuthorization(TypeCache);
    }


    [Fact]
    public void ReturnAuthorized()
        => DataProvider.Get().Should().HaveCount(2).And.AllSatisfy(x =>
        {
            if (!x.IsAuthorized)
                throw new(x.Name);
        });


    [Fact]
    public void RejectOnGet_Single()
        => ((Action)(() => DataProvider.Get(MockData.Unauthorized1)))
            .Should().Throw<DataProviderException.RuntimeException.DataObjectAccessFailedException>()
            .Which.Reason.Should().Be(DataProviderException.RuntimeException.DataObjectAccessFailedException.FailureReason.Unknown);

    [Fact]
    public void RejectOnGet_Ranged()
        => ((Action)(() => DataProvider.Get([MockData.Unauthorized1])))
            .Should().Throw<DataProviderException.RuntimeException.DataObjectAccessFailedException>()
            .Which.Reason.Should().Be(DataProviderException.RuntimeException.DataObjectAccessFailedException.FailureReason.Unknown);

    [Fact]
    public void RejectOnGet_MixedRanged()
        => ((Action)(() => DataProvider.Get([MockData.Unauthorized1, MockData.Authorized1])))
            .Should().Throw<DataProviderException.RuntimeException.DataObjectAccessFailedException>()
            .Which.Reason.Should().Be(DataProviderException.RuntimeException.DataObjectAccessFailedException.FailureReason.Unknown);

    [Fact]
    public void RejectOnAdd_Single()
        => ((Action)(() => DataProvider.Add(new MockEntity
            {
                Id = Guid.NewGuid(),
                Name = "test",
                IsAuthorized = false
            })))
            .Should().Throw<DataProviderException.RuntimeException.DataObjectAccessFailedException>()
            .Which.Reason.Should().Be(DataProviderException.RuntimeException.DataObjectAccessFailedException.FailureReason.AccessDenied);

    [Fact]
    public void RejectOnAdd_Ranged()
        => ((Action)(() => DataProvider.Add([
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "unauthorized",
                    IsAuthorized = false
                }
            ])))
            .Should().Throw<DataProviderException.RuntimeException.DataObjectAccessFailedException>()
            .Which.Reason.Should().Be(DataProviderException.RuntimeException.DataObjectAccessFailedException.FailureReason.AccessDenied);

    [Fact]
    public void RejectOnAdd_MixedRanged()
        => ((Action)(() => DataProvider.Add([
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "unauthorized_a",
                    IsAuthorized = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "unauthorized_b",
                    IsAuthorized = true
                }
            ])))
            .Should().Throw<DataProviderException.RuntimeException.DataObjectAccessFailedException>()
            .Which.Reason.Should().Be(DataProviderException.RuntimeException.DataObjectAccessFailedException.FailureReason.AccessDenied);

    [Fact]
    public void SuccessOnAdd_Single()
        => DataProvider.Add(new MockEntity
        {
            Id = Guid.NewGuid(),
            Name = "authorized",
            IsAuthorized = true
        });

    [Fact]
    public void SuccessOnAdd_Ranged()
        => DataProvider.Add([
            new()
            {
                Id = Guid.NewGuid(),
                Name = "authorized",
                IsAuthorized = true
            }
        ]);

    [Fact]
    public void SuccessOnRemove()
        => DataProvider.Add([
            new()
            {
                Id = Guid.NewGuid(),
                Name = "authorized",
                IsAuthorized = true
            }
        ]);

    [Fact]
    public void SuccessOnRemove_Authorized()
        => DataProvider.Remove(MockData.Authorized1);

    [Fact]
    public void RejectOnRemove_Id()
        => ((Action)(() => DataProvider.Remove(MockData.Unauthorized1))).Should()
            .Throw<DataProviderException.RuntimeException.DataObjectAccessFailedException>();

    [Fact]
    public void RejectOnRemove_Reference_Full()
    {
        var entity = DbContext.Set<MockEntity>().Single(x => x.Id == MockData.Unauthorized1);
        ((Action)(() => DataProvider.Remove(entity))).Should().Throw<DataProviderException.RuntimeException.DataObjectAccessFailedException>().Which.Reason.Should().Be(DataProviderException.RuntimeException.DataObjectAccessFailedException.FailureReason.AccessDenied);
    }

    [Fact]
    public void RejectOnRemove_Reference_Incomplete()
    {
        var entity = new MockEntity
        {
            Id = MockData.Unauthorized1
        };
        ((Action)(() => { DataProvider.Remove(entity); })).Should().Throw<DataProviderException.RuntimeException.DataObjectAccessFailedException>().Which.Reason.Should().Be(DataProviderException.RuntimeException.DataObjectAccessFailedException.FailureReason.AccessDenied);
    }

    [Fact]
    public void RejectOnRemove_Reference_Range_Incomplete()
    {
        var entity = new MockEntity[]
        {
            new()
            {
                Id = MockData.Unauthorized1
            },
            new()
            {
                Id = MockData.Authorized1
            }
        };
        ((Action)(() => { DataProvider.Remove(entity); })).Should().Throw<DataProviderException.RuntimeException.DataObjectAccessFailedException>().Which.Reason.Should().Be(DataProviderException.RuntimeException.DataObjectAccessFailedException.FailureReason.AccessDenied);
    }
}