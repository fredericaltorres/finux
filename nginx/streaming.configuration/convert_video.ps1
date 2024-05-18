
$ffmpegexe = "C:\Brainshark\scripts\ffmpeg-4.2.1-win64-static\bin\ffmpeg.exe"

function CreateDirectory($dir) {

    if(-not(Test-Path $dir)) {
        mkdir $dir
    }
}

function ToHls($filename, $idname)
{
    echo "Converting $idname to hls"
    $dir = "c:\temp\stream\hls\$idname"
    CreateDirectory $dir
    cd $dir

    ffmpeg -i "$filename" -filter_complex "[0:v]split=3[v1][v2][v3]; [v1]copy[v1out]; [v2]scale=w=1280:h=720[v2out]; [v3]scale=w=640:h=360[v3out]" -map "[v1out]" -c:v:0 libx264 -x264-params "nal-hrd=cbr:force-cfr=1" -b:v:0 5M -maxrate:v:0 5M -minrate:v:0 5M -bufsize:v:0 10M -preset slow -g 48 -sc_threshold 0 -keyint_min 48 -map "[v2out]" -c:v:1 libx264 -x264-params "nal-hrd=cbr:force-cfr=1" -b:v:1 3M -maxrate:v:1 3M -minrate:v:1 3M -bufsize:v:1 3M -preset slow -g 48 -sc_threshold 0 -keyint_min 48 -map "[v3out]" -c:v:2 libx264 -x264-params "nal-hrd=cbr:force-cfr=1" -b:v:2 1M -maxrate:v:2 1M -minrate:v:2 1M -bufsize:v:2 1M -preset slow -g 48 -sc_threshold 0 -keyint_min 48 -map a:0 -c:a:0 aac -b:a:0 96k -ac 2 -map a:0 -c:a:1 aac -b:a:1 96k -ac 2 -map a:0 -c:a:2 aac -b:a:2 48k -ac 2 -f hls -hls_time 2 -hls_playlist_type vod -hls_flags independent_segments -hls_segment_type mpegts -hls_segment_filename "$idname-%v/data%02d.ts" -master_pl_name "master.m3u8" -var_stream_map "v:0,a:0 v:1,a:1 v:2,a:2" "$idname-%v.m3u8"

    echo "video streaming url http://[IP]:8088/hls/$idname/$idname-0.m3u8"
}

