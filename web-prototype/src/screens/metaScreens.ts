import { Container, Graphics, Rectangle } from "pixi.js";
import { avatarOptions, getAvatarOption, palette, type RenderContext, type SettingsGroupId } from "../domain";
import { avatarSymbol, centerLabel, primaryButton, secondaryButton, themeButton, header, label, rect } from "../pixiPrimitives";
import { ui } from "../uiStyle";

export function drawCollection(stage: Container, ctx: RenderContext) {
  header(stage, ctx, "我的", "", true);
  stage.addChild(secondaryButton("设置", 820, 26, 66, 38, () => ctx.actions.nav("settings")));

  const scrollMask = new Graphics().rect(28, 86, 904, 444).fill(0xffffff);
  const content = new Container();
  content.y = -ctx.state.collectionScroll;
  content.mask = scrollMask;
  stage.addChild(scrollMask);
  stage.addChild(content);

  const selectedAvatar = getAvatarOption(ctx.state.selectedAvatar);
  content.addChild(rect(54, 92, 852, 86, palette.white, palette.line, ui.radius.md));
  content.addChild(avatarSymbol(82, 108, 52, ctx.state.selectedAvatar));
  content.addChild(label(selectedAvatar.label, 154, 106, 20, palette.ink, { fontWeight: ui.font.weight.bold }));
  content.addChild(label("家庭小队：小队长 + 爸爸 + 妈妈", 154, 132, 16, palette.ink, { fontWeight: ui.font.weight.bold }));
  content.addChild(secondaryButton("更换头像", 760, 122, 116, 34, () => ctx.actions.openAvatarPicker()));

  content.addChild(label("最近收获", 54, 202, 17, palette.ink, { fontWeight: ui.font.weight.bold }));
  content.addChild(rect(54, 226, 500, 126, palette.white, palette.line, ui.radius.md));
  content.addChild(rect(86, 250, 116, 76, ui.color.memoryPaper, palette.line, ui.radius.md));
  content.addChild(centerLabel("小卖部\n汽水贴纸", 144, 288, 17, palette.ink, { fontWeight: ui.font.weight.bold, align: "center", lineHeight: 22 }));
  content.addChild(label("今天解锁：课间补给站", 226, 252, 18, palette.ink, { fontWeight: ui.font.weight.bold }));
  content.addChild(label("来自：童年操场 / 操场木头人", 226, 282, 14, palette.muted, { fontWeight: ui.font.weight.semibold }));
  content.addChild(secondaryButton("查看详情 >", 408, 308, 118, 30, () => ctx.actions.nav("reward")));

  drawMiniReward(content, 584, 226, "课间十分钟徽章", "完成任意操场游戏");
  drawMiniReward(content, 584, 296, "课间冲刺王称号", "跑停表现高档");

  content.addChild(label("我的收藏", 54, 378, 17, palette.ink, { fontWeight: ui.font.weight.bold }));
  content.addChild(rect(54, 402, 852, 44, palette.white, palette.line, ui.radius.md));
  content.addChild(label("贴纸册 6 / 36", 82, 415, 15, palette.ink, { fontWeight: ui.font.weight.bold }));
  content.addChild(label("徽章 2 / 12", 262, 415, 15, palette.ink, { fontWeight: ui.font.weight.bold }));
  content.addChild(label("称号 3 / 20", 430, 415, 15, palette.ink, { fontWeight: ui.font.weight.bold }));
  content.addChild(primaryButton("查看全部 >", 764, 408, 112, 30, () => ctx.actions.nav("collection")));

  content.addChild(label("主题进度", 54, 466, 17, palette.ink, { fontWeight: ui.font.weight.bold }));
  drawThemeProgress(content, 54, 490, "童年操场", "贴纸 3/12", "徽章 1/4", palette.court);
  drawThemeProgress(content, 340, 490, "客厅街机", "贴纸 2/12", "徽章 1/4", palette.arcade);
  drawThemeProgress(content, 626, 490, "家庭节拍馆", "贴纸 1/12", "徽章 0/4", palette.tv);

  content.addChild(label("家庭成就", 54, 562, 17, palette.ink, { fontWeight: ui.font.weight.bold }));
  content.addChild(rect(54, 586, 852, 52, palette.chalk, palette.line, ui.radius.md));
  content.addChild(label("爸妈带玩徽章：已获得", 82, 602, 14, palette.ink, { fontWeight: ui.font.weight.bold }));
  content.addChild(label("家庭再来一局：还差 1 局", 312, 602, 14, palette.ink, { fontWeight: ui.font.weight.bold }));
  content.addChild(secondaryButton("查看 >", 792, 596, 84, 30, () => ctx.actions.nav("collection")));

  content.addChild(label("服务入口", 54, 668, 17, palette.ink, { fontWeight: ui.font.weight.bold }));
  content.addChild(rect(54, 692, 852, 46, palette.white, palette.line, ui.radius.md));
  content.addChild(label("账号与设备、客服反馈、协议与说明都在右上角设置中管理", 82, 706, 14, palette.muted, { fontWeight: ui.font.weight.semibold }));
  content.addChild(secondaryButton("去设置 >", 792, 700, 84, 30, () => ctx.actions.nav("settings")));

  drawScrollBar(stage, ctx.state.collectionScroll);
  if (ctx.state.avatarPickerOpen) drawAvatarPicker(stage, ctx);
}

