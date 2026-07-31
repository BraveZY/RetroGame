# MotionX Retro Pack / 主题视觉拓展规范

> 本文用于约束 `时光派对 / Time Play` 三大主题在首页、登录 / 入口、主题详情、游戏选择、入戏开场、游戏中 HUD、结算页和奖励卡中的视觉拓展方式。
> 它不替代 [游戏 DNA 与风格统一规范](./style-dna.md) 和 [AI 生图 Prompt 风格统一指南](./ai-image-prompt.md)，而是把 DNA 文档落成可执行的主题视觉规则、页面应用规则、Prompt 变量和评审标准。

相关参考图：

- `/Users/dukechen/Downloads/ChatGPT Image 2026年6月20日 20_43_40 (1).png`
- `/Users/dukechen/Downloads/ChatGPT Image 2026年6月20日 20_43_42 (2).png`
- `/Users/dukechen/Downloads/ChatGPT Image 2026年6月20日 20_43_43 (3).png`

## 1. 文档职责

| 文档 | 职责 | 不负责 |
| --- | --- | --- |
| [游戏 DNA 与风格统一规范](./style-dna.md) | 定义产品为什么像同一个 MotionX Retro Pack：家庭体感、童年场景、短局玩法、现代可读 UI、轻怀旧、原创安全 | 不规定每个页面如何摆放主题视觉元素 |
| 本文 | 定义三大主题如何扩展到不同页面和视觉资产：舞台、色彩、图形语言、锚点、页面强度、Prompt 和评审 | 不重新定义产品 DNA、玩法 DNA、音频 DNA 和文案 DNA |
| [AI 生图 Prompt 风格统一指南](./ai-image-prompt.md) | 定义可复用的 AI 生图语言、画风锚点和版权安全边界 | 不承担页面结构和主题应用矩阵 |

本文是执行层。所有主题视觉扩展都以 DNA 文档为上位规则，以本文作为生产和评审输入。

## 2. 统一美术口径

```text
现代家庭电视游戏体验，带一点爸妈小时候的记忆光晕。
```

不是：

```text
80/90 年代怀旧物件陈列馆。
```

三大主题不是三套独立产品皮肤，而是同一套家庭体感产品中的不同舞台。主题切换改变舞台氛围、色彩、主记忆锚点和少量二级细节；产品骨架、交互优先级、CTA 可读性和家庭友好气质保持统一。

怀旧负责情绪，现代 UI 负责品质。画面第一眼必须像一个清爽、明亮、可操作的家庭游戏体验，第二眼识别主题，第三眼才发现边角、卡片缝隙和远景里的童年记忆细节。

## 3. 主题视觉拓展总规则

| 规则 | 执行口径 |
| --- | --- |
| 共享产品骨架 | 顶部功能、主 CTA、返回 / 继续、分数、倒计时、HUD 和结算信息保持同一产品语言 |
| 主题只换舞台 | 变化集中在背景氛围、主舞台画面、主题色彩、平面图形点缀、锚点物件和局部动效 |
| 怀旧只做点题 | 每个画面保留 3 个主记忆锚点和 3-5 个二级细节，不堆满旧物 |
| 页面强度分级 | 首页最克制，主题详情更沉浸，游戏中 HUD 最克制，结算和奖励卡允许更强收藏感 |
| 动作信息优先 | 站位、目标、倒计时、分数、反馈、CTA 永远压过装饰和怀旧细节 |
| 环境表面干净 | 墙面、地面、跑道、客厅和大面积背景必须明亮清爽；怀旧靠物件、光线和场景关系表达，不靠污渍、灰尘、破损、暗角表达 |
| 原创安全 | 不出现真实品牌、真实街机、真实节目、明星、影视、老歌或可识别 IP |

## 4. 页面应用矩阵

