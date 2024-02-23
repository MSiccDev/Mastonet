﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Mastonet.Entities;
using Xunit;

namespace Mastonet.Tests
{
    public class PostTests : MastodonClientTests
    {
        [Fact]
        public async Task UploadMedia()
        {
            var client = GetTestClient();

            var bytes = await File.ReadAllBytesAsync($"./testimage.png");
            var attachment = await client.UploadMedia(bytes, "testimage.png");
            
            Assert.NotNull(attachment);
            Assert.NotNull(attachment.PreviewUrl);
            Assert.NotNull(attachment.Url);

            var status = await client.PublishStatus("Status with image", Visibility.Private,
                mediaIds: new string[] { attachment.Id });
            status = await client.GetStatus(status.Id);

            Assert.NotNull(status.MediaAttachments);
            Assert.True(status.MediaAttachments.Any());
            Assert.Equal(attachment.Url, status.MediaAttachments.First().Url);
        }

        [Fact]
        public async Task UploadMultipleMedia()
        {
            var client = GetTestClient();

            var attachments = new List<Attachment>();
            for (var i = 0; i < 4; i++)
            {
                var bytes = await File.ReadAllBytesAsync($"./testimage.png");
                var attachment = await client.UploadMedia(bytes, $"testimage-{i}.png");
                Assert.NotNull(attachment);
                Assert.NotNull(attachment.PreviewUrl);
                Assert.NotNull(attachment.Url);
                attachments.Add(attachment);
                ;
            }

            var status = await client.PublishStatus(
                status: "Status with multiple media attachments",
                visibility: Visibility.Private,
                mediaIds: attachments.Select(a => a.Id));

            status = await client.GetStatus(status.Id);

            Assert.NotNull(status.MediaAttachments);
            Assert.True(status.MediaAttachments.Count() == 4);
            foreach (var attachment in attachments)
            {
                Assert.Contains(status.MediaAttachments, a => a.Url.Equals(attachment.Url));
            }
        }

        [Fact]
        public async Task PostStatus()
        {
            var client = GetTestClient();
            var status = await client.PublishStatus("Yo1", Visibility.Unlisted);

            var client2 = GetPrivateClient();

            var statusFromApi = await client2.GetStatus(status.Id);

            Assert.Equal(status.Id, statusFromApi.Id);
            Assert.Equal(status.Content, statusFromApi.Content);
            Assert.Equal(Visibility.Unlisted, statusFromApi.Visibility);
        }

        [Fact]
        public async Task EditStatus()
        {
            var client = GetTestClient();
            var status = await client.PublishStatus("Initial status", Visibility.Unlisted);

            var client2 = GetPrivateClient();
            var statusFromApi = await client2.GetStatus(status.Id);

            Assert.Equal(status.Content, statusFromApi.Content);

            status = await client.EditStatus(statusFromApi.Id, "Edited status");
            var editedStatusFromApi = await client2.GetStatus(status.Id);

            Assert.Equal(status.Content, editedStatusFromApi.Content);
            Assert.NotEqual(statusFromApi.Content, editedStatusFromApi.Content);
        }

        [Fact]
        public async Task DeleteStatus()
        {
            var client = GetTestClient();
            var status = await client.PublishStatus("Yo1", Visibility.Public);
            var statusId = status.Id;

            status = await client.GetStatus(statusId);
            Assert.NotNull(status);

            await client.DeleteStatus(statusId);

            await Assert.ThrowsAsync<ServerErrorException>(async () => { status = await client.GetStatus(statusId); });
        }

        [Fact]
        public async Task ReblogUnreblog()
        {
            var testClient = GetTestClient();
            var status = await testClient.PublishStatus("Yo1", Visibility.Public);

            var client = GetPrivateClient();
            status = await client.GetStatus(status.Id);
            Assert.False(status.Reblogged);

            await client.Reblog(status.Id);
            status = await client.GetStatus(status.Id);
            Assert.True(status.Reblogged);


            await client.Unreblog(status.Id);
            status = await client.GetStatus(status.Id);
            Assert.False(status.Reblogged);
        }
    }
}