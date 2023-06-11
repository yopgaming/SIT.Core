# 详细分步安装教程

# 前置要求

在我们开始之前，请确保使用BSG下载并安装了最新版本的Escape From Tarkov 。SIT将无法使用过时或非法的游戏副本。

在整个指南中，我们将使用"0IT_DIR"作为安装SIT的根目录。在这个目录中，我们将创建三个单独的文件夹:

- SPT-AKI服务器的“server”文件夹。
- SIT启动器的“launcher”文件夹。
- 一个“game”文件夹存放EFT游戏文件。

*建议使用7Z进行解压文件，不要使用国内的压缩软件，可能存在问题。*

# 安装

## 1. [SIT Launcher](https://github.com/paulov-t/SIT.Launcher/releases) (自动安装方法)

1. 下载最新版本的 `SIT Launcher` 从 [Releases](https://github.com/paulov-t/SIT.Launcher/releases) .
2. 解压文件到 `SIT_DIR/launcher`.
3. 运行 `SIT.Launcher.exe`.
4. 第一次运行启动器时，它会提示你进行安装:
    
    *“No OFFLINE install found. Would you like to install now?”* 
    (未找到离线版本，你想要现在安装一个吗？)
    选择 "是".

5. 选择 `SIT_DIR/game` 作为安装目录.
6. 等待启动器下载完成。

## 2. [SPT-AKI Server](https://dev.sp-tarkov.com/SPT-AKI/Stable-releases/releases)

1. 下载最新的 `SPT-AKI Server` 从 [Releases](https://dev.sp-tarkov.com/SPT-AKI/Stable-releases/releases) .
2. 解压文件至 `SIT_DIR/server`.

## 3. [SIT Server Mod](https://github.com/paulov-t/SIT.Aki-Server-Mod)
1. 下载模组压缩包自 [GitHub](https://github.com/paulov-t/SIT.Aki-Server-Mod) (使用上面的Code按钮，即: *Code > Download Zip*).
2. 解压文件至 `SIT_DIR/server/user/mods`.

    *SPT-AKI在首次启动后会创建 user/mods 文件夹，请正常启动SPT-AKI一次后关闭SPT-AKI，再将MOD安装。*

# 配置服务器

## 本地部署

### Server
1. 打开配置文件 `SIT_DIR/server/user/mods/SIT.Aki Server-Mod/config/coopConfig.json`.

    *需运行一次MOD后，文件将会创建。*

2. 设置 `externalIP` 为 `http://127.0.0.1:6969`.
3. 设置 `useExternalIPFinder` 为 `false`.
4. (可选的), 设置 `logRequests` 到 `false` 于 `SIT_DIR/server/Aki_Data/Server/configs/http.json` 防止日志刷屏.

### Launcher
连接 `http://127.0.0.1:6969` 这一IP.

*本地部署是无法让其他玩家加入的.*

## 端口映射方式部署

### 服务侧
外部IP地址应该会自动检测到，不需要进一步的配置。
使用查询到的IP地址检查服务器输出日志中的 "COOP: Auto-External-IP-Finder" 是否一致。

### 启动器
使用显示的IP地址加上端口(通常为6969)来连接服务器。

# 启动一个战局

## 1. 启动服务器

运行 `Aki.Server.exe`

## 2. 启动游戏

通过 `SIT Launcher` 和对应的 IP地址 启动游戏。.

*第一次启动时，可能会被要求创建账户。*

## 3. 创建战局(详细可见架设教程其他文件)

See [How to join each other's match](https://github.com/paulov-t/SIT.Core/wiki/en/Guides/HOSTING.md#how-to-join-each-others-match) for in-game instructions.