| 页面 / 资产 | 主题视觉职责 | 主题强度 | 固定骨架 | 可变化内容 | 禁止滑向 |
| --- | --- | ---: | --- | --- | --- |
| 登录 / 入口 | 建立家庭客厅入场感，让用户愿意进入 | 中 | 产品名、年龄提示、登录按钮、游客入口、协议勾选、备案脚注 | 背景暖色、电视 / 客厅轮廓、边缘回忆物、平面收藏图形 | 假文字主视觉、老照片登录页、复古海报页 |
| 首页 | 作为现代家庭电视游戏大厅，承载主题切换和主 CTA | 中低 | 顶部轻量功能、左右切换、主舞台、底部信息区 | 当前主题标题、主舞台图、环境光、3 个主锚点、CTA 色彩 | 每个主题一套首页、完整主题实景页、道具展 |
| 主题详情 | 展示当前主题馆的内容池和玩法入口 | 中高 | 返回、主题名、游戏列表、人数 / 时长 / 强度、开始入口 | 更强舞台沉浸、主题背景、主题卡片边缘、主题化分区标题 | 主题馆变成长剧情展厅、卡片信息被插画盖住 |
| 游戏选择卡 | 让用户 5 秒内知道这局在哪里、怎么玩 | 中 | 游戏名、人数、时长、强度、开始按钮 | 1 个强视觉锚点、1-2 个二级细节、主题色条 | 小游戏图标墙、旧物件拼贴卡 |
| 入戏开场 | 让用户知道我在哪里、我是谁、为什么动 | 中高 | 10-20 秒内进入、站位提示、倒计时、开始反馈 | 场景启动动效、主题音画锚点、场景语言 | 长剧情、技术校准页、影视化片头 |
| 游戏中 HUD | 保证动作反馈、计分、倒计时和安全可读 | 低 | 分数、倒计时、目标、站位、成功 / 失败反馈；至少 55% 无阻挡动作区 | HUD 边缘贴纸、平面图形点缀、主题音效和局部反馈 | 装饰压住动作、所有模块同等放大、同一目标多处重复、扫描线 / 噪点影响识别 |
| 结算页 | 给家庭留下称号、分数和共同话题 | 中 | 分数、排名 / 星级、再来一局、返回、分享 / 收藏入口 | 主题称号、回忆标签、贴纸、灯牌、节目封面感 | 煽情长文、复杂收藏柜、真实 IP 引用 |
| 奖励卡 | 做轻收藏，让主题记忆可以被保存 | 中高 | 奖励名、稀有度 / 类型、获得条件、确认按钮 | 奖章、贴纸、票根、节目封面、计分券、主题边框 | 真实品牌票券、过度泛黄、信息不可读 |

## 5. 视觉比例

### 5.1 通用比例

| 层级 | 占比 | 职责 | 设计边界 |
| --- | ---: | --- | --- |
| 产品功能层 | 45% | 承载 CTA、导航、分数、倒计时、状态、核心信息 | 远距离可读，不能被主题装饰压住 |
| 主题舞台层 | 35% | 通过光线、色调、主图和场景说明当前主题 | 不讲完整故事，不变成独立产品皮肤 |
| 记忆锚点层 | 20% | 唤起父母的童年记忆，让主题更有回忆味 | 3 个主锚点清楚可见，3-5 个二级细节放在边缘、缝隙或远景 |

### 5.2 首页方向图比例

首页方向图保留更强的主卡表达：

| 层级 | 占比 | 职责 | 设计边界 |
| --- | ---: | --- | --- |
| 背景主题氛围 | 25% | 通过光线、色调、远景和少量符号说明当前主题 | 不讲完整故事，不堆实景道具 |
| 主视觉卡片 | 55% | 承载主题内容、玩法入口、动作理由和 CTA | 第一眼要知道在哪里、点哪里、玩什么 |
| 记忆锚点层 | 20% | 唤起父母的童年记忆，让主题更有回忆味 | 二级细节不能压住主卡、CTA 和玩法信息 |

### 5.3 平面图形比例

| 图形层 | 问题稿常见比例 | 目标比例 | 具体做法 |
| --- | ---: | ---: | --- |
| 泛黄纸张 / 胶带 / 老物件 | 约 60% | 25% | 作为可感知的回忆层出现，只放在边缘、角标、卡片缝隙和远景 |
| 现代平面卡片 / 清晰 CTA / 图标 | 约 20% | 45% | 主卡、切换控件、CTA、底部信息区使用干净、高对比的平面游戏 UI |
| 主题场景图形 | 约 20% | 30% | 用色块、远景轮廓、动作区和色调表达主题，不变成写实场景杂烩 |

## 6. 三主题视觉变量

每个主题固定 3 个主记忆锚点，同时允许 3-5 个二级记忆细节。主锚点负责 5 秒内识别主题，二级细节负责让父母产生“我见过这个”的回忆感。二级细节必须放在边缘、卡片缝隙、底部 HUD 缝隙或远景里。

