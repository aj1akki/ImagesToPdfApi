using ImagesToPdfApi.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImagesToPdfApi.Repository
{
    public class FileRepository : IFileRepository
    {
        private readonly ILogger<FileRepository> _logger;

        public FileRepository(ILogger<FileRepository> logger)
        {
            _logger = logger;
        }

        public async Task<byte[]> ConvertImagesToPdfAsync(IFormFileCollection files)
        {
            try
            {
                if (files == null || files.Count == 0)
                {
                    _logger.LogError("No files provided for PDF conversion.");
                    return Array.Empty<byte>();
                }

                var pdfDoc = new PdfDocument();
                var defaultPageSize = new XSize(595, 842);

                foreach (var file in files)
                {
                    try
                    {
                        var imageFormat = GetImageFormat(file.FileName);

                        if (IsSupportedImageFormat(imageFormat))
                        {
                            var imageBytes = await ReadFileAsByteArrayAsync(file);
                            await AddImageToPdfAsync(pdfDoc, imageBytes, defaultPageSize);
                        }
                        else
                        {
                            _logger.LogWarning($"Unsupported file format: {file.FileName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing file: {FileName}", file.FileName);
                    }
                }

                using (var ms = new MemoryStream())
                {
                    pdfDoc.Save(ms, false);
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting images to PDF");
                throw;
            }
        }

        private async Task AddImageToPdfAsync(PdfDocument pdfDoc, byte[] imageBytes, XSize defaultPageSize)
        {
            try
            {
                var img = XImage.FromStream(() => new MemoryStream(imageBytes));
                var page = pdfDoc.AddPage();
                var gfx = XGraphics.FromPdfPage(page);

                // Calculate the aspect ratio of the original image
                double aspectRatio = (double)img.PixelWidth / img.PixelHeight;

                // Calculate dimensions to fit within the specified page size
                double maxWidth = defaultPageSize.Width;
                double maxHeight = defaultPageSize.Height;
                double width = maxWidth;
                double height = maxWidth / aspectRatio;

                if (height > maxHeight)
                {
                    height = maxHeight;
                    width = maxHeight * aspectRatio;
                }

                // Calculate position to center the image on the page
                double x = (maxWidth - width) / 2;
                double y = (maxHeight - height) / 2;

                // Draw the image on the PDF page
                gfx.DrawImage(img, x, y, width, height);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding image to PDF");
                throw;
            }
        }


        private Task<byte[]> ReadFileAsByteArrayAsync(IFormFile file)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    return Task.FromResult(ms.ToArray());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading file as byte array: {FileName}", file.FileName);
                throw;
            }
        }

        private string GetImageFormat(string fileName)
        {
            try
            {
                var ext = fileName.Split('.').LastOrDefault()?.ToLower();
                switch (ext)
                {
                    case "png":
                    case "jpeg":
                    case "jpg":
                    case "gif":
                        return ext!;
                    default:
                        return "default"; // Provide a default value or handle null case appropriately
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting image format for file: {FileName}", fileName);
                throw;
            }
        }

        private bool IsSupportedImageFormat(string imageFormat)
        {
            try
            {
                return !string.IsNullOrEmpty(imageFormat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking supported image format");
                throw;
            }
        }
    }
}
