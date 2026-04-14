# 卡牌图鉴文档

这个目录包含了 Jekyll 构建的项目文档网站，采用 Anthropic 暗色主题设计风格。

## 设计风格

**Anthropic 暗色主题**：
- 背景色：#1C1C1C (深灰黑)
- 主文字：#E8E8E8 (浅灰白)
- 次要文字：#A0A0A0 (中灰)
- 边框：#2A2A2A / #3A3A3A
- 强调色：#E8E8E8 (白色按钮)

**设计特点**：
- 固定导航栏，带模糊背景
- 大标题，负字间距 (-0.02em ~ -0.03em)
- 简洁的卡片设计，细边框
- 微妙的悬停效果 (2px 位移)
- 响应式布局，适配移动端

## 目录结构

- `static/cards/` - 存放所有导出的卡牌图片
  - `joi-*.png` - 基础版本卡牌
  - `joi-*.upgraded.png` - 升级版本卡牌
- `gallery.html` - 卡牌图鉴页面（自动从 static/cards 读取）
- `index.html` - 首页
- `.gdignore` - 告诉 Godot 忽略此目录

## 页面结构

### 首页 (index.html)
1. **导航栏** - 固定顶部，带 logo 和 CTA 按钮
2. **Hero 区域** - 大标题 + 副标题 + 双按钮
3. **核心机制** - 3个带顶部强调色的卡片
4. **特色功能** - 6个功能卡片网格
5. **卡牌预览** - 6张卡牌缩略图
6. **安装方法** - 步骤列表
7. **Footer** - 作者信息

### 图鉴页 (gallery.html)
1. **导航栏** - 返回链接 + 页面标题
2. **页面标题** - 大标题 + 描述
3. **控制栏** - 筛选按钮 + 搜索框
4. **卡牌网格** - 响应式网格，支持版本切换
5. **模态框** - 点击查看大图

## 导出卡牌

在游戏中按 `~` 打开控制台，输入：

```
/card-export Joi
```

这会将所有卡牌（基础版本和升级版本）导出到：
```
%APPDATA%/Godot/app_userdata/Slay the Spire II/card_exports/Joi/
```

然后将导出的 PNG 文件复制到 `docs/static/cards/` 目录。

## 构建网站

```bash
cd docs
bundle install  # 首次运行
bundle exec jekyll build
bundle exec jekyll serve  # 本地预览
```

构建后的网站在 `_site/` 目录。

访问 http://localhost:4000/sts2-joi/

## 部署

网站会自动部署到 GitHub Pages：
https://xinrea.github.io/sts2-joi

## 特性

- **Anthropic 设计风格**：专业、简洁、现代的暗色主题
- **自动发现卡牌**：gallery.html 会自动扫描 `static/cards/` 目录
- **版本切换**：每张卡牌可以在基础版本和升级版本之间切换
- **搜索和筛选**：支持按名称搜索，按版本筛选
- **点击放大**：点击卡牌可以查看全尺寸图片
- **响应式布局**：自适应不同屏幕尺寸
- **性能优化**：图片懒加载，平滑过渡动画

## 添加新卡牌

1. 在游戏中使用 `/card-export Joi` 导出卡牌
2. 将新的 PNG 文件复制到 `docs/static/cards/`
3. 重新构建网站：`bundle exec jekyll build`
4. 提交并推送到 GitHub

无需手动编辑任何配置文件，Jekyll 会自动发现新文件。