function ToHls1080p($filename, $idname) {
    
    $medium = "medium"
    # Resolution #0 supplied is 1920x1080 1080p 5Mb/s
    # Resolution #1 960x540 1080p/2 3 Mb/s

    echo "---------------------------------------------"
    echo "Converting $idname to hls 1080p"
    echo "---------------------------------------------"
    
    $dir = "c:\temp\stream\hls\$idname"
    CreateDirectory $dir
    cd $dir

    & $ffmpegexe -i "$filename" `
    -filter_complex "[0:v]split=2[v1][v2]; [v1]copy[v1out]; [v2]scale=w=960:h=540[v2out]" `
    -map "[v1out]" -c:v:0 libx264 -x264-params "nal-hrd=cbr:force-cfr=1" -b:v:0 5M -maxrate:v:0 5M -minrate:v:0 5M -bufsize:v:0 5M  -preset "$medium" -g 48 -sc_threshold 0 -keyint_min 48 `
    -map "[v2out]" -c:v:1 libx264 -x264-params "nal-hrd=cbr:force-cfr=1" -b:v:1 3M -maxrate:v:1 3M -minrate:v:1 3M -bufsize:v:1 3M  -preset "$medium" -g 48 -sc_threshold 0 -keyint_min 48 `
    -map a:0 -c:a:0 aac -b:a:0 128k -ac 2 `
    -map a:0 -c:a:1 aac -b:a:1 128k -ac 2 `
    -f hls -hls_time 2 -hls_playlist_type vod `
    -hls_flags independent_segments -hls_segment_type mpegts `
    -hls_segment_filename "$idname-%v/data%04d.ts" -use_localtime_mkdir 1 `
    -master_pl_name "master.m3u8" `
    -var_stream_map "v:0,a:0 v:1,a:1" "$idname-%v.m3u8" `

#-hls_segment_filename "$idname-%v/data%02d.ts" -use_localtime_mkdir 1 \
#-hls_segment_filename "$idname-%v/data%02d.ts" -use_localtime_mkdir 1 \
#-use_localtime 1 -hls_segment_filename "file-%Y%m%d-%s.ts" \

# sudo ffmpeg -i "$filename" 
# -filter_complex "[0:v]split=3[v1][v2][v3]; [v1]copy[v1out]; [v2]scale=w=1280:h=720[v2out]; [v3]scale=w=640:h=360[v3out]" 
# -map "[v1out]" -c:v:0 libx264 -x264-params "nal-hrd=cbr:force-cfr=1" -b:v:0 5M -maxrate:v:0 5M -minrate:v:0 5M -bufsize:v:0 5M -preset slow -g 48 -sc_threshold 0 -keyint_min 48 
# -map "[v2out]" -c:v:1 libx264 -x264-params "nal-hrd=cbr:force-cfr=1" -b:v:1 3M -maxrate:v:1 3M -minrate:v:1 3M -bufsize:v:1 3M -preset slow -g 48 -sc_threshold 0 -keyint_min 48 
# -map "[v3out]" -c:v:2 libx264 -x264-params "nal-hrd=cbr:force-cfr=1" -b:v:2 1M -maxrate:v:2 1M -minrate:v:2 1M -bufsize:v:2 1M -preset slow -g 48 -sc_threshold 0 -keyint_min 48 

# -map a:0 -c:a:0 aac -b:a:0 96k -ac 2 
# -map a:0 -c:a:1 aac -b:a:1 96k -ac 2 
# -map a:0 -c:a:2 aac -b:a:2 48k -ac 2 
# -f hls -hls_time 2 -hls_playlist_type vod -hls_flags independent_segments 
# -hls_segment_type mpegts -hls_segment_filename "$idname-%v/data%02d.ts" 
# -master_pl_name "master.m3u8" -var_stream_map "v:0,a:0 v:1,a:1 v:2,a:2" 
# "$idname-%v.m3u8"
}

function pause2() {
    echo "Press Enter to continue..."
    pause
}


echo "Converting video to hls"
echo "Installing test videos..."
#pause2

CreateDirectory c:\temp\stream
CreateDirectory c:\temp\stream\hls
cd c:\temp\stream\hls

$joe_video_url         = "https://fredcloud.blob.core.windows.net/public/BindenMobilizeTrueInternationalizedDepressure.mp4?se=2025-01-23T21%3A57%3A07Z&sig=EHF4qQVXGD7aEkoJbugaKE5RqctXdWU4fDjY62eizwk%3D&sp=r&sr=b&ss=2024-05-04T20%3A57%3A11Z&sv=2014-02-14"
$fredband_video_url    = "https://fredcloud.blob.core.windows.net/public/sirosVariri.05.Video.FULL.mp4?se=2025-04-18T04%3A58%3A52Z&sig=TfjW9%2FTU0F7uenwNaCzdg0l3VAAcbmP%2FN7yXel7k05I%3D&sp=r&sr=b&ss=2024-05-16T04%3A58%3A57Z&sv=2014-02-14"
$AndYourBirdCanSing_video_url = "https://fredcloud.blob.core.windows.net/video/And%20Your%20Bird%20Can%20Sing.Video.mp4?se=2025-02-07T20%3A29%3A47Z&sig=krL4B2ICzz4MuIVnPsNsTmA9%2BC6RXkKIW5x0%2BJTp6aE%3D&sp=r&sr=b&ss=2024-05-11T19%3A29%3A52Z&sv=2014-02-14"

#ToHls1080p "$fredband_video_url" "fredTrio"
#pause
# https://fvideostream.blob.core.windows.net/hls/fredTrio/master.m3u8

ToHls1080p "$AndYourBirdCanSing_video_url" "AndYourBirdCanSing"
pause
# https://fvideostream.blob.core.windows.net/hls/AndYourBirdCanSing/master.m3u8

# curl.exe https://faiwebapiapimanagementservices.azure-api.net/video/hls/fredbandband/master.m3u8
# curl.exe https://faiwebapiapimanagementservices.azure-api.net/video/hls/fredbandband/fredbandband-0.m3u8
# curl.exe https://faiwebapiapimanagementservices.azure-api.net/video/hls/fredbandband/fredbandband-0.m3u8

# # lb 20.33.73.31 
# http://74.249.130.23:8088/hls/fredbandband/master.m3u8
# http://74.249.130.23:8088/hls/fredbandband/fredbandband-0.m3u8
# curl.exe https://faiwebapiapimanagementservices.azure-api.net/video/hls/fredbandband/master.m3u8

# https://fredcloud.blob.core.windows.net/public/stream/hls/fredTrio/master.m3u8

# Azure Blob Storage pricing
# https://azure.microsoft.com/en-us/pricing/details/storage/blobs/?ef_id=_k_Cj0KCQjwgJyyBhCGARIsAK8LVLMDXJ_jJO5HCvw_1t1TGhPCk3__vgweROpmiv91moAhzMl2rAm3D7kaAlv5EALw_wcB_k_&OCID=AIDcmm5edswduu_SEM__k_Cj0KCQjwgJyyBhCGARIsAK8LVLMDXJ_jJO5HCvw_1t1TGhPCk3__vgweROpmiv91moAhzMl2rAm3D7kaAlv5EALw_wcB_k_&gad_source=1&gclid=Cj0KCQjwgJyyBhCGARIsAK8LVLMDXJ_jJO5HCvw_1t1TGhPCk3__vgweROpmiv91moAhzMl2rAm3D7kaAlv5EALw_wcB