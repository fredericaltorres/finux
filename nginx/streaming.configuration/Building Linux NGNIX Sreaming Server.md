# Linux:UBUNTU v 20.xx + Ngnix + rtmp

# Steps
## VM CREATION
    Create UBUNTU v 20.x vm on azure
    name: fredlinuxg
    username: fredericaltorres
    save the file fredlinuxg_key.pem
    copy "*.pem" "C:\Users\ftorres\.ssh"

## ssh connection to vm
    find the ip
    open a powershell console on windows
    $vmip = "20.106.62.93"
    $vmletter = "g"
    ssh.exe -i ~/.ssh/fredlinux$( $vmletter )_key.pem "fredericaltorres@$( $vmip )"

# Nginx installation

sudo apt update
sudo apt install python3-pip -y
sudo apt install nginx
sudo apt install libnginx-mod-rtmp
sudo apt-get clean ; sudo apt-get update ; sudo apt-get check ; sudo apt-get purge ffmpeg* -y ; sudo apt-get autoremove -y ; sudo apt-get -f satisfy ffmpeg -y
### https://forum.linuxconfig.org/t/ffmpeg-solve-unmet-dependencies/5356

sudo nginx -v

## firewall:
    sudo ufw status
    sudo ufw app list
    sudo ufw allow 'Nginx HTTP'
    sudo ufw enable
    sudo ufw status
   
sudo systemctl restart nginx
sudo systemctl status nginx

sudo nginx -t
sudo nginx -s reload
Azure Portal - Netwwork setting - Allow in port 80 inbound rule.

# Try default home page
http://20.106.62.178 should bring home page

# install rtmp

apt-cache search rtmp | grep nginx
    Should return: libnginx-mod-rtmp - RTMP support for Nginx
sudo apt install libnginx-mod-rtmp


# Nginx configuration

cd /etc/nginx/sites-available
sudo rm default
sudo curl --output rtmp https://raw.githubusercontent.com/fredericaltorres/finux/main/nginx/streaming.configuration/sites-available.rtmp

cd /etc/nginx/sites-enabled
sudo rm default
sudo curl --output rtmp https://raw.githubusercontent.com/fredericaltorres/finux/main/nginx/streaming.configuration/sites-available.rtmp

cd /etc/nginx
sudo curl --output  nginx.conf https://raw.githubusercontent.com/fredericaltorres/finux/main/nginx/streaming.configuration/nginx.with-rtmp.conf

use file nginx.with-rtmp.conf and paste it into
    sudo nano /etc/nginx/nginx.conf

test:
    sudo nginx -t
restart: 
    sudo nginx -s reload
test: 
sudo systemctl status nginx

# testing static page

sudo mkdir /var/www/fred_static_page
sudo curl --output index.html https://raw.githubusercontent.com/fredericaltorres/finux/main/nginx/streaming.configuration/index.html

cd /etc/nginx/conf.d
sudo curl --output fred_static_page.conf https://raw.githubusercontent.com/fredericaltorres/finux/main/nginx/streaming.configuration/fred_static_page.conf

# test python end point
cd /home
sudo curl --output start_app_servers.py https://raw.githubusercontent.com/fredericaltorres/finux/main/nginx/streaming.configuration/start_app_servers.py

sudo python3 /home/start_app_servers.py

sudo curl --output binaryville.conf https://raw.githubusercontent.com/fredericaltorres/finux/main/nginx/streaming.configuration/binaryville.conf



# vm configuration info
cat /proc/cpuinfo

# ffmpeg install
sudo apt-get clean ; sudo apt-get update ; sudo apt-get check ; sudo apt-get purge ffmpeg* -y ; sudo apt-get autoremove -y ; sudo apt-get -f satisfy ffmpeg -y
https://forum.linuxconfig.org/t/ffmpeg-solve-unmet-dependencies/5356

# streaming port
sudo ufw allow 8088/tcp
sudo ufw status
Azure Portal - Netwwork setting - Allow in port 8088 inbound rule.

# streaming directory
sudo mkdir /var/www/html/stream
sudo mkdir /var/www/html/stream/hls
sudo mkdir /var/www/html/stream/dash
cd /var/www/html/stream

sudo chown -R $USER:$USER /var/www
# 775 read write execute
sudo chmod -R 775 /var/www


# video working directory
cd /home
sudo mkdir /home/videos
cd /home/videos

