﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;

namespace Squidex.Infrastructure.UsageTracking
{
    public class CachingUsageTrackerTests
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly CancellationToken ct;
        private readonly MemoryCache cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        private readonly string key = Guid.NewGuid().ToString();
        private readonly string category = Guid.NewGuid().ToString();
        private readonly DateTime date = DateTime.Today;
        private readonly IUsageTracker inner = A.Fake<IUsageTracker>();
        private readonly IUsageTracker sut;

        public CachingUsageTrackerTests()
        {
            ct = cts.Token;

            sut = new CachingUsageTracker(inner, cache);
        }

        [Fact]
        public void Should_forward_fallback_category()
        {
            A.CallTo(() => inner.FallbackCategory)
                .Returns("*");

            Assert.Equal("*", sut.FallbackCategory);
        }

        [Fact]
        public async Task Should_forward_delete_prefix_call()
        {
            await sut.DeleteByKeyPatternAsync("pattern", ct);

            A.CallTo(() => inner.DeleteByKeyPatternAsync("pattern", ct))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_forward_delete_call()
        {
            await sut.DeleteAsync(key, ct);

            A.CallTo(() => inner.DeleteAsync(key, ct))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_forward_track_call()
        {
            var counters = new Counters();

            await sut.TrackAsync(date, key, "my-category", counters, ct);

            A.CallTo(() => inner.TrackAsync(date, key, "my-category", counters, ct))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_forward_query_call()
        {
            var dateFrom = date;
            var dateTo = dateFrom.AddDays(10);

            await sut.QueryAsync(key, dateFrom, dateTo, ct);

            A.CallTo(() => inner.QueryAsync(key, dateFrom, dateTo, ct))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_cache_monthly_usage()
        {
            var counters = new Counters();

            A.CallTo(() => inner.GetForMonthAsync(key, date, category, ct))
                .Returns(counters);

            var actual1 = await sut.GetForMonthAsync(key, date, category, ct);
            var actual2 = await sut.GetForMonthAsync(key, date, category, ct);

            Assert.Same(counters, actual1);
            Assert.Same(counters, actual2);

            A.CallTo(() => inner.GetForMonthAsync(key, DateTime.Today, category, ct))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_cache_days_usage()
        {
            var counters = new Counters();

            var dateFrom = date;
            var dateTo = dateFrom.AddDays(10);

            A.CallTo(() => inner.GetAsync(key, dateFrom, dateTo, category, ct))
                .Returns(counters);

            var actual1 = await sut.GetAsync(key, dateFrom, dateTo, category, ct);
            var actual2 = await sut.GetAsync(key, dateFrom, dateTo, category, ct);

            Assert.Same(counters, actual1);
            Assert.Same(counters, actual2);

            A.CallTo(() => inner.GetAsync(key, dateFrom, dateTo, category, ct))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_not_cache_queries()
        {
            var dateFrom = date;
            var dateTo = dateFrom.AddDays(10);

            var actual1 = await sut.QueryAsync(key, dateFrom, dateTo, ct);
            var actual2 = await sut.QueryAsync(key, dateFrom, dateTo, ct);

            Assert.NotSame(actual2, actual1);

            A.CallTo(() => inner.QueryAsync(key, dateFrom, dateTo, ct))
                .MustHaveHappenedTwiceOrMore();
        }
    }
}
