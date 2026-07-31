import { Container, Graphics, Rectangle } from "pixi.js";
import { palette, type RenderContext } from "../domain";
import { centerLabel, header, label, leftCenterLabel, line, primaryButton, rect, secondaryButton, smallButton, themeButton } from "../pixiPrimitives";
import { ui } from "../uiStyle";

export function drawHealthAdvice(stage: Container, ctx: RenderContext) {
  drawSoftLaunchBackdrop(stage);
  stage.addChild(rect(206, 64, 548, 342, ui.color.inputFill, palette.line, 12, 0.96));
  const title = label("健康游戏忠告", 480, 96, 26, palette.ink, { fontWeight: "900", align: "center" });
  title.anchor.set(0.5, 0);
  stage.addChild(title);
  [
    ["抵制不良游戏", "拒绝盗版游戏"],
    ["注意自我保护", "谨防受骗上当"],
    ["适度游戏益脑", "沉迷游戏伤身"],
    ["合理安排时间", "享受健康生活"],
  ].forEach(([left, right], index) => {
    const y = 154 + index * 38;
    stage.addChild(label(left, 298, y, 22, palette.ink, { fontWeight: "900" }));
    stage.addChild(label(right, 504, y, 22, palette.ink, { fontWeight: "900" }));
  });
  const company = label("本公司积极履行", 480, 324, 19, palette.ink, { fontWeight: "900", align: "center" });
  company.anchor.set(0.5, 0);
  stage.addChild(company);
  const pledge = label("《网络游戏行业沉迷自律公约》", 480, 354, 19, palette.ink, { fontWeight: "900", align: "center" });
  pledge.anchor.set(0.5, 0);
  stage.addChild(pledge);
  stage.addChild(label("即将进入登录页面", 424, 420, 15, palette.muted, { fontWeight: "800" }));
  stage.addChild(themeButton("点击继续", 392, 452, 176, 36, () => ctx.actions.nav("login"), palette.court));
}

export function drawLogin(stage: Container, ctx: RenderContext) {
  stage.addChild(rect(18, 16, 924, 58, palette.paper, palette.line, 10));
  stage.addChild(label("时光派对", 38, 29, 23, palette.ink, { fontWeight: "900" }));
  drawAgeBadge(stage);
  drawLoginCard(stage, ctx);
  drawLegalFooter(stage, ctx);
}

export function drawPhoneLogin(stage: Container, ctx: RenderContext) {
  header(stage, ctx, "手机号登录", "保存收藏、奖励和家庭进度", true, "login");
  stage.addChild(rect(272, 128, 416, 274, palette.white, palette.line, 10));
  stage.addChild(label("手机号登录", 418, 160, 28, palette.ink, { fontWeight: "900" }));
  stage.addChild(label("用于保存家庭奖励、主题收藏和复玩记录", 338, 200, 16, palette.muted, { fontWeight: "800" }));
  stage.addChild(label("手机号", 326, 242, 15, palette.ink, { fontWeight: "900" }));
  stage.addChild(drawInputBox(ctx, "phone", 392, 234, 206, 34, ctx.state.phoneNumber, "请输入手机号"));
  stage.addChild(label("验证码", 326, 290, 15, palette.ink, { fontWeight: "900" }));
  stage.addChild(drawInputBox(ctx, "code", 392, 282, 118, 34, ctx.state.verificationCode, "短信验证码"));
  stage.addChild(smallButton("获取验证码", 522, 282, 96, 34, () => ctx.actions.nav("phoneLogin"), palette.blue));
  stage.addChild(themeButton("登录并进入", 392, 344, 176, 42, () => ctx.actions.nav("realNameAuth"), palette.court));
  stage.addChild(label("低保真占位：可点击输入框后用键盘输入手机号和验证码。", 330, 420, 14, palette.muted, { fontWeight: "800" }));
}

