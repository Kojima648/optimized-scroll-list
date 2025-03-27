# Optimized Scroll List for Unity

一个用于 **Unity UI 滚动列表虚拟化和对象池优化** 的完整解决方案，支持分页加载、刷新数据、切换不同数据源与预制体，并结合 UnityWebRequest 异步加载远程资源。

---

## 🎯 功能特性

- ✅ **虚拟化滚动列表**：只保留可见部分的 UI 实例，提升性能
- ✅ **对象池复用**：通过对象池管理 Item，避免频繁创建与销毁
- ✅ **分页加载**：按需拉取远端数据，适配海量数据场景
- ✅ **自定义数据结构**：支持不同类型的 Item 与动态属性扩展
- ✅ **图标缓存优化**：避免重复加载图片，支持异步加载和转圈反馈
- ✅ **可切换数据源**：通过按钮切换不同表结构与 Item 预制体

---

## 🧱 项目结构

```
Assets/
├── Scripts/
│   ├── PagedListWithPool.cs     # 主控制器
│   ├── ItemCell.cs              # 标准物品样式
│   ├── RareItemCell.cs          # 稀有物品样式
│   ├── ItemData.cs              # 数据结构定义
│	├── ObjectPool.cs          	 # 对象池实现
│	├── SpinnerRotate.cs    	 # icon加载转圈
│
├── Prefabs/
│   ├── ItemCell.prefab
│   └── ItemCell2.prefab
│
```

---

## 🚀 快速开始

1. 将 `PagedListWithPool.cs` 挂载到一个空 GameObject 上
2. 配置组件引用：ScrollRect、Content、按钮、ItemPrefab 等
3. 设置远程 API 地址和分页参数
4. 运行项目，体验高性能滑动列表加载！

​	也可以直接运行 Assets/Scenes/SampleScene.unity

---

## 📦 数据接口格式（示例）

后端需返回以下结构：

```json
{
  "data": [
    {
      "id": 1,
      "itemName": "稀有物品_星辰_0001",
      "description": "提升攻击力",
      "iconResourcePath": "image_AU_0001.png",
      "quality": "史诗"
    }
  ],
  "pagination": {
    "current_page": 1,
    "total_pages": 5,
    "total_items": 150,
    "page_size": 30
  }
}
```

---

## 📷 示例图

截图见：Documents\images，文档见：Documents\滑动列表测试.md

---

## 📄 License

[Apache-2.0 license](https://github.com/Kojima648/optimized-scroll-list#)
