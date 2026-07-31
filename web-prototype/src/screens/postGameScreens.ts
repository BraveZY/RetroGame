import { Container } from "pixi.js";
import { palette, type RenderContext } from "../domain";
import { centerLabel, primaryButton, secondaryButton, themeButton, header, label, rect } from "../pixiPrimitives";
import { ui } from "../uiStyle";

export function drawResult(stage: Container, ctx: RenderContext) {
  const theme = ctx.state.selectedTheme;
  header(stage, ctx, "本局结束", "", true);
  stage.addChild(rect(94, 96, 772, 332, palette.white, palette.line, 12));
  stage.addChild(centerLabel("860", 480, 150, 58, theme.color, { fontWeight: "900" }));
  stage.addChild(centerLabel(resultTitle(theme.id), 480, 208, 30, palette.ink, { fontWeight: "900" }));
  ["★", "★", "★"].forEach((star, index) => {
    stage.addChild(centerLabel(star, 416 + index * 64, 262, 34, palette.yellow, { fontWeight: "900" }));
  });
  ["放学后", "操场线", "一起笑场"].forEach((tag, index) => {
    stage.addChild(rect(300 + index * 118, 294, 98, 34, palette.paper, palette.line, 8));
    stage.addChild(centerLabel(tag, 349 + index * 118, 311, 16, palette.ink, { fontWeight: "800" }));
  });
  stage.addChild(rect(242, 354, 476, 44, ui.color.memoryPaper, palette.line, 8));
  stage.addChild(centerLabel("全家都参与进来了，再来一局会点亮新的课间贴纸", 480, 376, 17, palette.ink, { fontWeight: "800" }));
  stage.addChild(themeButton("再玩一次", 234, 448, 150, 46, () => ctx.actions.nav("prepare"), theme.color));
  stage.addChild(primaryButton("换个游戏", 406, 448, 150, 46, () => ctx.actions.nav("home")));
  stage.addChild(secondaryButton("查看奖励", 578, 448, 150, 46, () => ctx.actions.nav("reward")));
}

export function drawReward(stage: Container, ctx: RenderContext) {
  const theme = ctx.state.selectedTheme;
  header(stage, ctx, "获得奖励", "", true);
  stage.addChild(rect(300, 138, 360, 258, palette.white, palette.line, 12));
  stage.addChild(rect(348, 184, 264, 86, ui.color.memoryPaper, palette.line, 10));
  stage.addChild(centerLabel(rewardName(theme.id), 480, 228, 23, palette.ink, { fontWeight: "900" }));
  stage.addChild(centerLabel("完成一局" + theme.name + "后获得", 480, 316, 18, palette.ink, { fontWeight: "800" }));
  stage.addChild(themeButton("收下", 334, 438, 142, 46, () => ctx.actions.nav("collection"), theme.color));
  stage.addChild(primaryButton("查看收藏", 504, 438, 150, 46, () => ctx.actions.nav("collection")));
}

function resultTitle(themeId: string) {
  if (themeId === "playground") return "课间冲刺王";
  if (themeId === "arcade") return "街机高分手";
  return "客厅节拍星";
}

function rewardName(themeId: string) {
  if (themeId === "playground") return "小卖部汽水贴纸";
  if (themeId === "arcade") return "街机灯牌贴纸";
  return "客厅节拍贴纸";
}
