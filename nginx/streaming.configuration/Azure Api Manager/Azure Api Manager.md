# API Manager

## config
- See png file

<policies>
    <inbound>
        <base />
        <rewrite-uri id="rewrite-uri" template="@{

        var subUrl = context.Request.Url.Path.Replace("/video", "");
        return $"{subUrl}";              
}" />
    </inbound>
</policies>

# Link direct to the machine in http
curl.exe http://74.249.130.23:8088/hls/fredbandband/master.m3u8
curl.exe http://74.249.130.23:8088/hls/fredbandband/fredbandband-0.m3u8

# Links to the API Manager in https

## Ravnur Player fredbandband
https://strmsdemo.z13.web.core.windows.net?url=https://faiwebapiapimanagementservices.azure-api.net/video/hls/fredbandband/master.m3u8
https://strmsdemo.z13.web.core.windows.net?url=https://faiwebapiapimanagementservices.azure-api.net/video/hls/AndYourBirdCanSing/master.m3u8

curl.exe https://faiwebapiapimanagementservices.azure-api.net/video/hls/fredbandband/master.m3u8
curl.exe https://faiwebapiapimanagementservices.azure-api.net/video/hls/fredbandband/fredbandband-0.m3u8

curl.exe https://faiwebapiapimanagementservices.azure-api.net/video/hls/fredTrio/master.m3u8
curl.exe https://faiwebapiapimanagementservices.azure-api.net/video/hls/fredTrio/fredTrio-0.m3u8

curl.exe https://faiwebapiapimanagementservices.azure-api.net/video/hls/AndYourBirdCanSing/master.m3u8
curl.exe https://faiwebapiapimanagementservices.azure-api.net/video/hls/AndYourBirdCanSing/AndYourBirdCanSing-0.m3u8

curl --output ts.ts http://74.249.130.23:8088/hls/fredbandband/fredbandband-0/data00.ts
curl --output ts.ts https://faiwebapiapimanagementservices.azure-api.net/video/hls/fredbandband/fredbandband-0/data00.ts


## Ravnur Player AndYourBirdCanSing

curl.exe http://74.249.130.23:8088/hls/AndYourBirdCanSing/master.m3u8
curl.exe http://74.249.130.23:8088/hls/AndYourBirdCanSing/AndYourBirdCanSing-0.m3u8

curl.exe https://faiwebapiapimanagementservices.azure-api.net/video/hls/fredbandband/master.m3u8
curl.exe https://faiwebapiapimanagementservices.azure-api.net/video/hls/AndYourBirdCanSing/master.m3u8

curl.exe https://faiwebapiapimanagementservices.azure-api.net/video/hls/fredbandband/fredbandband-0.m3u8


curl --output ts.ts http://74.249.130.23:8088/hls/fredbandband/fredbandband-0/data00.ts
curl --output ts.ts https://faiwebapiapimanagementservices.azure-api.net/video/hls/fredbandband/fredbandband-0/data00.ts




Upload HLS Streaming contents to Azure Blob Storage and request CDN pre-load with Azure Functions
( https://github.com/ilseokoh/azurehlsupload)