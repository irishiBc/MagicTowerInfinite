# MagicTowerInfinite
Just Something about my own dream starter

# Week One 功能目标

## 功能闭环

- 可正常开始一局游戏
- 每层进入自动触发 1v1 战斗
- 速度判定先手正确
- 攻击轮转稳定执行
- HP 正确扣减
- 任意一方 HP ≤ 0 正确结束战斗
- 玩家死亡触发 Game Over
- 通关层数达成正确判定

## 数值与日志
- 暴击概率按 FinalCrit 正确生效
- 格挡概率按 FinalBlock 正确生效
- 战斗过程 Log 可完整显示
- 每场战斗结果可复盘

## 架构约束
- 无新增隐藏机制
- 无额外未定义公式
- 属性变更仅来源于胜利选择
- 数值可通过 Inspector 调整