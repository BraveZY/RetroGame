import "./styles.css";
import { Application, Container } from "pixi.js";
import { themes, VIEW_HEIGHT, VIEW_WIDTH, type AppActions, type AppState, type RenderContext } from "./domain";
import { drawBase, setUiRenderScale } from "./pixiPrimitives";
import { drawGame, drawPause } from "./screens/gameScreens";
import { drawHome } from "./screens/homeScreen";
import { drawCollection, drawHelp, drawSettings } from "./screens/metaScreens";
import { drawHealthAdvice, drawLogin, drawPhoneLogin, drawRealNameAuth } from "./screens/authScreens";
import { drawResult, drawReward } from "./screens/postGameScreens";
import { drawPrepare, drawRecognition } from "./screens/prepareScreen";
import { drawTheme } from "./screens/themeScreen";

const loginMusic = new Audio("/assets/audio/login-loop.mp3");
loginMusic.loop = true;
loginMusic.preload = "auto";
loginMusic.volume = 0.55;

let loginMusicUnlocked = false;

const stage = new Container();
const COLLECTION_SCROLL_MAX = 228;
const state: AppState = {
  screen: "healthAdvice",
  selectedTheme: themes[0],
  lightState: "green",
  selectedAvatar: "captain",
  pendingAvatar: "captain",
  avatarPickerOpen: false,
  agreementAccepted: false,
  agreementShakeOffset: 0,
  activeInput: null,
  phoneNumber: "",
  verificationCode: "",
  realName: "",
  idNumber: "",
  selectedSettingsGroup: "family",
  collectionScroll: 0,
};

const actions: AppActions = {
  nav: (screen) => {
    state.screen = screen;
    state.activeInput = null;
    state.avatarPickerOpen = false;
    state.pendingAvatar = state.selectedAvatar;
    if (screen !== "collection") state.collectionScroll = 0;
    render();
  },
  selectTheme: (theme) => {
    state.selectedTheme = theme;
    render();
  },
  toggleLight: () => {
    state.lightState = state.lightState === "green" ? "red" : "green";
    render();
  },
  requestAgreement: () => {
    const offsets = [-10, 10, -8, 8, -5, 5, 0];
    offsets.forEach((offset, index) => {
      window.setTimeout(() => {
        state.agreementShakeOffset = offset;
        render();
      }, index * 42);
    });
  },
  toggleAgreement: () => {
    state.agreementAccepted = !state.agreementAccepted;
    state.agreementShakeOffset = 0;
    render();
  },
  focusInput: (input) => {
    state.activeInput = input;
    render();
  },
  typeInput: (value) => {
    if (state.activeInput === "phone") state.phoneNumber = (state.phoneNumber + value.replace(/\D/g, "")).slice(0, 11);
    if (state.activeInput === "code") state.verificationCode = (state.verificationCode + value.replace(/\D/g, "")).slice(0, 6);
    if (state.activeInput === "realName") state.realName = (state.realName + value).slice(0, 8);
    if (state.activeInput === "idNumber") state.idNumber = (state.idNumber + value.toUpperCase()).replace(/[^0-9X]/g, "").slice(0, 18);
    render();
  },
  backspaceInput: () => {
    if (state.activeInput === "phone") state.phoneNumber = state.phoneNumber.slice(0, -1);
    if (state.activeInput === "code") state.verificationCode = state.verificationCode.slice(0, -1);
    if (state.activeInput === "realName") state.realName = state.realName.slice(0, -1);
    if (state.activeInput === "idNumber") state.idNumber = state.idNumber.slice(0, -1);
    render();
  },
  selectSettingsGroup: (group) => {
    state.selectedSettingsGroup = group;
    render();
  },
  scrollCollection: (deltaY) => {
    if (state.screen !== "collection") return;
    state.collectionScroll = Math.max(0, Math.min(COLLECTION_SCROLL_MAX, state.collectionScroll + deltaY));
    render();
  },
  openAvatarPicker: () => {
    state.pendingAvatar = state.selectedAvatar;
    state.avatarPickerOpen = true;
    render();
  },
  closeAvatarPicker: () => {
    state.pendingAvatar = state.selectedAvatar;
    state.avatarPickerOpen = false;
    render();
  },
  chooseAvatar: (avatar) => {
    state.pendingAvatar = avatar;
    render();
  },
  saveAvatar: () => {
    state.selectedAvatar = state.pendingAvatar;
    state.avatarPickerOpen = false;
    render();
  },
};

