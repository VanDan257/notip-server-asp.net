using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using notip_server.Interfaces;
using notip_server.Utils;

namespace notip_server.Service
{
    public class CommonService : ICommonService
    {
        private readonly IAmazonS3 _s3Client;

        public CommonService(IAmazonS3 s3Client)
        {
            _s3Client = s3Client;
        }

        public async Task UploadBlobFile(IFormFile file, string filePath)
        {
            try
            {
                if (file == null || file.Length == 0)
                throw new Exception("No file uploaded.");

                var uploadRequest = new PutObjectRequest
                {
                    InputStream = file.OpenReadStream(),
                    BucketName = "NotipCloud",
                    Key = "/" + filePath + "/" + file.FileName,
                    ContentType = file.ContentType
                };

                await _s3Client.PutObjectAsync(uploadRequest);
            }
            catch (Exception ex)
            {
                throw new Exception("Có lỗi xảy ra! Hãy thử lại!");
            }
        }
    }
}