# MotionX Retro Pack / AI 生图 Prompt 风格统一指南

> 本文用于统一 `MotionX Retro Pack / 复古体感包` 的 AI 生图提示词，确保主题馆主视觉、游戏场景概念图、UI 背景、奖励卡片和道具图标看起来属于同一个产品。  
> 本文不替代 [游戏 DNA 与风格统一规范](./style-dna.md) 和 [主题视觉拓展规范](./theme-visual-extension.md)，而是把其中的风格 DNA 和页面视觉规则转成可复用的生图 Prompt 模板。

## 1. 生图总原则

AI 生图必须服务产品 DNA：

```text
家庭共同参与
+ 童年场景记忆
+ 1-3 分钟短局体感
+ 明亮清晰的现代 UI
+ 轻怀旧元素点题
+ 原创安全表达
= 时光体感馆 / Time Play
```

Prompt 的核心不是“做旧”，而是让画面同时满足：

| 目标 | 生图要求 |
| --- | --- |
| 像同一个产品 | 固定平面画风、色彩、图形语言、构图层级 |
| 像家庭体感游戏 | 画面里有清楚动作空间、目标物、站位区或可玩场景 |
| 有轻怀旧感 | 用抽象化的操场、老电视、投币机、广播、奖状、队旗等元素点题 |
| 孩子愿意进入 | 明亮、温暖、干净、亲和，不暗、不脏、不破旧 |
| 能触动回忆 | 画面里有自然散落的 80/90 年代生活碎片，让父母觉得“我见过这个” |
| 版权安全 | 不出现真实 IP、明星、影视角色、真实游戏机、真实商标、真实老歌符号 |

### 1.1 干净怀旧底线

最新参考方向以“上方图”这类干净暖日光版本为准：怀旧来自场景、物件、贴纸、奖章、粉笔线、老电视轮廓和家庭动作关系，不来自污渍化的墙面、地面、暗角或破旧纹理。

| 要保留 | 要避免 |
| --- | --- |
| 明亮暖日光、清爽墙面、干净地面、可爱的轻微使用感 | 灰尘、泥污、霉斑、破损墙皮、脏旧木地板、暗角、褐色旧照片滤镜 |
| 生活感、小乱、边角贴纸、自然摆放的小物件 | grunge、abandoned、dirty room、aged wall、stained floor、smoky atmosphere |
| clean-film nostalgic look：像一张干净、温暖、色彩饱满的童年回忆插画 | aged-film / damaged-photo look：像脏旧照片、仓库、展陈或废弃空间 |

Prompt 固定加入：

```text
clean nostalgic look, clean-film childhood memory illustration, bright warm daylight, clean walls, clean floor, clean playground track, clean living room surfaces, fresh air, no grime, no dust, no stains, no mold, no cracked dirty walls, no muddy floor, no dark vignette
```

## 2. 固定画风锚点

所有 AI 图像默认使用同一套视觉锚点。

### 2.0 已确认目标效果图

当前用户确认的目标效果图：

- `references/approved-visual-targets/playground-gameplay-hud-approved-20260630.png`

这张图是后续游戏中效果图的优先基准。生成新图时，不复制具体铁环玩法、操场标语或画面像素，但要继承它的产品骨架：

| 维度 | 统一要求 |
| --- | --- |
| 比例 | 默认横版 16:9，面向 TV / 平板大屏体感；除非用户明确要求手机竖版，不把竖版地图作为默认输出 |
| 核心画面 | 中央可玩动作区、前景玩家站位区、清楚目标物、清楚运动轨迹 |
| HUD | 至少 55% 无阻挡动作区；仅当下唯一关键反馈大号呈现。顶部放紧凑分数 / 倒计时 / 目标摘要，底部只突出动作提示或玩家状态之一；远距离可读但不全屏铺满 |
| 风格 | 明亮的 90 后生活记忆场景 + 平面图形化 2D 道具 + 现代清晰游戏 UI，完成度接近高质量全龄家庭娱乐产品；亲和但不低幼 |
| 怀旧 | 跑道、粉笔格、广播、搪瓷杯、口哨、沙包等只放边缘和背景，不压动作目标 |
| 禁止 | 竖版经营地图、小游戏图标墙、抽卡按钮、纯绘本封面、旧物件展陈、脏旧怀旧滤镜 |

### 2.1 基础风格

```text
bright colorful premium family TV and mobile game UI,
family-friendly retro motion game app,
premium casual game interface,
landscape 16:9 TV gameplay composition by default,
clear body-motion gameplay area,
one large readable primary feedback at a time, compact score / countdown / target summary, lightweight player-status and prop cues,
visible standing zone and motion path,
compact readable rounded cards,
bright clean 90s Chinese everyday-memory game illustration,
flat graphic 2D game-art style,
clean color blocks and crisp outlines, not a children's picture book,
variety-show bold title lettering when a title is needed,
flat graphic props and natural-proportion family players,
minimal flat highlights,
warm bright background,
clean nostalgic look,
clean-film childhood memory illustration,
bright warm daylight,
clean walls and clean floor,
high-saturation theme colors,
emotionally rich childhood memory details,
curated nostalgic clutter,
clear readable visual hierarchy,
subtle Chinese childhood nostalgia,
modern crisp UI-ready composition,
original fictional props,
no real IP
```