## Download some video

sudo curl "https://fredcloud.blob.core.windows.net/public/BindenMobilizeTrueInternationalizedDepressure.mp4?se=2025-01-23T21%3A57%3A07Z&sig=EHF4qQVXGD7aEkoJbugaKE5RqctXdWU4fDjY62eizwk%3D&sp=r&sr=b&ss=2024-05-04T20%3A57%3A11Z&sv=2014-02-14" --output ./joe.mp4

maybe: sudo chmod a+rwx /home/videos/joe.mp4

sudo curl "https://fredcloud.blob.core.windows.net/public/FredTrioProJazz.SMALL.mp4?se=2024-10-24T21%3A10%3A14Z&sig=U3tzaA7OZQ2jHHPGfM%2BejX71oPJNMnmTDX5cJ0UXYUI%3D&sp=r&sr=b&ss=2024-05-04T21%3A10%3A18Z&sv=2014-02-14" --output ./fredband.mp4

# ffmpeg convert directly to hls in the stream folder
https://ottverse.com/hls-packaging-using-ffmpeg-live-vod/
https://trac.ffmpeg.org/wiki/StreamingGuide
https://www.ffmpeg.org/ffmpeg-formats.html#hls-1
https://www.ffmpeg.org/ffmpeg-formats.html#hls-2

cd /var/www/html/stream/hls
sudo ffmpeg -i "/home/videos/fredband.mp4" -f hls -hls_time 3 -hls_playlist_type vod -hls_flags independent_segments -hls_segment_type mpegts -hls_segment_filename fredband_%v/data%02d.ts fredband_%v.m3u8

## Fix manifest
sudo nano fredband_0.m3u8
    Prefix the data00.ts filename with folder fredband_0

From windows using vlc open in network http://20.106.62.178:8088/hls/fredband_0.m3u8

# ffmpeg convert directly to hls in windows
"C:\Brainshark\scripts\ffmpeg\v6.1.1\bin\ffmpeg.exe" -i "C:\Brainshark\Fred.DTA.VDO\BindenMobilizeTrueInternationalizedDepressure.mp4" -f hls -hls_time 3 -hls_playlist_type vod -hls_flags independent_segments -hls_segment_type mpegts -hls_segment_filename bbb_%v/data%02d.ts bbb%v.m3u8


# ffmpeg video conversion in multiple resolution
ffmpeg -i "/home/videos/fredband.mp4" -filter_complex "[0:v]split=3[v1][v2][v3]; [v1]copy[v1out]; [v2]scale=w=1280:h=720[v2out]; [v3]scale=w=640:h=360[v3out]" -map "[v1out]" -c:v:0 libx264 -x264-params "nal-hrd=cbr:force-cfr=1" -b:v:0 5M -maxrate:v:0 5M -minrate:v:0 5M -bufsize:v:0 10M -preset slow -g 48 -sc_threshold 0 -keyint_min 48 -map "[v2out]" -c:v:1 libx264 -x264-params "nal-hrd=cbr:force-cfr=1" -b:v:1 3M -maxrate:v:1 3M -minrate:v:1 3M -bufsize:v:1 3M -preset slow -g 48 -sc_threshold 0 -keyint_min 48 -map "[v3out]" -c:v:2 libx264 -x264-params "nal-hrd=cbr:force-cfr=1" -b:v:2 1M -maxrate:v:2 1M -minrate:v:2 1M -bufsize:v:2 1M -preset slow -g 48 -sc_threshold 0 -keyint_min 48 -map a:0 -c:a:0 aac -b:a:0 96k -ac 2 -map a:0 -c:a:1 aac -b:a:1 96k -ac 2 -map a:0 -c:a:2 aac -b:a:2 48k -ac 2 -f hls -hls_time 2 -hls_playlist_type vod -hls_flags independent_segments -hls_segment_type mpegts -hls_segment_filename fredband2_%v/data%02d.ts -master_pl_name master.m3u8 -var_stream_map "v:0,a:0 v:1,a:1 v:2,a:2" fredband2_%v.m3u8


From windows using vlc open in network 
http://20.106.62.178:8088/hls/fredband2_0.m3u8
http://20.106.62.178:8088/hls/fredband2_1.m3u8
http://20.106.62.178:8088/hls/fredband2_2.m3u8

fredband2_2/