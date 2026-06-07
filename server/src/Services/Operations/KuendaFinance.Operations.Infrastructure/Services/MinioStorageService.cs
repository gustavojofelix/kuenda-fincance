using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using KuendaFinance.Operations.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace KuendaFinance.Operations.Infrastructure.Services;

public class MinioStorageService : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _publicEndpointUrl;

    public MinioStorageService(IConfiguration configuration)
    {
        var endpoint = configuration["Minio:Endpoint"] ?? "http://localhost:9000";
        var accessKey = configuration["Minio:AccessKey"] ?? "kuenda";
        var secretKey = configuration["Minio:SecretKey"] ?? "kuendapassword";
        
        _bucketName = configuration["Minio:BucketName"] ?? "guarantees";
        _publicEndpointUrl = configuration["Minio:PublicEndpointUrl"] ?? "http://localhost:9000";

        var config = new AmazonS3Config
        {
            ServiceURL = endpoint,
            ForcePathStyle = true // Required for MinIO
        };

        _s3Client = new AmazonS3Client(accessKey, secretKey, config);
    }

    public async Task<string> UploadFileAsync(Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        // Ensure bucket exists
        var bucketExists = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, _bucketName);
        if (!bucketExists)
        {
            await _s3Client.PutBucketAsync(new PutBucketRequest { BucketName = _bucketName }, cancellationToken);
            
            // Set bucket policy to public read for development/minio purposes
            var publicPolicy = $@"{{
                ""Version"": ""2012-10-17"",
                ""Statement"": [
                    {{
                        ""Sid"": ""PublicRead"",
                        ""Effect"": ""Allow"",
                        ""Principal"": ""*"",
                        ""Action"": [""s3:GetObject""],
                        ""Resource"": [""arn:aws:s3:::{_bucketName}/*""]
                    }}
                ]
            }}";
            await _s3Client.PutBucketPolicyAsync(new PutBucketPolicyRequest
            {
                BucketName = _bucketName,
                Policy = publicPolicy
            }, cancellationToken);
        }

        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var putRequest = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = uniqueFileName,
            InputStream = stream,
            ContentType = contentType
        };

        await _s3Client.PutObjectAsync(putRequest, cancellationToken);

        // Return the public URL to access the uploaded file
        return $"{_publicEndpointUrl.TrimEnd('/')}/{_bucketName}/{uniqueFileName}";
    }
}