| 主题 | 目标感觉 | 3 个主记忆锚点 | 允许的二级细节 | 删除 / 弱化 | 禁止滑向 |
| --- | --- | --- | --- | --- | --- |
| 童年操场 | 明亮校园课间、空气感、清爽、有阳光 | 跑道线、粉笔格、下课铃 | 粉笔头、小口哨、广播喇叭远影、沙包、搪瓷杯、值日表小卡 | 班旗墙、红领巾堆叠、过多照片、过多纸张、奖章墙、粉笔盒堆叠、脏旧跑道、破损墙皮 | 老学校物件集合、完整校园实景页、班级荣誉墙、废弃学校 |
| 客厅街机 | 家庭客厅里临时开了一场街机挑战 | 分数灯、投币按钮、街机操控台 | 少量游戏币、票券角标、塑料篮球、小贴纸、掌机玩具、计分券 | 大量硬币、票券墙、强霓虹、电玩城海报、完整街机机器边框、脏旧木地板、灰尘墙面 | 夜店霓虹电玩城、商场街机厅、暗色商业街机空间、废旧客厅 |
| 家庭节拍馆 | 家庭健身舞蹈频道，全家在客厅跟着电视动起来 | 电视框、节目频道、节拍光点 | 遥控器、地垫圆点、节目贴纸、塑料凳、家庭相册角、少量音符光点 | 大量磁带、节目单墙、过多音符、迪斯科灯、过旧电视纹理、脏旧地面、发霉墙面 | 复古电视歌舞厅、老电视怀旧展、迪斯科舞厅、破旧出租屋 |

## 7. 首页统一结构

首页是主题视觉拓展的第一落点，但不是唯一落点。

首页不重复露出 `时光派对 / Time Play` 品牌。品牌已在启动、登录和产品级入口建立，进入首页后首屏注意力交给当前主题、左右切换和主 CTA。

| UI 区域 | 固定规则 | 主题变化 |
| --- | --- | --- |
| 顶部功能区 | 不显示品牌标题，只保留头像、帮助、设置等轻量入口 | 不随主题换成校园招牌、街机灯箱或电视节目台标 |
| 左右切换控件 | 保持同一位置、尺寸和箭头样式 | 只改变轻微高光，不做强主题道具化 |
| 主视觉卡片 | 承担首页 55% 的主题表达 | 不同主题可换主图、光线、锚点和 CTA 色 |
| 底部信息区 | 保持现代游戏面板质感 | 活力、成就、活动信息不贴纸化 |
| 背景 | 负责氛围，不负责讲故事 | 使用主题色调、远景轮廓、色块和少量平面图形 |

选中态色彩：

| 主题 | 选中态色彩 |
| --- | --- |
| 童年操场 | 浅黄 / 草绿 |
| 客厅街机 | 橙红 / 深蓝灯光 |
| 家庭节拍馆 | 紫粉 / 暖橙 |

左右切换控件不能变成作业本、街机控制面板或电视节目单。首页统一性优先于单主题装饰。

## 8. 当前三张首页方向图的修正结论

| 图像 | 当前有效点 | 主要问题 | 下一版方向 |
| --- | --- | --- | --- |
| 童年操场 | 明亮、清新、主题识别快 | 背景过度变成完整操场实景，主题页感强于首页入口感 | 保留阳光、跑道弧线、粉笔格、下课铃；弱化完整校园陈列和边角道具 |
| 客厅街机 | 家庭客厅 + 游戏挑战的方向成立 | 街机和霓虹表现偏重，容易滑向电玩城 / 商业街机厅 | 保留暖色客厅、LED 分数灯、投币按钮、小型操控台；减少暗色霓虹和硬币票券 |
| 家庭节拍馆 | 家庭跟电视运动的动作理由清楚 | 怀旧电视和歌舞厅感偏重，彩灯与节目感容易压过家庭运动 | 保留电视框、节目频道、节拍光点、地垫；减少磁带、节目单、复古舞厅符号 |

总问题不是“主题没呼应”，而是“主题呼应过猛”。画面从“怀旧亲切”滑到了“怀旧道具展览”。

## 8.1 已确认目标效果图

当前用户确认的目标效果图：

- `references/approved-visual-targets/playground-gameplay-hud-approved-20260630.png`

它确立了后续游戏中效果图的优先风格：横版 16:9、现代家庭体感 HUD、明亮童年操场、可玩动作区优先、平面图形化 2D、边缘少量怀旧小物。它不是要求所有游戏都画操场和铁环，而是要求所有游戏中效果图共享这套产品骨架和完成度。

应用到其他主题时：