中文解释：

| 维度 | 统一要求 |
| --- | --- |
| 画风 | 高完成度家庭体感游戏 UI，兼容电视大屏和手机端，明亮、饱满、玩具感，不做写实照片 |
| 生活记忆场景方向 | 可采用明亮干净的 90 后生活记忆场景：家庭客厅、校园操场、社区空地、夏日树荫、家人朋友互动。整体是成熟但亲和的风格化游戏美术，不做儿童绘本质感 |
| 形体 | 清晰平面卡片、易读按钮、有限投影、平面小物与自然比例的家庭角色；避免过圆、过萌和玩具感 |
| 明暗 | 温暖明亮底色 + 清晰色块对比，避免暗场景和体积光 |
| 图形语言 | 贴纸、徽章、游戏机、磁带、奖章、灯牌等均以扁平图形表达；墙面、地面、跑道和客厅表面保持干净，不使用细腻纹理 |
| 构图 | 默认优先横版 16:9 TV / 平板大屏体感构图，中央动作区和前景站位区清楚；手机端纵向卡片只在用户明确要求时使用 |
| 复古度 | 轻怀旧、风格化、clean-film；不做旧照片、重噪点、破旧仓库、脏旧滤镜、污渍墙面和泥脏地面，也不做低幼玩具化卡通 |
| UI 补足 | 如果画面偏封面或场景插画，必须补回现代游戏 UI 骨架：分数、倒计时、目标进度、玩家状态、道具栏、体感站位提示；首页和选择页再补主题卡、开始按钮、人数 / 时长 / 强度 |

### 2.1.1 90 后生活记忆场景 Prompt 锚点

用于童年操场、课间小游戏、主题详情头图、奖励卡、登录 / 入口大图或入戏开场：

```text
bright clean 90s Chinese everyday-memory game scene,
bright shared-life scene,
light nostalgic object anchors,
flat graphic 2D game-art style,
clean color blocks and crisp outlines, not a children's picture book, not childish,
soft edges, clean warm sunlight, not dirty, not dark,
warm yellow, grass green, light blue and creamy white palette,
old family living room, countryside path, school playground, willow tree shade,
family members and friends playing together, running, crouching, watching TV, gathering around a game, natural proportions,
old TV silhouette, radio, school award, green canvas schoolbag, red scarf color accent, rolling hoop, marbles, wooden chair, field path, playground white lines,
bold white title lettering with thick black outline when a title is needed,
modern clear family motion game UI overlay, readable CTA, player count, duration, intensity, timer, score and body-tracking standing zone
```

注意：这个方向的情绪很准，但不能只停留在怀旧封面、绘本插画或低幼卡通。用于 MotionX 时，必须保留清楚的现代 UI 层级和可操作入口。

### 2.2 情怀冗余层

Prompt 里不能只写一个“retro”或只放一个老电视。要让图像有“回忆被翻出来”的感觉：主场景清楚，周围自然出现一些小物件、小贴纸、小角标、小收集物，形成生活化的怀旧冗余。

这里的冗余不是乱，而是顶级大厂 UI 常见的“有控制的丰富度”：

| 类型 | 可自然加入的 80/90 年代元素 | 使用方式 |
| --- | --- | --- |
| 校园 / 操场 | 下课铃、广播喇叭、粉笔线、队旗、奖状、跳房子格子、沙包、红领巾色块、小卖部窗口、搪瓷杯 | 作为卡片边角、背景小物、贴纸、徽章或远景，不抢动作目标 |
| 客厅 / 电视 | 老电视轮廓、电视雪花感、录像带、磁带、节目贴纸、遥控器、塑料凳、茶几、墙上奖状、家庭相册 | 作为 UI 装饰、收集物、卡片底纹或奖励图标 |
| 街机 / 游乐 | 投币口、灯牌、票券、计分牌、塑料篮球、霓虹边框、游戏币、贴纸、徽章、掌机玩具 | 玩具化、虚构化，不画真实机器和真实品牌 |
| 收藏 / 奖励 | 贴纸册、集章卡、铁皮盒、礼物盒、奖章、纪念票、相册、童年图鉴、收集板 | 用于底部模块、奖励卡、徽章和分享图 |

Prompt 固定加入：

```text
emotionally rich childhood memory details,
curated nostalgic clutter,
small 80s/90s Chinese childhood objects naturally placed around the UI,
stickers, badges, tickets, cassette tapes, toy handheld device, school awards, chalk marks, loudspeaker, enamel cup,
not museum-like, not too formal, not overly symmetrical,
playful lived-in details while keeping premium game UI readability
```

中文判断：

| 要做到 | 不要变成 |
| --- | --- |
| 像小时候抽屉、贴纸册、操场角落、客厅电视柜里翻出的记忆 | 怀旧展览馆、素材拼贴墙、年代物件百科 |
| 有一点自然的小乱和惊喜 | 过度规整、过度对齐、像企业模板 |
| 每个小物件都服务场景情绪 | 无意义堆物、影响按钮和标题阅读 |

### 2.3 参考图风格锚点

当前参考图更接近“高饱和现代家庭游戏大厅 UI”，不是普通场景概念图。后续首页、主题馆入口、活动页、收集页优先采用这个完成度和丰富度，但构图必须按电视端、手机端和具体页面职责调整。

