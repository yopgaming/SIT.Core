# 安装教程

** 这个教程仅用户可以配置运营商的光猫/路由器防火墙端口转发的情况下适用 **

## 主机/服务器

1. 下载并解压最新的SPT-AKI服务端，请前往DEV-SPTARKOV下载，点击[这里](dev.sp-tarkov.com)跳转到官方Relese（现有的最新版本是3.5.7）
2. 安装[SIT.Aki-Server-Mod](https://github.com/paulov-t/SIT.Aki-Server-Mod)到服务端
4. 删除你已有的所有的离线塔科夫文件
5. 下载最新的 [SIT-Launcher](https://github.com/paulov-t/SIT.Launcher/releases)
6. 使用 SIT-Launcher 从你的在线版本塔科夫创建一个副本 (或者自行复制一份出来，取决于你自己)
7. 使用 SIT-Launcher 安装 SIT.COre 与 Assemblies文件
8. 配置服务端的 http.json 与 server.json为你的网卡地址，coopConfig.json为你的外部地址或域名，参照FAQ中的[DDNS](https://github.com/paulov-t/SIT.Core/wiki/FAQs#ddns-setup-step-if-you-dont-have-a-static-public-ip-address-and-you-want-to-use-a-domain-name-to-connect-to-the-server)
9. 启动服务器 (不要修改SPT-Aki服务端的任何其他地方)
10. 在你的光猫/路由器上转发配置的SPT-Aki服务器端口与SIT.Aki-Server-Mod端口，默认为6969与6970，并确认服务端的防火墙已经放行配置的端口
12. 和你的小伙伴找点乐子吧

## 客户端

1. 删除你已有的所有的离线塔科夫文件
2. 下载最新的 [SIT-Launcher](https://github.com/paulov-t/SIT.Launcher/releases)
3. 让 SIT-Launcher 从你的在线版本塔科夫创建一个副本 (或者自行复制一份出来，取决于你自己)
4. 让 SIT-Launcher 安装 SIT.Core 与 Assemblies文件
5. 在SIT-Launcher中填写服务器的地址并登录/注册账号
6. __不要使用和其他任何服务相同的用户名与密码！！__，因为服务端与客户端之间为明文传输，用户名与密码也是明文存储在服务端的！！

