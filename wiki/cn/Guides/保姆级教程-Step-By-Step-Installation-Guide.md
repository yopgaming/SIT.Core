
# 保姆级SIT安装教程

# 前置要求

在开始之前请确认,你逃离塔科夫游戏文件是经BSG启动器下载的最新版本.SIT无法在非最新或盗版来源的逃离塔科夫文件下运作.


此教程中的 `SIT_DIR` 指的均是安装SIT的根目录.此目录下请新建以下3个文件夹:

-`server` 用于SPT-AKI服务器

-`launcher` 用于SIT启动器

-`game` 用于逃离塔科夫游戏文件

*解压请使用[7zip](https://7-zip.org/)或WinRAR之类的工具.


# 安装


## 1. [SIT启动器](https://github.com/paulov-t/SIT.Launcher/releases) (自动安装)


1. 在[Releases](https://github.com/paulov-t/SIT.Launcher/releases)下载最新版的`SIT Launcher`
2. 将文件解压缩至 `SIT_DIR/launcher`
3. 启动 `SIT.Launcher.exe`
4. 第1次启动时,会有以下安装提示跳出:
    
    *"No OFFLINE install found. Would you like to install now?"* 
    
    点击"Yes"
5. 将安装根目录设置为 `SIT_DIR/game`
6. 启动器会自动安装,请耐心等待

## 2. [SPT-AKI服务器](https://dev.sp-tarkov.com/SPT-AKI/Stable-releases/releases)

1. 在[Releases](https://dev.sp-tarkov.com/SPT-AKI/Stable-releases/releases)页面下载最新版的 `SPT-AKI Server`.
2. 解压缩文件至 `SIT_DIR/server`.
## 3. [SIT服务器Mod](https://github.com/paulov-t/SIT.Aki-Server-Mod)
1. 从[GitHub](https://github.com/paulov-t/SIT.Aki-Server-Mod)下载服务器mod的zip文件 (那个大绿按钮底下: *Code > Download Zip*).
2. 解压缩文件至 `SIT_DIR/server/user/mods`.
3. 
        *`user/mods` 目录会在服务器第1次运行时自动创建. 运行`Aki.Server.exe` 即可创建此文件夹. 在目录被创建后,请停止并关闭服务器,并继续进行安装.*

# 服务器设置

## 基于localhost (用于测试)

### 服务器
1. 打开位于 `SIT_DIR/server/user/mods/SIT.Aki Server-Mod/config/coopConfig.json` 的合作服务器调试文件.

    *`coopConfig.json` 文件会在服务器mod第1次运行时自动创建. 运行 `Aki.Server.exe` 以创建该文件. 在文件被创建后,请停止并关闭服务器,并继续进行安装.*

    *请注意:请不要使用Word编辑此文件,去用不会破坏格式的软件,比如Notepad.*
2. 将 `externalIP` 设置为 `http://127.0.0.1:6969`.
3. 将 `useExternalIPFinder` 设置为 `false`.
4. *此条可选. 在 `SIT_DIR/server/Aki_Data/Server/configs/http.json`将 `logRequests` 设置为 `false` 以避免日志刷屏.

### 启动器
将服务器地址设置为 `http://127.0.0.1:6969` 并连接

*使用localhost将导致其他人无法加入你的游戏,但在debug连接问题时很有用.用这个方法来确认你游戏和mod装好没有.
## 用端口转发来联机

### 服务器
你的外部IP地址应该已经被自动检测到,无需额外调整.
检查服务器log中的 `COOP: Auto-External-IP-Finder` 是否是你的IP地址.
*此条可选. 在 `SIT_DIR/server/Aki_Data/Server/configs/http.json` 将 `logRequests` 设置为 `false` 以避免日志刷屏.

### 启动器
用在服务器 `COOP: Auto-External-IP-Finder` log找到的IP,或者用在https://www.whatismyip.com里看到的自己的IP来连接.(这2个IP应该相同)

## 用Hamachi VPN来联机

### 服务器
1. 运行 Hamachi.
2. 找到显示在 Hamachi 小部件 LogMeIn 里的IPv4地址并复制.此次教程我将使用 `100.10.1.10` 为例子.
3. 打开位于 `SIT_DIR/server/user/mods/SIT.Aki Server-Mod/config/coopConfig.json` 的合作服务器调试文件.

    *`coopConfig.json` 文件会在服务器mod第1次运行时自动创建. 运行 `Aki.Server.exe` 以创建该文件. 在文件被创建后,请停止并关闭服务器,并继续进行安装.*
    
    *请注意:请不要使用Word编辑此文件,去用不会破坏格式的软件,比如Notepad.*
4. 将`externalIP` 设置为从 LogMeIn 复制的 `http://100.10.1.10:6969`
5. 将 `useExternalIPFinder` 设置为 `false`.
6. 打开位于 `SIT_DIR/server/Aki_Data/Server/configs/http.json`的 SPT-AKI 服务器连接调试文件.
    *请注意:请不要使用Word编辑此文件,去用不会破坏格式的软件,比如Notepad.*
7. Set `ip` to `100.10.1.10`.

7. 将 `ip` 设置为 `100.10.1.10`(此处为例子,不是你的IP).
8.*此条可选. 将 `logRequests` 设置为 `false` 以避免日志刷屏.

### 启动器

找到显示在 Hamachi 小部件 LogMeIn 里的IPv4地址并复制.此次教程我将使用 `http://100.10.1.10:6969` 为例子.

# 如何开始玩

## 1. 开启服务器

运行 `Aki.Server.exe`

## 2. 打开游戏

用 `SIT Launcher` 开启游戏.

*第1次用新账号密码登录时,启动器会提示你新建账户,点击'Yes'(密码存储没有加密,不要用你以前用过的密码).游戏开启后有可能会提示你 Alt+F4, 倘若如此, 关闭游戏并重新从SIT启动器开始游戏.

## 3.创建战局

请查看[How to join each other's match](https://github.com/paulov-t/SIT.Core/wiki/en/Guides/HOSTING.md#how-to-join-each-others-match).