| 观察项 | Prompt 中应强化 |
| --- | --- |
| 整体气质 | `bright colorful premium family game UI`, `playful`, `polished`, `all-ages`, `not childish` |
| 背景 | `warm bright background`, `flat color blocks`, `tiny sparkles`, `sticker decorations` |
| 卡片 | `large readable content cards`, `clear borders`, `flat highlights`, `minimal drop shadows` |
| 主题分色 | 童年操场蓝绿、客厅街机橙黄、电视健美操紫粉、收集站奶油橙 |
| 角色 | `stylized family players`, `natural proportions`, `expressive but not exaggerated`, `not mascot-like` |
| 道具 | `toy-like cassette tape`, `retro handheld game device`, `stickers`, `badges`, `arcade hoop`, `TV frame` |
| UI 层级 | 按页面职责保留清楚主 CTA、导航、主舞台 / 主卡、状态信息和底部信息区 |
| 按钮 | `large flat CTA button`, `large readable button`, `icon arrow button` |
| 装饰 | 小星星、彩带、贴纸、角标、票券、磁带、掌机、奖章、集章卡，但不能压过标题和 CTA |

追加风格短语：

```text
in the style of a polished Chinese family TV and mobile casual game screen,
warm bright background, clean readable cards,
flat CTA buttons, stylized family players with natural proportions,
flat retro collectibles, sticker badges, tiny sparkles,
emotionally rich childhood memory details,
curated nostalgic clutter, playful lived-in details,
high contrast, high saturation, minimal shadows, premium app UI
```

### 2.4 统一色彩方向

| 类型 | 口径 |
| --- | --- |
| 页面底色 | 明亮暖光、浅色游戏大厅底色，不做纯白空页面，也不做大面积暗黄旧化 |
| 环境表面 | 墙面、地面、跑道、客厅木地板和家具大面保持清爽干净，用暖光和色彩表达年代感 |
| 主题卡色 | 童年操场蓝绿；客厅街机橙黄；电视健美操紫粉；收集站奶油橙 |
| CTA 色 | 白底蓝字 / 白底橙字 / 高亮渐变按钮，保持大面积可读 |
| 装饰色 | 星星黄、活力绿、挑战蓝、热荐橙、贴纸粉 |
| 禁止 | 大面积暗黄、脏绿、灰褐、低亮度、重颗粒、强扫描线、grunge 污渍、破损墙皮、脏旧地板、暗角压黑 |

## 3. 通用 Prompt 结构

所有生图 Prompt 使用同一结构：

```text
[固定画风锚点],
[主题馆 / 场景],
[玩家身份 / 家庭关系],
[动作空间 / 核心目标物],
[轻怀旧元素],
[构图要求],
[UI 可用性要求],
[版权安全要求],
[画面限制]
```

实际写法：

```text
Bright colorful premium all-ages family TV and mobile game UI for a retro motion game app, warm bright background, bright warm daylight, clean nostalgic look, clean-film everyday-memory game illustration, clean walls and clean floor, clean readable flat cards, flat highlights, flat graphic props, stylized family players with natural proportions, high saturation, minimal shadows, emotionally rich memory details, curated nostalgic clutter, subtle Chinese 90s nostalgia, modern crisp UI-ready composition, friendly but not childish.

Scene: [SCENE].
Players: [PLAYERS].
Action focus: [ACTION_FOCUS].
Nostalgic anchors: [NOSTALGIC_ANCHORS].
Memory details: small 80s/90s Chinese childhood objects naturally placed around the UI, stickers, badges, tickets, cassette tapes, toy handheld device, school awards, chalk marks, loudspeaker, enamel cup, playful lived-in details.
Clean surface rule: keep walls, floors, playground track, living room surfaces and large background areas clean, warm, bright and breathable; use only cute light wear, no grime.
Composition: [COMPOSITION].
Use original fictional props, no real brands, no real IP, no celebrity likeness, no readable text, no logos.
Avoid 3D rendering, subtle 3D, volumetric lighting, glassmorphism, glossy material, realistic texture, children's picture-book style, childish cartoon, chibi proportions, toy-like mascot, oversized childish eyes, overly cute UI, dark old-photo filters, heavy VHS noise, pixel art, cluttered collage, horror mood, dirty abandoned spaces, dirty walls, stained floors, cracked old plaster, mold, dust, mud, dark vignette, realistic school branding, minimalist corporate app style, overly formal layout, museum-like nostalgia display, sterile perfect symmetry.
```

## 4. 统一负面 Prompt

所有图像默认附加以下限制：

```text
no 3D rendering, no subtle 3D, no volumetric lighting, no glassmorphism, no glossy material, no realistic texture, no children's picture-book style, no childish cartoon, no chibi proportions, no toy-like mascot, no oversized childish eyes, no overly cute UI, no real brands, no logos, no copyrighted characters, no celebrity likeness, no real arcade machine design, no real TV show reference, no real song reference, no movie still, no anime IP, no old-photo filter, no heavy VHS noise, no dark dirty room, no dirty walls, no stained floor, no cracked old plaster, no mold, no dust, no mud, no grunge texture, no dark vignette, no abandoned space, no horror mood, no cluttered collage, no low contrast UI, no tiny unreadable UI, no photorealistic documentary style, no flat wireframe, no minimalist corporate app, no dull monochrome palette, no sterile corporate layout, no overly formal symmetry, no museum-like nostalgia display, no empty generic app template
```