export function drawHelp(stage: Container, ctx: RenderContext) {
  header(stage, ctx, "帮助中心", "站位、问题和家长放心", true);
  stage.addChild(label("卡住时先看这里：站得清楚、光线够亮、家人别互相挡住。", 82, 100, 17, palette.ink, { fontWeight: "800" }));
  stage.addChild(label("玩法教程留在主题页，这里只解决上场前最常见的问题。", 82, 128, 14, palette.muted, { fontWeight: "700" }));

  stage.addChild(rect(64, 168, 392, 218, palette.white, palette.line, ui.radius.md));
  stage.addChild(label("上场前自查", 92, 192, 22, palette.ink, { fontWeight: ui.font.weight.bold }));
  stage.addChild(label("像拍全家合照一样站好", 92, 224, 15, palette.muted, { fontWeight: ui.font.weight.semibold }));
  drawStandingGuide(stage, 96, 258);
  drawCheckLine(stage, 268, 266, "横放手机，全身入镜", palette.green);
  drawCheckLine(stage, 268, 302, "光线明亮，避免背光", palette.green);
  drawCheckLine(stage, 268, 338, "多人拉开，不互挡", palette.green);

  stage.addChild(rect(492, 168, 404, 218, palette.white, palette.line, ui.radius.md));
  stage.addChild(label("遇到问题", 520, 192, 22, palette.ink, { fontWeight: ui.font.weight.bold }));
  drawHelpIssue(stage, 520, 238, "看不到人", "后退一步，确认头到脚都在画面里。");
  drawHelpIssue(stage, 520, 286, "动作不准", "站到画面中间，动作做大一点。");
  drawHelpIssue(stage, 520, 334, "声音太大", "去设置调整音乐和音效音量。");

  stage.addChild(rect(64, 416, 586, 58, ui.color.memoryPaper, palette.line, ui.radius.md, 0.96));
  stage.addChild(label("家长放心", 92, 432, 17, palette.ink, { fontWeight: ui.font.weight.bold }));
  stage.addChild(label("摄像头只用于动作判断，不保存原始画面；奖励只能游玩获得。", 190, 434, 14, palette.muted, { fontWeight: ui.font.weight.semibold }));
  stage.addChild(secondaryButton("去设置", 704, 426, 96, 38, () => ctx.actions.nav("settings")));
}

