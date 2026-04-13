# UI 生成规范

本文档规定了如何通过 MCP 生成规范的 UI Prefab，确保生成的 UI 能正确集成到项目的 UI 系统中。

## 1. 面板根节点（Panel Root）

### 必要组件
- **RectTransform** - UI 布局
- **Canvas** 或 **RectTransform** - 作为容器
- **UIContainer** - 存储所有子 UI 元素的引用（**必须**）
- **ExPanel** - Panel 逻辑脚本（**必须**）

### 约定
```
Canvas
└── PanelRoot (RectTransform)
    ├── [UIContainer 组件]
    ├── [ExPanel 脚本]
    └── [其他 UI 子节点]
```

## 2. 按钮（Button）

### 组件配置
| 属性 | 规范 | 说明 |
|------|------|------|
| **脚本组件** | ExButton | 必须使用 ExButton，不用标准 Button |
| **显示文本** | TextMeshPro Text | 子节点中的文本显示 |
| **背景图** | Image | 按钮的背景图组件 |
| **命名** | m_XXXButton | 私有成员变量，首字母大写 |

### 示例结构
```
LoginButton (ExButton)
├── Image (背景)
└── Text (TextMeshPro)
```

## 3. 输入框（InputField）

### 组件配置
| 属性 | 规范 | 说明 |
|------|------|------|
| **脚本组件** | InputField（标准Unity） | 不使用自定义组件 |
| **背景** | Image | 输入框背景 |
| **文本** | TextMeshPro Text | 输入内容的文本显示 |
| **占位符** | TextMeshPro Text | Placeholder 文本 |
| **命名** | m_XXXInput | 私有成员变量，首字母大写 |

### 示例结构
```
UsernameInput (InputField)
├── Image (背景)
├── Text (TextMeshPro - 输入内容)
└── Placeholder (TextMeshPro - 占位符)
```

## 4. 文本（Text）

### 组件配置
| 属性 | 规范 | 说明 |
|------|------|------|
| **脚本组件** | ExText | 项目自定义扩展文本组件 |
| **命名** | m_XXXText 或 XXXLabel | 清晰表示用途 |

## 5. 图片（Image）

### 组件配置
| 属性 | 规范 | 说明 |
|------|------|------|
| **脚本组件** | ExImage（项目自定义） |
| **命名** | m_XXXImage | 私有成员变量 |

## 6. 面板（Panel）

### 组件配置
| 属性 | 规范 | 说明 |
|------|------|------|
| **脚本组件** | Image | 用作半透明背景 |
| **命名** | PanelBg 或 Panel | 清晰表示用途 |
| **色彩** | RGBA (0, 0, 0, 128) | 半透明黑色背景 |

## 7. MCP 创建规范

### 命名约定
- **面板脚本类名** - PascalCase（如 LoginPanel）
- **UI 元素名称** - 对应脚本成员变量名（如 m_LoginButton → 创建为 LoginButton）
- **标签** - 使用有意义的名称（如 UsernameLabel, PasswordLabel）

### 组件类型对应

| MCP type | Unity 组件 | 说明 |
|----------|-----------|------|
| button | ExButton + Image | 按钮 |
| inputfield | InputField + Image | 输入框 |
| text | TextMeshPro Text | 文本 |
| image | Image | 图片 |
| panel | Image | 面板背景 |

### MCP 创建示例

#### 创建面板根节点
```json
{
  "name": "LoginPanel",
  "parentInstanceId": "<CanvasInstanceId>",
  "type": "panel",
  "sizeDelta": {"x": 600, "y": 500},
  "anchoredPosition": {"x": 0, "y": 0}
}
// 然后手动添加 UIContainer 和 ExPanel 组件
```

#### 创建按钮（规范做法）
```json
{
  "name": "LoginButton",
  "parentInstanceId": "<PanelInstanceId>",
  "type": "button",
  "text": "登陆",
  "fontSize": 25,
  "sizeDelta": {"x": 200, "y": 60},
  "anchoredPosition": {"x": 0, "y": -150}
}
// 然后手动替换为 ExButton 组件
```

#### 创建输入框（规范做法）
```json
{
  "name": "UsernameInput",
  "parentInstanceId": "<PanelInstanceId>",
  "type": "inputfield",
  "placeholder": "请输入用户名",
  "text": "",
  "sizeDelta": {"x": 350, "y": 50},
  "anchoredPosition": {"x": 75, "y": 100}
}
```

## 8. UI 脚本编写规范

### 面板脚本模板

```csharp
using HotAssets.Scripts.UI.Tool.Component;
using UnityEngine.UI;

public class LoginPanel : ExPanel
{
    #region Auto Create
    private ExButton m_LoginButton;
    private InputField m_UsernameInput;
    private InputField m_PasswordInput;
    #endregion

    protected override void OnInit(object userData)
    {
        #region Auto Bind
        m_LoginButton = PanelUIContainer.Get<ExButton>("LoginButton");
        m_UsernameInput = PanelUIContainer.Get<InputField>("UsernameInput");
        m_PasswordInput = PanelUIContainer.Get<InputField>("PasswordInput");
        #endregion

        #region Auto Event
        m_LoginButton.SetClickListener(OnLoginButtonClick);
        #endregion

        base.OnInit(userData);
    }

    #region Auto Write Event
    /// <summary>
    /// 登陆按钮点击事件处理
    /// </summary>
    private void OnLoginButtonClick()
    {
        // 登陆逻辑
    }
    #endregion
}
```

## 9. 检查清单

生成 UI 后，需要验证：

- [ ] 面板根节点有 **UIContainer** 组件
- [ ] 面板根节点有 **ExPanel** 脚本
- [ ] 所有按钮使用 **ExButton** 组件（不是标准 Button）
- [ ] 所有输入框使用 **InputField** 组件
- [ ] 所有文本使用 **TextMeshPro Text** 组件
- [ ] UI 元素名称与脚本绑定名称一致
- [ ] 脚本中的成员变量声明格式正确
- [ ] 所有事件监听器已正确绑定

## 10. 常见问题

### Q: 为什么要用 ExButton 而不是标准 Button？
A: ExButton 是项目自定义的按钮组件，提供了额外的功能和与项目 UI 系统的集成。

### Q: InputField 需要自定义吗？
A: 不需要，使用标准的 Unity InputField 组件。

### Q: 如何绑定 UI 元素？
A: 使用 `PanelUIContainer.Get<ComponentType>("ElementName")` 获取元素。UIContainer 会自动存储所有子元素的引用。

### Q: 生成后需要手动调整什么？
A: 
1. 替换按钮的 Button 组件为 ExButton
2. 调整 UI 布局和大小
3. 设置合适的颜色和字体
4. 绑定事件监听器