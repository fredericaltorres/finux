
#!/bin/bash

# sudo chmod +x configure_all.sh
# ./configure_all.sh

ToHls() {
    local filename=$1
    local idname=$2

    echo "Converting $idname to hls"
    sudo mkdir /var/www/html/stream/hls/$idname
    cd /var/www/html/stream/hls/$idname

    sudo ffmpeg -i "$filename" -filter_complex "[0:v]split=3[v1][v2][v3]; [v1]copy[v1out]; [v2]scale=w=1280:h=720[v2out]; [v3]scale=w=640:h=360[v3out]" -map "[v1out]" -c:v:0 libx264 -x264-params "nal-hrd=cbr:force-cfr=1" -b:v:0 5M -maxrate:v:0 5M -minrate:v:0 5M -bufsize:v:0 10M -preset slow -g 48 -sc_threshold 0 -keyint_min 48 -map "[v2out]" -c:v:1 libx264 -x264-params "nal-hrd=cbr:force-cfr=1" -b:v:1 3M -maxrate:v:1 3M -minrate:v:1 3M -bufsize:v:1 3M -preset slow -g 48 -sc_threshold 0 -keyint_min 48 -map "[v3out]" -c:v:2 libx264 -x264-params "nal-hrd=cbr:force-cfr=1" -b:v:2 1M -maxrate:v:2 1M -minrate:v:2 1M -bufsize:v:2 1M -preset slow -g 48 -sc_threshold 0 -keyint_min 48 -map a:0 -c:a:0 aac -b:a:0 96k -ac 2 -map a:0 -c:a:1 aac -b:a:1 96k -ac 2 -map a:0 -c:a:2 aac -b:a:2 48k -ac 2 -f hls -hls_time 2 -hls_playlist_type vod -hls_flags independent_segments -hls_segment_type mpegts -hls_segment_filename "$idname-%v/data%02d.ts" -master_pl_name "master.m3u8" -var_stream_map "v:0,a:0 v:1,a:1 v:2,a:2" "$idname-%v.m3u8"

    echo "video streaming url http://[IP]:8088/hls/$idname/$idname-0.m3u8"
}