中文红线：

| 禁止 | 原因 |
| --- | --- |
| 真实商标 / logo | 版权和渠道风险 |
| 大段可读文字 | AI 文字容易出错；正式文字应由 UI 后期添加 |
| 明星脸 / 影视角色 / 动画角色 | IP 风险 |
| 真实街机或游戏机造型 | 容易误碰硬件和游戏 IP |
| 86 版西游或其他具体影视造型 | 国内版权和识别风险 |
| 重 VHS、像素、旧照片滤镜 | 影响儿童接受度和 UI 可读性 |
| 暗、脏、破、恐怖氛围 | 偏离家庭亲子产品 |
| 脏墙面 / 脏地面 / 破损环境表面 | 会把轻怀旧误读成废弃、贫旧或不适合儿童进入 |
| 极简企业 App 风 | 太冷静，和参考图的高饱和游戏大厅不一致 |
| 过度正规正矩 | 缺少童年回忆的自然小物和生活感 |
| 怀旧展陈感 | 像陈列 80/90 物件，不像一个能玩的家庭游戏大厅 |

## 5. Prompt 变量表

每次生图先填变量，再拼 Prompt。

| 变量 | 填写说明 | 示例 |
| --- | --- | --- |
| `[SCENE]` | 具体场景，不写抽象主题 | after-school playground, living room arcade corner, family TV fitness room |
| `[PLAYERS]` | 家庭关系或玩家身份 | parent and child players, kids lining up, family members following TV workout |
| `[ACTION_FOCUS]` | 核心动作空间和目标物 | running lane and freeze line, basketball arcade hoop, TV workout pose area |
| `[NOSTALGIC_ANCHORS]` | 1-3 个主怀旧锚点 | school bell, chalk line, loudspeaker, score lightbox |
| `[MEMORY_DETAILS]` | 3-8 个自然冗余的小回忆物件 | stickers, tickets, cassette tape, toy handheld device, school award, enamel cup |
| `[COMPOSITION]` | 构图用途 | wide key visual, UI background with empty center, reward card layout |
| `[ASPECT]` | 比例 | 16:9, 4:3, 1:1, 9:16 |
| `[DO_NOT_SHOW]` | 本次额外禁止 | no text, no logos, no real school name |

## 6. 通用母版 Prompt

### 6.0 默认游戏中 HUD 生图

用途：生成最接近当前确认方向的单局游戏效果图、GDD 主图、玩法 HUD 概念图。除非用户明确要求其他页面，游戏效果图优先使用这个母版。

```text
Create a 16:9 premium modern family TV motion game gameplay HUD concept image for MotionX Retro Pack / Time Play.

Overall art direction: bright clean 90s Chinese everyday-memory game scene, all-ages retro motion game app, premium casual game UI, modern crisp UI-ready composition, clean-film nostalgic look, flat graphic 2D game-art style, clean color blocks and crisp outlines, natural-proportion family players, warm bright daylight, clean walls, clean floor, clean playground track or clean living room surfaces, high-saturation theme colors, polished readable flat panels, flat CTA buttons, readable hierarchy, friendly but not childish.

Scene: [SCENE].
Theme: [THEME_HALL].
Player identity: [PLAYER_IDENTITY].
Action reason: [ACTION_REASON].
Action focus: [ACTION_FOCUS].

Gameplay composition:
landscape 16:9 TV gameplay screen, open central playable action area, visible player standing zone in the foreground, clear target object, clear motion path or rhythm path, target zones readable from 3 meters away, natural-proportion family avatars demonstrating the motion, warm all-ages family-game mood.

HUD composition:
reserve at least 55% of the screen as unobstructed central and foreground playable action space,
keep the combined top HUD under about 15% of screen height: top-left readable compact score, top-center round countdown timer, top-right compact target / progress summary,
keep the bottom HUD under about 12% of screen height: show either a prominent current-action prompt or a player-status card, never both; reduce the other to a small corner cue,
use a tiny corner prop / collectible badge area only,
one primary feedback focal point at a time; combo, success and warning are brief overlays and never permanent large cards,
do not duplicate the current objective across top, center and bottom.
Use modern clean high-contrast UI. Keep text minimal; formal typography and exact copy should be added later by UI / Unity.

Nostalgic anchors: [NOSTALGIC_ANCHORS], 1-3 primary anchors only.
Memory details: [MEMORY_DETAILS], 3-5 small 80s/90s Chinese childhood objects placed near edges, corners, grass edges, card gaps or far background only.

Clean surface rule: keep walls, floors, playground track, living room floor and large background areas clean, warm, bright and breathable; nostalgia comes from props, light, color and scene memory, not grime or damage.

Use original fictional props, no real brands, no real IP, no celebrity likeness, no readable logos, no real school names.
Avoid children's picture-book style, childish cartoon, chibi proportions, toy-like mascot, oversized childish eyes, overly cute UI, copying any reference image pixel-by-pixel, vertical mobile layout, business-sim map, mini-game icon wall, gacha buttons, crowded map, tiny characters everywhere, pure illustration cover, unreadable text, dark old-photo filters, heavy VHS noise, pixel art, cluttered collage, oversized persistent HUD cards, duplicate objective panels, top-and-bottom HUD bars that crowd the action space, dirty abandoned spaces, dirty walls, stained floors, cracked old plaster, mold, dust, mud, grunge texture, dark vignette, realistic school branding, museum-like nostalgia display, minimalist corporate app style, overly formal layout.
```

