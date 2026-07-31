import { Container, Graphics } from "pixi.js";
import { palette, type RenderContext, type Theme } from "../domain";
import { centerLabel, drawStickFigure, header, label, line, rect, secondaryButton, themeButton } from "../pixiPrimitives";
import { ui } from "../uiStyle";

export function drawTheme(stage: Container, ctx: RenderContext) {
  const theme = ctx.state.selectedTheme;
  if (theme.id === "playground") {
    drawPlaygroundTheme(stage, ctx, theme);
    return;
  }

  header(stage, ctx, "CH-TIME-ENTRY", theme.moment, true);
  stage.addChild(label("时光派对", 48, 78, 16, palette.muted, { fontWeight: "800" }));
  stage.addChild(label(theme.moment, 48, 112, 18, palette.ink, { fontWeight: "800" }));
  stage.addChild(label(theme.name, 48, 142, 28, palette.ink, { fontWeight: "900" }));
  stage.addChild(label(theme.line, 48, 184, 18, palette.ink, { wordWrap: true, wordWrapWidth: 500, lineHeight: 25 }));

  stage.addChild(rect(48, 250, 520, 148, ui.color.memoryPaper, palette.line, 10));
  stage.addChild(label(theme.id === "arcade" ? "投币开场" : "跟上节拍", 220, 274, 19, palette.muted, { fontWeight: "800" }));
  drawScenePreview(stage, 86, 300, theme);

  stage.addChild(rect(606, 106, 270, 292, palette.white, palette.line, 10));
  stage.addChild(label("今日主玩法", 632, 130, 18, palette.muted, { fontWeight: "800" }));
  stage.addChild(label(theme.gameName, 632, 164, 24, palette.ink, { fontWeight: "900" }));
  stage.addChild(label(theme.gameLine, 632, 202, 16, palette.ink, { wordWrap: true, wordWrapWidth: 210, lineHeight: 22 }));
  stage.addChild(label("人数 1-4人", 632, 252, 16, palette.ink, { fontWeight: "800" }));
  stage.addChild(label("时长 90秒", 632, 276, 16, palette.ink, { fontWeight: "800" }));
  stage.addChild(label(theme.goal.replace("：", " "), 632, 306, 16, palette.ink, { fontWeight: "800" }));
  stage.addChild(label(theme.unlock.replace("可解锁：", "本次可解锁 "), 632, 330, 16, palette.ink, { fontWeight: "800" }));
  stage.addChild(themeButton("开始挑战", 632, 354, 190, 40, () => ctx.actions.nav("prepare"), theme.color));

  stage.addChild(label(theme.id === "arcade" ? "街机玩法" : "节拍玩法", 48, 426, 17, palette.ink, { fontWeight: "800" }));
  theme.tags.forEach((tag, index) => {
    const x = 48 + index * 146;
    stage.addChild(rect(x, 452, 130, 54, index === 0 ? theme.color : palette.paper, palette.line, 8));
    stage.addChild(label(tag, x + 16, 460, 16, index === 0 ? palette.white : palette.ink, { fontWeight: "900" }));
    stage.addChild(label(index === 0 ? "首发样板" : "样板预告", x + 16, 482, 14, index === 0 ? palette.white : palette.muted));
  });
}

function drawPlaygroundTheme(stage: Container, ctx: RenderContext, theme: Theme) {
  stage.addChild(rect(18, 16, 924, 492, 0x8ccdf3, palette.line, ui.radius.lg));
  stage.addChild(rect(18, 332, 924, 176, 0xdca66d, 0xdca66d, 0, 0.62));
  stage.addChild(rect(18, 410, 924, 98, 0x78b15d, 0x78b15d, 0, 0.36));
  drawClouds(stage);

  stage.addChild(secondaryButton("<", 28, 26, 42, 38, () => ctx.actions.nav("home")));
  stage.addChild(label("童年操场", 82, 31, 22, palette.white, { fontWeight: "900" }));
  stage.addChild(rect(748, 25, 168, 34, 0x735c36, palette.line, ui.radius.pill, 0.82));
  stage.addChild(centerLabel("上午课间", 832, 42, 16, palette.white, { fontWeight: "900" }));

  drawPlaygroundMenuBoard(stage);
  drawPlaygroundHeroCard(stage, ctx, theme);
}

function drawClouds(stage: Container) {
  stage.addChild(new Graphics().circle(596, 60, 24).fill({ color: palette.white, alpha: 0.55 }));
  stage.addChild(new Graphics().circle(626, 54, 32).fill({ color: palette.white, alpha: 0.5 }));
  stage.addChild(new Graphics().circle(660, 60, 22).fill({ color: palette.white, alpha: 0.45 }));
  stage.addChild(new Graphics().circle(118, 74, 24).fill({ color: palette.white, alpha: 0.28 }));
  stage.addChild(new Graphics().circle(148, 72, 34).fill({ color: palette.white, alpha: 0.24 }));
}

