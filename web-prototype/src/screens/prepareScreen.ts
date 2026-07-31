import { Container, Graphics, Rectangle } from "pixi.js";
import { palette, type RenderContext } from "../domain";
import { centerLabel, label, line, rect, secondaryButton, themeButton } from "../pixiPrimitives";
import { ui } from "../uiStyle";

export function drawPrepare(stage: Container, ctx: RenderContext) {
  const theme = ctx.state.selectedTheme;
  drawPlaygroundReadyBackdrop(stage);
  stage.addChild(secondaryButton("<", 28, 24, 42, 38, () => ctx.actions.nav("theme")));
  stage.addChild(secondaryButton("?", 878, 24, 42, 38, () => ctx.actions.nav("help")));
  stage.addChild(rect(92, 18, 206, 46, 0xf7dfac, palette.line, ui.radius.md, 0.96));
  stage.addChild(centerLabel("操场木头人", 195, 41, 22, palette.ink, { fontWeight: "900" }));
  stage.addChild(centerLabel("叮铃铃，下课了", 500, 62, 32, theme.color, { fontWeight: "900" }));
  stage.addChild(rect(324, 92, 312, 34, 0xf4deb5, palette.line, ui.radius.sm, 0.96));
  stage.addChild(centerLabel("站到粉笔线后，准备开跑", 480, 109, 17, palette.ink, { fontWeight: "900" }));

  drawStandingZone(stage, 232, 154);
  drawReadyCards(stage, 232, 372, theme.color);
  drawCountdownCard(stage, 410, 432, ctx, theme.color);
}

export function drawRecognition(stage: Container, ctx: RenderContext) {
  drawPrepare(stage, ctx);
}

function drawSchoolyardMood(stage: Container, accent: number) {
  stage.addChild(new Graphics().circle(126, 124, 24).fill({ color: palette.yellow, alpha: 0.22 }));
  stage.addChild(new Graphics().circle(828, 160, 34).fill({ color: palette.arcade, alpha: 0.16 }));
  stage.addChild(rect(86, 416, 118, 22, accent, palette.line, 11, 0.14));
  stage.addChild(rect(730, 116, 108, 22, palette.court, palette.line, 11, 0.18));
  stage.addChild(line(126, 366, 254, 322, ui.color.dividerSoft, 4));
  stage.addChild(line(706, 334, 834, 380, ui.color.dividerSoft, 4));
  stage.addChild(line(132, 154, 828, 154, ui.color.dividerSoft, 3));
}

function drawPlaygroundReadyBackdrop(stage: Container) {
  stage.addChild(rect(18, 16, 924, 492, 0x83caef, palette.line, ui.radius.lg));
  stage.addChild(new Graphics().circle(760, 64, 24).fill({ color: palette.white, alpha: 0.55 }));
  stage.addChild(new Graphics().circle(794, 58, 34).fill({ color: palette.white, alpha: 0.52 }));
  stage.addChild(new Graphics().circle(832, 66, 22).fill({ color: palette.white, alpha: 0.45 }));
  stage.addChild(rect(18, 330, 924, 178, 0xc7834d, 0xc7834d, 0, 0.58));
  stage.addChild(rect(18, 430, 924, 78, 0x8fb46f, 0x8fb46f, 0, 0.45));
  stage.addChild(rect(666, 112, 190, 56, 0xf5d6a7, palette.line, ui.radius.sm, 0.92));
  stage.addChild(centerLabel("教学楼", 760, 140, 20, palette.ink, { fontWeight: "900" }));
  stage.addChild(line(62, 338, 900, 310, palette.chalk, 3));
  stage.addChild(line(90, 370, 860, 318, palette.chalk, 3));
  stage.addChild(line(122, 402, 840, 330, palette.chalk, 3));
  stage.addChild(rect(846, 160, 48, 122, palette.red, palette.line, ui.radius.sm, 0.82));
  stage.addChild(centerLabel("三(2)班", 870, 198, 15, palette.yellow, { fontWeight: "900" }));
}

