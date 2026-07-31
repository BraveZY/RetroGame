import { Container, Graphics, Rectangle, type FederatedPointerEvent } from "pixi.js";
import { palette, themes, type RenderContext, type Theme, VIEW_WIDTH } from "../domain";
import { avatarButton, centerLabel, label, line, rect, secondaryButton, themeButton } from "../pixiPrimitives";
import { ui } from "../uiStyle";

const carouselArea = { x: 72, y: 78, width: 816, height: 332 };

export function drawHome(stage: Container, ctx: RenderContext) {
  const selectedTheme = ctx.state.selectedTheme;
  drawThemeAtmosphere(stage, selectedTheme);
  drawHomeHeader(stage, ctx);
  drawCarousel(stage, ctx, selectedTheme);
  drawThemeTabs(stage, ctx, selectedTheme);
  drawBottomCards(stage, selectedTheme);
}

function drawHomeHeader(stage: Container, ctx: RenderContext) {
  stage.addChild(rect(318, 24, 324, 30, 0xf8ead3, palette.white, ui.radius.pill, 0.84));
  stage.addChild(centerLabel("今天去哪儿玩", 480, 39, 15, palette.ink, { fontWeight: "900" }));
  stage.addChild(centerLabel("✦", 352, 39, 13, palette.yellow, { fontWeight: "900" }));
  stage.addChild(centerLabel("✦", 608, 39, 13, palette.yellow, { fontWeight: "900" }));

  stage.addChild(avatarButton(VIEW_WIDTH - 132, 20, () => ctx.actions.nav("collection"), ctx.state.selectedAvatar));
  stage.addChild(secondaryButton("?", VIEW_WIDTH - 88, 20, 34, 34, () => ctx.actions.nav("help")));
  stage.addChild(secondaryButton("⚙", VIEW_WIDTH - 48, 20, 34, 34, () => ctx.actions.nav("settings")));
}

function drawCarousel(stage: Container, ctx: RenderContext, theme: Theme) {
  drawArrow(stage, carouselArea.x - 52, carouselArea.y + 152, "<", () => selectAdjacentTheme(ctx, -1));
  drawArrow(stage, carouselArea.x + carouselArea.width + 18, carouselArea.y + 152, ">", () => selectAdjacentTheme(ctx, 1));

  const cardX = 120;
  const cardY = 86;
  const cardW = 720;
  const cardH = 244;
  stage.addChild(rect(cardX, cardY, cardW, cardH, palette.white, palette.white, 16, 0.52));
  stage.addChild(rect(cardX + 8, cardY + 8, cardW - 16, cardH - 16, 0xffffff, palette.white, 14, 0.36));

  drawSwipeHitArea(stage, ctx);
  drawThemeHero(stage, ctx, theme, cardX, cardY, cardW, cardH);
}

function drawArrow(stage: Container, x: number, y: number, text: string, onTap: () => void) {
  const group = new Container();
  group.addChild(new Graphics().circle(x + 22, y + 22, 22).fill({ color: palette.white, alpha: 0.82 }).stroke({ width: 2, color: palette.line, alpha: 0.65 }));
  group.addChild(centerLabel(text, x + 22, y + 20, 26, palette.muted, { fontWeight: "900" }));
  group.eventMode = "static";
  group.cursor = "pointer";
  group.hitArea = new Rectangle(x, y, 44, 44);
  group.on("pointertap", (event: FederatedPointerEvent) => {
    event.stopPropagation();
    onTap();
  });
  stage.addChild(group);
}

