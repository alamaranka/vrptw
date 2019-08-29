using Amazon;
using Amazon.S3;
using System;
using System.Collections.Generic;
using System.Text;

namespace VRPTW.Data
{
    public class AWSManager
    {
        private string _accessKey = "";
        private string _secretKey = "";

        public AWSManager()
        {
            CreateClient();
        }

        private void CreateClient()
        {
            AmazonS3Client client = new AmazonS3Client(_accessKey, _secretKey, RegionEndpoint.USEast2);
        }
    }
}