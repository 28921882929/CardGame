# 事件测试Demo使用说明

## 场景设置步骤

1. 创建一个新的Unity场景或使用现有场景
2. 在场景中创建以下UI元素：
   - Canvas (画布)
   - 在Canvas下创建Panel (面板)
   - 在Panel下创建两个Button (按钮) 和三个Text (文本)组件
   - 第一个Text用于显示标题
   - 第二个Text用于显示EventTestDemo状态
   - 第三个Text用于显示EventTestReceiver状态

3. 在场景中创建两个空游戏对象:
   - 创建一个名为"EventTestDemo"的GameObject，挂载EventTestDemo脚本
   - 创建一个名为"EventTestReceiver"的GameObject，挂载EventTestReceiver脚本

4. 设置引用:
   - 选择EventTestDemo对象，将两个按钮拖放到Inspector中的button1和button2字段
   - 将第二个Text拖放到statusText字段
   - 选择EventTestReceiver对象，将第三个Text拖放到receiverStatusText字段

5. 设置按钮文本:
   - 第一个按钮文本设置为："触发无参数事件"
   - 第二个按钮文本设置为："触发带参数事件"
   - 第一个Text（标题）设置为："事件测试Demo"
   - 第二个Text（EventTestDemo状态）可以留空
   - 第三个Text（EventTestReceiver状态）可以留空

## 测试操作

1. 运行场景
2. 点击"触发无参数事件"按钮，查看两个状态文本是否都更新，表示事件被两个组件接收到
3. 点击"触发带参数事件"按钮，查看两个状态文本是否都显示相同的参数消息

## 测试原理

- EventTestDemo组件会发送事件
- EventTestReceiver组件会监听相同的事件
- 当事件被触发时，两个组件都会收到通知并更新各自的状态文本
- 这验证了事件系统可以将一个事件分发给多个不同的监听者

## 注意事项

- 确保场景中已加载EventManager，或者它会在运行时自动创建
- 如果测试不成功，请检查控制台是否有错误信息
- 确保按钮的Button组件和Text组件都已正确设置 