### 6.1 主题馆主视觉

用途：主题馆入口、宣传图、首页大图。

```text
Bright colorful premium all-ages family game key visual, retro motion game app, flat graphic props, stylized family players with natural proportions, minimal flat highlights, bright warm daylight, clean nostalgic look, clean-film everyday-memory game illustration, clean walls and clean floor, high saturation, minimal shadows, emotionally rich memory details, curated nostalgic clutter, subtle Chinese 90s nostalgia, modern crisp UI-ready composition, friendly but not childish.

Create a wide key visual for [THEME_HALL].
Scene: [SCENE].
Players: parent and child players are about to join the activity, cheerful and energetic, not photorealistic.
Action focus: [ACTION_FOCUS] must be clearly visible in the foreground.
Nostalgic anchors: [NOSTALGIC_ANCHORS], abstract and original, used as secondary decoration.
Memory details: [MEMORY_DETAILS], naturally scattered as stickers, badges, tickets, toy props, corner decorations and collectible objects, playful but controlled.
Composition: 16:9 wide image, strong foreground action zone, readable middle area for UI overlay, warm and inviting family game atmosphere.
Clean surface rule: walls, floors, playground track, living room and large background areas stay clean and bright, with only cute light wear.
Use original fictional props, no real brands, no real IP, no celebrity likeness, no readable text, no logos.
Avoid dark old-photo filters, heavy VHS noise, pixel art, cluttered collage, horror mood, dirty abandoned spaces, dirty walls, stained floors, cracked old plaster, mold, dust, mud, dark vignette, realistic school branding, sterile perfect symmetry, museum-like nostalgia display.
```

### 6.2 首页 / 主题入口 UI 生图

用途：生成首页方向稿、主题馆入口页、电视端 16:9 大厅图或手机端纵向探索图。

```text
Bright colorful premium all-ages family TV and mobile game UI for a retro motion game app, in the style of a polished Chinese family casual game home screen, warm bright background, bright warm daylight, clean nostalgic look, clean-film everyday-memory game illustration, clean walls and clean floor, flat color blocks, tiny sparkles, sticker decorations, flat highlights, high saturation, minimal shadows, emotionally rich memory details, curated nostalgic clutter, modern crisp UI-ready composition, friendly but not childish.

Create a [16:9 TV home lobby / vertical mobile app home screen] for MotionX Retro Pack / Time Play.
Layout: stable product home structure, clear primary theme stage, large readable CTA, lightweight top utility area, theme switch controls, and bottom activity / achievement / collection modules.
For 16:9 TV: use a central main stage card with left and right theme switch arrows, large CTA, and bottom information cards.
For mobile: use stacked rounded theme cards, touch-friendly CTA buttons, and compact collection / activity modules.
Theme card 1: Playground Memories, blue-green card, after-school running track, family player in motion, school building silhouette, bright sky, clear action space.
Theme card 2: Living Room Arcade, orange card, fictional basketball arcade hoop, glowing score lightbox, basketball props, energetic arcade atmosphere.
Theme card 3: Family Fitness TV, purple card, warm living room, fictional TV workout screen, parent and child following rhythm.
UI style: large readable flat cards, clear borders, flat CTA buttons, corner tags, sticker badges, readable visual hierarchy, playful but clean, not too formal.
Clean surface rule: walls, floors, playground track, living room wooden floor and large background surfaces are clean, warm, bright and breathable; nostalgia comes from props and scene memory, not grime or damage.
Characters and props: stylized family players with natural proportions, restrained retro collectibles, original fictional props only; no mascot-like character design.
Memory details: small 80s/90s Chinese childhood objects naturally placed around card corners and module edges, like stickers, ticket stubs, cassette tapes, toy handheld device, school award, chalk marks, loudspeaker icon, enamel cup, plastic stool, family photo album, collectible badges.
Leave text areas clean for later UI typography; if text appears, keep it as simple placeholder-like blocks, no brand logos.
Avoid real brands, real IP, real arcade cabinet design, celebrity likeness, dark old-photo filters, heavy VHS noise, pixel art, cluttered collage, dirty walls, stained floors, cracked old plaster, mold, dust, mud, grunge texture, dark vignette, minimalist corporate app, dull monochrome palette, sterile corporate layout, museum-like nostalgia display, perfectly rigid grid.
```

### 6.3 主题卡片生图

用途：单独生成首页、主题详情或游戏选择中的一张主题馆卡片，便于切图、重绘或作为 UI 卡片背景。

