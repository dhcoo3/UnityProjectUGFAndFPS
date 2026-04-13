# Project [赛博科幻横版射击游戏项目]


## 技术栈
- C#

## 代码规范
- 修改函数必须加上注释
- 字符串处理需要使用ZString
- 打印统一使用UnityGameFramework.Runtime.Log
- 不要使用浮点数，用定点数fix类

## 项目结构
Assets/HotAssets/Scripts #业务代码
Assets/HotAssets/Scripts/GamePlay 游戏玩法脚本

## UI业务代码结构


## 策划配置表目录路径
Config\DataTables\Datas 游戏配置表目录
## 策划需求文档目录路径

## 程序开发文档目录路径

## 网络协议文件路径

## 测试要求

## AI 如何操作 Unity (MCP)
本项目已集成 [AnkleBreaker Unity MCP](https://github.com/AnkleBreaker-Studio/unity-mcp-server)，支持 AI 直接操控 Unity Editor 进行场景搭建、UI 生成、资源管理等工作。

### 1. 环境准备
- **Unity 插件安装**：在 Unity Package Manager 中通过 Git URL 添加：`https://github.com/AnkleBreaker-Studio/unity-mcp-plugin.git`
- **MCP Server 位置**：`Tools/unity-mcp-server`
- **默认端口**：7890 (可在 Unity 菜单 `Window > MCP Dashboard` 中修改)

### 2. 核心功能
- **场景管理**：创建物体、修改 Transform、管理组件、处理 Prefab。
- **UI 搭建**：自动生成 Canvas、Image、Text、Button 及其布局。
- **资源管理**：搜索 Asset、导入资源、创建材质。
- **运行时控制**：控制播放/停止、读取 Console 日志、运行单元测试。

### 3. 使用说明
AI 已安装相关工具。如需 AI 操作 Unity，请确保：
1. Unity Editor 已启动。
2. 已安装并启动 `unity-mcp-plugin`（Unity 菜单 `Window > MCP Dashboard` 显示绿色 Connected）。
3. AI 将优先通过 MCP 工具链进行可视化操作，如失败则回退至生成 C# 编辑器脚本。

## 沟通风格
- 回复需简洁，避免冗长，使用中文
- 遇到不确定事项应先询问
- 提供方案时需说明利弊取舍

