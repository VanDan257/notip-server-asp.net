using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using notip_server.Interfaces;
using notip_server.Utils;

namespace notip_server.Service
{
    public class CommonService : ICommonService
    {
        private readonly string _bucketName = EnviConfig.BucketNameAwsS3;
        private readonly IAmazonS3 _s3Client;

        public CommonService(IAmazonS3 s3Client, string bucketName)
        {
            _s3Client = s3Client;
            _bucketName = bucketName;
        }

        public async Task UploadBlobFile(IFormFile file, string filePath)
        {
            if (file == null || file.Length == 0)
            throw new Exception("No file uploaded.");

            var uploadRequest = new PutObjectRequest
            {
                InputStream = file.OpenReadStream(),
                BucketName = _bucketName,
                Key = "/" + filePath + file.FileName,
                ContentType = file.ContentType
            };

            await _s3Client.PutObjectAsync(uploadRequest);
        }
    }
}