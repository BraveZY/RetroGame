import { Container, Graphics } from "pixi.js";
import { palette, type RenderContext, VIEW_WIDTH } from "../domain";
import { primaryButton, secondaryButton, themeButton, button, drawStickFigure, label, line, rect } from "../pixiPrimitives";

export function drawGame(stage: Container, ctx: RenderContext) {
  const game = ctx.state.selectedTheme.game;
  if (game === "wooden") drawWoodenGame(stage, ctx);
  if (game === "basket") drawBasketGame(stage, ctx);
  if (game === "aerobics") drawAerobicsGame(stage, ctx);
}

export function drawPause(stage: Container, ctx: RenderContext) {
  const theme = ctx.state.selectedTheme;
  stage.addChild(label("暂停", 456, 70, 28, palette.ink, { fontWeight: "900" }));
  stage.addChild(rect(126, 128, 708, 252, palette.white, palette.line, 14));
  stage.addChild(label(theme.gameName, 180, 166, 26, palette.ink, { fontWeight: "900" }));
  stage.addChild(label("当前进度 65%", 638, 172, 18, palette.muted));
  stage.addChild(themeButton("继续游戏", 180, 248, 160, 58, () => ctx.actions.nav("game"), theme.color));
  stage.addChild(primaryButton("重新开始", 400, 248, 160, 58, () => ctx.actions.nav("recognition")));
  stage.addChild(secondaryButton("回到主题页", 620, 248, 160, 58, () => ctx.actions.nav("theme")));
  stage.addChild(label("提示：把手机横放并固定好，再继续。", 316, 338, 18, palette.ink));
}

function drawGameTop(stage: Container, ctx: RenderContext, title: string, mid: string, right: string) {
  stage.addChild(rect(22, 18, VIEW_WIDTH - 44, 54, palette.paper, palette.line, 10));
  stage.addChild(label(title, 42, 35, 21, palette.ink, { fontWeight: "900" }));
  stage.addChild(label(mid, 344, 36, 19, palette.ink, { fontWeight: "800" }));
  stage.addChild(label(right, 584, 36, 19, palette.ink, { fontWeight: "800" }));
  stage.addChild(secondaryButton("暂停", 842, 26, 76, 38, () => ctx.actions.nav("pause")));
}

function drawWoodenGame(stage: Container, ctx: RenderContext) {
  const theme = ctx.state.selectedTheme;
  const green = ctx.state.lightState === "green";
  drawGameTop(stage, ctx, "操场木头人", green ? "01:12" : "00:48", green ? "分数 360" : "分数 440");
  drawPlayerProgress(stage, 58, 96, "A", green ? "120" : "180", green ? "########----" : "##########--");
  drawPlayerProgress(stage, 350, 96, "B", green ? "090" : "150", green ? "######------" : "########----");
  drawPlayerProgress(stage, 642, 96, "C", green ? "060" : "110", green ? "####--------" : "######------");
  if (!green) {
    stage.addChild(label("A 定住成功", 92, 146, 17, palette.green, { fontWeight: "900" }));
    stage.addChild(label("B 动了，少十分", 384, 146, 17, palette.red, { fontWeight: "900" }));
    stage.addChild(label("C 定住成功", 704, 146, 17, palette.green, { fontWeight: "900" }));
  }
  const lightColor = green ? palette.green : palette.red;
  stage.addChild(rect(360, 210, 240, 108, lightColor, palette.line, 12));
  stage.addChild(label(green ? "绿灯" : "红灯", 444, 226, 36, palette.white, { fontWeight: "900" }));
  stage.addChild(label(green ? "快跑" : "定住", 444, 272, 36, palette.white, { fontWeight: "900" }));
  stage.addChild(label(green ? "摆动手臂，原地跑起来" : "老师回头了，别动", 350, 356, 22, palette.ink, { fontWeight: "900" }));
  stage.addChild(label("终点线 ----------------------------------------------------------->", 96, 410, 18, palette.ink, { fontWeight: "800" }));
  stage.addChild(button("切换口令", 60, 446, 126, 44, ctx.actions.toggleLight, lightColor));
  stage.addChild(themeButton("结束本局", 772, 446, 126, 44, () => ctx.actions.nav("result"), theme.color));
}

function drawPlayerProgress(stage: Container, x: number, y: number, player: string, score: string, progress: string) {
  stage.addChild(label(`${player} ${score}`, x, y, 18, palette.ink, { fontWeight: "900" }));
  stage.addChild(label(progress, x + 64, y, 18, palette.ink, { fontWeight: "800" }));
}

function drawBasketGame(stage: Container, ctx: RenderContext) {
  const theme = ctx.state.selectedTheme;
  drawGameTop(stage, ctx, "客厅街机投篮机", "00:36", "分数 240   最高分 520");
  stage.addChild(label("连中 4 次", 428, 100, 24, palette.ink, { fontWeight: "900" }));
  stage.addChild(rect(354, 150, 252, 104, palette.paper, palette.line, 8));
  stage.addChild(label("篮筐", 456, 168, 25, palette.ink, { fontWeight: "900" }));
  stage.addChild(new Graphics().circle(480, 222, 26).stroke({ width: 5, color: palette.red }));
  stage.addChild(rect(150, 306, 220, 78, palette.arcade, palette.line, 8));
  stage.addChild(rect(590, 306, 220, 78, palette.arcade, palette.line, 8));
  stage.addChild(label("左侧出手区", 198, 324, 23, palette.white, { fontWeight: "900" }));
  stage.addChild(label("举手投篮", 216, 354, 19, palette.white));
  stage.addChild(label("右侧出手区", 638, 324, 23, palette.white, { fontWeight: "900" }));
  stage.addChild(label("挥臂投篮", 656, 354, 19, palette.white));
  stage.addChild(label("玩家一已上场", 414, 420, 18, palette.ink, { fontWeight: "900" }));
  stage.addChild(label("再进 3 个，点亮加分时间", 64, 464, 19, palette.ink, { fontWeight: "800" }));
  stage.addChild(themeButton("结束本局", 772, 452, 126, 44, () => ctx.actions.nav("result"), theme.color));
}

function drawAerobicsGame(stage: Container, ctx: RenderContext) {
  const theme = ctx.state.selectedTheme;
  drawGameTop(stage, ctx, "家庭节拍馆", "01:25", "家庭同步感 不错");
  stage.addChild(label("抬膝 + 摆臂", 392, 124, 32, palette.ink, { fontWeight: "900" }));
  drawStickFigure(stage, 310, 306, palette.line);
  drawStickFigure(stage, 480, 306, palette.line);
  drawStickFigure(stage, 650, 306, palette.line);
  stage.addChild(label("教练", 286, 344, 20, palette.ink, { fontWeight: "800" }));
  stage.addChild(label("玩家一", 452, 344, 20, palette.ink, { fontWeight: "800" }));
  stage.addChild(label("玩家二", 622, 344, 20, palette.ink, { fontWeight: "800" }));
  stage.addChild(label("节拍  | 拍 | . | 拍 | . | 拍 | . | 拍 | . |", 176, 418, 25, palette.ink, { fontWeight: "900" }));
  stage.addChild(label("下一个动作：跟拍手", 360, 470, 19, palette.ink, { fontWeight: "900" }));
  stage.addChild(themeButton("结束本局", 772, 458, 126, 44, () => ctx.actions.nav("result"), theme.color));
}