function drawThemeHero(stage: Container, ctx: RenderContext, theme: Theme, x: number, y: number, width: number, height: number) {
  drawThemeScene(stage, theme, x + 10, y + 10, width - 20, height - 20);

  const titleColor = theme.id === "fitness" ? palette.red : theme.id === "arcade" ? palette.chalk : 0x17713a;
  const copyColor = theme.id === "arcade" ? palette.chalk : palette.ink;
  const titleX = theme.id === "fitness" ? x + 38 : x + 68;
  const titleY = y + 56;

  stage.addChild(centerLabel(theme.id === "playground" ? "☼" : theme.id === "arcade" ? "✦" : "♪", titleX + 38, y + 38, 26, theme.color, { fontWeight: "900" }));
  stage.addChild(label(theme.name, titleX, titleY, 42, titleColor, { fontWeight: "900" }));
  stage.addChild(label(homeLine(theme), titleX, titleY + 58, 16, copyColor, { fontWeight: "800", wordWrap: true, wordWrapWidth: 280, lineHeight: 24 }));
  stage.addChild(themeButton("进入" + theme.name + "  >", titleX, titleY + 104, 194, 38, () => ctx.actions.nav("theme"), theme.color));
}

function drawThemeScene(stage: Container, theme: Theme, x: number, y: number, width: number, height: number) {
  if (theme.id === "playground") {
    drawPlaygroundScene(stage, x, y, width, height);
  } else if (theme.id === "arcade") {
    drawArcadeScene(stage, x, y, width, height);
  } else {
    drawFitnessScene(stage, x, y, width, height);
  }
}

function drawPlaygroundScene(stage: Container, x: number, y: number, width: number, height: number) {
  stage.addChild(rect(x, y, width, height, 0xcbeaff, palette.white, 14, 0.82));
  stage.addChild(rect(x, y + height * 0.55, width, height * 0.45, 0xd9955c, 0xd9955c, 0, 0.62));
  stage.addChild(rect(x, y + height * 0.73, width, height * 0.27, 0x86c36f, 0x86c36f, 0, 0.42));
  stage.addChild(rect(x + 280, y + 30, 180, 56, 0xf5d5a7, palette.white, 6, 0.72));
  stage.addChild(centerLabel("教学楼", x + 370, y + 58, 18, palette.ink, { fontWeight: "900" }));
  for (let i = 0; i < 5; i += 1) {
    stage.addChild(line(x + 170 + i * 42, y + 138, x + 96 + i * 84, y + height - 20, palette.chalk, 3));
  }
  drawHopscotch(stage, x + 390, y + 132);
  drawRunner(stage, x + 470, y + 168, palette.court, 1.15);
  drawRunner(stage, x + 548, y + 156, palette.blue, 0.92);
  drawRunner(stage, x + 624, y + 164, palette.green, 0.82);
  drawBell(stage, x + 34, y + height - 54);
}

function drawArcadeScene(stage: Container, x: number, y: number, width: number, height: number) {
  stage.addChild(rect(x, y, width, height, 0xa76438, palette.white, 14, 0.82));
  stage.addChild(rect(x + 360, y + 10, width - 380, height - 20, 0x4b2e35, palette.white, 12, 0.5));
  stage.addChild(rect(x + 466, y + 42, 132, 126, 0x263e6c, palette.line, 10, 0.96));
  stage.addChild(rect(x + 488, y + 58, 88, 46, palette.line, palette.yellow, 4, 0.92));
  stage.addChild(centerLabel("258", x + 532, y + 82, 24, palette.red, { fontWeight: "900" }));
  stage.addChild(new Graphics().circle(x + 532, y + 140, 30).stroke({ width: 7, color: palette.yellow }));
  stage.addChild(rect(x + 50, y + 168, 128, 34, 0x744630, palette.line, 8, 0.82));
  stage.addChild(rect(x + 72, y + 150, 52, 20, palette.red, palette.line, 10, 0.95));
  stage.addChild(new Graphics().circle(x + 104, y + 186, 12).fill(palette.yellow).stroke({ width: 2, color: palette.line }));
  drawRunner(stage, x + 406, y + 180, palette.yellow, 1.08);
  drawRunner(stage, x + 342, y + 186, palette.blue, 0.86);
  drawRunner(stage, x + 624, y + 190, palette.court, 0.74);
}

