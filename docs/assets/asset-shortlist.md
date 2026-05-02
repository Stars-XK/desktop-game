# 资源清单（可商用免费优先 + 付费建议）

本清单用于把“桌面 AI 女友”的视觉质感拉高。默认策略：**只用明确允许商用/再分发的资源，并把 LICENSE/作者信息随资源一起放进项目**。

---

## 1) 免费可商用（推荐优先）

### 1.1 贴图/材质/环境（强推荐，安全）
- **Poly Haven**（CC0）：HDRI、贴图、模型（可商用、可再分发）  
  用途：试衣间环境光、地面材质、金属/布料 PBR。  
  获取：在站点里筛选 CC0，并把下载页的许可说明保存到 `docs/assets/licenses/polyhaven/`。
- **ambientCG**（CC0）：PBR 贴图、Atlas、Decals  
  用途：UI 装饰纹理、微噪点、金边/织物贴图。

### 1.2 字体（可商用、可打包）
- **Noto Sans SC / Noto Serif SC**（SIL OFL）  
  用途：UI 字体统一（比 Arial 质感更好）。  
  注意：把 OFL 许可文本放入项目随包分发。

### 1.3 UI 图标（可商用）
- **Material Symbols / Material Icons**（Apache 2.0）  
  用途：设置/返回/衣橱/麦克风等基础图标。

### 1.4 角色/动画（免费“高精”非常稀缺，建议从付费入手）
- 免费可商用的“高精二次元女角色（含表情/口型/动画）”基本不稳定，容易踩授权坑。  
  推荐策略：免费阶段先把 **灯光/后期/镜头/皮肤材质** 拉满 + 用中档可商用角色占位；等你确认方向后用付费资产替换成高精。

---

## 2) 付费资源建议（你买，我接入）

下面是“买了就能显著提升到产品级”的方向清单（以用途为主，具体链接我可以按你偏好风格再细化到 3 个最合适的商品页）：

### 2.1 角色（核心）
- **带表情/口型 BlendShape 的高质量女性角色**（Unity Asset Store / Sketchfab 商用许可）  
  必要条件：Humanoid 绑定、Facial BlendShapes（眼/嘴/眉）、材质可调、最好含基础动作包。

### 2.2 动作包（提升“女友感”最快）
- **Idle/撒娇/害羞/生气/招手/眨眼/摸头反应** 动作包  
  必要条件：Humanoid 动作、可循环 Idle、包含过渡动画。

### 2.3 头发与服装（接近“暖暖”的关键）
- **可换装服装套装 + 多套头发**  
  必要条件：拆分部件（Top/Bottom/Shoes/Hair/Accessory）、支持换色或多材质。

### 2.4 视觉氛围（立竿见影）
- **URP 后期/体积光/柔光滤镜类** 工具或预设（Asset Store）  
  用途：让画面“柔”“亮”“通透”，更像摄影棚。

---

## 3) 资源落库规范（避免后期混乱）

建议目录：
- `Assets/Art/Characters/<pack>/...`
- `Assets/Art/Clothes/<pack>/...`
- `Assets/Art/Animations/<pack>/...`
- `Assets/Art/Textures/<source>/...`
- `docs/assets/licenses/<source-or-pack>/LICENSE.txt`

每次新增外部资源，必须同时提交：
- 资源原始下载链接（写在该目录的 `SOURCE.txt`）
- LICENSE / 商用条款文本（放到 `docs/assets/licenses/...`）