export function drawSettings(stage: Container, ctx: RenderContext) {
  const selectedGroup = settingsGroups.find((group) => group.id === ctx.state.selectedSettingsGroup) ?? settingsGroups[0];
  header(stage, ctx, "设置", "", true);

  const { navX, panelX, contentY, navW, panelW, panelH, rowH, rowStep } = ui.layout.settings;
  stage.addChild(rect(navX, contentY, navW, panelH, palette.white, palette.line, ui.radius.md));
  stage.addChild(label("设置分组", navX + 28, contentY + 24, 17, palette.muted, { fontWeight: ui.font.weight.semibold }));
  settingsGroups.forEach((item, index) => {
    const active = item.id === selectedGroup.id;
    const y = contentY + 62 + index * 44;
    stage.addChild(label((active ? "> " : "  ") + item.title, navX + 48, y, 16, active ? palette.ink : palette.muted, { fontWeight: ui.font.weight.semibold }));
    const hit = new Container();
    hit.eventMode = "static";
    hit.cursor = "pointer";
    hit.hitArea = new Rectangle(navX + 18, y - 10, 178, 36);
    hit.on("pointertap", () => ctx.actions.selectSettingsGroup(item.id));
    stage.addChild(hit);
  });

  stage.addChild(rect(panelX, contentY, panelW, panelH, palette.white, palette.line, ui.radius.md));
  stage.addChild(label(selectedGroup.title, panelX + 30, contentY + 24, 23, palette.ink, { fontWeight: ui.font.weight.bold }));
  selectedGroup.items.forEach((item, index) => {
    const y = contentY + 76 + index * rowStep;
    stage.addChild(rect(panelX + 24, y - 10, 548, rowH, palette.chalk, palette.chalk, ui.radius.sm));
    const { title, desc, control } = item;
    const titleSize = title.length > 6 ? 14 : 16;
    stage.addChild(label(title, panelX + 42, y - 2, titleSize, palette.ink, { fontWeight: ui.font.weight.bold }));
    stage.addChild(
      label(desc, panelX + 42, y + 21, 13, palette.muted, {
        wordWrap: true,
        wordWrapWidth: 342,
        lineHeight: 17,
      }),
    );
    if (control.type === "segmented") {
      drawSegmented(stage, panelX + 380, y - 3, control.options, control.value);
    }
    if (control.type === "slider") {
      drawSlider(stage, panelX + 408, y - 2, control.value);
    }
    if (control.type === "link") {
      drawSmallAction(stage, panelX + 452, y - 3, control.label);
    }
    if (control.type === "switch" || control.type === "lockedSwitch") {
      drawSwitch(stage, panelX + 452, y - 3, control);
    }
    if (control.type === "status") {
      drawStatusPill(stage, panelX + 452, y - 3, control.label);
    }
    if (control.type === "action") {
      drawSmallAction(stage, panelX + 452, y - 3, control.label);
    }
  });
}

function drawStandingGuide(stage: Container, x: number, y: number) {
  stage.addChild(rect(x, y, 148, 96, ui.color.cameraDark, palette.line, ui.radius.md));
  stage.addChild(rect(x + 20, y + 16, 108, 64, palette.cream, palette.chalk, ui.radius.sm, 0.12));
  stage.addChild(new Graphics().circle(x + 74, y + 34, 10).fill(palette.chalk));
  stage.addChild(new Graphics().roundRect(x + 58, y + 48, 32, 30, 12).fill(palette.chalk));
  stage.addChild(new Graphics().moveTo(x + 46, y + 82).lineTo(x + 106, y + 82).stroke({ width: 3, color: palette.court }));
  stage.addChild(label("全身", x + 62, y + 64, 11, palette.ink, { fontWeight: ui.font.weight.bold }));
}

function drawCheckLine(stage: Container, x: number, y: number, text: string, color: number) {
  stage.addChild(new Graphics().circle(x, y + 8, 7).fill(color).stroke({ width: ui.stroke.thin, color: palette.line }));
  stage.addChild(label("✓", x - 4, y + 1, 11, palette.white, { fontWeight: ui.font.weight.bold }));
  stage.addChild(label(text, x + 16, y + 1, 12, palette.ink, { fontWeight: ui.font.weight.semibold }));
}

function drawHelpIssue(stage: Container, x: number, y: number, title: string, desc: string) {
  stage.addChild(rect(x, y - 8, 334, 40, palette.chalk, palette.chalk, ui.radius.sm));
  stage.addChild(label(title, x + 18, y, 15, palette.ink, { fontWeight: ui.font.weight.bold }));
  stage.addChild(label(desc, x + 118, y + 1, 13, palette.muted, { fontWeight: ui.font.weight.medium }));
}