function drawFitnessScene(stage: Container, x: number, y: number, width: number, height: number) {
  stage.addChild(rect(x, y, width, height, 0xffd8ac, palette.white, 14, 0.82));
  stage.addChild(rect(x + 392, y + 30, 176, 112, 0x6d67c8, palette.line, 10, 0.96));
  stage.addChild(centerLabel("节拍频道", x + 480, y + 58, 18, palette.white, { fontWeight: "900" }));
  stage.addChild(rect(x + 428, y + 82, 104, 42, 0xf2e1ff, palette.white, 8, 0.48));
  for (let i = 0; i < 5; i += 1) {
    stage.addChild(new Graphics().circle(x + 304 + i * 58, y + 184 + (i % 2) * 10, 12).fill({ color: i % 2 ? palette.court : palette.yellow, alpha: 0.58 }));
  }
  drawRunner(stage, x + 342, y + 166, palette.green, 1);
  drawRunner(stage, x + 596, y + 174, palette.red, 0.82);
  drawRunner(stage, x + 668, y + 184, palette.yellow, 0.68);
  stage.addChild(rect(x + 52, y + 164, 86, 26, palette.chalk, palette.line, 8, 0.72));
  stage.addChild(centerLabel("遥控器", x + 95, y + 177, 13, palette.muted, { fontWeight: "900" }));
}

function drawSwipeHitArea(stage: Container, ctx: RenderContext) {
  const hit = new Container();
  let pointerStartX: number | null = null;
  hit.eventMode = "static";
  hit.cursor = "grab";
  hit.hitArea = new Rectangle(carouselArea.x, carouselArea.y, carouselArea.width, carouselArea.height);
  hit.on("pointerdown", (event: FederatedPointerEvent) => {
    pointerStartX = event.global.x;
  });
  hit.on("pointerup", (event: FederatedPointerEvent) => {
    if (pointerStartX === null) return;
    const delta = event.global.x - pointerStartX;
    pointerStartX = null;
    if (Math.abs(delta) < 42) return;
    selectAdjacentTheme(ctx, delta < 0 ? 1 : -1);
  });
  hit.on("pointerupoutside", () => {
    pointerStartX = null;
  });
  stage.addChild(hit);
}

function drawThemeTabs(stage: Container, ctx: RenderContext, selectedTheme: Theme) {
  const startX = 358;
  themes.forEach((theme, index) => {
    const x = startX + index * 90;
    const active = theme.id === selectedTheme.id;
    stage.addChild(new Graphics().circle(x, 352, active ? 6 : 5).fill(active ? theme.color : palette.white).stroke({ width: 2, color: palette.white }));
    stage.addChild(centerLabel(theme.name, x, 374, 11, active ? palette.ink : palette.muted, { fontWeight: active ? "900" : "800" }));
    const hit = new Container();
    hit.eventMode = "static";
    hit.cursor = "pointer";
    hit.hitArea = new Rectangle(x - 36, 340, 72, 42);
    hit.on("pointertap", () => ctx.actions.selectTheme(theme));
    stage.addChild(hit);
  });
}

function drawBottomCards(stage: Container, theme: Theme) {
  drawInfoCard(stage, 92, 432, 204, "今日活力", "68 /100", palette.red, "🔥");
  drawInfoCard(stage, 334, 432, 204, "最新成就", latestAchievement(theme), palette.yellow, "🏆");
  drawInfoCard(stage, 576, 432, 204, "本周活动", weeklyActivity(theme), palette.blue, "▣");
}

