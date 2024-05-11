# API Manager

## config
- See png file

<policies>
    <inbound>
        <base />
        <rewrite-uri id="rewrite-uri" template="@{

        var subUrl = context.Request.Url.Path.ToLower().Replace("/video", "");
        return $"{subUrl}";              
}" />
    </inbound>
</policies>

# Link direct to the machine in http
curl.exe http://74.249.130.23:8088/hls/fredbandband/master.m3u8
curl.exe http://74.249.130.23:8088/hls/fredbandband/fredbandband-0.m3u8

# Links to the API Manager in https

## Ravnur Player
https://strmsdemo.z13.web.core.windows.net?url=https://faiwebapiapimanagementservices.azure-api.net/video/hls/fredbandband/master.m3u8

curl.exe https://faiwebapiapimanagementservices.azure-api.net/video/hls/fredbandband/master.m3u8
curl.exe https://faiwebapiapimanagementservices.azure-api.net/video/hls/fredbandband/fredbandband-0.m3u8

curl --output ts.ts http://74.249.130.23:8088/hls/fredbandband/fredbandband-0/data00.ts
curl --output ts.ts https://faiwebapiapimanagementservices.azure-api.net/video/hls/fredbandband/fredbandband-0/data00.ts