```text
Bright colorful premium all-ages family game UI card, retro motion game app, large clean flat content card, minimal flat highlights, clear border treatment, high saturation, stylized family players with natural proportions, flat original props, emotionally rich memory details, clear readable composition, friendly but not childish.

Create one horizontal rounded theme card for [THEME_HALL].
Card color direction: [CARD_COLOR].
Scene: [SCENE].
Action focus: [ACTION_FOCUS], clearly visible and suitable for body motion gameplay.
Nostalgic anchors: [NOSTALGIC_ANCHORS], stylized and secondary.
Memory details: [MEMORY_DETAILS], small objects naturally placed around the card corners, not a museum display.
UI elements: empty title area on the left, small corner tag, pill-shaped CTA button area, subtle sparkles, sticker decorations, playful asymmetry.
Composition: character and target object should overlap the scene slightly for depth, strong foreground, clean background, suitable for a home lobby, theme detail page or game selection card.
Use original fictional props, no real brands, no real IP, no readable text, no logos.
Avoid children's picture-book style, childish cartoon, chibi proportions, toy-like mascot, overly cute UI, photorealism, old-photo filter, heavy VHS noise, cluttered collage, flat wireframe, dull colors, overly formal symmetry.
```

### 6.4 单个游戏场景概念图

用途：GDD、玩法说明、入戏开场概念。

```text
Bright colorful all-ages family retro motion game scene, premium casual game illustration, flat graphic props, stylized family players with natural proportions, minimal flat highlights, warm daylight, clean nostalgic look, clean-film everyday-memory game illustration, clean walls and clean floor, clear action space, emotionally rich memory details, subtle Chinese 90s nostalgia, modern crisp UI-ready composition, friendly but not childish.

Create a game concept scene for [GAME_NAME].
Scene: [SCENE].
Player identity: [PLAYER_IDENTITY].
Action reason: [ACTION_REASON].
Action focus: [ACTION_FOCUS], designed for body motion gameplay, clear target zones and safe movement space.
Nostalgic anchors: [NOSTALGIC_ANCHORS], subtle and original.
Memory details: [MEMORY_DETAILS], naturally integrated into the scene as small props, stickers, rewards, toys and background clues.
Composition: show where the player stands, what the player interacts with, and what success looks like. Keep background simple and readable.
Clean surface rule: keep walls, floors, tracks and room surfaces clean and bright, with only cute light wear.
Use original fictional props, no real brands, no real IP, no celebrity likeness, no readable text, no logos.
Avoid vertical mobile map as the default output, business-sim map, mini-game icon wall, gacha buttons, dark old-photo filters, heavy VHS noise, pixel art, cluttered collage, dirty walls, stained floors, cracked old plaster, mold, dust, mud, dark vignette, realistic copyrighted references, complex unreadable UI.
```

### 6.5 UI 背景图

用途：主题页、游戏选择页、结算页背景。

```text
Bright colorful premium all-ages family game UI background, retro motion game app, warm bright background, bright warm daylight, clean nostalgic look, clean-film everyday-memory game illustration, clean walls and clean floor, flat color blocks, subtle sparkles, sticker decorations, minimal flat highlights, emotionally rich memory details, subtle Chinese 90s nostalgia, modern crisp UI-ready composition, friendly but not childish.

Create a clean UI background for [SCREEN_TYPE].
Scene: [SCENE].
Nostalgic anchors: [NOSTALGIC_ANCHORS], placed around the edges and corners.
Memory details: [MEMORY_DETAILS], arranged near edges and module gaps, like a remembered desk drawer or sticker album, while preserving clean UI space.
Composition: leave clear empty areas for rounded cards, large titles, CTA buttons and score widgets, avoid visual clutter in the center, high contrast zones for interactive UI.
Mood: warm, playful, family-friendly, bright and readable.
Clean surface rule: large background surfaces stay clean and breathable; no dirty wall, stained floor, mold, dust, mud or dark vignette.
Use original fictional props, no real brands, no real IP, no readable text, no logos.
Avoid dark old-photo filters, heavy VHS noise, pixel art, cluttered collage, dirty walls, stained floors, cracked old plaster, mold, dust, mud, dark vignette, low contrast, messy details behind UI, minimalist corporate app style.
```

### 6.6 奖励卡片 / 称号卡

用途：结算后奖励卡、贴纸、徽章、分享图。

```text
Bright colorful premium all-ages family game reward card, flat graphic 2D game illustration, clean readable shapes, warm bright colors, controlled rounded geometry, flat highlights, sticker badges, emotionally rich memory details, subtle Chinese 90s nostalgia, original fictional props, friendly but not childish.

Create a reward card for [REWARD_TITLE].
Theme: [THEME_HALL].
Main visual: [MAIN_VISUAL].
Nostalgic anchors: [NOSTALGIC_ANCHORS], simple and iconic.
Memory details: [MEMORY_DETAILS], small collectible stickers, ticket stubs, stamps, badges, cassette tape or childhood album fragments around the reward.
Composition: centered badge or collectible card, clear rounded border, minimal shadow, empty space for title text added later, readable at small size, cheerful completion feeling.
Use original fictional props, no real brands, no real IP, no celebrity likeness, no readable text, no logos.
Avoid old-photo filters, heavy texture, complex tiny details, realistic certificates, copyrighted symbols, dull monochrome palette.
```

### 6.7 道具 / 图标

用途：主题馆图标、玩法图标、奖励贴纸。

```text
Bright colorful premium family game icon, flat graphic 2D object, clean silhouette, controlled rounded geometry, flat highlight, warm colors, subtle Chinese 90s nostalgia, emotionally memorable, original fictional prop; not mascot-like or overly cute.

Create an icon of [OBJECT].
Style: simple, readable at small size, game UI friendly, clean outline, soft highlight, no background clutter.
Use original fictional design, no real brands, no readable text, no logos, no copyrighted object design.
Avoid photorealism, messy details, heavy noise, dark colors, old-photo texture.
```