function drawPlaygroundMenuBoard(stage: Container) {
  const boardX = 54;
  const boardY = 74;
  stage.addChild(rect(boardX, boardY, 220, 382, 0x74542f, palette.line, ui.radius.lg));
  stage.addChild(rect(boardX + 16, boardY + 16, 188, 58, 0x315843, palette.line, ui.radius.md));
  stage.addChild(label("今日课间", boardX + 48, boardY + 32, 22, palette.chalk, { fontWeight: "900" }));
  stage.addChild(line(boardX + 42, boardY + 62, boardX + 172, boardY + 42, palette.chalk, 2));

  const rows = [
    { title: "操场木头人", active: true },
    { title: "跳房子", active: false },
    { title: "丢手绢", active: false },
    { title: "踢毽子", active: false },
  ];

  rows.forEach((row, index) => {
    const y = boardY + 96 + index * 48;
    stage.addChild(rect(boardX + 22, y, 176, 38, row.active ? 0xfff0d2 : 0xf5ead3, palette.line, ui.radius.sm, 0.97));
    stage.addChild(label(row.active ? ">" : "", boardX + 36, y + 8, 16, row.active ? palette.red : palette.muted, { fontWeight: "900" }));
    stage.addChild(label(row.title, boardX + 60, y + 8, 16, row.active ? palette.red : palette.ink, { fontWeight: "900" }));
  });

  stage.addChild(label("今日值日", boardX + 36, boardY + 308, 16, palette.chalk, { fontWeight: "900" }));
  stage.addChild(rect(boardX + 34, boardY + 334, 150, 36, 0x315843, palette.line, ui.radius.pill, 0.96));
  stage.addChild(centerLabel("定住 5 次", boardX + 109, boardY + 352, 15, palette.chalk, { fontWeight: "900" }));
}

function drawPlaygroundHeroCard(stage: Container, ctx: RenderContext, theme: Theme) {
  const cardX = 306;
  const cardY = 72;
  const cardW = 586;
  const cardH = 384;
  stage.addChild(rect(cardX, cardY, cardW, cardH, palette.paper, palette.line, ui.radius.xl, 0.98));
  stage.addChild(centerLabel(theme.gameName, cardX + cardW / 2, cardY + 38, 28, palette.ink, { fontWeight: "900" }));

  drawPlaygroundPreview(stage, cardX + 22, cardY + 68, cardW - 44, 202);

  stage.addChild(rect(cardX, cardY + 286, cardW, 98, 0xfff3d7, palette.line, ui.radius.xl, 0.95));
  stage.addChild(label("红灯停，绿灯跑。听口令定住。", cardX + 24, cardY + 306, 17, palette.ink, { fontWeight: "900" }));
  drawInfoPill(stage, cardX + 24, cardY + 358, "1-4人");
  drawInfoPill(stage, cardX + 100, cardY + 358, "90秒");
  drawInfoPill(stage, cardX + 174, cardY + 358, "轻中强度", palette.green);
  stage.addChild(themeButton("开始挑战  >", cardX + 390, cardY + 324, 154, 50, () => ctx.actions.nav("prepare"), theme.color));
}

function drawPlaygroundPreview(stage: Container, x: number, y: number, width: number, height: number) {
  stage.addChild(rect(x, y, width, height, 0xaedaf2, palette.line, ui.radius.lg));
  stage.addChild(rect(x + 22, y + 30, 188, 58, 0xf6d7a9, palette.line, ui.radius.sm, 0.92));
  stage.addChild(rect(x + 28, y + 48, 176, 8, palette.red, palette.red, 0, 0.58));
  stage.addChild(centerLabel("教学楼", x + 116, y + 68, 18, palette.ink, { fontWeight: "900" }));
  stage.addChild(rect(x, y + 112, width, 102, 0xd59258, 0xd59258, 0, 0.62));
  for (let i = 0; i < 6; i += 1) {
    stage.addChild(line(x + 26, y + 128 + i * 14, x + width - 56, y + 124 + i * 4, palette.chalk, 2));
  }
  stage.addChild(line(x + 72, y + 126, x + 258, y + 198, palette.chalk, 3));
  stage.addChild(line(x + 122, y + 126, x + 314, y + 198, palette.chalk, 3));
  stage.addChild(rect(x + 286, y + 18, 84, 20, palette.red, palette.red, ui.radius.xs, 0.78));
  stage.addChild(centerLabel("打铃上课  天真无邪", x + 328, y + 28, 11, palette.white, { fontWeight: "900" }));

  drawRunner(stage, x + 150, y + 154, palette.blue, 1.35);
  drawRunner(stage, x + 246, y + 150, palette.court, 1);
  drawRunner(stage, x + 334, y + 144, palette.yellow, 0.86);
  drawTeacher(stage, x + 430, y + 152);
  drawTrafficLight(stage, x + width - 74, y + 54);
  stage.addChild(rect(x + width - 102, y + 142, 76, 34, 0xf7f5dc, palette.green, ui.radius.sm, 0.96));
  stage.addChild(centerLabel("[绿灯]", x + width - 64, y + 152, 12, palette.green, { fontWeight: "900" }));
  stage.addChild(centerLabel("开始奔跑", x + width - 64, y + 168, 11, palette.green, { fontWeight: "900" }));
}