export function drawRealNameAuth(stage: Container, ctx: RenderContext) {
  header(stage, ctx, "实名认证", "防沉迷系统要求完成实名信息", true, "phoneLogin");
  stage.addChild(label("根据国家网络游戏防沉迷要求，进入游戏前需要完成实名认证。", 230, 108, 17, palette.ink, { fontWeight: "800" }));
  stage.addChild(rect(272, 150, 416, 304, palette.white, palette.line, 10));
  stage.addChild(label("实名认证", 418, 184, 28, palette.ink, { fontWeight: "900" }));
  stage.addChild(label("信息仅用于实名与防沉迷校验。", 374, 226, 14, palette.muted, { fontWeight: "800" }));
  stage.addChild(label("本原型不保存真实数据。", 394, 248, 14, palette.muted, { fontWeight: "800" }));
  stage.addChild(label("姓名", 326, 294, 15, palette.ink, { fontWeight: "900" }));
  stage.addChild(drawInputBox(ctx, "realName", 392, 286, 206, 34, ctx.state.realName, "请输入真实姓名"));
  stage.addChild(label("证件号", 326, 344, 15, palette.ink, { fontWeight: "900" }));
  stage.addChild(drawInputBox(ctx, "idNumber", 392, 336, 206, 34, ctx.state.idNumber, "请输入身份证号"));
  stage.addChild(themeButton("完成认证并进入", 376, 392, 208, 42, () => ctx.actions.nav("home"), palette.court));
  stage.addChild(label("模拟流程：正式版本需接入实名校验与未成年人游戏时长限制。", 304, 470, 14, palette.muted, { fontWeight: "800" }));
}

function drawSoftLaunchBackdrop(stage: Container) {
  stage.addChild(new Graphics().circle(146, 116, 34).fill({ color: palette.yellow, alpha: 0.28 }));
  stage.addChild(new Graphics().circle(836, 138, 42).fill({ color: palette.arcade, alpha: 0.2 }));
  stage.addChild(new Graphics().circle(198, 424, 46).fill({ color: palette.tv, alpha: 0.22 }));
  stage.addChild(new Graphics().circle(758, 420, 44).fill({ color: palette.court, alpha: 0.22 }));
  stage.addChild(rect(84, 380, 146, 28, palette.yellow, palette.line, 14, 0.38));
  stage.addChild(rect(710, 98, 128, 26, palette.court, palette.line, 12, 0.26));
}

function drawAgeBadge(stage: Container) {
  stage.addChild(rect(802, 22, 118, 46, ui.color.ageBadgeBg, palette.line, 8));
  stage.addChild(rect(810, 28, 44, 34, ui.color.ageBadgeGreen, palette.green, 7));
  stage.addChild(centerLabel("4+", 832, 45, 22, palette.white, { fontWeight: "900" }));
  stage.addChild(label("适龄\n提示", 862, 28, 13, palette.ink, { fontWeight: "900", lineHeight: 16 }));
}

function drawLoginCard(stage: Container, ctx: RenderContext) {
  stage.addChild(rect(282, 126, 396, 202, palette.white, palette.line, 10));
  stage.addChild(label("时光派对", 420, 150, 30, palette.ink, { fontWeight: "900" }));
  stage.addChild(label("客厅开场，全家上场", 392, 192, 17, palette.muted, { fontWeight: "800" }));
  stage.addChild(themeButton("继续进入  >", 374, 224, 212, 42, () => ctx.actions.nav("home"), palette.court));
  stage.addChild(
    primaryButton(
      "手机号登录",
      334,
      288,
      128,
      34,
      () => {
        if (ctx.state.agreementAccepted) {
          ctx.actions.nav("phoneLogin");
          return;
        }
        ctx.actions.requestAgreement();
      },
    ),
  );
  stage.addChild(secondaryButton("游客体验", 498, 288, 116, 34, () => ctx.actions.nav("home")));
}

