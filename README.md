# ClashRoyale (2017)
[![clash royale](https://img.shields.io/badge/Clash%20Royale-1.9.2-brightred.svg?style=flat")](https://clash-royale.en.uptodown.com/android/download/1632865)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)
![Build Status](https://action-badges.now.sh/Zordon1337/ZrdRoyale)

-----------------------------------------
## ✔What was added/changed in this fork✔
1. Fixed DB(before fixing saving players progress always failed due to non existing column)<br />
2. Now server is more customizable without recompiling it, now you can edit:
```
Minimum Trophies and Maximum Trophies after Win
Default amount of gems and gold
```

# TODO ✅
```
Console commands
Remake README.md
```
## Clash royale server for version 1.9.2/1.9.3 written in .NET


## Battles
The server supports battles, for those a patched client is neccessary.

[See the wiki for a tutorial](https://github.com/retroroyale/ClashRoyale/wiki/Patch-for-battles)

## How to start

#### Requirements:
  - [.NET Core SDK 3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1)
  - MySql Database (on Debian i suggest LAMP with PMA or on windows i suggest XAMPP with PMA)

for Ubuntu use these commands to set it up:
```
mkdir ClashRoyale
git clone https://github.com/retroroyale/ClashRoyale.git && cd ClashRoyale/src/ClashRoyale

dotnet publish "ClashRoyale.csproj" -c Release -o app
```
To configurate your server, such as the database you have to edit the ```config.json``` file.

#### Run the server:

###### Main Server:
```dotnet app/ClashRoyale.dll```

###### Battle Server:
```dotnet app/ClashRoyale.Battles.dll```

#### Update the server:
###### Main Server:
```git pull && dotnet publish "ClashRoyale.csproj" -c Release -o app && dotnet app/ClashRoyale.dll```

###### Battle Server:
```git pull && dotnet publish "ClashRoyale.Battles.csproj" -c Release -o app && dotnet app/ClashRoyale.Battles.dll```

## Need help?
Contact me on Discord (Incredible#2109) or open an issue.
