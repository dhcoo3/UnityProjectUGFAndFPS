# 4. 表现层与美术技术规范 (Technical Art Spec)

## 4.1 逻辑与表现的解耦
- **逻辑事件驱动**：表现层注册事件（如 `OnPlayerShoot`, `OnUnitHit`），在回调中播放动画或音效。
- **插值平滑**：Render 层每帧获取 Logic 层的 `fix` 坐标，使用 `Vector3.Lerp` 平滑同步 Unity 坐标。

## 4.2 角色动画规范
- **必要状态名**：
    - `Idle`, `Run`, `Jump`, `Fall`, `Dash`
    - `Attack_Start`, `Attack_Active`, `Attack_End` (对应前摇、判定、后摇)
- **帧事件 (Animation Events)**：
    - `OnAttackHit`: 逻辑判定帧。
    - `OnEffectSpawn`: 视觉特效触发。
    - `OnFootstep`: 脚步声触发。

## 4.3 特效 (VFX) 管理
- **分类**：
    - `OneShot`: 播放即销毁（如爆炸）。
    - `Loop`: 随实体状态存在（如喷气背包）。
- **优化**：
    - 粒子数量限制。
    - 使用对象池 (`VariablePoolComponent`) 循环利用。

## 4.4 地图资源规范
- **Tile 尺寸**：1x1 Unity Unit。
- **图集 (Atlas)**：所有关卡元素合并图集，减少 DrawCall。
- **层级设定**：
    - Layer 0: 背景。
    - Layer 1: 装饰(非阻挡)。
    - Layer 2: 核心地图块(阻挡层)。
    - Layer 3: 前景装饰。