function drawPlayerPanels(stage: Container, x: number, y: number, accent: number) {
  stage.addChild(rect(x - 20, y - 18, 668, 228, palette.paper, palette.line, ui.radius.xl, 0.98));
  const players = [
    { name: "玩家A", status: "已站好", color: palette.blue, shirt: palette.blue, skin: 0xf1c59d },
    { name: "玩家B", status: "已站好", color: palette.red, shirt: palette.court, skin: 0xf4c99d },
    { name: "玩家C", status: "已站好", color: palette.green, shirt: palette.green, skin: 0xf1c59d },
  ];
  players.forEach((player, index) => {
    const panelX = x + index * 218;
    stage.addChild(rect(panelX, y, 202, 164, 0xfff0d2, palette.line, ui.radius.lg, 0.96));
    drawRoomPanel(stage, panelX + 10, y + 10, 182, 120, player.shirt, player.skin);
    stage.addChild(new Graphics().circle(panelX + 20, y + 20, 6).fill(palette.green).stroke({ width: 1, color: palette.line }));
    stage.addChild(rect(panelX + 54, y + 132, 96, 32, player.color, palette.line, ui.radius.pill, 0.92));
    stage.addChild(centerLabel(player.name.replace("玩家", ""), panelX + 102, y + 148, 18, palette.white, { fontWeight: "900" }));
  });
}

function drawRoomPanel(stage: Container, x: number, y: number, width: number, height: number, shirt: number, skin: number) {
  stage.addChild(rect(x, y, width, height, 0xf0d2a7, 0xf0d2a7, ui.radius.md, 0.9));
  stage.addChild(rect(x + 12, y + 12, 48, 84, 0xc9905d, palette.line, ui.radius.sm, 0.8));
  stage.addChild(rect(x + 118, y + 20, 42, 28, 0xe8c08a, palette.line, ui.radius.sm, 0.88));
  stage.addChild(rect(x + 38, y + 84, 108, 24, 0xd29c73, 0xd29c73, ui.radius.pill, 0.9));
  stage.addChild(line(x + 18, y + height - 12, x + width - 18, y + height - 12, palette.chalk, 4));
  drawReadyKid(stage, x + width / 2, y + 82, shirt, skin);
}

function drawReadyKid(stage: Container, x: number, y: number, shirt: number, skin: number) {
  stage.addChild(new Graphics().circle(x, y - 44, 13).fill(skin).stroke({ width: 2, color: palette.line }));
  stage.addChild(line(x, y - 30, x, y + 14, shirt, 10));
  stage.addChild(line(x - 8, y - 20, x - 38, y - 44, skin, 5));
  stage.addChild(line(x + 8, y - 20, x + 38, y - 44, skin, 5));
  stage.addChild(line(x - 2, y + 14, x - 20, y + 50, palette.line, 5));
  stage.addChild(line(x + 2, y + 14, x + 22, y + 50, palette.line, 5));
}

function drawStandingZone(stage: Container, x: number, y: number) {
  stage.addChild(rect(x, y, 496, 184, 0xfff0d2, palette.line, ui.radius.xl, 0.96));
  stage.addChild(rect(x + 24, y + 24, 448, 132, 0xf7dfac, palette.line, ui.radius.lg, 0.92));
  stage.addChild(centerLabel("全家站位区", x + 248, y + 70, 28, palette.ink, { fontWeight: "900" }));
  stage.addChild(line(x + 64, y + 118, x + 432, y + 118, palette.chalk, 5));
  [
    ["A 已上场", palette.blue],
    ["B 已上场", palette.red],
    ["C 已上场", palette.green],
  ].forEach(([text, color], index) => {
    const markerX = x + 146 + index * 102;
    stage.addChild(new Graphics().circle(markerX, y + 116, 16).fill(color as number).stroke({ width: 2, color: palette.line }));
    stage.addChild(centerLabel(text as string, markerX, y + 146, 13, palette.ink, { fontWeight: "900" }));
  });
}

function drawCountdownCard(stage: Container, x: number, y: number, ctx: RenderContext, accent: number) {
  stage.addChild(centerLabel("3", x + 70, y - 28, 46, accent, { fontWeight: "900" }));
  stage.addChild(themeButton("准备开始", x, y, 140, 44, () => ctx.actions.nav("game"), accent));
}

