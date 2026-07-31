import { Container, Graphics, Rectangle, Text, type FederatedPointerEvent, type TextStyleOptions } from "pixi.js";
import { getAvatarOption, palette, VIEW_HEIGHT, VIEW_WIDTH, type AvatarId, type RenderContext, type Screen } from "./domain";
import { ui } from "./uiStyle";

type TextStyleToken = keyof typeof ui.textStyle;

let uiRenderScale = 1;

export function setUiRenderScale(scale: number) {
  uiRenderScale = Math.max(1, scale);
}

export function label(
  value: string,
  x: number,
  y: number,
  size = 22,
  color = palette.ink,
  options: Partial<TextStyleOptions> = {},
) {
  const textResolution = Math.min(4, Math.max(window.devicePixelRatio || 1, (window.devicePixelRatio || 1) * uiRenderScale));
  const node = new Text({
    text: value,
    resolution: textResolution,
    style: {
      fontFamily: ui.font.family,
      fontSize: size,
      fill: color,
      fontWeight: options.fontWeight ?? ui.font.weight.regular,
      align: options.align ?? "left",
      wordWrap: options.wordWrap ?? false,
      wordWrapWidth: options.wordWrapWidth,
      lineHeight: options.lineHeight,
    },
  });
  node.position.set(x, y);
  return node;
}

export function centerLabel(
  value: string,
  x: number,
  y: number,
  size = 22,
  color = palette.ink,
  options: Partial<TextStyleOptions> = {},
) {
  const node = label(value, x, y + ui.font.containerTextOffsetY, size, color, options);
  node.anchor.set(0.5);
  return node;
}

export function leftCenterLabel(
  value: string,
  x: number,
  y: number,
  size = 22,
  color = palette.ink,
  options: Partial<TextStyleOptions> = {},
) {
  const node = label(value, x, y + ui.font.containerTextOffsetY, size, color, options);
  node.anchor.set(0, 0.5);
  return node;
}

export function styledLabel(
  style: TextStyleToken,
  value: string,
  x: number,
  y: number,
  color = palette.ink,
  options: Partial<TextStyleOptions> = {},
) {
  const token = ui.textStyle[style];
  return label(value, x, y, token.size, color, {
    fontWeight: token.weight,
    lineHeight: "lineHeight" in token ? token.lineHeight : options.lineHeight,
    ...options,
  });
}

export function styledCenterLabel(
  style: TextStyleToken,
  value: string,
  x: number,
  y: number,
  color = palette.ink,
  options: Partial<TextStyleOptions> = {},
) {
  const token = ui.textStyle[style];
  return centerLabel(value, x, y, token.size, color, {
    fontWeight: token.weight,
    lineHeight: "lineHeight" in token ? token.lineHeight : options.lineHeight,
    ...options,
  });
}

export function rect(
  x: number,
  y: number,
  width: number,
  height: number,
  fill = palette.white,
  stroke = palette.line,
  radius = 8,
  alpha = 1,
) {
  return new Graphics()
    .roundRect(x, y, width, height, radius)
    .fill({ color: fill, alpha })
    .stroke({ width: ui.stroke.default, color: stroke, alpha: Math.min(1, alpha + 0.1) });
}

export function line(x1: number, y1: number, x2: number, y2: number, color = palette.line, width = 2) {
  return new Graphics().moveTo(x1, y1).lineTo(x2, y2).stroke({ width, color });
}

export function button(labelText: string, x: number, y: number, width: number, height: number, onTap: () => void, fill = palette.ink) {
  const group = new Container();
  group.addChild(rect(x, y, width, height, fill, palette.line, ui.component.button.radius));
  const node = styledCenterLabel("buttonText", labelText, x + width / 2, y + height / 2, palette.white, {
    align: "center",
  });
  group.addChild(node);
  group.eventMode = "static";
  group.cursor = "pointer";
  group.hitArea = new Rectangle(x, y, width, height);
  group.on("pointertap", (event: FederatedPointerEvent) => {
    event.stopPropagation();
    onTap();
  });
  return group;
}

export function primaryButton(labelText: string, x: number, y: number, width: number, height: number, onTap: () => void) {
  return button(labelText, x, y, width, height, onTap, ui.component.button.primaryFill);
}

export function secondaryButton(labelText: string, x: number, y: number, width: number, height: number, onTap: () => void) {
  return button(labelText, x, y, width, height, onTap, ui.component.button.secondaryFill);
}

export function dangerButton(labelText: string, x: number, y: number, width: number, height: number, onTap: () => void) {
  return button(labelText, x, y, width, height, onTap, ui.component.button.dangerFill);
}

export function ghostButton(labelText: string, x: number, y: number, width: number, height: number, onTap: () => void) {
  const group = new Container();
  group.addChild(rect(x, y, width, height, ui.component.button.ghostFill, palette.line, ui.component.button.radius));
  const node = styledCenterLabel("buttonText", labelText, x + width / 2, y + height / 2, palette.ink, {
    align: "center",
  });
  group.addChild(node);
  group.eventMode = "static";
  group.cursor = "pointer";
  group.hitArea = new Rectangle(x, y, width, height);
  group.on("pointertap", (event: FederatedPointerEvent) => {
    event.stopPropagation();
    onTap();
  });
  return group;
}