function drawAvatarPicker(stage: Container, ctx: RenderContext) {
  stage.addChild(new Graphics().rect(0, 0, 960, 540).fill({ color: palette.shadow, alpha: 0.34 }));
  stage.addChild(rect(270, 118, 420, 304, palette.white, palette.line, ui.radius.lg));
  stage.addChild(centerLabel("更换头像", 480, 154, 24, palette.ink, { fontWeight: ui.font.weight.bold }));
  stage.addChild(centerLabel("选择一个家庭头像", 480, 186, 15, palette.muted, { fontWeight: ui.font.weight.semibold }));

  avatarOptions.forEach((avatar, index) => {
    const col = index % 3;
    const row = Math.floor(index / 3);
    const x = 310 + col * 122;
    const y = 214 + row * 76;
    const active = avatar.id === ctx.state.pendingAvatar;
    stage.addChild(rect(x, y, 96, 58, active ? ui.color.memoryPaper : palette.chalk, active ? avatar.color : palette.line, ui.radius.md, 0.98));
    stage.addChild(avatarSymbol(x + 12, y + 10, 38, avatar.id));
    stage.addChild(label(avatar.label, x + 56, y + 20, 13, active ? palette.ink : palette.muted, { fontWeight: ui.font.weight.bold }));

    const hit = new Container();
    hit.eventMode = "static";
    hit.cursor = "pointer";
    hit.hitArea = new Rectangle(x, y, 96, 58);
    hit.on("pointertap", () => ctx.actions.chooseAvatar(avatar.id));
    stage.addChild(hit);
  });

  stage.addChild(secondaryButton("取消", 346, 360, 112, 42, () => ctx.actions.closeAvatarPicker()));
  stage.addChild(themeButton("保存", 502, 360, 112, 42, () => ctx.actions.saveAvatar(), getAvatarOption(ctx.state.pendingAvatar).color));
}

type SettingControl =
  | { type: "switch"; value: boolean }
  | { type: "lockedSwitch"; value: boolean }
  | { type: "slider"; value: number }
  | { type: "segmented"; options: string[]; value: string }
  | { type: "link"; label: string }
  | { type: "status"; label: string }
  | { type: "action"; label: string };

type SettingItem = {
  title: string;
  desc: string;
  control: SettingControl;
};

type SettingsGroup = {
  id: SettingsGroupId;
  title: string;
  items: SettingItem[];
};

const settingsGroups: SettingsGroup[] = [
  {
    id: "family",
    title: "家庭保护",
    items: [
      { title: "适龄内容", desc: "隐藏高强度和不适合低龄儿童的内容。", control: { type: "switch", value: true } },
      { title: "休息提醒", desc: "连续游玩后提醒喝水休息，不强制中断。", control: { type: "switch", value: true } },
      { title: "奖励保护", desc: "贴纸、徽章、称号和奖励卡片只能游玩获得。", control: { type: "lockedSwitch", value: true } },
      { title: "外部链接家长确认", desc: "打开活动页、购买页或外部链接前做确认。", control: { type: "switch", value: true } },
    ],
  },
  {
    id: "experience",
    title: "体验设置",
    items: [
      { title: "提示字大小", desc: "游戏中动作提示和倒计时使用大字号。", control: { type: "segmented", options: ["小", "中", "大"], value: "大" } },
      { title: "音乐音量", desc: "保留热闹氛围，不压过语音提示。", control: { type: "slider", value: 70 } },
      { title: "音效音量", desc: "投币、铃声、鼓点等反馈适中。", control: { type: "slider", value: 80 } },
      { title: "低闪烁显示", desc: "减少快速闪白和高频闪烁效果。", control: { type: "switch", value: true } },
    ],
  },
  {
    id: "privacy",
    title: "权限与隐私",
    items: [
      { title: "摄像头", desc: "用于站位和动作判断，不保存原始画面。", control: { type: "switch", value: true } },
      { title: "成绩卡分享", desc: "默认只分享称号、贴纸和分数摘要。", control: { type: "switch", value: true } },
      { title: "第三方清单", desc: "查看第三方 SDK 与信息共享清单。", control: { type: "link", label: "查看" } },
      { title: "昵称展示", desc: "家庭昵称可隐藏或改为默认称呼。", control: { type: "switch", value: true } },
    ],
  },
  {
    id: "account",
    title: "账号与设备",
    items: [
      { title: "当前账号", desc: "小队长    手机号 138****0000", control: { type: "status", label: "已登录" } },
      { title: "切换账号", desc: "更换手机号或家庭资料前会保存当前进度。", control: { type: "action", label: "切换" } },
      { title: "退出游戏", desc: "离开前会确认，避免误触中断家庭游戏。", control: { type: "action", label: "退出" } },
      { title: "注销账号", desc: "高风险操作，需要二次确认。", control: { type: "action", label: "注销" } },
    ],
  },
  {
    id: "legal",
    title: "协议与说明",
    items: [
      { title: "用户服务协议", desc: "查看账号、游客体验和服务条款。", control: { type: "link", label: "查看" } },
      { title: "隐私政策", desc: "查看个人信息处理和权限说明。", control: { type: "link", label: "查看" } },
      { title: "儿童隐私保护协议", desc: "查看儿童个人信息保护说明。", control: { type: "link", label: "查看" } },
      { title: "第三方清单", desc: "查看第三方 SDK 与信息共享清单。", control: { type: "link", label: "查看" } },
    ],
  },
  {
    id: "about",
    title: "关于与支持",
    items: [
      { title: "当前版本", desc: "MotionX Retro Pack 低保真原型。", control: { type: "status", label: "0.1" } },
      { title: "问题反馈", desc: "收集体验问题和设备适配问题。", control: { type: "action", label: "提交" } },
      { title: "客服支持", desc: "正式版接入客服与帮助中心。", control: { type: "status", label: "待接入" } },
      { title: "资质信息", desc: "登录页展示备案、出版和公司信息。", control: { type: "status", label: "已展示" } },
    ],
  },
];