function drawSchoolyardViewfinder(stage: Container, x: number, y: number, width: number, height: number, accent: number) {
  stage.addChild(rect(x, y, width, height, ui.color.cameraDark, palette.line, ui.radius.xl));
  stage.addChild(label("操场取景中", x + 24, y + 18, 17, palette.chalk, { fontWeight: "900" }));
  stage.addChild(label("粉笔起跑线", x + width - 112, y + 18, 13, palette.chalk, { fontWeight: "800" }));

  const field = new Graphics();
  field.roundRect(x + 22, y + 52, width - 44, height - 74, 8).fill({ color: palette.cream, alpha: 0.08 });
  field.moveTo(x + 84, y + 70).lineTo(x + 84, y + height - 38).stroke({ width: 4, color: palette.chalk, alpha: 0.86 });
  field.moveTo(x + 116, y + 82).lineTo(x + width - 52, y + 82).stroke({ width: 3, color: accent, alpha: 0.72 });
  field.moveTo(x + 116, y + 124).lineTo(x + width - 52, y + 124).stroke({ width: 3, color: accent, alpha: 0.72 });
  field.moveTo(x + 116, y + 166).lineTo(x + width - 52, y + 166).stroke({ width: 3, color: accent, alpha: 0.72 });
  stage.addChild(field);

  drawPlayerMarker(stage, x + 210, y + 112, "小队长", "已站好", palette.green);
  drawPlayerMarker(stage, x + 354, y + 112, "同桌", "已站好", palette.green);
  drawPlayerMarker(stage, x + 486, y + 112, "后排同学", "跑过来", palette.yellow);

  stage.addChild(rect(x + 42, y + 144, 82, 28, palette.paper, palette.line, ui.radius.pill, 0.96));
  stage.addChild(label("别踩线", x + 60, y + 150, 13, palette.ink, { fontWeight: "900" }));
  stage.addChild(line(x + 64, y + 104, x + 64, y + 156, palette.chalk, 2));
  stage.addChild(rect(x + 68, y + 98, 38, 16, palette.court, palette.court, 2, 0.82));
}

function drawPlayerMarker(stage: Container, x: number, y: number, name: string, status: string, color: number) {
  const marker = new Graphics();
  marker.circle(x, y - 20, 11).fill({ color, alpha: 0.9 }).stroke({ width: 2, color: palette.chalk, alpha: 0.8 });
  marker.roundRect(x - 14, y - 8, 28, 24, 10).fill({ color, alpha: 0.72 });
  stage.addChild(marker);
  const nameLabel = label(name, x, y + 24, 14, palette.chalk, { fontWeight: "900", align: "center" });
  nameLabel.anchor.set(0.5, 0);
  stage.addChild(nameLabel);
  const statusLabel = label(status, x, y + 44, 13, color === palette.yellow ? palette.yellow : palette.chalk, { fontWeight: "800", align: "center" });
  statusLabel.anchor.set(0.5, 0);
  stage.addChild(statusLabel);
}

function drawReadyCards(stage: Container, x: number, y: number, accent: number) {
  [
    ["A 小队长", "已上场", palette.blue],
    ["B 爸爸", "已上场", palette.red],
    ["C 妈妈", "已上场", palette.green],
  ].forEach(([title, desc, color], index) => {
    const cardX = x + index * 170;
    stage.addChild(rect(cardX, y, 144, 46, palette.paper, palette.line, ui.radius.md, 0.98));
    stage.addChild(new Graphics().circle(cardX + 22, y + 23, 14).fill(color as number).stroke({ width: 2, color: palette.line }));
    stage.addChild(label(title as string, cardX + 44, y + 8, 13, palette.ink, { fontWeight: "900" }));
    stage.addChild(label(desc as string, cardX + 44, y + 25, 11, color as number, { fontWeight: "900" }));
    stage.addChild(centerLabel("✓", cardX + 130, y + 32, 14, palette.green, { fontWeight: "900" }));
  });
}
