#!/bin/bash

# chmod +x install_all.sh
# ./install_all.sh

pause() {
    echo "Press Enter to continue..."; read;
}

restart_nginx() {
    sudo nginx -t
    sudo nginx -s reload
}

echo "Installing nginx, pythoh3, ffmpeg and rtmp modules..."
pause

echo "Os Info"
cat /etc/lsb-release
pause

echo "Drive Info"
df -h
pause

echo "OS update"
sudo apt update

sudo apt-get upgrade cloud-init

echo "Python3 installation"
sudo apt install python3-pip -y

echo "Nginx installation"
sudo apt install nginx

echo "rtmp module installation"
sudo apt install libnginx-mod-rtmp

echo "ffmpeg installation"
sudo apt-get clean ; sudo apt-get update ; sudo apt-get check ; sudo apt-get purge ffmpeg* -y ; sudo apt-get autoremove -y ; sudo apt-get -f satisfy ffmpeg -y

echo "Firewall configuration"
sudo ufw app list
sudo ufw allow 'Nginx HTTP'
####### sudo ufw enable
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