function drawLegalFooter(stage: Container, ctx: RenderContext) {
  stage.addChild(label("ICP备案号：示例ICP备00000000号", 382, 344, 12, palette.ink, { fontWeight: "800" }));
  stage.addChild(label("查询：", 378, 362, 12, palette.ink, { fontWeight: "800" }));
  stage.addChild(drawExternalLink("https://beian.miit.gov.cn/", 414, 362, 166, 17));
  stage.addChild(drawAgreementRow(ctx));
  stage.addChild(label("著作权人：北京金鼎汉克科技有限公司", 80, 448, 11, palette.ink, { fontWeight: "800" }));
  stage.addChild(label("出版服务单位：示例电子音像出版社有限公司", 456, 448, 10, palette.ink, { fontWeight: "800" }));
  stage.addChild(label("运营单位：北京金鼎汉克科技有限公司", 80, 468, 11, palette.ink, { fontWeight: "800" }));
  stage.addChild(label("批准文号：示例国新出审[20XX]0001号", 456, 468, 10, palette.ink, { fontWeight: "800" }));
  stage.addChild(label("出版物号：示例ISBN 978-7-0000-0000-0", 690, 468, 10, palette.ink, { fontWeight: "800" }));
  stage.addChild(label("V0.0.1", 460, 496, 12, palette.muted, { fontWeight: "900" }));
}

function drawAgreementRow(ctx: RenderContext) {
  const group = new Container();
  const x = 172 + ctx.state.agreementShakeOffset;
  const checked = ctx.state.agreementAccepted;
  const checkboxStroke = ctx.state.agreementShakeOffset === 0 ? palette.line : palette.red;
  group.addChild(rect(x, 400, 10, 10, palette.paper, checkboxStroke, 1));
  if (checked) {
    group.addChild(line(x + 2, 405, x + 5, 408, palette.green, 2));
    group.addChild(line(x + 5, 408, x + 10, 401, palette.green, 2));
  }
  group.addChild(
    label("我已经详细阅读并同意《用户服务协议》《隐私政策》《儿童隐私保护协议》《第三方信息共享清单》", x + 16, 397, 12, palette.ink, {
      fontWeight: "800",
    }),
  );
  if (ctx.state.agreementShakeOffset !== 0) {
    group.addChild(label("请先勾选同意协议", x + 322, 415, 11, palette.red, { fontWeight: "800" }));
  }
  group.eventMode = "static";
  group.cursor = "pointer";
  group.hitArea = new Rectangle(x - 6, 393, 620, 28);
  group.on("pointertap", (event) => {
    event.stopPropagation();
    ctx.actions.toggleAgreement();
  });
  return group;
}

function drawInputBox(
  ctx: RenderContext,
  input: NonNullable<RenderContext["state"]["activeInput"]>,
  x: number,
  y: number,
  width: number,
  height: number,
  value: string,
  placeholder: string,
) {
  const group = new Container();
  const active = ctx.state.activeInput === input;
  group.addChild(rect(x, y, width, height, ui.color.inputFill, active ? palette.blue : palette.line, 6));
  group.addChild(leftCenterLabel(value || placeholder, x + 14, y + height / 2, 14, value ? palette.ink : palette.muted, { fontWeight: "700" }));
  if (active) {
    group.addChild(line(x + width - 12, y + 8, x + width - 12, y + height - 8, palette.blue, 2));
  }
  group.eventMode = "static";
  group.cursor = "text";
  group.hitArea = new Rectangle(x, y, width, height);
  group.on("pointertap", (event) => {
    event.stopPropagation();
    ctx.actions.focusInput(input);
  });
  return group;
}

function drawExternalLink(text: string, x: number, y: number, width: number, height: number) {
  const group = new Container();
  const url = text;
  group.addChild(label(text, x, y, 13, palette.blue, { fontWeight: "900" }));
  group.addChild(line(x, y + height - 2, x + width, y + height - 2, palette.blue, 1));
  group.eventMode = "static";
  group.cursor = "pointer";
  group.hitArea = new Rectangle(x, y - 2, width, height + 4);
  group.on("pointertap", () => {
    window.open(url, "_blank", "noopener,noreferrer");
  });
  return group;
}