| 页面 / 主题 | 保持不变 | 允许变化 |
| --- | --- | --- |
| 游戏中 HUD | 横版大屏构图、至少 55% 无阻挡动作区、清晰关键分数 / 倒计时 / 目标摘要、清楚站位区、目标路径、边缘轻量道具栏 | 场景舞台、主题色、目标物、动作理由和 3 个主记忆锚点 |
| 童年操场 | 跑道线、粉笔格、下课铃 / 广播、草地动作区 | 木头人、滚铁环、丢沙包、套圈等不同目标物 |
| 客厅街机 | 同一 HUD 骨架和可玩动作区 | 客厅角落、分数灯、投币按钮、小型操控台 |
| 家庭节拍馆 | 同一 HUD 骨架和可玩动作区 | 电视框、节目频道、节拍光点、地垫站位 |

## 9. 生图输入母版

用途：生成首页、登录 / 入口、主题详情、游戏选择卡、入戏开场、结算页或奖励卡方向图。

```text
Create a 16:9 premium modern family TV motion game screen for MotionX Retro Pack / Time Play.

Screen type:
[home lobby / login entry / theme detail / game selection card / opening scene / gameplay HUD / result screen / reward card]

Overall art direction:
modern family TV game experience, bright clean premium casual game UI, warm and clear, clean nostalgic look, clean-film everyday-memory game illustration, clean walls, clean floors, clean playground track, polished flat cards, flat CTA buttons, crisp outlines, readable hierarchy, all-ages, high-quality Chinese casual game UI, friendly but not childish.

Core mood:
"a modern family TV game experience with a soft glow of parents' childhood memories".
Not an 80s/90s nostalgia object exhibition.

Theme behavior:
the selected theme changes only the stage atmosphere, color palette, flat graphic accents, 3 primary nostalgic anchors and 3-5 secondary memory details.
Keep the product structure unified across screens.
The background is a theme atmosphere background, not a full realistic scene and not a prop collage.
Keep walls, floors, playground track, living room wooden floor and large background areas clean, bright and breathable; nostalgia comes from props, light, color and family scene memory, not grime, dust, stains, cracked walls or dark vignette.

UI composition:
clear primary action, readable product hierarchy, stable navigation or return control when needed, visible key information, clean CTA button.
For home lobby: compact top utility area without repeated brand title, left and right theme switch arrows, large main theme card as the focus, bottom activity / achievement / weekly cards.
For gameplay HUD: use the approved target style. Landscape 16:9 TV gameplay screen with at least 55% unobstructed central/foreground action space. Keep the combined top HUD under about 15% of screen height: a readable score, round countdown timer and compact target/progress summary. Keep the bottom HUD under about 12%: make either the current-action prompt or player status prominent, never both; reduce the other to a small corner cue. Use only one primary feedback focal point at a time; combo, success and warning feedback must be brief overlays, not permanent large cards. Do not repeat the current objective in top, center and bottom panels. Keep props, badges and nostalgic details small and at the edges.
For result or reward card: use stronger collectible details while keeping all text areas clean and readable.

Flat graphic ratio:
45% modern flat cards / clear CTA / icon UI,
30% theme illustration,
25% nostalgic paper / sticker / retro object details.

Selected theme:
[童年操场 / 客厅街机 / 家庭节拍馆]

Nostalgic anchors:
[3 primary nostalgic anchors, list them here]

Secondary memory details:
[3-5 small childhood memory details placed near edges, card corners, module gaps or far background, list them here]

Avoid:
3D rendering, subtle 3D, volumetric lighting, glassmorphism, glossy material, realistic texture, vertical mobile map as the default output, business-sim map, mini-game icon wall, gacha buttons, museum-like nostalgia display, cluttered prop collage, old paper everywhere, tape everywhere, too many tickets, too many coins, too many photos, heavy yellow aged texture, dark arcade room, cyberpunk neon, disco hall, vintage TV showroom, full realistic scene background, unreadable UI, low contrast, dirty old-photo filter, heavy VHS noise, dirty walls, stained floors, cracked old plaster, mold, dust, mud, grunge texture, dark vignette, abandoned spaces, real brands, real IP, readable logos, fake UI text, fake legal text.
```

## 10. 三主题 Prompt 变量