function drawSmallAction(stage: Container, x: number, y: number, text: string) {
  const { width, height } = ui.component.settingsControl;
  stage.addChild(rect(x, y, width, height, palette.paper, palette.line, ui.radius.md));
  stage.addChild(centerLabel(text, x + width / 2, y + height / 2, ui.font.size.chip, palette.green, { fontWeight: ui.font.weight.bold, align: "center" }));
}

function drawStatusPill(stage: Container, x: number, y: number, text: string) {
  const { width, height } = ui.component.settingsControl;
  stage.addChild(rect(x, y, width, height, palette.chalk, palette.line, ui.radius.md));
  stage.addChild(centerLabel(text, x + width / 2, y + height / 2, ui.font.size.chip, palette.muted, { fontWeight: ui.font.weight.bold, align: "center" }));
}

function drawSlider(stage: Container, x: number, y: number, value: number) {
  const safePercent = Math.max(0, Math.min(100, value));
  const trackWidth = ui.component.settingsControl.sliderWidth;
  const trackH = ui.component.settingsControl.sliderH;
  const knobR = ui.component.settingsControl.sliderKnob;
  const fillWidth = Math.round((trackWidth * safePercent) / 100);
  const knobX = x + fillWidth;

  stage.addChild(rect(x, y + 11, trackWidth, trackH, ui.color.sliderTrack, palette.line, ui.radius.xs));
  stage.addChild(rect(x, y + 11, fillWidth, trackH, palette.green, palette.green, ui.radius.xs));
  stage.addChild(new Graphics().circle(knobX, y + 15, knobR).fill(palette.white).stroke({ width: ui.stroke.default, color: palette.line }));
  stage.addChild(centerLabel(`${safePercent}%`, x + trackWidth + 38, y + 15, ui.font.size.chip, palette.muted, { fontWeight: ui.font.weight.bold, align: "center" }));
}

function drawSegmented(stage: Container, x: number, y: number, options: string[], active: string) {
  const segmentWidth = ui.component.settingsControl.segmentWidth;
  const totalWidth = segmentWidth * options.length;
  stage.addChild(rect(x, y, totalWidth, ui.component.settingsControl.height, palette.paper, palette.line, ui.radius.md));
  options.forEach((option, index) => {
    const activeOption = option === active;
    const segmentX = x + index * segmentWidth;
    if (activeOption) {
      stage.addChild(rect(segmentX + 3, y + 3, segmentWidth - 6, 24, palette.green, palette.green, ui.radius.sm));
    }
    stage.addChild(
      centerLabel(option, segmentX + segmentWidth / 2, y + ui.component.settingsControl.height / 2, ui.font.size.chip, activeOption ? palette.white : palette.muted, {
        fontWeight: ui.font.weight.bold,
        align: "center",
      }),
    );
  });
}

