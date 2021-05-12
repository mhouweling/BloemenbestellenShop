﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Smartstore.Core.Content.Media;
using Smartstore.Core.Data;
using Smartstore.Core.Security;
using Smartstore.Web.Controllers;

namespace Smartstore.Admin.Controllers
{
    public class DownloadController : AdminControllerBase
    {
        private const string DOWNLOAD_TEMPLATE = "EditorTemplates/Download";

        private readonly SmartDbContext _db;
        private readonly IDownloadService _downloadService;
        private readonly IMediaService _mediaService;
        private readonly MediaSettings _mediaSettings;

        public DownloadController(SmartDbContext db, IDownloadService downloadService, IMediaService mediaService, MediaSettings mediaSettings)
        {
            _db = db;
            _downloadService = downloadService;
            _mediaService = mediaService;
            _mediaSettings = mediaSettings;
        }

        [Permission(Permissions.Media.Download.Read)]
        public async Task<IActionResult> DownloadFile(int downloadId)
        {
            var download = await _db.Downloads
                .Include(x => x.MediaFile)
                .FindByIdAsync(downloadId, false);

            if (download == null)
            {
                return Content(T("Common.Download.NoDataAvailable"));
            }   

            if (download.UseDownloadUrl)
            {
                return new RedirectResult(download.DownloadUrl);
            }
            else
            {
                // Use stored data
                var data = await _downloadService.OpenDownloadStreamAsync(download);

                if (data == null || data.Length == 0)
                {
                    return Content(T("Common.Download.NoDataAvailable"));
                }      

                var fileName = download.MediaFile.Name;
                var contentType = download.MediaFile.MimeType;

                return new FileStreamResult(data, contentType)
                {
                    FileDownloadName = fileName
                };
            }
        }

        /// <summary>
        /// TODO: (mh) (core) Add documentation.
        /// </summary>
        [HttpPost]
        [Permission(Permissions.Media.Download.Create)]
        public async Task<IActionResult> SaveDownloadUrl(string downloadUrl, bool minimalMode = false, string fieldName = null, int entityId = 0, string entityName = "")
        {
            var download = new Download
            {
                EntityId = entityId,
                EntityName = entityName,
                DownloadGuid = Guid.NewGuid(),
                UseDownloadUrl = true,
                DownloadUrl = downloadUrl,
                IsTransient = true,
                UpdatedOnUtc = DateTime.UtcNow
            };

            _db.Downloads.Add(download);
            await _db.SaveChangesAsync();

            return Json(new
            {
                success = true,
                downloadId = download.Id,
                // TODO: (core) Make new overload for this
                html = this.InvokeViewAsync(DOWNLOAD_TEMPLATE, download.Id, new { minimalMode, fieldName })
            });
        }

        [HttpPost]
        [Permission(Permissions.Media.Download.Create)]
        public async Task<IActionResult> CreateDownloadFromMediaFile(int mediaFileId, int entityId = 0, string entityName = "")
        {
            var download = new Download
            {
                EntityId = entityId,
                EntityName = entityName,
                MediaFileId = mediaFileId,
                DownloadGuid = Guid.NewGuid(),
                UseDownloadUrl = false,
                IsTransient = true,
                UpdatedOnUtc = DateTime.UtcNow
            };

            _db.Downloads.Add(download);
            await _db.SaveChangesAsync();

            return Json(new
            {
                success = true,
                downloadId = download.Id,
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Media.Download.Create)]
        public async Task<IActionResult> AsyncUpload(string clientCtrlId)
        {
            var postedFile = Request.Form.Files["file[0]"];
            if (postedFile == null)
            {
                throw new ArgumentException(T("Common.NoFileUploaded"));
            }

            var path = _mediaService.CombinePaths(SystemAlbumProvider.Downloads, postedFile.FileName);
            // TODO: (mh) (core) Check if .NET core disposes the stream on request end.
            using var stream = postedFile.OpenReadStream();
            var file = await _mediaService.SaveFileAsync(path, stream, dupeFileHandling: DuplicateFileHandling.Rename);

            return Json(new
            {
                success = true,
                clientCtrlId,
                id = file.Id,
                name = file.Name,
                type = file.MediaType,
                thumbUrl = await _mediaService.GetUrlAsync(file.Id, _mediaSettings.ProductThumbPictureSize, host: string.Empty)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Media.Download.Update)]
        public async Task<IActionResult> AddChangelog(int downloadId, string changelogText)
        {
            var success = false;
            var download = await _db.Downloads.FindByIdAsync(downloadId);

            if (download != null)
            {
                download.Changelog = changelogText;
                await _db.SaveChangesAsync();
                success = true;
            }

            return Json(new { success });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Media.Download.Read)]
        public async Task<IActionResult> GetChangelogText(int downloadId)
        {
            var success = false;
            var changeLogText = string.Empty;

            var download = await _db.Downloads.FindByIdAsync(downloadId, false);

            if (download != null)
            {
                changeLogText = download.Changelog;
                success = true;
            }

            return Json(new
            {
                success,
                changelog = changeLogText
            });
        }

        /// <summary>
        /// TODO: (mh) (core) Add documentation.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Media.Download.Delete)]
        public async Task<IActionResult> DeleteDownload(bool minimalMode = false, string fieldName = null)
        {
            // We don't actually delete here. We just return the editor in it's init state.
            // So the download entity can be set to transient state and deleted later by a scheduled task.
            return Json(new
            {
                success = true,
                html = await this.InvokeViewAsync(DOWNLOAD_TEMPLATE, null, new { minimalMode, fieldName })
            });
        }
    }
}