function drawInfoCard(stage: Container, x: number, y: number, width: number, title: string, value: string, color: number, icon: string) {
  stage.addChild(rect(x, y, width, 68, palette.white, palette.white, 10, 0.78));
  stage.addChild(new Graphics().circle(x + 28, y + 34, 19).fill(color));
  stage.addChild(centerLabel(icon, x + 28, y + 33, 18, palette.white, { fontWeight: "900" }));
  stage.addChild(label(title, x + 58, y + 14, 12, palette.muted, { fontWeight: "800" }));
  stage.addChild(label(value, x + 58, y + 34, 17, palette.ink, { fontWeight: "900" }));
}

function drawThemeAtmosphere(stage: Container, theme: Theme) {
  const bg = theme.id === "playground" ? 0xbfe4fb : theme.id === "arcade" ? 0xd08a4c : 0xffc995;
  stage.addChild(rect(18, 16, 924, 492, bg, bg, 0, 0.88));
  stage.addChild(new Graphics().circle(130, 92, 86).fill({ color: palette.white, alpha: theme.id === "arcade" ? 0.12 : 0.28 }));
  stage.addChild(new Graphics().circle(812, 88, 96).fill({ color: palette.white, alpha: 0.18 }));
  stage.addChild(new Graphics().circle(738, 406, 132).fill({ color: theme.color, alpha: 0.12 }));
}

function selectAdjacentTheme(ctx: RenderContext, direction: -1 | 1) {
  const currentIndex = themes.findIndex((theme) => theme.id === ctx.state.selectedTheme.id);
  const nextIndex = (currentIndex + direction + themes.length) % themes.length;
  ctx.actions.selectTheme(themes[nextIndex]);
}

function homeLine(theme: Theme) {
  if (theme.id === "playground") return "下课铃响起，粉笔线和跑道都在等你上场。";
  if (theme.id === "arcade") return "投币声响起，分数灯亮起来。";
  return "打开电视，全家跟着节拍动起来。";
}

function latestAchievement(theme: Theme) {
  if (theme.id === "playground") return "跳房子高手";
  if (theme.id === "arcade") return "投篮高分手";
  return "节拍同步王";
}

function weeklyActivity(theme: Theme) {
  if (theme.id === "playground") return "欢乐运动周";
  if (theme.id === "arcade") return "高分挑战周";
  return "全家节拍周";
}

function drawHopscotch(stage: Container, x: number, y: number) {
  for (let i = 0; i < 3; i += 1) {
    stage.addChild(rect(x, y + i * 24, 42, 22, 0xffffff, palette.chalk, 2, 0.2));
    stage.addChild(centerLabel(String(i + 1), x + 21, y + i * 24 + 11, 12, palette.chalk, { fontWeight: "900" }));
  }
}

function drawBell(stage: Container, x: number, y: number) {
  stage.addChild(new Graphics().moveTo(x + 18, y).lineTo(x + 34, y + 30).lineTo(x, y + 30).closePath().fill(palette.yellow).stroke({ width: 2, color: palette.line }));
  stage.addChild(line(x + 4, y + 34, x + 36, y + 34, palette.line, 3));
  stage.addChild(new Graphics().circle(x + 18, y + 38, 4).fill(palette.line));
}

function drawRunner(stage: Container, x: number, y: number, color: number, scale: number) {
  stage.addChild(new Graphics().circle(x, y - 32 * scale, 9 * scale).fill(0xf2c9a0).stroke({ width: 2, color: palette.line }));
  stage.addChild(line(x, y - 22 * scale, x - 12 * scale, y + 8 * scale, color, 7 * scale));
  stage.addChild(line(x - 7 * scale, y - 10 * scale, x - 28 * scale, y + 2 * scale, color, 5 * scale));
  stage.addChild(line(x + 1 * scale, y - 10 * scale, x + 24 * scale, y + 4 * scale, color, 5 * scale));
  stage.addChild(line(x - 10 * scale, y + 8 * scale, x - 30 * scale, y + 28 * scale, color, 5 * scale));
  stage.addChild(line(x - 10 * scale, y + 8 * scale, x + 18 * scale, y + 24 * scale, color, 5 * scale));
}