function drawSwitch(stage: Container, x: number, y: number, control: Extract<SettingControl, { type: "switch" | "lockedSwitch" }>) {
  const locked = control.type === "lockedSwitch";
  const isOn = control.value;
  const trackColor = locked ? palette.yellow : isOn ? palette.green : palette.paper;
  const textColor = locked || isOn ? palette.white : palette.muted;
  const knobX = isOn ? x + 58 : x + 14;
  const display = locked ? "锁定" : isOn ? "开" : "关";

  stage.addChild(rect(x, y, ui.component.settingsControl.width, ui.component.settingsControl.height, trackColor, palette.line, ui.radius.pill));
  stage.addChild(new Graphics().circle(knobX, y + 15, ui.component.settingsControl.switchKnob).fill(palette.white).stroke({ width: ui.stroke.default, color: palette.line }));
  stage.addChild(centerLabel(display, isOn ? x + 27 : x + 58, y + ui.component.settingsControl.height / 2, ui.font.size.caption, textColor, { fontWeight: ui.font.weight.bold, align: "center" }));
}

function drawMiniReward(stage: Container, x: number, y: number, title: string, desc: string) {
  stage.addChild(rect(x, y, 322, 56, palette.white, palette.line, ui.radius.md));
  stage.addChild(rect(x + 16, y + 12, 34, 32, ui.color.memoryPaper, palette.line, ui.radius.sm));
  stage.addChild(label(title, x + 66, y + 10, 15, palette.ink, { fontWeight: ui.font.weight.bold }));
  stage.addChild(label(desc, x + 66, y + 34, 13, palette.muted, { fontWeight: ui.font.weight.semibold }));
}

function drawThemeProgress(stage: Container, x: number, y: number, title: string, sticker: string, badge: string, color: number) {
  stage.addChild(rect(x, y, 260, 42, palette.white, palette.line, ui.radius.md));
  stage.addChild(new Graphics().circle(x + 20, y + 21, 8).fill(color).stroke({ width: ui.stroke.thin, color: palette.line }));
  stage.addChild(label(title, x + 38, y + 8, 14, palette.ink, { fontWeight: ui.font.weight.bold }));
  stage.addChild(label(`${sticker}    ${badge}`, x + 118, y + 10, 12, palette.muted, { fontWeight: ui.font.weight.semibold }));
}

function drawScrollBar(stage: Container, scroll: number) {
  const trackX = 918;
  const trackY = 96;
  const trackH = 418;
  const thumbH = 108;
  const maxScroll = 228;
  const ratio = maxScroll <= 0 ? 0 : Math.max(0, Math.min(1, scroll / maxScroll));
  const thumbY = trackY + (trackH - thumbH) * ratio;
  stage.addChild(rect(trackX, trackY, 6, trackH, 0xe6ddc9, 0xe6ddc9, ui.radius.pill, 0.75));
  stage.addChild(rect(trackX - 1, thumbY, 8, thumbH, palette.muted, palette.muted, ui.radius.pill, 0.86));
}

function drawCardRow(stage: Container, title: string, y: number, cards: string[][]) {
  stage.addChild(label(title, 54, y - 24, 16, palette.ink, { fontWeight: "900" }));
  cards.forEach(([cardTitle, line1, line2], index) => {
    const x = 54 + index * 286;
    stage.addChild(rect(x, y, 250, 74, palette.white, palette.line, 8));
    stage.addChild(label(cardTitle, x + 16, y + 12, 15, palette.ink, { fontWeight: "900" }));
    stage.addChild(label(line1, x + 16, y + 36, 13, palette.muted));
    if (line2) stage.addChild(label(line2, x + 16, y + 56, 13, palette.blue, { fontWeight: "800" }));
  });
}
