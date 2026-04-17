# Project [赛博科幻横版射击游戏项目]


## 技术栈
- C#

## 代码规范
- 修改函数必须加上注释
- 字符串处理需要使用ZString
- 打印统一使用UnityGameFramework.Runtime的Log输出日志
- 不要使用浮点数，用定点数fix类
- 禁使用魔数，需要定义到相对应文件夹的Const类，使用枚举
- 要注意代码健壮性，稳定性，特别注意当对象为null时候的容错处理
- 外部对象调用内部参数时，使用GET和SET方法封装后返回，避免暴露参数，方便追溯SET根源。

## 项目结构
Assets/HotAssets/Scripts #业务代码
Assets/HotAssets/Scripts/GamePlay 游戏玩法脚本

## 配置表目录路径
Config\DataTables\Datas 游戏配置表目录

## AI自动生成资源Unity (MCP)
本项目已集成 [AnkleBreaker Unity MCP](https://github.com/AnkleBreaker-Studio/unity-mcp-server)，支持 AI 直接操控 Unity Editor 进行场景搭建、UI 生成、资源管理等工作。
Unity启动会自动连接MCP插件
生成规范参考Tools\UnityMcp\总说明.md

## 沟通风格
- 回复需简洁，避免冗长，使用中文
- 遇到不确定事项应先询问
- 提供方案时需说明利弊取舍

