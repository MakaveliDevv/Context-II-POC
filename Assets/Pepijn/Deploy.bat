set BuildLocation="C:\Unity Projects\Context-II-POC\ServerBuild"
ssh root@context2server.atlas-technologies.co.uk "cd /root/docker/pengin-unity-server && docker compose down"
scp -r %BuildLocation% root@context2server.atlas-technologies.co.uk:/root/docker/pengin-unity-server/
ssh root@context2server.atlas-technologies.co.uk "cd /root/docker/pengin-unity-server && docker compose up -d --force-recreate --build && tail -F logs/server.log"
pause