## 7. 三个首批主题馆 Prompt 片段

### 7.1 童年操场 / Playground Memories

```text
after-school playground, chalk lines, running lane, simple school building silhouette, playground loudspeaker, school bell feeling, team flags, warm daylight, clean playground track, clean school walls, kids and parents ready to play, clear running and freeze zones
```

避免：

```text
no real school name, no realistic uniform branding, no political slogans, no dirty abandoned school, no stained playground, no muddy floor, no cracked dirty school wall, no dark old-photo filter
```

### 7.2 客厅街机 / Living Room Arcade

```text
living room arcade corner, fictional basketball arcade hoop, score lightbox, coin-start feeling, colorful machine lights, family party atmosphere, clean living room floor, clean wall surfaces, clear challenge zone, bright and readable
```

避免：

```text
no real arcade machine design, no real game cabinet, no brand logos, no casino mood, no cyberpunk darkness, no dirty room, no stained wooden floor, no dark vignette
```

### 7.3 家庭电视健身 / Family Fitness TV

```text
warm family living room, retro-inspired fictional TV frame, workout pose preview area, colorful TV program accents, parent and child following rhythm, clean floor space, clean wall surfaces, bright morning light
```

避免：

```text
no real TV show reference, no celebrity fitness coach, no 1980s music video imitation, no dark living room, no stained floor, no dirty wall, no realistic brand electronics
```

## 8. 首批样板 Prompt 示例

### 8.1 操场木头人

```text
Bright colorful premium all-ages family game UI card, retro motion game app, large blue-green flat content card, minimal flat highlights, clear border treatment, high saturation, stylized family players with natural proportions, flat original props, clear readable composition, friendly but not childish.

Create one horizontal rounded theme card for Freeze Playground.
Scene: an after-school playground with a running lane and chalk start line.
Player identity: parent and child players are classmates joining a recess game.
Action reason: run forward when the signal starts, freeze still when the teacher looks back.
Action focus: clear running lane, freeze line, teacher gaze indicator, safe standing zones for body motion gameplay.
Nostalgic anchors: school bell, playground loudspeaker, chalk line, small team flags.
Memory details: small red scarf color accents, school award sticker, chalk dust marks, sandbag toy, hopscotch corner, enamel cup near the edge, tiny ticket stub, naturally scattered but readable.
UI elements: empty title area on the left, pill-shaped CTA button area, small hot tag in the corner, subtle sparkles and sticker decorations, playful asymmetry.
Composition: character running across the foreground, simple school building silhouette and bright sky in the background, suitable for a home lobby, theme detail page or game selection card.
Use original fictional props, no real brands, no real IP, no celebrity likeness, no readable text, no logos.
Avoid dark old-photo filters, heavy VHS noise, pixel art, dirty abandoned school, realistic school branding, flat wireframe, dull colors.
```

### 8.2 电玩城投篮机

```text
Bright colorful premium all-ages family game UI card, retro motion game app, large orange flat content card, minimal flat highlights, clear border, high saturation, flat graphic arcade props, clear readable composition, friendly but not childish.

Create one horizontal rounded theme card for Arcade Shootout.
Scene: a cozy living room arcade corner with a fictional basketball arcade hoop.
Player identity: parent and child players are taking turns to challenge the high score.
Action reason: swing arms and reach toward the target to score within the countdown.
Action focus: basketball hoop target, score lightbox, safe player standing zone, clear throw direction.
Nostalgic anchors: coin-start feeling, colorful lightbox, simple score board, party spectators.
Memory details: toy game coins, ticket stubs, cassette tape sticker, plastic basketball, small score coupon, old-school button stickers, naturally placed around the card edges.
UI elements: empty title area on the left, flat CTA button area, blue challenge tag in the corner, small sticker decorations, playful asymmetry.
Composition: glowing fictional arcade hoop and basketball props on the right, warm orange light, clean left-side area for title and button, suitable for a home lobby, theme detail page or game selection card.
Use original fictional props, no real brands, no real IP, no real arcade cabinet design, no readable text, no logos.
Avoid cyberpunk darkness, casino mood, heavy VHS noise, cluttered machines, real arcade cabinet design, flat wireframe, dull colors.
```

### 8.3 家庭电视健美操

```text
Bright colorful premium all-ages family game UI card, retro motion game app, large purple flat content card, minimal flat highlights, clear border treatment, high saturation, stylized family characters with natural proportions, flat original props, clear readable composition, friendly but not childish.

Create one horizontal rounded theme card for Family Fitness TV.
Scene: a warm family living room with a fictional retro-inspired TV workout program.
Player identity: family members are standing in front of the TV and following the rhythm together.
Action reason: raise knees, swing arms, squat and clap to match the TV workout beat.
Action focus: clear floor standing zones, large motion preview area on the TV, simple rhythm indicators.
Nostalgic anchors: TV static feeling, colorful program accents, soft floor mat, family morning exercise mood.
Memory details: toy remote control, cassette tape, family photo album corner, plastic stool, small exercise badge, TV program sticker, tiny music-note stickers, naturally placed around the card edges.
UI elements: empty title area on the left, flat CTA button area, green energy recommendation tag, tiny music-note and sticker decorations, playful asymmetry.
Composition: child watching and following a fictional TV workout on the right, cozy living room depth, clean left-side area for title and button, suitable for a home lobby, theme detail page or game selection card.
Use original fictional props, no real brands, no real IP, no celebrity coach, no real TV show reference, no readable text, no logos.
Avoid dark living room, music video imitation, heavy old-photo filter, cluttered furniture, real TV show reference, flat wireframe, dull colors.
```