ToHls1080p() {
    local filename=$1
    local idname=$2
    local medium= "medium"
# Resolution #0 supplied is 1920x1080 1080p 5Mb/s
# Resolution #1 960x540 1080p/2 3 Mb/s

    echo "---------------------------------------------"
    echo "Converting $idname to hls 1080p"
    echo "---------------------------------------------"
    
    sudo mkdir /var/www/html/stream/hls/$idname
    cd /var/www/html/stream/hls/$idname

    sudo ffmpeg -i "$filename" \
    -filter_complex "[0:v]split=2[v1][v2]; [v1]copy[v1out]; [v2]scale=w=960:h=540[v2out]" \
    -map "[v1out]" -c:v:0 libx264 -x264-params "nal-hrd=cbr:force-cfr=1" -b:v:0 5M -maxrate:v:0 5M -minrate:v:0 5M -bufsize:v:0 5M  -preset "$medium" -g 48 -sc_threshold 0 -keyint_min 48 \
    -map "[v2out]" -c:v:1 libx264 -x264-params "nal-hrd=cbr:force-cfr=1" -b:v:1 3M -maxrate:v:1 3M -minrate:v:1 3M -bufsize:v:1 3M  -preset "$medium" -g 48 -sc_threshold 0 -keyint_min 48 \
    -map a:0 -c:a:0 aac -b:a:0 128k -ac 2 \
    -map a:0 -c:a:1 aac -b:a:1 128k -ac 2 \
    -f hls -hls_time 2 -hls_playlist_type vod \
    -hls_flags independent_segments -hls_segment_type mpegts \
    -hls_segment_filename "$idname-%v/data%04d.ts" -use_localtime_mkdir 1 \
    -master_pl_name "master.m3u8" \
    -var_stream_map "v:0,a:0 v:1,a:1" "$idname-%v.m3u8"  

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

pause() {
    echo "Press Enter to continue..."; read;
}


echo "Converting video to hls"
pause

echo "Installing test videos..."
pause

joe_video_url="https://fredcloud.blob.core.windows.net/public/BindenMobilizeTrueInternationalizedDepressure.mp4?se=2025-01-23T21%3A57%3A07Z&sig=EHF4qQVXGD7aEkoJbugaKE5RqctXdWU4fDjY62eizwk%3D&sp=r&sr=b&ss=2024-05-04T20%3A57%3A11Z&sv=2014-02-14"
fredband_video_url="https://fredcloud.blob.core.windows.net/public/FredTrioProJazz.SMALL.mp4?se=2024-10-24T21%3A10%3A14Z&sig=U3tzaA7OZQ2jHHPGfM%2BejX71oPJNMnmTDX5cJ0UXYUI%3D&sp=r&sr=b&ss=2024-05-04T21%3A10%3A18Z&sv=2014-02-14"
AndYourBirdCanSing_video_url="https://fredcloud.blob.core.windows.net/video/And%20Your%20Bird%20Can%20Sing.Video.mp4?se=2025-02-07T20%3A29%3A47Z&sig=krL4B2ICzz4MuIVnPsNsTmA9%2BC6RXkKIW5x0%2BJTp6aE%3D&sp=r&sr=b&ss=2024-05-11T19%3A29%3A52Z&sv=2014-02-14"

cd /home/videos
# sudo curl --output ./joe.mp4 "$joe_video_url"
# sudo curl  --output ./fredband.mp4 "$fredband_video_url"
# ls -ltr
# pause

echo "Convert to hls fredffmpeg video conversion in multiple resolution"
## ffmpeg -i "/home/videos/fredband.mp4" -filter_complex "[0:v]split=3[v1][v2][v3]; [v1]copy[v1out]; [v2]scale=w=1280:h=720[v2out]; [v3]scale=w=640:h=360[v3out]" -map "[v1out]" -c:v:0 libx264 -x264-params "nal-hrd=cbr:force-cfr=1" -b:v:0 5M -maxrate:v:0 5M -minrate:v:0 5M -bufsize:v:0 10M -preset slow -g 48 -sc_threshold 0 -keyint_min 48 -map "[v2out]" -c:v:1 libx264 -x264-params "nal-hrd=cbr:force-cfr=1" -b:v:1 3M -maxrate:v:1 3M -minrate:v:1 3M -bufsize:v:1 3M -preset slow -g 48 -sc_threshold 0 -keyint_min 48 -map "[v3out]" -c:v:2 libx264 -x264-params "nal-hrd=cbr:force-cfr=1" -b:v:2 1M -maxrate:v:2 1M -minrate:v:2 1M -bufsize:v:2 1M -preset slow -g 48 -sc_threshold 0 -keyint_min 48 -map a:0 -c:a:0 aac -b:a:0 96k -ac 2 -map a:0 -c:a:1 aac -b:a:1 96k -ac 2 -map a:0 -c:a:2 aac -b:a:2 48k -ac 2 -f hls -hls_time 2 -hls_playlist_type vod -hls_flags independent_segments -hls_segment_type mpegts -hls_segment_filename fredband2_%v/data%02d.ts -master_pl_name master.m3u8 -var_stream_map "v:0,a:0 v:1,a:1 v:2,a:2" fredband2_%v.m3u8
# ToHls "/home/videos/fredband.mp4" "fredbandband" # use a local file
#ToHls1080p "$AndYourBirdCanSing_video_url" "AndYourBirdCanSing" # use a url
#pause

ToHls1080p "$fredband_video_url" "fredbandband" # use a url
pause

# curl.exe https://faiwebapiapimanagementservices.azure-api.net/video/hls/fredbandband/master.m3u8
# curl.exe https://faiwebapiapimanagementservices.azure-api.net/video/hls/fredbandband/fredbandband-0.m3u8
# curl.exe https://faiwebapiapimanagementservices.azure-api.net/video/hls/fredbandband/fredbandband-0.m3u8

# # lb 20.33.73.31 
# http://74.249.130.23:8088/hls/fredbandband/master.m3u8
# http://74.249.130.23:8088/hls/fredbandband/fredbandband-0.m3u8


# curl.exe https://faiwebapiapimanagementservices.azure-api.net/video/hls/fredbandband/master.m3u8