function drawRunner(stage: Container, x: number, y: number, color: number, scale: number) {
  stage.addChild(new Graphics().circle(x, y - 32 * scale, 9 * scale).fill(0xf2c9a0).stroke({ width: 2, color: palette.line }));
  stage.addChild(line(x, y - 22 * scale, x - 14 * scale, y + 10 * scale, color, 7 * scale));
  stage.addChild(line(x - 8 * scale, y - 8 * scale, x - 34 * scale, y + 4 * scale, color, 5 * scale));
  stage.addChild(line(x - 10 * scale, y + 8 * scale, x - 34 * scale, y + 30 * scale, color, 5 * scale));
  stage.addChild(line(x - 10 * scale, y + 8 * scale, x + 20 * scale, y + 24 * scale, color, 5 * scale));
}

function drawTeacher(stage: Container, x: number, y: number) {
  stage.addChild(new Graphics().circle(x, y - 34, 9).fill(0xf2c9a0).stroke({ width: 2, color: palette.line }));
  stage.addChild(line(x, y - 24, x, y + 12, palette.red, 10));
  stage.addChild(line(x, y + 12, x - 14, y + 34, palette.line, 4));
  stage.addChild(line(x, y + 12, x + 14, y + 34, palette.line, 4));
}

function drawTrafficLight(stage: Container, x: number, y: number) {
  stage.addChild(rect(x, y, 38, 96, 0x2b2b22, palette.line, ui.radius.md));
  stage.addChild(new Graphics().circle(x + 19, y + 22, 12).fill(palette.red).stroke({ width: 2, color: palette.line }));
  stage.addChild(new Graphics().circle(x + 19, y + 52, 12).fill(0x564b32).stroke({ width: 2, color: palette.line }));
  stage.addChild(new Graphics().circle(x + 19, y + 82, 12).fill(palette.green).stroke({ width: 2, color: palette.line }));
  stage.addChild(line(x + 19, y + 96, x + 19, y + 142, palette.line, 5));
}

function drawInfoPill(stage: Container, x: number, y: number, text: string, color = palette.muted) {
  const width = text.length > 8 ? 132 : 64;
  stage.addChild(rect(x, y, width, 22, palette.chalk, palette.line, ui.radius.pill, 0.96));
  stage.addChild(centerLabel(text, x + width / 2, y + 11, 11, color, { fontWeight: "900" }));
}

function drawScenePreview(stage: Container, x: number, y: number, theme: Theme) {
  if (theme.id === "playground") {
    stage.addChild(line(x + 8, y + 92, x + 438, y + 92, palette.court, 8));
    stage.addChild(line(x + 8, y + 118, x + 438, y + 118, palette.court, 8));
    stage.addChild(rect(x + 30, y + 22, 170, 54, palette.chalk, palette.line, 4));
    stage.addChild(label("教学楼", x + 76, y + 38, 20, palette.ink, { fontWeight: "800" }));
    stage.addChild(label("广播  小队旗  跑道", x + 248, y + 54, 20, palette.ink));
  } else if (theme.id === "arcade") {
    stage.addChild(rect(x + 26, y + 24, 130, 118, palette.arcade, palette.line, 8));
    stage.addChild(rect(x + 196, y + 36, 190, 72, palette.yellow, palette.line, 8));
    stage.addChild(label("HIGH SCORE", x + 220, y + 58, 24, palette.ink, { fontWeight: "900" }));
    stage.addChild(new Graphics().circle(x + 92, y + 84, 28).stroke({ width: 6, color: palette.red }));
  } else {
    stage.addChild(rect(x + 24, y + 34, 186, 102, palette.tv, palette.line, 10));
    stage.addChild(label("家庭节目", x + 68, y + 66, 24, palette.white, { fontWeight: "900" }));
    stage.addChild(label("教练动作 / 地毯 / 彩条", x + 252, y + 70, 21, palette.ink));
    drawStickFigure(stage, x + 336, y + 132, palette.line);
  }
}