export function themeButton(labelText: string, x: number, y: number, width: number, height: number, onTap: () => void, themeColor: number) {
  return button(labelText, x, y, width, height, onTap, themeColor);
}

export function smallButton(labelText: string, x: number, y: number, width: number, height: number, onTap: () => void, fill = ui.component.button.secondaryFill) {
  return button(labelText, x, y, width, height, onTap, fill);
}

export function avatarSymbol(x: number, y: number, size: number, avatarId: AvatarId) {
  const group = new Container();
  const avatar = getAvatarOption(avatarId);
  group.addChild(new Graphics().circle(x + size / 2, y + size / 2, size / 2).fill(avatar.color).stroke({ width: ui.stroke.default, color: palette.line }));
  group.addChild(new Graphics().circle(x + size * 0.68, y + size * 0.28, size * 0.1).fill({ color: palette.white, alpha: 0.82 }));
  group.addChild(centerLabel(avatar.mark, x + size / 2, y + size / 2, Math.max(14, size * 0.42), palette.white, { fontWeight: "900" }));
  return group;
}

export function avatarButton(x: number, y: number, onTap: () => void, avatarId: AvatarId = "captain") {
  const group = new Container();
  const size = 38;
  group.addChild(avatarSymbol(x, y, size, avatarId));
  group.eventMode = "static";
  group.cursor = "pointer";
  group.hitArea = new Rectangle(x, y, size, size);
  group.on("pointertap", (event: FederatedPointerEvent) => {
    event.stopPropagation();
    onTap();
  });
  return group;
}

export function header(stage: Container, ctx: RenderContext, title: string, subtitle: string, showBack = false, backTarget: Screen = "home") {
  stage.addChild(rect(ui.layout.header.x, ui.layout.header.y, VIEW_WIDTH - ui.layout.header.x * 2, ui.layout.header.height, palette.paper, palette.line, ui.radius.lg));
  if (showBack) {
    stage.addChild(secondaryButton("<", 32, ui.layout.header.buttonY, 42, ui.layout.header.buttonH, () => ctx.actions.nav(backTarget)));
  }
  stage.addChild(styledLabel("headerTitle", title, showBack ? 90 : ui.layout.header.titleX, ui.layout.header.titleY, palette.ink));
  const actionButtons = [
    ctx.state.screen !== "collection" && ctx.state.screen !== "settings" && ctx.state.screen !== "help" && ctx.state.screen !== "theme"
      ? { kind: "avatar" as const, width: 38, onTap: () => ctx.actions.nav("collection" as const) }
      : null,
    ctx.state.screen !== "collection" && ctx.state.screen !== "settings" && ctx.state.screen !== "help" && ctx.state.screen !== "theme"
      ? { kind: "button" as const, text: "帮助", width: 58, onTap: () => ctx.actions.nav("help" as const) }
      : null,
    ctx.state.screen !== "collection" && ctx.state.screen !== "settings" && ctx.state.screen !== "help" && ctx.state.screen !== "theme"
      ? { kind: "button" as const, text: "设置", width: 58, onTap: () => ctx.actions.nav("settings" as const) }
      : null,
  ].filter((item): item is { kind: "avatar"; width: number; onTap: () => void } | { kind: "button"; text: string; width: number; onTap: () => void } => item !== null);
  const actionWidth = actionButtons.reduce((sum, item) => sum + item.width, 0) + Math.max(0, actionButtons.length - 1) * 8;
  const actionStartX = VIEW_WIDTH - 44 - actionWidth;
  const subtitleNode = label(subtitle, actionStartX - 18, 34, ui.font.size.list - 1, palette.muted);
  subtitleNode.anchor.set(1, 0);
  stage.addChild(subtitleNode);
  let x = actionStartX;
  actionButtons.forEach((item) => {
    if (item.kind === "avatar") {
      stage.addChild(avatarButton(x, ui.layout.header.buttonY, item.onTap, ctx.state.selectedAvatar));
    } else {
      stage.addChild(secondaryButton(item.text, x, ui.layout.header.buttonY, item.width, ui.layout.header.buttonH, item.onTap));
    }
    x += item.width + 8;
  });
}

export function drawBase(stage: Container) {
  stage.removeChildren();
  stage.addChild(rect(0, 0, VIEW_WIDTH, VIEW_HEIGHT, palette.cream, palette.cream, 0));
  stage.addChild(rect(12, 12, VIEW_WIDTH - 24, VIEW_HEIGHT - 24, palette.paper, palette.line, ui.radius.phone));
  stage.addChild(new Graphics().circle(30, VIEW_HEIGHT / 2, 4).fill(palette.shadow));
}

export function drawStickFigure(stage: Container, x: number, y: number, color: number) {
  stage.addChild(new Graphics().circle(x, y - 62, 15).stroke({ width: 4, color }));
  stage.addChild(line(x, y - 46, x, y - 8, color, 4));
  stage.addChild(line(x - 28, y - 32, x + 28, y - 32, color, 4));
  stage.addChild(line(x, y - 8, x - 24, y + 28, color, 4));
  stage.addChild(line(x, y - 8, x + 24, y + 28, color, 4));
}