| 主题 | Selected theme | Nostalgic anchors | Atmosphere | Negative focus |
| --- | --- | --- | --- | --- |
| 童年操场 | Playground Memories / 童年操场 | running track lines, chalk hopscotch grid, school bell | bright morning recess, light sunshine, airy playground feeling, blurred school building far away | no class flag wall, no scarf pile, no awards collage, no school prop exhibition |
| 客厅街机 | Living Room Arcade / 客厅街机 | LED score light, coin button, small arcade control panel | warm living room corner lit by a playful game challenge | no dark arcade hall, no heavy neon, no coin pile, no ticket wall, no poster wall |
| 家庭节拍馆 | Family Rhythm TV / 家庭节拍馆 | TV frame, program channel accent, rhythm light dots | cozy family living room, soft light, family workout and parent-child rhythm | no disco hall, no heavy old TV texture, no cassette pile, no music-note clutter, no retro TV exhibition |

| 主题 | Secondary memory details |
| --- | --- |
| 童年操场 | tiny chalk sticks, small whistle, distant loudspeaker silhouette, sandbag toy, enamel cup, tiny class duty card |
| 客厅街机 | a few toy coins, ticket corner tag, plastic basketball, sticker badges, handheld toy, tiny score coupon |
| 家庭节拍馆 | toy remote control, floor mat dots, TV program sticker, plastic stool, family photo album corner, subtle music-light dots |

## 11. 主题视觉拓展评审清单

| 检查项 | 通过标准 |
| --- | --- |
| 产品统一 | 第一眼仍然像同一款现代家庭体感游戏，不像换了另一套产品 |
| 页面职责 | 当前页面的核心任务清楚：进入、选择、开场、操作、结算或领取奖励 |
| 主题识别 | 5 秒内能识别当前主题 |
| 主信息可读 | CTA、分数、倒计时、站位、规则、奖励名等关键内容不被装饰遮挡 |
| 背景克制 | 背景提供氛围和色调，不讲完整故事 |
| 锚点数量 | 每个主题有 3 个主记忆锚点，并有 3-5 个二级细节 |
| 平面图形比例 | 现代平面 UI 明显多于泛黄纸张和旧物件；不使用玻璃、亚克力或细腻材质表达 |
| 页面强度 | 首页克制，主题详情沉浸，游戏中 HUD 最克制，结算 / 奖励卡允许更收藏化 |
| HUD 占屏与焦点 | 游戏中至少 55% 是无阻挡动作区；顶部约不超过 15%、底部约不超过 12%；同一时刻只有一个主视觉反馈，当前目标不重复出现 |
| 明亮清爽 | 不暗、不脏、不旧、不压迫 |
| 环境表面 | 墙面、地面、跑道、客厅木地板和大面积背景干净暖亮，没有污渍、霉斑、破损墙皮、泥脏地面或暗角压黑 |
| 家庭感 | 画面像家里能一起玩的游戏，不像商业展陈或夜场空间 |
| 版权安全 | 不出现真实品牌、真实街机、真实节目、明星或可识别 IP |

## 12. 不合格修正表

| 问题 | 修正输入 |
| --- | --- |
| 怀旧物件太少 | `increase secondary memory details to 3-5 items, place them near edges and card gaps, keep CTA clean` |
| 怀旧物件太多 | `3 primary nostalgic anchors plus 3-5 secondary details only, no prop collage, no museum-like nostalgia display` |
| 背景太像完整实景 | `theme atmosphere background, soft blurred far background, leave clean UI space` |
| UI 太旧纸张化 | `modern flat UI cards, flat clear CTA buttons, reduce old paper and tape texture` |
| 墙面 / 地面显脏 | `clean walls, clean floors, clean playground track, clean living room surfaces, bright warm daylight, clean-film childhood memory illustration, no grime, no dust, no stains, no mold, no dark vignette` |
| 街机太暗太电玩城 | `warm living room arcade corner, no dark arcade hall, no cyberpunk neon, no poster wall` |
| 操场太像主题页 | `modern TV game product first, playground only through sunlight, track curve, chalk grid` |
| 节拍馆太像歌舞厅 | `family TV workout channel, soft home light, no disco hall, no stage lighting` |
| 首页切换控件过度主题化 | `unified left and right switch arrows, only subtle highlight changes` |
| 游戏中 HUD 过大 / 被装饰干扰 | `reserve at least 55% unobstructed action space; keep the top HUD under 15% and bottom HUD under 12% of screen height; only one primary feedback focal point at a time; do not duplicate the current objective; keep memory details at edges only` |
| 结算 / 奖励卡太像展品 | `keep reward information readable, use light collectible stickers and badges, avoid museum display` |

## 13. 一句话验收

```text
用户第一眼看到的是同一款现代、清爽、可操作的家庭体感游戏；
第二眼感受到当前主题；
第三眼能在边角、卡片缝隙和远景里发现爸妈小时候的记忆细节。
```
