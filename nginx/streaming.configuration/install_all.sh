#!/bin/bash

# chmod +x install_all.sh
# ./install_all.sh

ToHls() {
    local filename=$1
    local idname=$2
    sudo mkdir /var/www/html/stream/hls/$idname
    cd /var/www/html/stream/hls/$idname
    sudo ffmpeg -i "$filename" -filter_complex "[0:v]split=3[v1][v2][v3]; [v1]copy[v1out]; [v2]scale=w=1280:h=720[v2out]; [v3]scale=w=640:h=360[v3out]" -map "[v1out]" -c:v:0 libx264 -x264-params "nal-hrd=cbr:force-cfr=1" -b:v:0 5M -maxrate:v:0 5M -minrate:v:0 5M -bufsize:v:0 10M -preset slow -g 48 -sc_threshold 0 -keyint_min 48 -map "[v2out]" -c:v:1 libx264 -x264-params "nal-hrd=cbr:force-cfr=1" -b:v:1 3M -maxrate:v:1 3M -minrate:v:1 3M -bufsize:v:1 3M -preset slow -g 48 -sc_threshold 0 -keyint_min 48 -map "[v3out]" -c:v:2 libx264 -x264-params "nal-hrd=cbr:force-cfr=1" -b:v:2 1M -maxrate:v:2 1M -minrate:v:2 1M -bufsize:v:2 1M -preset slow -g 48 -sc_threshold 0 -keyint_min 48 -map a:0 -c:a:0 aac -b:a:0 96k -ac 2 -map a:0 -c:a:1 aac -b:a:1 96k -ac 2 -map a:0 -c:a:2 aac -b:a:2 48k -ac 2 -f hls -hls_time 2 -hls_playlist_type vod -hls_flags independent_segments -hls_segment_type mpegts -hls_segment_filename "$idname_%v/data%02d.ts" -master_pl_name "$idname_master.m3u8" -var_stream_map "v:0,a:0 v:1,a:1 v:2,a:2" $idname_%v.m3u8
}

pause() {
    echo "Press Enter to continue..."; read;
}

restart_nginx() {
    sudo nginx -t
    sudo nginx -s reload
}

echo "Installing nginx, pythoh3, ffmpeg and rtmp modules..."
pause

echo "OS update"
sudo apt update
pause

echo "Python3 installation"
sudo apt install python3-pip -y
pause

echo "Nginx installation"
sudo apt install nginx
pause

echo "rtmp module installation"
sudo apt install libnginx-mod-rtmp
pause

echo "ffmpeg installation"
sudo apt-get clean ; sudo apt-get update ; sudo apt-get check ; sudo apt-get purge ffmpeg* -y ; sudo apt-get autoremove -y ; sudo apt-get -f satisfy ffmpeg -y
pause

echo "Firewall configuration"
sudo ufw app list
sudo ufw allow 'Nginx HTTP'
sudo ufw enable
sudo ufw status
pause

echo "Firewall configuration - streaming port"
sudo ufw allow 8088/tcp
sudo ufw status
pause

echo "Azure Networking configuration - open inbound port 8088 and 80"
pause
read

echo "Creating local folder"
pause

sudo mkdir /var/www/html/stream
sudo mkdir /var/www/html/stream/hls
sudo mkdir /var/www/html/stream/dash
sudo chown -R $USER:$USER /var/www
sudo chmod -R 775 /var/www

sudo mkdir /home/videos
cd /home/videos

echo "re start nginx"
pause
restart_nginx


echo "Installing test videos..."
pause

cd /home/videos
sudo curl "https://fredcloud.blob.core.windows.net/public/BindenMobilizeTrueInternationalizedDepressure.mp4?se=2025-01-23T21%3A57%3A07Z&sig=EHF4qQVXGD7aEkoJbugaKE5RqctXdWU4fDjY62eizwk%3D&sp=r&sr=b&ss=2024-05-04T20%3A57%3A11Z&sv=2014-02-14" --output ./joe.mp4
sudo curl "https://fredcloud.blob.core.windows.net/public/FredTrioProJazz.SMALL.mp4?se=2024-10-24T21%3A10%3A14Z&sig=U3tzaA7OZQ2jHHPGfM%2BejX71oPJNMnmTDX5cJ0UXYUI%3D&sp=r&sr=b&ss=2024-05-04T21%3A10%3A18Z&sv=2014-02-14" --output ./fredband.mp4
ls -ltr
pause


echo "Convert to hls fredffmpeg video conversion in multiple resolution"
## ffmpeg -i "/home/videos/fredband.mp4" -filter_complex "[0:v]split=3[v1][v2][v3]; [v1]copy[v1out]; [v2]scale=w=1280:h=720[v2out]; [v3]scale=w=640:h=360[v3out]" -map "[v1out]" -c:v:0 libx264 -x264-params "nal-hrd=cbr:force-cfr=1" -b:v:0 5M -maxrate:v:0 5M -minrate:v:0 5M -bufsize:v:0 10M -preset slow -g 48 -sc_threshold 0 -keyint_min 48 -map "[v2out]" -c:v:1 libx264 -x264-params "nal-hrd=cbr:force-cfr=1" -b:v:1 3M -maxrate:v:1 3M -minrate:v:1 3M -bufsize:v:1 3M -preset slow -g 48 -sc_threshold 0 -keyint_min 48 -map "[v3out]" -c:v:2 libx264 -x264-params "nal-hrd=cbr:force-cfr=1" -b:v:2 1M -maxrate:v:2 1M -minrate:v:2 1M -bufsize:v:2 1M -preset slow -g 48 -sc_threshold 0 -keyint_min 48 -map a:0 -c:a:0 aac -b:a:0 96k -ac 2 -map a:0 -c:a:1 aac -b:a:1 96k -ac 2 -map a:0 -c:a:2 aac -b:a:2 48k -ac 2 -f hls -hls_time 2 -hls_playlist_type vod -hls_flags independent_segments -hls_segment_type mpegts -hls_segment_filename fredband2_%v/data%02d.ts -master_pl_name master.m3u8 -var_stream_map "v:0,a:0 v:1,a:1 v:2,a:2" fredband2_%v.m3u8
ToHls "/home/videos/fredband.mp4" "fredband2"
pause

echo "INSTALLATION COMPLETED!"
pause



### https://forum.linuxconfig.org/t/ffmpeg-solve-unmet-dependencies/5356
# # Check if installations were successful
# if [ $? -eq 0 ]; then
#     echo "Packages installed successfully."
# else
#     echo "Error occurred during package installation."
# fi

# Perform additional tasks or configurations related to the installed packages
# For example, you can start a service or modify configuration files