## 9. 一致性生产流程

每批图像按以下顺序生产：

1. 先定用途：主题馆主视觉 / 游戏概念图 / UI 背景 / 奖励卡 / 图标。
2. 填写变量表：场景、玩家身份、动作焦点、怀旧锚点、构图比例。
3. 使用对应母版 Prompt。
4. 附加统一负面 Prompt。
5. 同一批图尽量保持相同比例、光照、画风和构图密度。
6. 游戏中效果图优先对照 `references/approved-visual-targets/playground-gameplay-hud-approved-20260630.png` 检查横版体感 HUD 骨架、动作区和 UI 可读性。
7. 每张图生成后按验收清单筛选，不合格则改 Prompt，不直接拿去做视觉标准。

## 10. 验收清单

| 检查项 | 通过标准 |
| --- | --- |
| 是否像同一个产品 | 平面画风、亮度、图形语言、构图层级一致，接近高饱和现代家庭游戏大厅 |
| 是否贴近确认目标图 | 游戏中效果图应接近已确认的横版体感 HUD：至少 55% 无阻挡动作区、清晰关键分数与倒计时、紧凑目标摘要、站位区、边缘轻怀旧；不是所有模块都放大 |
| 是否明亮清楚 | 不是暗黄、脏旧、重噪点 |
| 环境表面是否干净 | 墙面、地面、跑道、客厅和大面积背景清爽暖亮，不靠污渍、灰尘、破损表达怀旧 |
| 是否像可用 UI | 有清晰主 CTA、导航 / 返回、圆角卡片、主题入口、状态信息或收集入口 |
| 是否有平面图形品质 | 明亮暖光底色、强主题分色、平面小物、清晰边界、有限阴影和高对比按钮成立 |
| 是否有可玩空间 | 看得出玩家站哪里、动哪里、目标是什么 |
| 是否轻怀旧 | 有 1-3 个记忆锚点，但没有旧物堆叠 |
| 是否全龄且不低幼 | 亲和、温暖、没有压抑或破败感；同时没有绘本、幼态角色、玩具化吉祥物或过度卖萌 UI |
| 是否 UI 可用 | 中央或关键区域能承载按钮、标题、分数 |
| 是否版权安全 | 没有真实 IP、logo、明星、影视造型、真实街机 |
| 是否无错误文字 | 图中不生成可读文字；文字后期由 UI 加 |
| 是否不像换皮 | 场景、动作理由和目标物能说明这个游戏的不同 |

## 11. 不合格图像的常见修正

| 问题 | Prompt 修正 |
| --- | --- |
| 太旧、太脏 | 加 `bright, clean, modern UI-ready, warm daylight, clean nostalgic look, clean-film childhood memory illustration`，去掉 `vintage photo`, `grunge`, `aged paper`, `dirty wall`, `stained floor`, `dark vignette` |
| 墙面或地面脏旧 | 加 `clean walls, clean floor, clean playground track, clean living room surfaces, fresh air, only cute light wear`，避免 `grime`, `dust`, `mold`, `mud`, `cracked old plaster` |
| 太像真实照片 | 加 `stylized 2.5D game illustration, soft rounded geometry, not photorealistic` |
| 太像普通概念场景图 | 加 `premium family game UI, rounded cards, clear CTA buttons, corner tags, activity or collection modules` |
| 不像参考图的高饱和 UI | 加 `cream warm background, high saturation theme colors, flat CTA buttons, sticker badges, flat graphic collectibles, minimal shadows` |
| 画面太乱 | 加 `simple background, clear action space, no clutter, UI overlay area` |
| 复古物件抢戏 | 加 `nostalgic anchors as secondary decoration, action focus in foreground` |
| 有文字乱码 | 加 `no readable text, no typography, no labels, no logo` |
| 像真实 IP | 加 `original fictional design, no copyrighted characters, no real brands, no recognizable arcade cabinet` |
| 不像体感游戏 | 加 `clear standing zone, body motion gameplay, target zones, safe movement space` |
| 太像竖版地图 / 手游入口 | 加 `landscape 16:9 TV gameplay HUD, reserve at least 55% unobstructed action space, compact score and countdown timer, compact target progress summary, visible standing zone, no business-sim map, no mini-game icon wall, no gacha buttons` |
| HUD 模块都太大 | 加 `restrained gameplay HUD, top HUD under 15% and bottom HUD under 12% of screen height, only one primary feedback focal point, no duplicate objective panels, brief combo overlay only` |
| 太像极简工具 App | 加 `playful kid-friendly family game lobby, colorful cards, cute mascot, sparkles, badges, collectible toys` |

## 12. 最终统一口径

> AI 生图不是为了生成“怀旧图片”，而是为了稳定生成 `MotionX Retro Pack` 的可玩场景、UI 背景和情绪锚点。  
> 画面必须明亮、清楚、亲子、原创；怀旧只做记忆提示，不能压过动作空间和玩法目标。