const ctx: RenderContext = { state, actions };

function render() {
  drawBase(stage);
  if (state.screen === "healthAdvice") drawHealthAdvice(stage, ctx);
  if (state.screen === "login") drawLogin(stage, ctx);
  if (state.screen === "phoneLogin") drawPhoneLogin(stage, ctx);
  if (state.screen === "realNameAuth") drawRealNameAuth(stage, ctx);
  if (state.screen === "home") drawHome(stage, ctx);
  if (state.screen === "theme") drawTheme(stage, ctx);
  if (state.screen === "prepare") drawPrepare(stage, ctx);
  if (state.screen === "recognition") drawRecognition(stage, ctx);
  if (state.screen === "game") drawGame(stage, ctx);
  if (state.screen === "pause") drawPause(stage, ctx);
  if (state.screen === "result") drawResult(stage, ctx);
  if (state.screen === "reward") drawReward(stage, ctx);
  if (state.screen === "collection") drawCollection(stage, ctx);
  if (state.screen === "help") drawHelp(stage, ctx);
  if (state.screen === "settings") drawSettings(stage, ctx);
  syncLoginMusic();
}

function syncLoginMusic() {
  if (state.screen !== "login") {
    loginMusic.pause();
    return;
  }

  if (!loginMusicUnlocked) return;

  void loginMusic.play().catch(() => {
    loginMusicUnlocked = false;
  });
}

function unlockLoginMusic() {
  loginMusicUnlocked = true;
  syncLoginMusic();
}

async function bootstrap() {
  const root = document.querySelector<HTMLDivElement>("#pixi-root");
  if (!root) {
    throw new Error("Missing #pixi-root");
  }

  const app = new Application();
  await app.init({
    width: VIEW_WIDTH,
    height: VIEW_HEIGHT,
    background: "#fff6df",
    antialias: true,
    autoDensity: true,
    resolution: window.devicePixelRatio || 1,
  });

  app.canvas.className = "pixi-flow-canvas";
  root.appendChild(app.canvas);
  app.stage.addChild(stage);
  const resizeStage = () => {
    const width = Math.max(1, root.clientWidth);
    const height = Math.max(1, root.clientHeight);
    const scale = Math.min(width / VIEW_WIDTH, height / VIEW_HEIGHT);
    const resolution = Math.min(3, window.devicePixelRatio || 1);
    app.renderer.resize(width, height, resolution);
    stage.scale.set(scale);
    setUiRenderScale(scale);
    render();
  };
  const resizeObserver = new ResizeObserver(resizeStage);
  resizeObserver.observe(root);
  window.addEventListener("resize", resizeStage);
  window.addEventListener("pointerdown", unlockLoginMusic);
  window.addEventListener("keydown", unlockLoginMusic);
  window.addEventListener(
    "wheel",
    (event) => {
      if (state.screen !== "collection") return;
      event.preventDefault();
      actions.scrollCollection(event.deltaY * 0.55);
    },
    { passive: false },
  );
  window.addEventListener("keydown", (event) => {
    if (!state.activeInput) return;
    if (event.key === "Backspace") {
      actions.backspaceInput();
      return;
    }
    if (event.key.length === 1) {
      actions.typeInput(event.key);
    }
  });
  resizeStage();
  window.setTimeout(() => {
    if (state.screen === "healthAdvice") {
      actions.nav("login");
    }
  }, 1600);
}

void bootstrap();
