﻿#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using static Newtonsoft.Json.JsonConvert;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Cache;

[Collection(CacheServiceTests)]
public class ReleaseCacheServiceTests : CacheServiceTestFixture
{
    private const string PublicationSlug = "publication-slug";
    private const string ReleaseSlug = "2022";
    private static readonly Guid ReleaseId = Guid.NewGuid();

    private readonly ReleaseCacheViewModel _releaseViewModel = new(ReleaseId)
    {
        NextReleaseDate = new PartialDate(),
        Published = DateTime.UtcNow,
        Updates = new List<ReleaseNoteViewModel>
        {
            new()
            {
                Id = Guid.NewGuid(),
                On = DateTime.UtcNow,
            }
        },
        Content = new List<ContentSectionViewModel>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Order = 1,
                Content = new List<IContentBlockViewModel>
                {
                    new HtmlBlockViewModel
                    {
                        Id = Guid.NewGuid()
                    },
                    new MarkDownBlockViewModel
                    {
                        Id = Guid.NewGuid()
                    },
                    new DataBlockViewModel
                    {
                        Id = Guid.NewGuid(),
                        Charts = new List<IChart>
                        {
                            new LineChart()
                        }
                    }
                }
            }
        },
        SummarySection = ContentSectionWithHtmlBlock(),
        HeadlinesSection = ContentSectionWithHtmlBlock(),
        KeyStatisticsSection = ContentSectionWithHtmlBlock(),
        KeyStatisticsSecondarySection = ContentSectionWithHtmlBlock(),
        RelatedDashboardsSection = ContentSectionWithHtmlBlock(),
        DownloadFiles = new List<FileInfo>
        {
            new()
            {
                Id = Guid.NewGuid()
            }
        },
        DataLastPublished = DateTime.UtcNow,
        Type = ReleaseType.NationalStatistics,
        RelatedInformation = new List<LinkViewModel>
        {
            new()
            {
                Id = Guid.NewGuid()
            }
        }
    };

    [Fact]
    public async Task GetRelease_NoCachedRelease()
    {
        var cacheKey = new ReleaseCacheKey(PublicationSlug, ReleaseSlug);

        PublicBlobCacheService
            .Setup(s => s.GetItem(cacheKey, typeof(ReleaseCacheViewModel)))
            .ReturnsAsync(null);

        PublicBlobCacheService
            .Setup(s => s.SetItem<object>(cacheKey, _releaseViewModel))
            .Returns(Task.CompletedTask);

        var releaseService = new Mock<IReleaseService>(Strict);

        releaseService.Setup(s => s.GetRelease(PublicationSlug, ReleaseSlug))
            .ReturnsAsync(_releaseViewModel);

        var service = BuildService(releaseService: releaseService.Object);

        var result = await service.GetRelease(PublicationSlug, ReleaseSlug);

        VerifyAllMocks(releaseService, PublicBlobCacheService);

        result.AssertRight(_releaseViewModel);
    }

    [Fact]
    public async Task GetRelease_CachedRelease()
    {
        var cacheKey = new ReleaseCacheKey(PublicationSlug, ReleaseSlug);

        PublicBlobCacheService
            .Setup(s => s.GetItem(cacheKey, typeof(ReleaseCacheViewModel)))
            .ReturnsAsync(_releaseViewModel);

        var service = BuildService();

        var result = await service.GetRelease(PublicationSlug, ReleaseSlug);

        VerifyAllMocks(PublicBlobCacheService);

        result.AssertRight(_releaseViewModel);
    }

    [Fact]
    public async Task GetRelease_ReleaseNotFound()
    {
        var cacheKey = new ReleaseCacheKey(PublicationSlug, ReleaseSlug);

        PublicBlobCacheService
            .Setup(s => s.GetItem(cacheKey, typeof(ReleaseCacheViewModel)))
            .ReturnsAsync(null);

        var releaseService = new Mock<IReleaseService>(Strict);

        releaseService.Setup(s => s.GetRelease(PublicationSlug, ReleaseSlug))
            .ReturnsAsync(new NotFoundResult());

        var service = BuildService(releaseService: releaseService.Object);

        var result = await service.GetRelease(PublicationSlug, ReleaseSlug);

        VerifyAllMocks(releaseService, PublicBlobCacheService);

        result.AssertNotFound();
    }

    [Fact]
    public async Task GetRelease_LatestRelease_NoCachedRelease()
    {
        var cacheKey = new ReleaseCacheKey(PublicationSlug);

        PublicBlobCacheService
            .Setup(s => s.GetItem(cacheKey, typeof(ReleaseCacheViewModel)))
            .ReturnsAsync(null);

        PublicBlobCacheService
            .Setup(s => s.SetItem<object>(cacheKey, _releaseViewModel))
            .Returns(Task.CompletedTask);

        var releaseService = new Mock<IReleaseService>(Strict);

        releaseService.Setup(s => s.GetRelease(PublicationSlug, null))
            .ReturnsAsync(_releaseViewModel);

        var service = BuildService(releaseService: releaseService.Object);

        var result = await service.GetRelease(PublicationSlug);

        VerifyAllMocks(releaseService, PublicBlobCacheService);

        result.AssertRight(_releaseViewModel);
    }

    [Fact]
    public async Task GetRelease_LatestRelease_CachedRelease()
    {
        var cacheKey = new ReleaseCacheKey(PublicationSlug);

        PublicBlobCacheService
            .Setup(s => s.GetItem(cacheKey, typeof(ReleaseCacheViewModel)))
            .ReturnsAsync(_releaseViewModel);

        var service = BuildService();

        var result = await service.GetRelease(PublicationSlug);

        VerifyAllMocks(PublicBlobCacheService);

        result.AssertRight(_releaseViewModel);
    }

    [Fact]
    public async Task GetRelease_LatestRelease_ReleaseNotFound()
    {
        var cacheKey = new ReleaseCacheKey(PublicationSlug);

        PublicBlobCacheService
            .Setup(s => s.GetItem(cacheKey, typeof(ReleaseCacheViewModel)))
            .ReturnsAsync(null);

        var releaseService = new Mock<IReleaseService>(Strict);

        releaseService.Setup(s => s.GetRelease(PublicationSlug, null))
            .ReturnsAsync(new NotFoundResult());

        var service = BuildService(releaseService: releaseService.Object);

        var result = await service.GetRelease(PublicationSlug);

        VerifyAllMocks(releaseService, PublicBlobCacheService);

        result.AssertNotFound();
    }

    [Fact]
    public async Task UpdateRelease()
    {
        const bool staging = false;
        var published = DateTime.UtcNow;
        var cacheKey = new ReleaseCacheKey(staging, PublicationSlug, ReleaseSlug);

        var releaseService = new Mock<IReleaseService>(Strict);

        releaseService
            .Setup(s => s.GetRelease(ReleaseId, published))
            .ReturnsAsync(_releaseViewModel);

        PublicBlobCacheService
            .Setup(s => s.SetItem<object>(cacheKey, _releaseViewModel))
            .Returns(Task.CompletedTask);

        var service = BuildService(releaseService: releaseService.Object);

        var result = await service.UpdateRelease(staging,
            published,
            ReleaseId,
            PublicationSlug,
            ReleaseSlug);

        // There should be no attempt on the cache service to get the cached resource

        VerifyAllMocks(releaseService, PublicBlobCacheService);

        result.AssertRight(_releaseViewModel);
    }

    [Fact]
    public async Task UpdateRelease_LatestRelease()
    {
        const bool staging = false;
        var published = DateTime.UtcNow;
        var cacheKey = new ReleaseCacheKey(staging, PublicationSlug);

        var releaseService = new Mock<IReleaseService>(Strict);

        releaseService
            .Setup(s => s.GetRelease(ReleaseId, published))
            .ReturnsAsync(_releaseViewModel);

        PublicBlobCacheService
            .Setup(s => s.SetItem<object>(cacheKey, _releaseViewModel))
            .Returns(Task.CompletedTask);

        var service = BuildService(releaseService: releaseService.Object);

        var result = await service.UpdateRelease(staging,
            published,
            ReleaseId,
            PublicationSlug);

        // There should be no attempt on the cache service to get the cached resource

        VerifyAllMocks(releaseService, PublicBlobCacheService);

        result.AssertRight(_releaseViewModel);
    }

    [Fact]
    public void ReleaseCacheViewModel_SerializeAndDeserialize()
    {
        var converted = DeserializeObject<ReleaseCacheViewModel>(SerializeObject(_releaseViewModel));
        converted.AssertDeepEqualTo(_releaseViewModel);
    }

    private static ContentSectionViewModel ContentSectionWithHtmlBlock()
    {
        return new ContentSectionViewModel
        {
            Id = Guid.NewGuid(),
            Order = 1,
            Content = new List<IContentBlockViewModel>
            {
                new HtmlBlockViewModel
                {
                    Id = Guid.NewGuid()
                }
            }
        };
    }

    private static ReleaseCacheService BuildService(
        IReleaseService? releaseService = null
    )
    {
        return new ReleaseCacheService(
            releaseService: releaseService ?? Mock.Of<IReleaseService>(Strict));
    }
}
