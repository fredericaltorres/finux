#!/bin/bash

# sudo chmod +x configure_all.sh
# ./configure_all.sh


pause() {
    echo "Press Enter to continue..."; read;
}
restart_nginx() {
    sudo nginx -t
    sudo nginx -s reload
}

echo "Configuting nginx as server"
pause

cd /etc/nginx/sites-available
sudo rm default
sudo curl --output rtmp https://raw.githubusercontent.com/fredericaltorres/finux/main/nginx/streaming.configuration/sites-available.rtmp

cd /etc/nginx/sites-enabled
sudo rm default
sudo curl --output rtmp https://raw.githubusercontent.com/fredericaltorres/finux/main/nginx/streaming.configuration/sites-available.rtmp

cd /etc/nginx
sudo curl --output nginx.conf https://raw.githubusercontent.com/fredericaltorres/finux/main/nginx/streaming.configuration/nginx.with-rtmp.conf



echo "Configuting nginx static pages"

sudo mkdir /var/www/fred_static_page
cd /var/www/fred_static_page
sudo curl --output index.html https://raw.githubusercontent.com/fredericaltorres/finux/main/nginx/streaming.configuration/index.html

cd /etc/nginx/conf.d
sudo curl --output fred_static_page.conf https://raw.githubusercontent.com/fredericaltorres/finux/main/nginx/streaming.configuration/fred_static_page.conf



echo "Install python script to start app servers"
cd /home
sudo curl --output start_app_servers.py https://raw.githubusercontent.com/fredericaltorres/finux/main/nginx/streaming.configuration/start_app_servers.py
#  sudo python3 /home/start_app_servers.py


restart_nginx
